$target = 'S:\SteamLibrary\steamapps\common\Skyrim Special Edition\Data'
$source = 'C:\Users\Netrve\source\repos\ImmersiveFirstPersonView\Build\Debug\Data\*'

Copy-Item -Path $source -Destination $target -Recurse -force
Get-ChildItem $target -Include 'ref' -Recurse -force | Remove-Item -Force -Recurse