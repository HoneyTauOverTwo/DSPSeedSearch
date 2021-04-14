# DSPSeedSearch

This mod uses BepInEx to inject into the Dyson Sphere Program's executable: https://github.com/BepInEx/BepInEx/releases. It was originally created for [Francis John's playthrough](https://www.youtube.com/watch?v=1qjqsdjLJ9A).

The results for the top 200 seeds for the biggest Dyson Sphere searched through every single possible seed [can be found here](https://github.com/HoneyTauOverTwo/DSPSeedSearch/releases/download/1.2.1/DSPseedSearchTableTop200.csv). This was done with `UniverseGen.algoVersion = 20200403` (DSP version Early access 0.6.17.6137, will still be valid for later versions until they change the UniverseGen).

This table also shows how many planets are around the particular star. Note that the 200th seed has a Dyson Sphere radius only 0.082% smaller than the 1st seed. So basically, if you're just looking for a big Dyson Sphere, all of these seeds are basically the same and you can pick your seed based on other aspects of the galaxy.

## How to install

First you need to install BepInEx into the game. Download the [latest BepInEx_x64 release](https://github.com/BepInEx/BepInEx/releases) and extract its contents into the game's root folder.

After opening the game with BepInEx installed, it should create more files and folders under the BepInEx folder. If BepInEx's debug console is not shown, edit the file BepInEx\config\BepInEx.cfg changing the value under `[Logging.Console]` to `Enabled = true`. This will allow you to see the mod's output.

Download the [latest release of DSPSeedSearch](https://github.com/HoneyTauOverTwo/DSPSeedSearch/releases/) and extract the dll under BepInEx\plugins folder.

## How to use

The mod operates in two different ways:
1. showing you the biggest dyson sphere star for the current seed being viewed in the `New Game` window;
2. searching through a configurable amount of seeds when you click the `Credits` window.

The first one will only output some text in BepInEx's console. The latter will create files in the game's save location folder (%USERPROFILE%\Documents\Dyson Sphere Program\ on windows).

### Search parameters

When using the mod through the `Credits` screen, it should create a file called `DSPseedSearchParameters.txt`. You may edit this file to change the search parameters:

`totalSeeds`: the total number of seeds to search for.

`keepSeeds`: the number of seeds to keep in memory and write down in the `DSPseedSearchTable.csv` output file.

`runOnGalaxySelect`: controls if the mod will run in the `New Game` screen. `0` for disabled, `1` for enabled.

`searchEveryPossibleSeed`: basically overrides `totalSeeds` value to `100000000` (the maximum seed value + 1) if it's value is `1`.

`useParallelism`: controls if the mod will use parallel threads to search the seeds. `0` for single-threaded operation, `1` for multithreaded.

Note that I had to copy and edit a portion of the map generation code from the game to make it thread-safe, which means that, if the map generation algorithm gets updated, the edited files have to be updated as well. If the algorithm version diverges, the mod *should* show a warning message.
The single-threaded version of the search will use the game's generation code directly.

Another important thing to note is that this mode will search through the seeds using the default settings for map generation (number of stars and resource multiplier).

### Searching through every single seed

It should take about 4.5 days of processing on a i7 using the multithreading mode to search through every seed (I'm currently running this, will put the results here when they're ready). Also, if you choose to keep 200 seeds, the game will use about 5 GB of RAM. RAM usage will increase proportional to the number of seeds to keep multiplied by the number of logical processors you have.

For each thread, for every 1000 seeds searched, the mod will write to files named `DSPseedSearchAllStatus{00}`, where `{00}` is an integer indexing each thread. These files are used to resume the search that was started on a previous session.

If the execution is aborted during the search, there is a possibility that one of these files gets corrupted. For this reason, the mod creates a file named `requestStop.txt`, it should only contain the character `0` on it, if you ever want to stop the calculation, write a `1` on its place and save the file. This will make the threads stop working whenever they reach the end of the next 1000 seeds. Wait until the mod outputs "Search aborted. You may close the program." to close it. Whenever you're ready to resume the search, just make sure to revert back the `requestStop.txt` value to `0`.

There is one issue on this mode, I'm getting some null exceptions at the end of a search (when it's trying to sort each thread's results together). That only happens if that search goes on for too long, which makes me think it's related to Unity's memory allocation for parallel jobs. The workaround is just launching the search again, it will read the last search's status and complete the job properly.

## How to build the mod by yourself

**TODO**
