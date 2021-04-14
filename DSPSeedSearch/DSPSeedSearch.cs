using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Unity.Jobs;
using Unity.Collections;

namespace DSPSeedSearch
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class DSPSeedSearch : BaseUnityPlugin
    {
        public const string pluginGuid = "org.picamula.plugins.dspseedsearch";
        public const string pluginName = "DSPSeedSearch";
        public const string pluginVersion = "1.2.1.0";

        public static BepInEx.Logging.ManualLogSource PublicLogger;
        public static ModSettings modSettings = null;

        Harmony harmony;
        internal void Awake()
        {
            Logger.LogInfo("DSPSeedSearch Awoken");
            PublicLogger = Logger;

            harmony = new Harmony(pluginGuid);
            try
            {
                harmony.PatchAll(typeof(DSPSeedSearch));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // This will run on the New Game screen, when you open and press Random too
        [HarmonyPrefix, HarmonyPatch(typeof(UIGalaxySelect), "UpdateParametersUIDisplay")]
        public static void UIGalaxySelect_UpdateParametersUIDisplay_Prefix(UIGalaxySelect __instance)
        {
            modSettings = ModSettings.LoadSettings();
            if (modSettings.RunOnGalaxySelect)
            {
                UIVirtualStarmap starmap = Traverse.Create(__instance).Field("starmap").GetValue() as UIVirtualStarmap;

                GalaxyData gd = starmap.galaxyData;

                //List<float> dsRadii = (from star in gd.stars select star.dysonRadius).ToList(); // Game doesn't like Linq
                List<float> dsRadii = new List<float>();
                for (int i = 0; i < gd.starCount; i++)
                {
                    dsRadii.Add(gd.stars[i].dysonRadius);
                }

                PublicLogger.LogMessage(string.Format("seed = {0} maxDSradius = {1} planetCount = {3} name = {2}", gd.seed, dsRadii.Max(), gd.stars[dsRadii.IndexOf(dsRadii.Max())].name, gd.stars[dsRadii.IndexOf(dsRadii.Max())].planetCount));
            }

        }

        // This will run on Credits screen
        [HarmonyPrefix, HarmonyPatch(typeof(UIRoot), "OpenCreditsScreen")]
        public static void UIRoot_OpenCreditsScreen_Prefix(UIGalaxySelect __instance)
        {
            modSettings = ModSettings.LoadSettings();

            PublicLogger.LogMessage(string.Format("Searching {0} seeds, keeping the best {1} ones",
                modSettings.SearchEveryPossibleSeed ? "all" : modSettings.TotalSeeds.ToString(), modSettings.KeepSeeds));

            int nThreads = modSettings.UseParallelism ? Environment.ProcessorCount : 1; // Can't find a way for Unity's library to run more than this. And .Net's Parallel.For and Thread classes won't work either.

            System.Random rnd = new System.Random((int)(DateTime.Now.Ticks / 10000));

            NativeArray<int> seeds = new NativeArray<int>(nThreads, Allocator.Persistent);
            NativeArray<SeedStarDataSorter> innerSorters = new NativeArray<SeedStarDataSorter>(nThreads, Allocator.Persistent);
            NativeArray<bool> aborted = new NativeArray<bool>(nThreads, Allocator.Persistent);
            for (int n = 0; n < nThreads; n++)
            {
                seeds[n] = rnd.Next();
            }

            SeedSearchParallelJob jobData = new SeedSearchParallelJob();
            jobData.innerSorters = innerSorters;
            jobData.seeds = seeds;
            jobData.nThreads = nThreads;
            jobData.aborted = aborted;

            JobHandle handle = jobData.Schedule(nThreads, 1);

            handle.Complete();
            bool anyAborted = false;
            foreach (bool v in aborted)
            {
                anyAborted |= v;
            }
            if (anyAborted)
            {
                PublicLogger.LogMessage("Search aborted. You may close the program.");
                return;
            }

            SeedStarDataSorter sorter = SeedStarDataSorter.SortMultiple(innerSorters.ToArray(), modSettings.KeepSeeds);


            if (File.Exists(modSettings.FilePathSearchTable))
            {
                File.Delete(modSettings.FilePathSearchTable);
            }

            using (StreamWriter w = new StreamWriter(File.OpenWrite(modSettings.FilePathSearchTable)))
            {
                w.WriteLine(sorter.PrintTable());
            }

            #region just for debugging multithreading
            /*for (int i = 0; i < innerSorters.Count(); i++)
            {
                string path = Path.Combine(GameConfig.gameSaveFolder, string.Format("DSPseedSearchTable{0}.csv", i));
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                PublicLogger.LogMessage(innerSorters[i].seeds.Count());
                PublicLogger.LogMessage(innerSorters[i].radius.Count());
                PublicLogger.LogMessage(innerSorters[i].stars.Count());
                for (int j = 0; j < innerSorters[i].seeds.Count(); j++)
                {
                    PublicLogger.LogMessage(string.Format("innerSorters[{0}].seeds[{1}] = {2}", i, j, innerSorters[i].seeds[j]));
                    PublicLogger.LogMessage(string.Format("innerSorters[{0}].radius[{1}] = {2}", i, j, innerSorters[i].radius[j]));
                    PublicLogger.LogMessage(string.Format("innerSorters[{0}].stars[{1}] = {2}", i, j, innerSorters[i].stars[j] == null ? "null" : "notnull"));
                }

                innerSorters[i].Sort();

                using (StreamWriter w = new StreamWriter(File.OpenWrite(path)))
                {
                    w.WriteLine(innerSorters[i].PrintTable());
                }
            }*/
            #endregion

            PublicLogger.LogMessage(string.Format("Search complete, results saved in {0}", modSettings.FilePathSearchTable));

            innerSorters.Dispose();
            seeds.Dispose();
        }


        static void AnalyseSeed(GameDesc gameDesc, out StarData maxDSradius, bool threadSafe = false)
        {
            GalaxyData gd;

            if (threadSafe)
            {
                if (ThreadSafeUniverseGen.algoVersion != UniverseGen.algoVersion)
                {
                    PublicLogger.LogMessage(string.Format("Warning: the game's UniverseGen algorithm version ({0}) doesn't match the mod's ThreadSafeUniverseGen version ({1}). The mod needs to be updated to be used with multithreading.)", 
                        UniverseGen.algoVersion, ThreadSafeUniverseGen.algoVersion));
                }
                ThreadSafeUniverseGen ug = new ThreadSafeUniverseGen();
                gd = ug.CreateGalaxy(gameDesc);
            }
            else
            {
                gd = UniverseGen.CreateGalaxy(gameDesc);
            }

            StarData[] stars = gd.stars;

            float maxDSradiusv = float.NegativeInfinity;
            int maxPosDSRadius = -1;
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i].dysonRadius > maxDSradiusv)
                {
                    maxDSradiusv = stars[i].dysonRadius;
                    maxPosDSRadius = i;
                }
            }

            maxDSradius = stars[maxPosDSRadius];

        }

        static void SaveSearchAllStatus(int nThreads, int n, int currentSeedIndex, SeedStarDataSorter innerSorter)
        {
            string filePath = string.Format(modSettings.FilePathSearchAllStatus, n);

            // there is a possibility that a file might be partially written because the game was closed
            // so I delete it instead of overwriting. If the file is partially written, then the specific thread will start from scratch
            // (not ideal but ensures true result)
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (BinaryWriter w = new BinaryWriter(File.OpenWrite(filePath)))
            {
                w.Write(currentSeedIndex);
                w.Write(nThreads);
                w.Write(innerSorter.keep);
                w.Write(innerSorter.seeds.Count());
                for (int i = 0; i < innerSorter.seeds.Count(); i++)
                {
                    w.Write(innerSorter.seeds[i]);
                }
            }
        }

        static SeedStarDataSorter LoadSearchAllStatus(int nThreads, int n, out int currentSeedIndex)
        {
            try
            {
                string filePath = string.Format(modSettings.FilePathSearchAllStatus, n);
                SeedStarDataSorter innerSorter = new SeedStarDataSorter(modSettings.KeepSeeds);
                using (BinaryReader r = new BinaryReader(File.OpenRead(filePath)))
                {
                    int v = r.ReadInt32();
                    currentSeedIndex = v;
                    v = r.ReadInt32();
                    if (v != nThreads)
                    {
                        PublicLogger.LogMessage(string.Format("LoadSearchAllStatus[{0}] number of threads don't match", n));
                        return innerSorter;
                    }
                    v = r.ReadInt32();
                    if (v != modSettings.KeepSeeds)
                    {
                        PublicLogger.LogMessage(string.Format("LoadSearchAllStatus[{0}] keepSeeds don't match", n));
                        return innerSorter;
                    }
                    v = r.ReadInt32();
                    for (int i = 0; i < v; i++)
                    {
                        int seed = r.ReadInt32();
                        GameDesc gameDesc = new GameDesc();
                        gameDesc.SetForNewGame(nThreads > 1 ? ThreadSafeUniverseGen.algoVersion : UniverseGen.algoVersion, seed, 64, 1, 1f);

                        StarData starData;
                        AnalyseSeed(gameDesc, out starData, nThreads > 1);
                        innerSorter.Add(seed, starData);
                    }
                }

                PublicLogger.LogMessage(string.Format("LoadSearchAllStatus[{0}] successfully loaded", n));
                return innerSorter;
            }
            catch (Exception)
            {
                PublicLogger.LogMessage(string.Format("LoadSearchAllStatus[{0}] nonexistent or invalid status file", n));
                currentSeedIndex = 0;
                return new SeedStarDataSorter(modSettings.KeepSeeds);
            }
        }

        static bool CheckStopRequested()
        {
            if (File.Exists(modSettings.FilePathRequestStop))
            {
                try
                {
                    using (StreamReader r = new StreamReader(File.Open(modSettings.FilePathRequestStop, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        return r.ReadLine().Equals("1");
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                using (StreamWriter w = new StreamWriter(File.Open(modSettings.FilePathRequestStop, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite)))
                {
                    w.WriteLine("0");
                }
                return false;
            }
        }

        public struct SeedSearchParallelJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<int> seeds;
            public int nThreads;
            public NativeArray<bool> aborted;
            public NativeArray<SeedStarDataSorter> innerSorters;

            public void Execute(int n)
            {
                aborted[n] = false;
                bool changed = false;
                System.Random random = new System.Random(seeds[n]);
                int currentSeedIndex = 0;
                SeedStarDataSorter innerSorter = modSettings.SearchEveryPossibleSeed ? LoadSearchAllStatus(nThreads, n, out currentSeedIndex) : new SeedStarDataSorter(modSettings.KeepSeeds);
                innerSorters[n] = innerSorter;
                int start = n * modSettings.TotalSeeds / nThreads;
                int end = (n + 1) * modSettings.TotalSeeds / nThreads;
                if (n == nThreads - 1)
                {
                    end = modSettings.TotalSeeds;
                }


                for (int i = currentSeedIndex; i < end - start; i++)
                {
                    GameDesc gameDesc = new GameDesc();
                    int seed = -1;
                    if (modSettings.SearchEveryPossibleSeed)
                    {
                        seed = i + start;
                    }
                    else
                    {
                        do
                        {
                            seed = random.Next(ModSettings.SeedMaxValue);
                        }
                        while (innerSorter.seeds.Contains(seed));
                    }

                    gameDesc.SetForNewGame(nThreads > 1 ? ThreadSafeUniverseGen.algoVersion : UniverseGen.algoVersion, seed, 64, 1, 1f);

                    StarData sradius;
                    AnalyseSeed(gameDesc, out sradius, threadSafe: nThreads > 1);

                    changed |= innerSorter.Add(seed, sradius);

                    if ((i + 1) % 1000 == 0)
                    {
                        PublicLogger.LogMessage(string.Format("Thread {3}/{4}: {0} out of {1} seeds searched {2:P}",
                            i + 1, end - start, (i + 1.0) / (end - start), n + 1, nThreads));
                        if (CheckStopRequested())
                        {
                            PublicLogger.LogMessage(string.Format("Thread {0}/{1}: this thread is ready to be terminated. You may close the program whenever all threads are at this state.",
                                n + 1, nThreads));
                            aborted[n] = true;
                            SaveSearchAllStatus(nThreads, n, i + 1, innerSorter);
                            return;
                        }
                        if (changed && modSettings.SearchEveryPossibleSeed)
                        {
                            changed = false;
                            SaveSearchAllStatus(nThreads, n, i + 1, innerSorter);
                        }
                    }
                }

                if ((end - start) % 1000 != 0)
                {
                    SaveSearchAllStatus(nThreads, n, end - start, innerSorter);
                    PublicLogger.LogMessage(string.Format("Thread {3}/{4}: {0} out of {1} seeds searched {2:P}",
                        end - start, end - start, 1.0, n + 1, nThreads));
                }

            }
        }

    }
}
