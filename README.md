# HexcellsInfiniteMultiplayer
A multiplayer mod for Hexcells Infinite

## How it works
To host a multiplayer game, go to the Hexcells Infinite properties on Steam, and set the Launch Options to `-mphost`

To join a multiplayer game, go to the Hexcells Infinite properties on STeam, and set the Launch Options to `-mpjoin IP` ex. `-mpjoin 192.168.0.1`

The mod runs on port 6666

## Dependencies for building/running
- Requires IPA [https://github.com/Eusth/IPA]
- Requires Harmony (net35 build) [https://github.com/pardeike/Harmony]
- Requires the original game, and referencing the original game assemblies (UnityEngine, Assembly-csharp, Assembly-csharp-firstpass)
- Requires a 32 bit build of ENet, installed into the `Hexcells Infinite_Data\Plugins` folder
