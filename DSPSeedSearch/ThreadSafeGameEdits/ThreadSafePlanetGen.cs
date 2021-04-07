using System;
using System.Collections.Generic;
using UnityEngine;

public class ThreadSafePlanetGen
{
    public const double GRAVITY = 1.3538551990520382E-06;

    public const double PI = Math.PI;

    private List<int> tmp_theme;

    public PlanetData CreatePlanet(GalaxyData galaxy, StarData star, GameDesc gameDesc, int index, int orbitAround, int orbitIndex, int number, bool gasGiant, int info_seed, int gen_seed)
    {
        PlanetData planetData = new PlanetData();
        System.Random random = new System.Random(info_seed);
        planetData.index = index;
        planetData.galaxy = star.galaxy;
        planetData.star = star;
        planetData.seed = gen_seed;
        planetData.orbitAround = orbitAround;
        planetData.orbitIndex = orbitIndex;
        planetData.number = number;
        planetData.id = star.id * 100 + index + 1;
        StarData[] stars = galaxy.stars;
        int num = 0;
        for (int i = 0; i < star.index; i++)
        {
            num += stars[i].planetCount;
        }

        num += index;
        if (orbitAround > 0)
        {
            for (int j = 0; j < star.planetCount; j++)
            {
                if (orbitAround == star.planets[j].number && star.planets[j].orbitAround == 0)
                {
                    planetData.orbitAroundPlanet = star.planets[j];
                    if (orbitIndex > 1)
                    {
                        planetData.orbitAroundPlanet.singularity |= EPlanetSingularity.MultipleSatellites;
                    }

                    break;
                }
            }

            Assert.NotNull(planetData.orbitAroundPlanet);
        }

        //string text = null;
        planetData.name = string.Concat(str2: (star.planetCount > 20) ? (index + 1).ToString() : NameGen.roman[index + 1], str0: star.name, str1: " ", str3: "号星".Translate());
        double num2 = random.NextDouble();
        double num3 = random.NextDouble();
        double num4 = random.NextDouble();
        double num5 = random.NextDouble();
        double num6 = random.NextDouble();
        double num7 = random.NextDouble();
        double num8 = random.NextDouble();
        double num9 = random.NextDouble();
        double num10 = random.NextDouble();
        double num11 = random.NextDouble();
        double num12 = random.NextDouble();
        double num13 = random.NextDouble();
        double rand = random.NextDouble();
        double num14 = random.NextDouble();
        double rand2 = random.NextDouble();
        double rand3 = random.NextDouble();
        double rand4 = random.NextDouble();
        int theme_seed = random.Next();
        float num15 = Mathf.Pow(1.2f, (float)(num2 * (num3 - 0.5) * 0.5));
        float num16 = 0f;
        if (orbitAround == 0)
        {
            num16 = StarGen.orbitRadius[orbitIndex] * star.orbitScaler;
            float num17 = (num15 - 1f) / Mathf.Max(1f, num16) + 1f;
            num16 *= num17;
        }
        else
        {
            num16 = (float)((double)((1600f * (float)orbitIndex + 200f) * Mathf.Pow(star.orbitScaler, 0.3f) * Mathf.Lerp(num15, 1f, 0.5f) + planetData.orbitAroundPlanet.realRadius) / 40000.0);
        }

        planetData.orbitRadius = num16;
        planetData.orbitInclination = (float)(num4 * 16.0 - 8.0);
        if (orbitAround > 0)
        {
            planetData.orbitInclination *= 2.2f;
        }

        planetData.orbitLongitude = (float)(num5 * 360.0);
        if (star.type >= EStarType.NeutronStar)
        {
            if (planetData.orbitInclination > 0f)
            {
                planetData.orbitInclination += 3f;
            }
            else
            {
                planetData.orbitInclination -= 3f;
            }
        }

        if (planetData.orbitAroundPlanet == null)
        {
            planetData.orbitalPeriod = Math.Sqrt(39.478417604357432 * (double)num16 * (double)num16 * (double)num16 / (1.3538551990520382E-06 * (double)star.mass));
        }
        else
        {
            planetData.orbitalPeriod = Math.Sqrt(39.478417604357432 * (double)num16 * (double)num16 * (double)num16 / 1.0830842106853677E-08);
        }

        planetData.orbitPhase = (float)(num6 * 360.0);
        if (num14 < 0.039999999105930328)
        {
            planetData.obliquity = (float)(num7 * (num8 - 0.5) * 39.9);
            if (planetData.obliquity < 0f)
            {
                planetData.obliquity -= 70f;
            }
            else
            {
                planetData.obliquity += 70f;
            }

            planetData.singularity |= EPlanetSingularity.LaySide;
        }
        else if (num14 < 0.10000000149011612)
        {
            planetData.obliquity = (float)(num7 * (num8 - 0.5) * 80.0);
            if (planetData.obliquity < 0f)
            {
                planetData.obliquity -= 30f;
            }
            else
            {
                planetData.obliquity += 30f;
            }
        }
        else
        {
            planetData.obliquity = (float)(num7 * (num8 - 0.5) * 60.0);
        }

        planetData.rotationPeriod = (num9 * num10 * 1000.0 + 400.0) * (double)((orbitAround != 0) ? 1f : Mathf.Pow(num16, 0.25f)) * (double)((!gasGiant) ? 1f : 0.2f);
        if (!gasGiant)
        {
            if (star.type == EStarType.WhiteDwarf)
            {
                planetData.rotationPeriod *= 0.5;
            }
            else if (star.type == EStarType.NeutronStar)
            {
                planetData.rotationPeriod *= 0.20000000298023224;
            }
            else if (star.type == EStarType.BlackHole)
            {
                planetData.rotationPeriod *= 0.15000000596046448;
            }
        }

        planetData.rotationPhase = (float)(num11 * 360.0);
        planetData.sunDistance = ((orbitAround != 0) ? planetData.orbitAroundPlanet.orbitRadius : planetData.orbitRadius);
        planetData.scale = 1f;
        double num18 = (orbitAround != 0) ? planetData.orbitAroundPlanet.orbitalPeriod : planetData.orbitalPeriod;
        planetData.rotationPeriod = 1.0 / (1.0 / num18 + 1.0 / planetData.rotationPeriod);
        if (orbitAround == 0 && orbitIndex <= 4 && !gasGiant)
        {
            if (num14 > 0.95999997854232788)
            {
                planetData.obliquity *= 0.01f;
                planetData.rotationPeriod = planetData.orbitalPeriod;
                planetData.singularity |= EPlanetSingularity.TidalLocked;
            }
            else if (num14 > 0.93000000715255737)
            {
                planetData.obliquity *= 0.1f;
                planetData.rotationPeriod = planetData.orbitalPeriod * 0.5;
                planetData.singularity |= EPlanetSingularity.TidalLocked2;
            }
            else if (num14 > 0.89999997615814209)
            {
                planetData.obliquity *= 0.2f;
                planetData.rotationPeriod = planetData.orbitalPeriod * 0.25;
                planetData.singularity |= EPlanetSingularity.TidalLocked4;
            }
        }

        if (num14 > 0.85 && num14 <= 0.9)
        {
            planetData.rotationPeriod = 0.0 - planetData.rotationPeriod;
            planetData.singularity |= EPlanetSingularity.ClockwiseRotate;
        }

        planetData.runtimeOrbitRotation = Quaternion.AngleAxis(planetData.orbitLongitude, Vector3.up) * Quaternion.AngleAxis(planetData.orbitInclination, Vector3.forward);
        if (planetData.orbitAroundPlanet != null)
        {
            planetData.runtimeOrbitRotation = planetData.orbitAroundPlanet.runtimeOrbitRotation * planetData.runtimeOrbitRotation;
        }

        planetData.runtimeSystemRotation = planetData.runtimeOrbitRotation * Quaternion.AngleAxis(planetData.obliquity, Vector3.forward);
        float habitableRadius = star.habitableRadius;
        if (gasGiant)
        {
            planetData.type = EPlanetType.Gas;
            planetData.radius = 80f;
            planetData.scale = 10f;
            planetData.habitableBias = 100f;
        }
        else
        {
            float num19 = Mathf.Ceil((float)star.galaxy.starCount * 0.29f);
            if (num19 < 11f)
            {
                num19 = 11f;
            }

            float num20 = num19 - (float)star.galaxy.habitableCount;
            float num21 = star.galaxy.starCount - star.index;
            float sunDistance = planetData.sunDistance;
            float num22 = 1000f;
            float num23 = 1000f;
            if (habitableRadius > 0f && sunDistance > 0f)
            {
                num23 = sunDistance / habitableRadius;
                num22 = Mathf.Abs(Mathf.Log(num23));
            }

            float num24 = Mathf.Clamp(Mathf.Sqrt(habitableRadius), 1f, 2f) - 0.04f;
            float a = num20 / num21;
            a = Mathf.Lerp(a, 0.35f, 0.5f);
            a = Mathf.Clamp(a, 0.08f, 0.8f);
            planetData.habitableBias = num22 * num24;
            planetData.temperatureBias = 1.2f / (num23 + 0.2f) - 1f;
            float f = Mathf.Clamp01(planetData.habitableBias / a);
            float p = a * 10f;
            f = Mathf.Pow(f, p);
            if ((num12 > (double)f && star.index > 0) || (planetData.orbitAround > 0 && planetData.orbitIndex == 1 && star.index == 0))
            {
                planetData.type = EPlanetType.Ocean;
                star.galaxy.habitableCount++;
            }
            else if (num23 < 0.833333f)
            {
                float num25 = Mathf.Max(0.15f, num23 * 2.5f - 0.85f);
                if (num13 < (double)num25)
                {
                    planetData.type = EPlanetType.Desert;
                }
                else
                {
                    planetData.type = EPlanetType.Vocano;
                }
            }
            else if (num23 < 1.2f)
            {
                planetData.type = EPlanetType.Desert;
            }
            else
            {
                float num26 = 0.9f / num23 - 0.1f;
                if (num13 < (double)num26)
                {
                    planetData.type = EPlanetType.Desert;
                }
                else
                {
                    planetData.type = EPlanetType.Ice;
                }
            }

            planetData.radius = 200f;
        }

        if (planetData.type != EPlanetType.Gas && planetData.type != 0)
        {
            planetData.precision = 200;
            planetData.segment = 5;
        }
        else
        {
            planetData.precision = 64;
            planetData.segment = 2;
        }

        planetData.luminosity = Mathf.Pow(planetData.star.lightBalanceRadius / (planetData.sunDistance + 0.01f), 0.6f);
        if (planetData.luminosity > 1f)
        {
            planetData.luminosity = Mathf.Log(planetData.luminosity) + 1f;
            planetData.luminosity = Mathf.Log(planetData.luminosity) + 1f;
            planetData.luminosity = Mathf.Log(planetData.luminosity) + 1f;
        }

        planetData.luminosity = Mathf.Round(planetData.luminosity * 100f) / 100f;
        SetPlanetTheme(planetData, star, gameDesc, 0, 0, rand, rand2, rand3, rand4, theme_seed);
        star.galaxy.astroPoses[planetData.id].uRadius = planetData.realRadius;
        return planetData;
    }

    public void SetPlanetTheme(PlanetData planet, StarData star, GameDesc game_desc, int set_theme, int set_algo, double rand1, double rand2, double rand3, double rand4, int theme_seed)
    {
        if (set_theme > 0)
        {
            planet.theme = set_theme;
        }
        else
        {
            if (tmp_theme == null)
            {
                tmp_theme = new List<int>();
            }
            else
            {
                tmp_theme.Clear();
            }

            int[] themeIds = game_desc.themeIds;
            int num = themeIds.Length;
            for (int i = 0; i < num; i++)
            {
                ThemeProto themeProto = LDB.themes.Select(themeIds[i]);
                bool flag = false;
                if (planet.star.index == 0 && planet.type == EPlanetType.Ocean)
                {
                    if (themeProto.Distribute == EThemeDistribute.Birth)
                    {
                        flag = true;
                    }
                }
                else if (themeProto.PlanetType == planet.type && themeProto.Temperature * planet.temperatureBias >= -0.1f)
                {
                    if (planet.star.index == 0)
                    {
                        if (themeProto.Distribute == EThemeDistribute.Default)
                        {
                            flag = true;
                        }
                    }
                    else if (themeProto.Distribute != EThemeDistribute.Birth)
                    {
                        flag = true;
                    }
                }

                if (flag)
                {
                    for (int j = 0; j < planet.index; j++)
                    {
                        if (planet.star.planets[j].theme == themeProto.ID)
                        {
                            flag = false;
                            break;
                        }
                    }
                }

                if (flag)
                {
                    tmp_theme.Add(themeProto.ID);
                }
            }

            if (tmp_theme.Count == 0)
            {
                for (int k = 0; k < num; k++)
                {
                    ThemeProto themeProto2 = LDB.themes.Select(themeIds[k]);
                    bool flag2 = false;
                    if (themeProto2.PlanetType == EPlanetType.Desert)
                    {
                        flag2 = true;
                    }

                    if (flag2)
                    {
                        for (int l = 0; l < planet.index; l++)
                        {
                            if (planet.star.planets[l].theme == themeProto2.ID)
                            {
                                flag2 = false;
                                break;
                            }
                        }
                    }

                    if (flag2)
                    {
                        tmp_theme.Add(themeProto2.ID);
                    }
                }
            }

            if (tmp_theme.Count == 0)
            {
                for (int m = 0; m < num; m++)
                {
                    ThemeProto themeProto3 = LDB.themes.Select(themeIds[m]);
                    if (themeProto3.PlanetType == EPlanetType.Desert)
                    {
                        tmp_theme.Add(themeProto3.ID);
                    }
                }
            }

            planet.theme = tmp_theme[(int)(rand1 * (double)tmp_theme.Count) % tmp_theme.Count];
        }

        ThemeProto themeProto4 = LDB.themes.Select(planet.theme);
        if (set_algo > 0)
        {
            planet.algoId = set_algo;
        }
        else
        {
            planet.algoId = 0;
            if (themeProto4 != null && themeProto4.Algos != null && themeProto4.Algos.Length > 0)
            {
                planet.algoId = themeProto4.Algos[(int)(rand2 * (double)themeProto4.Algos.Length) % themeProto4.Algos.Length];
                planet.mod_x = (double)themeProto4.ModX.x + rand3 * (double)(themeProto4.ModX.y - themeProto4.ModX.x);
                planet.mod_y = (double)themeProto4.ModY.x + rand4 * (double)(themeProto4.ModY.y - themeProto4.ModY.x);
            }
        }

        if (themeProto4 == null)
        {
            return;
        }

        planet.type = themeProto4.PlanetType;
        planet.ionHeight = themeProto4.IonHeight;
        planet.windStrength = themeProto4.Wind;
        planet.waterHeight = themeProto4.WaterHeight;
        planet.waterItemId = themeProto4.WaterItemId;
        planet.levelized = themeProto4.UseHeightForBuild;
        if (planet.type == EPlanetType.Gas)
        {
            int num2 = themeProto4.GasItems.Length;
            int num3 = themeProto4.GasSpeeds.Length;
            int[] array = new int[num2];
            float[] array2 = new float[num3];
            float[] array3 = new float[num2];
            for (int n = 0; n < num2; n++)
            {
                array[n] = themeProto4.GasItems[n];
            }

            double num4 = 0.0;
            System.Random random = new System.Random(theme_seed);
            for (int num5 = 0; num5 < num3; num5++)
            {
                float num6 = themeProto4.GasSpeeds[num5];
                num6 *= (float)random.NextDouble() * 0.190909147f + 0.9090909f;
                array2[num5] = num6 * Mathf.Pow(star.resourceCoef, 0.3f);
                ItemProto itemProto = LDB.items.Select(array[num5]);
                array3[num5] = itemProto.HeatValue;
                num4 += (double)(array3[num5] * array2[num5]);
            }

            planet.gasItems = array;
            planet.gasSpeeds = array2;
            planet.gasHeatValues = array3;
            planet.gasTotalHeat = num4;
        }
    }
}
