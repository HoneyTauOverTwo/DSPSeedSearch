using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DSPSeedSearch
{
    public class ModSettings
    {


        string filePathSR;
        string filePathST;
        string filePathSP;
        public string FilePathSR { get => filePathSR; private set => filePathSR = value; }
        public string FilePathST { get => filePathST; private set => filePathST = value; }
        public string FilePathSP { get => filePathSP; private set => filePathSP = value; }


        int totalSeeds;
        int keepSeeds;
        bool runOnGalaxySelect;
        bool searchEveryPossibleSeed;
        int nThreads;

        public int TotalSeeds { get => totalSeeds; private set => totalSeeds = value; }
        public int KeepSeeds { get => keepSeeds; private set => keepSeeds = value; }
        public bool RunOnGalaxySelect { get => runOnGalaxySelect; private set => runOnGalaxySelect = value; }
        public bool SearchEveryPossibleSeed { get => searchEveryPossibleSeed; private set => searchEveryPossibleSeed = value; }
        public int NThreads { get => nThreads; private set => nThreads = value; }

        private ModSettings(string root = null)
        {
            if (root == null)
            {
                root = GameConfig.gameSaveFolder;
            }
            filePathSR = Path.Combine(root, "DSPseedSearchResults.txt");
            filePathST = Path.Combine(root, "DSPseedSearchTable.csv");
            filePathSP = Path.Combine(root, "DSPseedSearchParameters.txt");

            if (!ParseSearchParametersFile())
            {
                CreateSearchParametersFile(filePathSP);
                if (!ParseSearchParametersFile())
                {
                    DSPSeedSearch.PublicLogger.LogMessage("It appears that we cannot access the settings file, using default settings"); // thils should never happen anyway
                    totalSeeds = 100;
                    keepSeeds = 10;
                    runOnGalaxySelect = false;
                    searchEveryPossibleSeed = false;
                }
            }
        }

        public static ModSettings LoadSettings()
        {
            return new ModSettings();
        }

        bool ParseSearchParametersFile()
        {
            totalSeeds = -1;
            keepSeeds = -1;
            runOnGalaxySelect = false;
            searchEveryPossibleSeed = false;

            int runOnGalaxySelectInt = int.MaxValue;
            int searchEveryPossibleSeedInt = int.MaxValue;

            if (!File.Exists(filePathSP))
            {
                return false;
            }

            string s;
            string[] t;
            try
            {
                using (StreamReader r = new StreamReader(File.OpenRead(filePathSP)))
                {
                    for (int i = 0; i < 2; i++)
                    {
                        s = r.ReadLine();
                        t = s.Split('=');
                        t[0] = t[0].Trim();
                        t[1] = t[1].Trim();

                        if (t[0].ToUpper().CompareTo("totalSeeds".ToUpper()) == 0)
                        {
                            if (!int.TryParse(t[1], out totalSeeds))
                            {
                                return false;
                            }
                        }
                        else if (t[0].ToUpper().CompareTo("keepSeeds".ToUpper()) == 0)
                        {
                            if (!int.TryParse(t[1], out keepSeeds))
                            {
                                return false;
                            }
                        }
                        else if (t[0].ToUpper().CompareTo("runOnGalaxySelect".ToUpper()) == 0)
                        {
                            if (!int.TryParse(t[1], out runOnGalaxySelectInt))
                            {
                                return false;
                            }
                            runOnGalaxySelect = runOnGalaxySelectInt != 0;
                        }
                        else if (t[0].ToUpper().CompareTo("searchEveryPossibleSeed".ToUpper()) == 0)
                        {
                            if (!int.TryParse(t[1], out searchEveryPossibleSeedInt))
                            {
                                return false;
                            }
                            searchEveryPossibleSeed = searchEveryPossibleSeedInt != 0;
                        }
                        else
                        {
                            return false;
                        }
                    }

                }
            }
            catch (Exception e)
            {
                DSPSeedSearch.PublicLogger.LogMessage("ParseSearchParametersFile exception = " + e.StackTrace);
                return false;
            }
            if (totalSeeds > 100000000)
            {
                totalSeeds = 100000000;
                searchEveryPossibleSeed = true;
            }


            return totalSeeds > 0 && keepSeeds > 0 && runOnGalaxySelectInt != int.MaxValue && searchEveryPossibleSeedInt != int.MaxValue;
        }

        static void CreateSearchParametersFile(string filePath, int totalSeeds = 100, int keepSeeds = 10, bool runOnGalaxySelect = false, bool searchEveryPossibleSeed = false)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            using (StreamWriter w = new StreamWriter(File.OpenWrite(filePath)))
            {
                w.WriteLine(string.Format("totalSeeds = {0}", totalSeeds));
                w.WriteLine(string.Format("keepSeeds = {0}", keepSeeds));
                w.WriteLine(string.Format("runOnGalaxySelect = {0}", runOnGalaxySelect));
                w.WriteLine(string.Format("searchEveryPossibleSeed = {0}", searchEveryPossibleSeed));
            }
        }
    }
}
