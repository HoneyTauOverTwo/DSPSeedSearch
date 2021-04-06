using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSPSeedSearch
{

    class SeedStarDataSorter
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

        public void SortThis(int seed, StarData star)
        {
            if (seeds.Count() < keep)
            {
                seeds.Add(seed);
                radius.Add(star.dysonRadius);
                stars.Add(star);
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
                }
            }
        }

        public void SortAll()
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

        public string Print()
        {
            StringBuilder sb = new StringBuilder();
            //string s = "";
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
            sb.Append("seed,dysonRadius,dysonLuminosity,starName,type,mass,temperature\r\n");
            for (int i = 0; i < stars.Count; i++)
            {
                StarData star = stars[i];
                sb.Append(string.Format("{0},{3},{2},{4},{5},{6},{7}\r\n",
                    seeds[i], star.luminosity, star.dysonLumino, star.dysonRadius, star.displayName, star.type, star.mass, star.temperature));
            }
            sb.AppendLine();
            return sb.ToString();
        }
    }
}
