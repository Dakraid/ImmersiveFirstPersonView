// ReSharper disable InconsistentNaming

namespace IFPV
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using NetScriptFramework;
    using NetScriptFramework.SkyrimSE;
    using Main = NetScriptFramework.Main;
    using Timer = NetScriptFramework.Tools.Timer;

    public sealed class IFPVPlugin : Plugin
    {
        private readonly Dictionary<IntPtr, ulong> _fp_called = new Dictionary<IntPtr, ulong>();
        private bool _had_free_look = true;
        private int _is_spine;
        private long _lastDiff;
        private long _lastTimer;
        private long _time;

        private IntPtr MagicNodeArt1;
        private IntPtr MagicNodeArt2;
        private IntPtr MagicNodeArt3;
        private IntPtr MagicNodeArt4;

        private Timer Timer;

        private bool WasGamePaused;

        public override string Author => "meh321";

        public override string Key => "ifpv";

        public override string Name => "Immersive First Person View";

        public override int RequiredLibraryVersion => 13;

        public Settings Settings { get; private set; }

        public long Time
        {
            get => Interlocked.Read(ref this._time);
            private set => Interlocked.Exchange(ref this._time, value);
        }

        public override int Version => 100;

        public override string Website => "https://www.nexusmods.com/skyrimspecialedition/mods/22306";

        internal static IFPVPlugin Instance { get; private set; }

        internal long _lastDiff2 { get; private set; }

        internal IntPtr Actor_GetMoveDirection { get; private set; }

        internal IntPtr ActorTurnX { get; private set; }

        internal IntPtr ActorTurnZ { get; private set; }

        internal CameraMain CameraMain { get; private set; }

        internal IntPtr NiNode_ctor { get; private set; }

        internal IntPtr PlayerControls_IsCamSwitchControlsEnabled { get; private set; }

        internal IntPtr SwitchSkeleton { get; private set; }

        protected override bool Initialize(bool loadedAny)
        {
            Instance = this;
            this.init();

            return true;
        }

        private void init()
        {
            this.Timer = new Timer();
            this.Timer.Start();

            this.Settings = new Settings();
            this.Settings.Load();

            this.PlayerControls_IsCamSwitchControlsEnabled =
                this.PrepareFunction("player camera switch controls check", 41263, 0);

            this.NiNode_ctor = this.PrepareFunction("ninode ctor", 68936, 0);
            this.MagicNodeArt1 = this.PrepareFunction("magic node art 1", 33403, 0x6F);
            this.MagicNodeArt2 = this.PrepareFunction("magic node art 2", 33391, 0x64);
            this.MagicNodeArt3 = this.PrepareFunction("magic node art 3", 33375, 0xF5);
            this.MagicNodeArt4 = this.PrepareFunction("magic node art 4", 33683, 0x63);
            this.ActorTurnX = this.PrepareFunction("actor turn x", 36603, 0);
            this.ActorTurnZ = this.PrepareFunction("actor turn z", 36250, 0);
            this.SwitchSkeleton = this.PrepareFunction("switch skeleton", 39401, 0);
            this.Actor_GetMoveDirection = this.PrepareFunction("actor move direction", 36935, 0);

            if (this.Settings.AllowLookDownAlot)
            {
                var allowLookDownMore = Main.GameInfo.GetAddressOf(49978, 0xBA, 0, "F3 0F 5C 15");
                var allowLookDownDisableCheck = Main.GameInfo.GetAddressOf(49978, 0x100, 0, "0F 57 C9 0F 2F C1 73");
                var skipSetAddr = allowLookDownDisableCheck + 0x20;
                if (!Memory.VerifyBytes(skipSetAddr, "48 8B 44 24 40"))
                {
                    throw new ArgumentException("Failed to verify bytes for disabling down look check!");
                }

                var allowMoveDownDisableCheck = Main.GameInfo.GetAddressOf(36602, 0xE4, 0, "44 0F 2F C7");

                Memory.WriteHook(new HookParameters
                {
                    Address = allowLookDownMore,
                    ReplaceLength = 8,
                    IncludeLength = 0,
                    Before = ctx =>
                    {
                        const float half = (float)(Math.PI * 0.5);
                        if (this.CameraMain != null && this.CameraMain.IsEnabled)
                        {
                            ctx.XMM2f -= half * 1.5f;
                        }
                        else
                        {
                            ctx.XMM2f -= half;
                        }
                    }
                });

                Memory.WriteHook(new HookParameters
                {
                    Address = allowLookDownDisableCheck,
                    ReplaceLength = 8,
                    IncludeLength = 0,
                    Before = ctx =>
                    {
                        var angle = ctx.XMM0f;
                        var allowed = 0.0f;
                        if (this.CameraMain != null && this.CameraMain.IsEnabled)
                        {
                            allowed = -(float)(Math.PI / 4.0);
                        }

                        if (angle <= allowed)
                        {
                            ctx.IP = skipSetAddr;
                        }
                    }
                });

                Memory.WriteHook(new HookParameters
                {
                    Address = allowMoveDownDisableCheck,
                    IncludeLength = 0,
                    ReplaceLength = 0x1A,
                    Before = ctx =>
                    {
                        var cur = ctx.XMM8f;
                        var min = ctx.XMM7f;
                        var max = ctx.XMM6f;

                        if (this.CameraMain != null && this.CameraMain.IsEnabled)
                        {
                            max += (float)(Math.PI / 4.0);
                        }

                        if (cur > max)
                        {
                            ctx.XMM1f = max;
                        }
                        else if (cur < min)
                        {
                            ctx.XMM1f = min;
                        }
                        else
                        {
                            ctx.XMM1f = cur;
                        }
                    }
                });
            }

            this.CameraMain = new CameraMain(this);

            Events.OnFrame.Register(e =>
            {
                var main = NetScriptFramework.SkyrimSE.Main.Instance;
                if (main == null)
                {
                    return;
                }
#if PROFILING
                    if (this.CameraMain._prof_state == 1)
                    {
                        this.CameraMain.end_track(CameraMain._performance_track.Frame);
                        this.CameraMain.begin_track(CameraMain._performance_track.Frame);
                    }
#endif

                var paused = main.IsGamePaused;
                var anyPaused = paused;
                if (paused != this.WasGamePaused)
                {
                    anyPaused = true;
                    this.WasGamePaused = paused;
                }

                var now = this.Timer.Time;
                this._lastDiff = 0;
                if (!anyPaused)
                {
                    var diff = now - this._lastTimer;
                    if (diff > 200)
                    {
                        diff = 200;
                    }

                    this.Time += diff;
                    this._lastDiff = diff;
                }

                this._lastTimer = now;
                this._lastDiff2 = this._lastDiff;
            });

            Events.OnMainMenu.Register(e =>
                {
                    if (this.CameraMain != null && !this.CameraMain.IsInitialized)
                    {
                        this.CameraMain.Initialize();
                    }
                },
                0,
                1);

            Events.OnUpdateCamera.Register(e =>
                {
                    if (this.CameraMain == null || !this.CameraMain.IsInitialized)
                    {
                        return;
                    }
#if PROFILING
                    if (this.CameraMain._prof_state == 1)
                        this.CameraMain.begin_track(CameraMain._performance_track.CameraUpdate);
#endif

                    // bool ok;
                    if (this.CameraMain.Update(e))
                    {
                        return;
                    }

                    if (this._lastDiff <= 0)
                    {
                        return;
                    }

                    this.Time -= this._lastDiff;
                    this._lastDiff = 0;

#if PROFILING
                    if (this.CameraMain._prof_state == 1)
                        this.CameraMain.end_track(CameraMain._performance_track.CameraUpdate);
#endif

#if PROFILING
                    if (ok && this.CameraMain._prof_state == 0)
                    {
                        this.CameraMain._prof_state = 1;
                        this.CameraMain.begin_track(CameraMain._performance_track.Frame);
                    }
                    else if (this.CameraMain._prof_state == 1 && NetScriptFramework.Tools.Input.IsPressed(NetScriptFramework.Tools.VirtualKeys.N9))
                    {
                        this.CameraMain._prof_state = 2;
                        this.CameraMain._end_profiling();
                    }
#endif
                },
                1000);

            Events.OnUpdatedPlayerHeadtrack.Register(e =>
                {
                    if (this.CameraMain != null && this.CameraMain.IsInitialized)
                    {
                        this.CameraMain.UpdateHeadtrack();
                    }
                },
                50);

            Events.OnUpdatePlayerTurnToCamera.Register(e =>
                {
                    if (this.CameraMain == null || !this.CameraMain.IsInitialized)
                    {
                        return;
                    }

                    var value = this.CameraMain.Values.FaceCamera.CurrentValue;
                    if (value >= 1.0)
                    {
                        e.FreeLook = false;
                    }
                    else if (value <= -1.0)
                    {
                        e.FreeLook = true;
                    }

                    var isFree = e.FreeLook;
                    if (this._had_free_look == isFree)
                    {
                        return;
                    }

                    this._had_free_look = isFree;
                    if (!isFree && this.CameraMain.IsEnabled)
                    {
                        this.CameraMain.OnMakeTurn();
                    }
                },
                50);

            Events.OnShadowCullingBegin.Register(e =>
                {
                    if (this.CameraMain == null || !this.CameraMain.IsInitialized || !this.CameraMain.IsEnabled ||
                        !this.Settings.SeparateShadowCulling)
                    {
                        return;
                    }

                    e.Separate = true;
                    this.CameraMain.OnShadowCulling(0);
                },
                1000);

            Events.OnShadowCullingEnd.Register(e =>
                {
                    if (this.CameraMain != null &&
                        this.CameraMain.IsInitialized &&
                        this.CameraMain.IsEnabled &&
                        this.Settings.SeparateShadowCulling)
                    {
                        this.CameraMain.OnShadowCulling(1);
                    }
                },
                1000);

            Events.OnWeaponFireProjectilePosition.Register(e =>
            {
                if (this.CameraMain == null || !this.CameraMain.IsInitialized)
                {
                    return;
                }

                var obj = e.Attacker;
                var did = this.CameraMain.GetOverwriteWeaponNode(obj, e.Position);

                if (did)
                {
                    e.Node = null;
                }
            });

            this.InstallHook("magic fire node art",
                33361,
                0x7E,
                7,
                "41 FF 90 78 03 00 00",
                ctx =>
                {
                    if (this.CameraMain == null ||
                        !this.CameraMain.IsInitialized ||
                        !this.CameraMain.WasUsingFirstPersonArms)
                    {
                        return;
                    }

                    var caster = MemoryObject.FromAddress<MagicCaster>(ctx.DI);
                    var node = this.CameraMain.GetOverwriteMagicNode(caster);
                    if (node != null)
                    {
                        ctx.DX = new IntPtr((long)1);
                    }
                });

            this.InstallHook("magic fire node",
                33361,
                0,
                6,
                "40 57 48 83 EC 20",
                ctx =>
                {
                    if (this.CameraMain == null ||
                        !this.CameraMain.IsInitialized ||
                        !this.CameraMain.WasUsingFirstPersonArms)
                    {
                        return;
                    }

                    var calledFrom = Memory.ReadPointer(ctx.SP);
                    if (calledFrom == this.MagicNodeArt1 ||
                        calledFrom == this.MagicNodeArt2 ||
                        calledFrom == this.MagicNodeArt3 ||
                        calledFrom == this.MagicNodeArt4)

                        //NetScriptFramework.Main.WriteDebugMessage("Bad called from: " + calledFrom.ToBase().ToHexString());
                    {
                        return;
                    }

                    var node = this.CameraMain.GetOverwriteMagicNode(MemoryObject.FromAddress<MagicCaster>(ctx.CX));
                    if (node == null)

                        //NetScriptFramework.Main.WriteDebugMessage("Null overwrite");
                    {
                        return;
                    }

                    ctx.Skip();
                    ctx.IP += 0x2D;
                    ctx.AX = node.Address;

                    //NetScriptFramework.Main.WriteDebugMessage("Replaced node");
                });

            // Block character model fading out
            this.InstallHook("block fade out",
                49899,
                0x3C,
                6,
                "4C 8B F2 48 8B F9",
                ctx =>
                {
                    if (this.CameraMain == null || !this.CameraMain.IsInitialized ||
                        !(this.CameraMain.Values.BlockPlayerFadeOut.CurrentValue >= 0.5))
                    {
                        return;
                    }

                    if (Settings.Instance.HidePlayerWhenColliding != 1 || !this.CameraMain.DidCollideLastUpdate)
                    {
                        ctx.R13 = new IntPtr(0);
                    }
                });

            // Overwrite the turn part
            this.InstallHook("actor turn overwrite",
                49968,
                0xBB,
                0x2E,
                "F3 0F 10 8B D4 00 00 00",
                ctx =>
                {
                    var third = MemoryObject.FromAddress<ThirdPersonState>(ctx.BX);
                    var actor = MemoryObject.FromAddress<Actor>(ctx.AX);

                    if (this.CameraMain != null && this.CameraMain.IsInitialized)
                    {
                        this.CameraMain.HandleActorTurnToCamera(actor, third, true);
                    }

                    ctx.IP += 0x10;
                },
                null,
                true);

            if (Settings.Instance.ReplaceDefaultCamera)
            {
                // Make sure we always use the custom code instead of allowing min zoom to enter first person.
                this.InstallHook("replace zoom #1", 49970, 0x1E1, 5, "E8", null, ctx => { ctx.AX = IntPtr.Zero; });

                // Custom zoom
                this.InstallHook("replace zoom #1",
                    49970,
                    0x22F,
                    7,
                    "48 8B 8B E8 01 00 00",
                    ctx =>
                    {
                        var cptr = Memory.ReadPointer(ctx.AX);
                        var dptr = Memory.ReadPointer(ctx.BX + 0x1E8);
                        var third = MemoryObject.FromAddress<ThirdPersonState>(ctx.DI);

                        if (this.CameraMain != null && this.CameraMain.IsInitialized && third != null)
                        {
                            this.CameraMain.HandleZoom(third, cptr == dptr);
                        }

                        ctx.Skip();
                        ctx.IP = ctx.IP + 0x40;
                    });

                // Dragon and horse must use regular toggle pov handler
                {
                    var ptr = this.PrepareFunction("dragon toggle pov", 32363, 0x1F);
                    if (!Memory.VerifyBytes(ptr, "74 52", true))
                    {
                        throw new InvalidOperationException("Couldn't verify byte pattern for 'dragon toggle pov'!");
                    }

                    Memory.WriteUInt8(ptr, 0xEB, true);
                    ptr = this.PrepareFunction("horse toggle pov", 49832, 0x1F);
                    if (!Memory.VerifyBytes(ptr, "74 52", true))
                    {
                        throw new InvalidOperationException("Couldn't verify byte pattern for 'horse toggle pov'!");
                    }

                    Memory.WriteUInt8(ptr, 0xEB, true);
                }

                // Don't toggle pov from zoom delayed parameter
                {
                    var ptr = this.PrepareFunction("zoom delayed toggle pov", 49977, 0x291);
                    if (!Memory.VerifyBytes(ptr, "74 1C", true))
                    {
                        throw new InvalidOperationException(
                            "Couldn't verify byte pattern for 'zoom delayed toggle pov'!");
                    }

                    Memory.WriteUInt8(ptr, 0xEB, true);
                }

                // Toggle POV hotkey was pressed
                this.InstallHook("toggle pov",
                    49970,
                    0xD5,
                    5,
                    "E8",
                    null,
                    ctx =>
                    {
                        // Skip default action
                        ctx.AX = new IntPtr((long)0);

                        if (this.CameraMain != null && this.CameraMain.IsInitialized)
                        {
                            this.CameraMain.SetWantState(this.CameraMain.AlreadyHasWantState()
                                ? CameraMain.WantStates.DisabledFromTogglePOV
                                : CameraMain.WantStates.EnabledFromTogglePOV);
                        }
                    });

                // Replace forced first person mode from papyrus or other scripted events.
                this.InstallHook("replace first person",
                    49858,
                    0,
                    6,
                    "40 53 48 83 EC 20",
                    ctx =>
                    {
                        if (this.CameraMain == null || !this.CameraMain.IsInitialized)
                        {
                            return;
                        }

                        var calledFrom = Memory.ReadPointer(ctx.SP);
                        if (!this._fp_called.TryGetValue(calledFrom, out var vid))
                        {
                            var fn = Main.GameInfo.GetFunctionInfo(calledFrom, true);
                            if (fn != null)
                            {
                                vid = fn.Id;
                            }

                            this._fp_called[calledFrom] = vid;
                        }

                        var skip = false;
                        var want = false;

                        switch (vid)
                        {
                            case 22463: // Console command
                            case 43115: // Forced by game scripted camera
                                break;

                            case 43098: // Some kind of VATS thing?
                                skip = true;

                                //want = true;
                                break;

                            case 49880: // Piece of furniture had a keyword on it
                                skip = true;

                                //want = true;
                                break;
                        }

                        if (skip)
                        {
                            ctx.Skip();
                            ctx.IP = ctx.IP + 0x39;
                        }

                        if (want && !this.CameraMain.AlreadyHasWantState())
                        {
                            this.CameraMain.SetWantState(CameraMain.WantStates.EnabledFromTogglePOV);
                        }
                    });
            }

            this.InstallHook("fix crosshair pick",
                39534,
                0x159,
                10,
                "F3 0F 58 45 E8 F3 0F 11 45 D8",
                ctx =>
                {
                    if (this.CameraMain == null || !this.CameraMain.IsInitialized || !this.CameraMain.IsEnabled)
                    {
                        return;
                    }

                    var pt = MemoryObject.FromAddress<NiPoint3>(ctx.BP - 0x30);
                    var pcam = PlayerCamera.Instance;
                    if (pcam == null)
                    {
                        return;
                    }

                    ctx.Skip();

                    pt.CopyFrom(pcam.LastNodePosition);
                });

            this.InstallHook("fix look sensitivity",
                41275,
                0x38D,
                0xE,
                "75 0E",
                ctx =>
                {
                    if (this.CameraMain == null || !this.CameraMain.IsInitialized)
                    {
                        return;
                    }

                    var x = Memory.ReadFloat(ctx.BX);
                    var y = Memory.ReadFloat(ctx.BX + 4);
                    this.CameraMain.FixMouseSensitivity(ref x, ref y, ctx.XMM1f);
                    Memory.WriteFloat(ctx.BX, x);
                    Memory.WriteFloat(ctx.BX + 4, y);
                },
                null,
                true);

            this.InstallHook("switch skeleton override",
                39401,
                0,
                7,
                "40 55 56 48 83 EC 78",
                ctx =>
                {
                    if (this.CameraMain == null || !this.CameraMain.IsInitialized)
                    {
                        return;
                    }

                    if (!this.CameraMain.HookSwitchSkeleton(MemoryObject.FromAddress<Actor>(ctx.CX),
                        ctx.DX.ToBool()))
                    {
                        return;
                    }

                    ctx.Skip();
                    ctx.IP += 0x2DD;
                });

            this.InstallHook("fix bound node update",
                18683,
                0x7C,
                6,
                "FF 90 68 04 00 00",
                ctx =>
                {
                    if (this.CameraMain == null || !this.CameraMain.IsInitialized || !this.CameraMain.IsEnabled)
                    {
                        return;
                    }

                    ctx.Skip();
                    var obj = MemoryObject.FromAddress<TESObjectREFR>(ctx.CX);
                    var node = obj.GetSkeletonNode(false);
                    ctx.AX = node?.Address ?? IntPtr.Zero;
                });

            this.InstallHook("fix spine twist",
                59246,
                0x75,
                5,
                "48 83 C4 20 5F",
                ctx =>
                {
                    if (this.CameraMain != null && this.CameraMain.IsInitialized &&
                        Interlocked.CompareExchange(ref this._is_spine, 0, 0) > 0
                    )
                    {
                        this.CameraMain.FixSpineTwist(ctx.DI);
                    }
                });

            this.InstallHook("player update animation",
                39445,
                0x97,
                5,
                "E8",
                ctx => { Interlocked.Increment(ref this._is_spine); },
                ctx => { Interlocked.Decrement(ref this._is_spine); });

            this.InstallHook("player control inc counter",
                41259,
                0,
                5,
                "48 89 5C 24 08",
                ctx => { Interlocked.Increment(ref this._is_spine); });

            this.InstallHook("player controls dec counter",
                41259,
                0x241,
                5,
                "48 83 C4 30 5F",
                ctx => { Interlocked.Decrement(ref this._is_spine); });

            this.InstallHook("player movement controller type",
                40937,
                0x2EA,
                6,
                "FF 90 88 03 00 00",
                null,
                ctx =>
                {
                    if (this.CameraMain == null || !this.CameraMain.IsInitialized)
                    {
                        return;
                    }

                    if (this.CameraMain.Values.ExtraResponsiveControls.CurrentValue >= 0.5)
                    {
                        ctx.AX = IntPtr.Zero;
                    }
                });

            this.InstallHook("before draw",
                35560,
                0x199,
                7,
                "83 8F F4 00 00 00 01",
                ctx =>
                {
                    if (this.CameraMain != null && this.CameraMain.IsInitialized && this.CameraMain.IsEnabled)
                    {
                        this.CameraMain.UpdateSkeletonWithLastParameters();
                    }
                });
        }

        private void InstallHook(string name,
            ulong vid,
            int offset,
            int length,
            string hex,
            Action<CPURegisters> func,
            Action<CPURegisters> after = null,
            bool skip = false)
        {
            var addr = Main.GameInfo.GetAddressOf(vid, offset, 0, hex);

            Memory.WriteHook(new HookParameters
            {
                Address = addr,
                IncludeLength = skip ? 0 : length,
                ReplaceLength = length,
                Before = func,
                After = after
            });
        }

        private IntPtr PrepareFunction(string name, ulong vid, int offset) => Main.GameInfo.GetAddressOf(vid, offset);
    }
}