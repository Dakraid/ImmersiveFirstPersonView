#include <fstream>
#include <strsafe.h>
#include <vector>
#include <Windows.h>
#include <DbgHelp.h>
#include "ReplaceImport.h"

HINSTANCE mHinst = nullptr, mHinstDLL = nullptr;
std::vector<HINSTANCE> mLoadedLib;
PROC mOrigFunc = nullptr;
PROC mOrigFunc2 = nullptr;
int i_error = 0;
extern "C" UINT_PTR mProcs[77] = {0};

void Error(const char* msg)
{
	MessageBox(nullptr, msg, "DLL Plugin Loader", 0);
}

auto Log_Error(const std::string& name, std::ofstream& f_log) -> void
{
	// Retrieve the system error message for the last-error code
	auto* h_memory = HeapCreate(0L, 0L, 0L);
	if (h_memory == nullptr)
	{
		return;
	}
	auto* lp_name = const_cast<LPTSTR>(name.c_str());
	LPSTR lp_msg_buf = nullptr;
	const auto dw = GetLastError();

	FormatMessage(
		FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
		nullptr,
		dw,
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		reinterpret_cast<LPSTR>(&lp_msg_buf),
		0,
		nullptr);

	// Display the error message and exit the process
	auto* lp_display_buf = HeapAlloc(h_memory, 0x00000004,
	                                 (static_cast<unsigned long long>(lstrlen(lp_msg_buf)) + lstrlen(lp_name) + 40) *
	                                 sizeof(TCHAR));
	if (lp_display_buf == nullptr)
	{
		return;
	}

	StringCchPrintf(static_cast<LPTSTR>(lp_display_buf), HeapSize(h_memory, 0L, lp_display_buf) / sizeof(TCHAR),
	                TEXT("%s failed with error %d: %s"), lp_name, dw, lp_msg_buf);

	f_log << static_cast<LPTSTR>(lp_display_buf) << "\n";

	HeapFree(h_memory, 0L, lp_display_buf);
}

typedef void (*MYPROC)();

int LoadDLLPlugin(const char* path)
{
	auto state = -1;
	try
	{
		//const std::string sPath = path;
		//if (sPath.find("Ijwhost.dll") != std::string::npos)
		//	return 3;

		auto* lib = LoadLibraryEx(path, nullptr, 0x00001000);
		if (lib == nullptr)
		{
			auto err = GetLastError();
			return 0;
		}

		auto ok = 1;
		const auto func_addr = (MYPROC) GetProcAddress(lib, "Initialize");
		if (func_addr != nullptr)
		{
			state = -2;
			(func_addr)();
			ok = 2;
		}

		mLoadedLib.push_back(lib);
		return ok;
	}
	catch (std::exception& e)
	{
		Error(e.what());
	}

	return state;
}

std::string GetPluginsDirectory()
{
	return "Data\\DLLPlugins\\";
}

void LoadLib()
{
	static auto loadLibrary = true;
	if (loadLibrary)
	{
		loadLibrary = false;

		std::ofstream fLog;
		{
			auto iLog = std::ifstream("binkw64.log");
			if (iLog.good())
			{
				iLog.close();
				fLog = std::ofstream("binkw64.log");
			}
		}

		WIN32_FIND_DATA wfd;
		auto dir = GetPluginsDirectory();
		auto search_dir = dir + "*.dll";
		auto* hFind = FindFirstFile(search_dir.c_str(), &wfd);
		if (hFind != INVALID_HANDLE_VALUE)
		{
			do
			{
				if ((wfd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0)
					continue;

				std::string name = wfd.cFileName;
				name = dir + name;

				if (fLog.good())
					fLog << "Checking \"" << name.c_str() << "\" ... ";

				auto result = LoadDLLPlugin(name.c_str());
				switch (result)
				{
				case 3:
				{
					{
						if (fLog.good())
						{
							std::string err = "Skipped loading ";
							err = err + name;
							fLog << err << "\n";
						}
						break;
					}
				}
				case 2:
					{
						if (fLog.good())
							fLog << "OK - loaded and called Initialize().\n";
						break;
					}

				case 1:
					{
						if (fLog.good())
							fLog << "OK - loaded.\n";
						break;
					}

				case 0:
					{
						if (fLog.good())
						{
							fLog << "LoadLibrary failed!\n";
							Log_Error(name, fLog);
						}
						i_error = 1;
						std::string err = "LoadLibrary failed on ";
						err = err + name;
						Error(err.c_str());
						break;
					}

				case -1:
					{
						if (fLog.good())
						{
							fLog << "LoadLibrary crashed! This means there's a problem in the plugin DLL file.\n";
							Log_Error(name, fLog);
						}
						i_error = 1;
						std::string err = "LoadLibrary crashed on ";
						err = err + name;
						err = err +
							". This means there's a problem in the plugin DLL file. Contact the author of that plugin.";
						Error(err.c_str());
						break;
					}

				case -2:
					{
						if (fLog.good())
						{
							fLog << "Initialize() crashed! This means there's a problem in the plugin DLL file.\n";
							Log_Error(name, fLog);
						}
						i_error = 1;
						std::string err = "Initialize() crashed on ";
						err = err + name;
						err = err +
							". This means there's a problem in the plugin DLL file. Contact the author of that plugin.";
						Error(err.c_str());
						break;
					}
				}
			}
			while (i_error == 0 && FindNextFile(hFind, &wfd));

			FindClose(hFind);
		}
		else
		{
			if (fLog.good())
				fLog << "Failed to get search handle to \"" << search_dir.c_str() << "\"!\n";
		}
	}
}

PVOID Do_Hook2(PVOID arg1, PVOID arg2)
{
	LoadLib();

	return reinterpret_cast<PVOID(*)(PVOID, PVOID)>(mOrigFunc2)(arg1, arg2);
}

void HookLib()
{
	const auto result = ReplaceImport::Replace("api-ms-win-crt-runtime-l1-1-0.dll", "_initterm_e",
	                                           reinterpret_cast<PROC>(Do_Hook2), &mOrigFunc2);
	switch (result)
	{
	case 0: break;
	case 1: Error("Failed to get handle to main module!");
		break;
	case 2: Error("Failed to find import table in executable!");
		break;
	case 3: Error("Failed to change protection flags on memory page!");
		break;
	case 4: Error("Failed to find API function in module!");
		break;
	case 5: Error("Failed to find module!");
		break;
	default: Error("Unknown error occurred!");
		break;
	}
}

LPCSTR mImportNames[] = {
	"BinkBufferBlit", "BinkBufferCheckWinPos", "BinkBufferClear", "BinkBufferClose", "BinkBufferGetDescription",
	"BinkBufferGetError", "BinkBufferLock", "BinkBufferOpen", "BinkBufferSetDirectDraw", "BinkBufferSetHWND",
	"BinkBufferSetOffset", "BinkBufferSetResolution", "BinkBufferSetScale", "BinkBufferUnlock", "BinkCheckCursor",
	"BinkClose", "BinkCloseTrack", "BinkControlBackgroundIO", "BinkControlPlatformFeatures", "BinkCopyToBuffer",
	"BinkCopyToBufferRect", "BinkDDSurfaceType", "BinkDX8SurfaceType", "BinkDX9SurfaceType", "BinkDoFrame",
	"BinkDoFrameAsync", "BinkDoFrameAsyncWait", "BinkDoFramePlane", "BinkFreeGlobals", "BinkGetError",
	"BinkGetFrameBuffersInfo", "BinkGetKeyFrame", "BinkGetPalette", "BinkGetPlatformInfo", "BinkGetRealtime",
	"BinkGetRects", "BinkGetSummary", "BinkGetTrackData", "BinkGetTrackID", "BinkGetTrackMaxSize", "BinkGetTrackType",
	"BinkGoto", "BinkIsSoftwareCursor", "BinkLogoAddress", "BinkNextFrame", "BinkOpen", "BinkOpenDirectSound",
	"BinkOpenMiles", "BinkOpenTrack", "BinkOpenWaveOut", "BinkOpenWithOptions", "BinkOpenXAudio2", "BinkPause",
	"BinkRegisterFrameBuffers", "BinkRequestStopAsyncThread", "BinkRestoreCursor", "BinkService", "BinkSetError",
	"BinkSetFileOffset", "BinkSetFrameRate", "BinkSetIO", "BinkSetIOSize", "BinkSetMemory", "BinkSetPan",
	"BinkSetSimulate", "BinkSetSoundOnOff", "BinkSetSoundSystem", "BinkSetSoundTrack", "BinkSetSpeakerVolumes",
	"BinkSetVideoOnOff", "BinkSetVolume", "BinkSetWillLoop", "BinkShouldSkip", "BinkStartAsyncThread", "BinkWait",
	"BinkWaitStopAsyncThread", "RADTimerRead"
};

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
	mHinst = hinstDLL;
	if (fdwReason == DLL_PROCESS_ATTACH)
	{
		mHinstDLL = LoadLibrary("binkw64_.dll");
		if (!mHinstDLL)
		{
			Error(
				"Failed to load binkw64_.dll! Make sure the original file exists with an underscore at the end of name.");
			return FALSE;
		}

		for (auto i = 0; i < 77; i++)
			mProcs[i] = reinterpret_cast<UINT_PTR>(GetProcAddress(mHinstDLL, mImportNames[i]));

		HookLib();
	}
	else if (fdwReason == DLL_PROCESS_DETACH)
	{
		FreeLibrary(mHinstDLL);

		if (!mLoadedLib.empty())
		{
			for (auto* lib : mLoadedLib)
				FreeLibrary(lib);
			mLoadedLib.clear();
		}
	}
	return TRUE;
}

extern "C" void BinkBufferBlit_wrapper();
extern "C" void BinkBufferCheckWinPos_wrapper();
extern "C" void BinkBufferClear_wrapper();
extern "C" void BinkBufferClose_wrapper();
extern "C" void BinkBufferGetDescription_wrapper();
extern "C" void BinkBufferGetError_wrapper();
extern "C" void BinkBufferLock_wrapper();
extern "C" void BinkBufferOpen_wrapper();
extern "C" void BinkBufferSetDirectDraw_wrapper();
extern "C" void BinkBufferSetHWND_wrapper();
extern "C" void BinkBufferSetOffset_wrapper();
extern "C" void BinkBufferSetResolution_wrapper();
extern "C" void BinkBufferSetScale_wrapper();
extern "C" void BinkBufferUnlock_wrapper();
extern "C" void BinkCheckCursor_wrapper();
extern "C" void BinkClose_wrapper();
extern "C" void BinkCloseTrack_wrapper();
extern "C" void BinkControlBackgroundIO_wrapper();
extern "C" void BinkControlPlatformFeatures_wrapper();
extern "C" void BinkCopyToBuffer_wrapper();
extern "C" void BinkCopyToBufferRect_wrapper();
extern "C" void BinkDDSurfaceType_wrapper();
extern "C" void BinkDX8SurfaceType_wrapper();
extern "C" void BinkDX9SurfaceType_wrapper();
extern "C" void BinkDoFrame_wrapper();
extern "C" void BinkDoFrameAsync_wrapper();
extern "C" void BinkDoFrameAsyncWait_wrapper();
extern "C" void BinkDoFramePlane_wrapper();
extern "C" void BinkFreeGlobals_wrapper();
extern "C" void BinkGetError_wrapper();
extern "C" void BinkGetFrameBuffersInfo_wrapper();
extern "C" void BinkGetKeyFrame_wrapper();
extern "C" void BinkGetPalette_wrapper();
extern "C" void BinkGetPlatformInfo_wrapper();
extern "C" void BinkGetRealtime_wrapper();
extern "C" void BinkGetRects_wrapper();
extern "C" void BinkGetSummary_wrapper();
extern "C" void BinkGetTrackData_wrapper();
extern "C" void BinkGetTrackID_wrapper();
extern "C" void BinkGetTrackMaxSize_wrapper();
extern "C" void BinkGetTrackType_wrapper();
extern "C" void BinkGoto_wrapper();
extern "C" void BinkIsSoftwareCursor_wrapper();
extern "C" void BinkLogoAddress_wrapper();
extern "C" void BinkNextFrame_wrapper();
extern "C" void BinkOpen_wrapper();
extern "C" void BinkOpenDirectSound_wrapper();
extern "C" void BinkOpenMiles_wrapper();
extern "C" void BinkOpenTrack_wrapper();
extern "C" void BinkOpenWaveOut_wrapper();
extern "C" void BinkOpenWithOptions_wrapper();
extern "C" void BinkOpenXAudio2_wrapper();
extern "C" void BinkPause_wrapper();
extern "C" void BinkRegisterFrameBuffers_wrapper();
extern "C" void BinkRequestStopAsyncThread_wrapper();
extern "C" void BinkRestoreCursor_wrapper();
extern "C" void BinkService_wrapper();
extern "C" void BinkSetError_wrapper();
extern "C" void BinkSetFileOffset_wrapper();
extern "C" void BinkSetFrameRate_wrapper();
extern "C" void BinkSetIO_wrapper();
extern "C" void BinkSetIOSize_wrapper();
extern "C" void BinkSetMemory_wrapper();
extern "C" void BinkSetPan_wrapper();
extern "C" void BinkSetSimulate_wrapper();
extern "C" void BinkSetSoundOnOff_wrapper();
extern "C" void BinkSetSoundSystem_wrapper();
extern "C" void BinkSetSoundTrack_wrapper();
extern "C" void BinkSetSpeakerVolumes_wrapper();
extern "C" void BinkSetVideoOnOff_wrapper();
extern "C" void BinkSetVolume_wrapper();
extern "C" void BinkSetWillLoop_wrapper();
extern "C" void BinkShouldSkip_wrapper();
extern "C" void BinkStartAsyncThread_wrapper();
extern "C" void BinkWait_wrapper();
extern "C" void BinkWaitStopAsyncThread_wrapper();
extern "C" void RADTimerRead_wrapper();
