using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSPSeedSearch
{

    public struct SeedStarDataSorter
    {
        public int keep;
        public List<int> seeds;
        public List<float> radius;
        public List<StarData> stars;
        public SeedStarDataSorter(int keep)
        {
            this.keep = keep;
            seeds = new List<int>();
            radius = new List<float>();
            stars = new List<StarData>();
        }

        public bool Add(int seed, StarData star)
        {
            if (seeds.Count() < keep)
            {
                seeds.Add(seed);
                radius.Add(star.dysonRadius);
                stars.Add(star);
                return true;
            }
            else
            {
                float minRadius = radius.Min();
                if (star.dysonRadius > minRadius)
                {
                    int minrindex = radius.IndexOf(minRadius);
                    seeds[minrindex] = seed;
                    radius[minrindex] = star.dysonRadius;
                    stars[minrindex] = star;
                    return true;
                }
            }
            return false;
        }

        public void Sort()
        {
            for (int i = 0; i < radius.Count() - 1; i++)
            {
                for (int j = 0; j < radius.Count - i - 1; j++)
                {
                    if (radius[j] < radius[j + 1])
                    {
                        float tradius = radius[j];
                        radius[j] = radius[j + 1];
                        radius[j + 1] = tradius;

                        int tseed = seeds[j];
                        seeds[j] = seeds[j + 1];
                        seeds[j + 1] = tseed;

                        StarData tstar = stars[j];
                        stars[j] = stars[j + 1];
                        stars[j + 1] = tstar;
                    }
                }
            }
        }

        public static SeedStarDataSorter SortMultiple(SeedStarDataSorter[] sorters, int keep)
        {
            SeedStarDataSorter result = new SeedStarDataSorter(keep);
            try
            {
                if (sorters == null)
                {
                    DSPSeedSearch.PublicLogger.LogMessage("SeedStarDataSorter[] sorters is null");
                }


                int n = 0;
                foreach (SeedStarDataSorter s in sorters)
                {
                    n++;
                    if (s.seeds == null)
                    {
                        DSPSeedSearch.PublicLogger.LogMessage(string.Format("SeedStarDataSorter[] sorters; sorters[{0}].seeds is null", n));
                    }
                    if (s.stars == null)
                    {
                        DSPSeedSearch.PublicLogger.LogMessage(string.Format("SeedStarDataSorter[] sorters; sorters[{0}].stars is null", n));
                    }
                    for (int i = 0; i < s.seeds.Count(); i++)
                    {
                        if (s.stars[i] == null)
                        {
                            DSPSeedSearch.PublicLogger.LogMessage(string.Format("SeedStarDataSorter[] sorters; sorters[{0}].stars[{1}] is null", n, i));
                        }
                        if (result.seeds.Contains(s.seeds[i]))
                        {
                            continue;
                        }
                        result.Add(s.seeds[i], s.stars[i]);
                    }
                }

                result.Sort();
            }
            catch (Exception e)
            {
                DSPSeedSearch.PublicLogger.LogMessage(e.StackTrace);
                throw e;
            }
            return result;

        }

        public string Print()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < stars.Count; i++)
            {
                StarData star = stars[i];
                sb.Append(string.Format("Maximum Luminosities Found: seed = {0}; dysonRadius = {3}; dysonLuminosity = {2}; star name = {4}; type = {5}; mass = {6}; temperature = {7}\r\n",
                    seeds[i], star.luminosity, star.dysonLumino, star.dysonRadius, star.displayName, star.type, star.mass, star.temperature));
            }
            sb.AppendLine();
            return sb.ToString();
        }

        public string PrintTable()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("seed,dysonRadius,planetCount,starName,type,mass,temperature,dysonLuminosity\r\n");
            for (int i = 0; i < stars.Count; i++)
            {
                StarData star = stars[i];
                sb.Append(string.Format("{0},{3},{8},{4},{5},{6},{7},{2}\r\n",
                    seeds[i], star.luminosity, star.dysonLumino, star.dysonRadius, star.displayName, star.type, star.mass, star.temperature, star.planetCount));
            }
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
