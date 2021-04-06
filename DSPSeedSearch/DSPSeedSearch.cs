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

namespace DSPSeedSearch
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class DSPSeedSearch : BaseUnityPlugin
    {
        public const string pluginGuid = "org.picamula.plugins.dspseedsearch";
        public const string pluginName = "DSPSeedSearch";
        public const string pluginVersion = "1.2.0.0";

        public static BepInEx.Logging.ManualLogSource PublicLogger;
        public static ModSettings modSettings = null;

        Harmony harmony;
        internal void Awake()
        {
            Logger.LogInfo("DSPSeedSearch Awoken");
            PublicLogger = Logger;

            harmony = new Harmony("org.picamula.plugins.dspseedsearch");
            try
            {
                harmony.PatchAll(typeof(DSPSeedSearch));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            modSettings = ModSettings.LoadSettings();
        }

        static StarData AnalyseSeedMaxDSradius(GameDesc gameDesc)
        {
            GalaxyData gd = UniverseGen.CreateGalaxy(gameDesc);

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

            return stars[maxPosDSRadius];
        }

        static void SearchSeed(GameDesc gameDesc, out StarData maxLumi, out StarData maxDSradius, out StarData maxDSlumi)
        {
            GalaxyData gd = UniverseGen.CreateGalaxy(gameDesc);

            StarData[] stars = gd.stars;

            float maxLumiv = float.NegativeInfinity;
            int maxPosLum = -1;
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i].luminosity > maxLumiv)
                {
                    maxLumiv = stars[i].luminosity;
                    maxPosLum = i;
                }
            }

            maxLumi = stars[maxPosLum];

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

            float maxDSlumiv = float.NegativeInfinity;
            int maxPosDSlumi = -1;
            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i].dysonRadius > maxDSlumiv)
                {
                    maxDSlumiv = stars[i].dysonRadius;
                    maxPosDSlumi = i;
                }
            }

            maxDSlumi = stars[maxPosDSlumi];

        }


        [HarmonyPrefix, HarmonyPatch(typeof(UIGalaxySelect), "SetStarmapGalaxy")]
        public static void UIGalaxySelect_SetStarmapGalaxy_Prefix(UIGalaxySelect __instance)
        {
            if (modSettings.RunOnGalaxySelect)
            {
                GameDesc gameDesc = Traverse.Create(__instance).Field("gameDesc").GetValue() as GameDesc;
                GalaxyData gd = UniverseGen.CreateGalaxy(gameDesc);

                List<float> dsRadii = (from star in gd.stars select star.dysonRadius).ToList();

                PublicLogger.LogMessage(string.Format("seed = {0} maxDSradius = {1} name = {2}", gameDesc.galaxySeed, dsRadii.Max(), gd.stars[dsRadii.IndexOf(dsRadii.Max())].name));
            }

        }



        [HarmonyPrefix, HarmonyPatch(typeof(UIRoot), "OpenCreditsScreen")]
        public static void UIRoot_OpenCreditsScreen_Prefix(UIGalaxySelect __instance)
        {

            PublicLogger.LogMessage(string.Format("Searching {0} seeds, keeping the best {1} ones",
                modSettings.SearchEveryPossibleSeed ? "all" : modSettings.TotalSeeds.ToString(), modSettings.KeepSeeds));

            System.Random random = new System.Random((int)(DateTime.Now.Ticks / 10000));

            SeedStarDataSorter sorter = new SeedStarDataSorter(modSettings.KeepSeeds);

            for (int i = 0; i < modSettings.TotalSeeds; i++)
            {
                GameDesc gameDesc = new GameDesc();
                int seed = -1;
                if (modSettings.TotalSeeds <= 20000000)
                {
                    do
                    {
                        seed = random.Next(100000000);
                    }
                    while (sorter.seeds.Contains(seed));
                }
                else
                {
                    seed = (int)(i / (double)modSettings.TotalSeeds * 100000000);
                }

                gameDesc.SetForNewGame(UniverseGen.algoVersion, seed, 64, 1, 1f);

                StarData slum, sradius, sdslum;
                SearchSeed(gameDesc, out slum, out sradius, out sdslum);

                sorter.SortThis(seed, sradius);

                if ((i + 1) % 1000 == 0)
                {
                    PublicLogger.LogMessage(string.Format("{0} out of {1} seeds searched {2:P}", i + 1, modSettings.TotalSeeds, (i + 1.0) / modSettings.TotalSeeds));
                }
            }

            if (modSettings.TotalSeeds % 1000 != 0) PublicLogger.LogMessage(string.Format("{0} out of {1} seeds searched {2:P00}", modSettings.TotalSeeds, modSettings.TotalSeeds, 1.0));
            PublicLogger.LogMessage(string.Format("saving files"));


            sorter.SortAll(); 

            string resultString = sorter.Print();
            /*
            StarData star = maxDSLuminosity;
            resultString = resultString + string.Format("Maximum Luminosity Found: seed = {0}; dysonLuminosity = {2}; dysonRadius = {3}; star name = {4}; type = {5}; mass = {6}; temperature = {7}\r\n", 
                maxDSLuminositySeed, star.luminosity, star.dysonLumino, star.dysonRadius, star.displayName, star.type, star.mass, star.temperature);

            star = maxDSradius;
            resultString = resultString + string.Format("Maximum Dyson Radius Found: seed = {0}; dysonLuminosity = {2}; dysonRadius = {3}; star name = {4}; type = {5}; mass = {6}; temperature = {7}\r\n\r\n",
                maxDSLuminositySeed, star.luminosity, star.dysonLumino, star.dysonRadius, star.displayName, star.type, star.mass, star.temperature);
            */
            PublicLogger.LogMessage(resultString);

            using (StreamWriter w = new StreamWriter(File.OpenWrite(modSettings.FilePathSR)))
            {
                w.BaseStream.Seek(0, SeekOrigin.End);
                w.WriteLine(resultString);
                //w.WriteLine(string.Format("seed = {0} maxLum = {1} name = {2}", seed, "?", "?"));
            }


            using (StreamWriter w = new StreamWriter(File.OpenWrite(modSettings.FilePathSR)))
            {
                //w.BaseStream.Seek(0, SeekOrigin.End);
                w.WriteLine(sorter.PrintTable());
                //w.WriteLine(string.Format("seed = {0} maxLum = {1} name = {2}", seed, "?", "?"));
            }
        }


    }
}
