# DSPSeedSearch

This mod uses BepInEx to inject into the game: https://github.com/BepInEx/BepInEx/releasesIt was originally created for [Francis John's playthrough](https://www.youtube.com/watch?v=1qjqsdjLJ9A)

## How to install

First you need to install BepInEx into the game. Download the [latest BepInEx_x64 release](https://github.com/BepInEx/BepInEx/releases) and extract its contents into the game's root folder.

After opening the game with BepInEx installed, it should create more files and folders under the BepInEx folder. If BepInEx's debug console is not shown, edit the file BepInEx\config\BepInEx.cfg changing the value under `[Logging.Console]` to `Enabled = true`. This will allow you to see the mod's output.

## How to use

The mod operates in two different ways:
1. showing you the biggest dyson sphere star for the current seed being viewed in the `New Game` window and
2. searching through a configurable amount of seeds when you click the `Credits` window.

The first one will only output some text in BepInEx's console. The latter will create files in the game's save location folder (%USERPROFILE%\Documents\Dyson Sphere Program\ on windows).

### Searching through seeds

When using the mod through the `Credits` screen, it should create a file called `DSPseedSearchParameters.txt`. You may edit this file to change the search parameters:
`totalSeeds`: the total number of seeds to search for.
`keepSeeds`: the number of seeds to keep in memory and write down in the `DSPseedSearchTable.csv` output file.
`runOnGalaxySelect`: controls if the mod will run in the `New Game` screen. `0` for disabled, `1` for enabled.
`searchEveryPossibleSeed`: basically overrides `totalSeeds` value to `100000000`. The total number of seeds.
`useParallelism`: controls if the mod will use parallel threads to search the seeds. `0` for single-threaded operation, `1` for multithreaded.

Note that I had to copy and edit a portion of the map generation code from the game to make it thread-safe, which means that, if the map generation algorithm gets updated, the edited files have to be updated as well. If the algorithm version diverges, the mod *should* show a warning message.
The single-threaded version of the search will use the game's generation code directly.

Another important thing to note is that this mode will search through the seeds using the default settings for map generation (**list settings later**).

## How to build it yourself

*TODO*
