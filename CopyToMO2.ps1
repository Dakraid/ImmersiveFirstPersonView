$target = 'S:\ModOrganizerSE\mods\.NET5 Script Framework + IFPV 100\'
$source = 'C:\Users\Netrve\source\repos\ImmersiveFirstPersonView\Build\Debug\Data\*'

Copy-Item -Path $source -Destination $target -Recurse -force
Get-ChildItem $target -Include 'ref' -Recurse -force | Remove-Item -Force -Recurse