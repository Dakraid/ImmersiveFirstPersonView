
# Immersive First Person View (10)
This is my personal fork of IFPV9 by meh321 which also includes his .NET Script Framework for Skyrim SE.
All of my changes are based on his work, all of the heavy lifting has been done by him. 

His original source code can be found on his modpages, link to his profile: [meh321 on Nexusmods](https://www.nexusmods.com/skyrimspecialedition/users/2964753?tab=user%20files) 

I uploaded my forked source code with meh321's explicit permission.

# Description (quoted from meh321's modpage)
### Immersive First Person View ([Original Modpage](https://www.nexusmods.com/skyrimspecialedition/mods/22306))
Lets you see your character's body in first person view. Also adds first person view when you normally can't such as horse riding, crafting, werewolf, vampire lord and almost any other time when the game would force you to exit the first person view.

This mod is similar to the Immersive First Person View (Legendary Edition) but it's been remade completely from scratch for SSE so there will be big differences if you're used to the old one.

### .NET Script Framework ([Original Modpage](https://www.nexusmods.com/skyrimspecialedition/mods/21294))
.NET Script Framework allows mod authors to write DLL plugins for any game or application in any .NET language. Currently it only supports 64 bit but there are plans to support 32 bit applications in the future.  
  
As of right now this is a very early release and it is not recommended to release any plugins created with the framework just yet or they may end up breaking if the plugin format is changed. I will try to avoid any such changes though.  
  
Tutorials for the framework are planned.  
  
The reason for releasing it this early is because mod authors can play around with it hopefully and let me know if improvements could be made to make plugin authoring easier or if any issues are found. Regular users can also benefit from the framework's crash logs to help troubleshoot other mod issues they may be experiencing.

# Changes
Currently my fork updated the used .NET Framework to v4.8 in the C# projects as well as retargeted the SDK and Platform Toolset to the latest available on my system. I also set the C and C++ Language Standard to their respective latest ISO (C17 and C++17).

I applied my personal style and formatting, and adjusted some minor logic to condense and in some cases streamline the existing program code.

My focus is to implement some personal wishes I have with IFPV to accommodate my personal preferences as well as some bugs I have come across. The following list should provide a rough but incomplete overview and is in now way set in stone.

# Feature List
Immersive First Person Camera:
 - [x] Implement configuration reloading through hotkey
 - [ ] Fix the head-bobbing being applied to the horizontal view axis 
 - [ ] Normalize the orientation of the camera no matter the translation of the skeleton node
 - [ ] Allow for velocity based adjustments of certain camera behavior (no inherited rotation eg.)

.NET Script Framework:
 - [ ] Allow for (hot) reloading of already loaded DLLs automatically while in-game
 - [ ] Provide an unified and central logging file for all plugins to use (unless they use their own)
 - [ ] Look into the possibility of porting the project to .NET 5
