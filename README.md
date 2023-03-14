# Hexcells Infinite Multiplayer
A multiplayer mod for Hexcells Infinite

## Starting/joining a session
To host a multiplayer game, go to the Hexcells Infinite properties on Steam, and set the Launch Options to `-mphost`

To join a multiplayer game, go to the Hexcells Infinite properties on Steam, and set the Launch Options to `-mpjoin IP` ex. `-mpjoin 192.168.0.1`

The mod runs on port 6666

## Playing
Currently the mod only supports the Infinite (Generated) levels mode. The host should go into the infinite screen, select a seed and difficulty, and upon starting, other players in the session will be pulled into the game with the same settings. From here you race to the finish with the pressure of your peers completing the same puzzle.

Your final placement is determined by time. Each mistake adds one minute to your time, so be careful!

If the host quits, or returns to the menu after completing a puzzle, all other players will be returned to the menu as well.

## Dependencies for building/running
- Requires IPA [https://github.com/Eusth/IPA]
- Requires Harmony (net35 build) [https://github.com/pardeike/Harmony]
- Requires the original game, and referencing the original game assemblies (UnityEngine, Assembly-csharp, Assembly-csharp-firstpass)
- Requires a 32 bit build of ENet, installed into the `Hexcells Infinite_Data\Plugins` folder

### Side notes
This is my first experience using IPA, Harmony, and ENet. Bearing that in mind, I may not have written this with complete perfection.
