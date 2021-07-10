using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

namespace DSPSeedSearch
{
    public class ModSettings
    {
        public const int ParallelThreshold = 50000;
        public const int SeedMaxValue = 100000000;

        string filePathSearchResults;
        string filePathSearchTable;
        string filePathSearchParameters;
        string filePathSearchAllStatus;
        string filePathRequestStop;
        public string FilePathSearchResults { get => filePathSearchResults; private set => filePathSearchResults = value; }
        public string FilePathSearchTable { get => filePathSearchTable; private set => filePathSearchTable = value; }
        public string FilePathSearchParameters { get => filePathSearchParameters; private set => filePathSearchParameters = value; }
        public string FilePathSearchAllStatus { get => filePathSearchAllStatus; private set => filePathSearchAllStatus = value; }
        public string FilePathRequestStop { get => filePathRequestStop; private set => filePathRequestStop = value; }


        int totalSeeds;
        int keepSeeds;
        bool runOnGalaxySelect;
        bool searchEveryPossibleSeed;
        bool useParallelism;

        public int TotalSeeds { get => totalSeeds; private set => totalSeeds = value; }
        public int KeepSeeds { get => keepSeeds; private set => keepSeeds = value; }
        public bool RunOnGalaxySelect { get => runOnGalaxySelect; private set => runOnGalaxySelect = value; }
        public bool SearchEveryPossibleSeed { get => searchEveryPossibleSeed; private set => searchEveryPossibleSeed = value; }
        public bool UseParallelism { get => useParallelism; private set => useParallelism = value; }

        private ModSettings(string root = null)
        {
            if (root == null)
            {
                root = GameConfig.gameSaveFolder;
            }
            filePathSearchResults = Path.Combine(root, "DSPseedSearchResults.txt");
            filePathSearchTable = Path.Combine(root, "DSPseedSearchTable.csv");
            filePathSearchParameters = Path.Combine(root, "DSPseedSearchParameters.txt");
            filePathSearchAllStatus = Path.Combine(root, "DSPseedSearchAllStatus{0:00}");
            filePathRequestStop = Path.Combine(root, "requestStop.txt");
            
            if (!ParseSearchParametersFile())
            {
                CreateSearchParametersFile(filePathSearchParameters);
                if (!ParseSearchParametersFile())
                {
                    DSPSeedSearch.PublicLogger.LogMessage("It appears that we cannot access the settings file, using default settings"); // thils should never happen anyway
                    totalSeeds = 100;
                    keepSeeds = 10;
                    runOnGalaxySelect = false;
                    searchEveryPossibleSeed = false;
                    useParallelism = false;
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
            useParallelism = false;

            int runOnGalaxySelectInt = int.MaxValue;
            int searchEveryPossibleSeedInt = int.MaxValue;
            int useParallelismInt = int.MaxValue;

            if (!File.Exists(filePathSearchParameters))
            {
                return false;
            }

            string s;
            string[] t;
            try
            {
                using (StreamReader r = new StreamReader(File.OpenRead(filePathSearchParameters)))
                {
                    do
                    {
                        s = r.ReadLine();
                        t = s.Split('=');
                        if (t.Length < 2)
                        {
                            continue;
                        }
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
                        else if (t[0].ToUpper().CompareTo("useParallelism".ToUpper()) == 0)
                        {
                            if (!int.TryParse(t[1], out useParallelismInt))
                            {
                                return false;
                            }
                            useParallelism = useParallelismInt != 0;
                        }
                    }
                    while (!r.EndOfStream);
                }
            }
            catch (Exception e)
            {
                DSPSeedSearch.PublicLogger.LogMessage("ParseSearchParametersFile exception = " + e.StackTrace);
                return false;
            }
            if (totalSeeds > SeedMaxValue)
            {
                totalSeeds = SeedMaxValue;
                searchEveryPossibleSeed = true;
            }
            if (searchEveryPossibleSeed)
            {
                totalSeeds = SeedMaxValue;
            }

            return totalSeeds > 0 && keepSeeds > 0 && runOnGalaxySelectInt != int.MaxValue && searchEveryPossibleSeedInt != int.MaxValue && useParallelismInt != int.MaxValue;
        }

        static void CreateSearchParametersFile(string filePath, int totalSeeds = 100, int keepSeeds = 10, bool runOnGalaxySelect = false, bool searchEveryPossibleSeed = false, bool useParallelism = false)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            using (StreamWriter w = new StreamWriter(File.OpenWrite(filePath)))
            {
                w.WriteLine(string.Format("totalSeeds = {0}", totalSeeds));
                w.WriteLine(string.Format("keepSeeds = {0}", keepSeeds));
                w.WriteLine(string.Format("runOnGalaxySelect = {0}", runOnGalaxySelect ? 1 : 0));
                w.WriteLine(string.Format("searchEveryPossibleSeed = {0}", searchEveryPossibleSeed ? 1 : 0));
                w.WriteLine(string.Format("useParallelism = {0}", useParallelism ? 1 : 0));
            }
        }
    }
}
