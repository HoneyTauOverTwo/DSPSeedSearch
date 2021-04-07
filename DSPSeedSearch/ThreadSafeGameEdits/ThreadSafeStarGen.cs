
using System;
using UnityEngine;

public class ThreadSafeStarGen
{
    public static float[] orbitRadius = new float[17]
    {
        0f,
        0.4f,
        0.7f,
        1f,
        1.4f,
        1.9f,
        2.5f,
        3.3f,
        4.3f,
        5.5f,
        6.9f,
        8.4f,
        10f,
        11.7f,
        13.5f,
        15.4f,
        17.5f
    };

    public static float specifyBirthStarMass = 0f;

    public static float specifyBirthStarAge = 0f;

    private double[] pGas = new double[10];

    private const double PI = Math.PI;

    ThreadSafePlanetGen planetGen = new ThreadSafePlanetGen();

    public StarData CreateStar(GalaxyData galaxy, VectorLF3 pos, int id, int seed, EStarType needtype, ESpectrType needSpectr = ESpectrType.X)
    {
        StarData starData = new StarData();
        starData.galaxy = galaxy;
        starData.index = id - 1;
        if (galaxy.starCount > 1)
        {
            starData.level = (float)starData.index / (float)(galaxy.starCount - 1);
        }
        else
        {
            starData.level = 0f;
        }

        starData.id = id;
        starData.seed = seed;
        System.Random random = new System.Random(seed);
        int seed2 = random.Next();
        int seed3 = random.Next();
        starData.position = pos;
        float num = (float)pos.magnitude;
        float num2 = num / 32f;
        if (num2 > 1f)
        {
            num2 = Mathf.Log(num2) + 1f;
            num2 = Mathf.Log(num2) + 1f;
            num2 = Mathf.Log(num2) + 1f;
            num2 = Mathf.Log(num2) + 1f;
            num2 = Mathf.Log(num2) + 1f;
        }

        starData.resourceCoef = Mathf.Pow(7f, num2) * 0.6f;
        System.Random random2 = new System.Random(seed3);
        double num3 = random2.NextDouble();
        double num4 = random2.NextDouble();
        double num5 = random2.NextDouble();
        double rn = random2.NextDouble();
        double rt = random2.NextDouble();
        double num6 = (random2.NextDouble() - 0.5) * 0.2;
        double num7 = random2.NextDouble() * 0.2 + 0.9;
        double num8 = random2.NextDouble() * 0.4 - 0.2;
        double num9 = Math.Pow(2.0, num8);
        float num10 = Mathf.Lerp(-0.98f, 0.88f, starData.level);
        num10 = ((!(num10 < 0f)) ? (num10 + 0.65f) : (num10 - 0.65f));
        float standardDeviation = 0.33f;
        if (needtype == EStarType.GiantStar)
        {
            num10 = ((!(num8 > -0.08)) ? 1.6f : (-1.5f));
            standardDeviation = 0.3f;
        }

        float num11 = RandNormal(num10, standardDeviation, num3, num4);
        switch (needSpectr)
        {
            case ESpectrType.M:
                num11 = -3f;
                break;
            case ESpectrType.O:
                num11 = 3f;
                break;
        }

        num11 = ((!(num11 > 0f)) ? (num11 * 1f) : (num11 * 2f));
        num11 = Mathf.Clamp(num11, -2.4f, 4.65f) + (float)num6 + 1f;
        switch (needtype)
        {
            case EStarType.BlackHole:
                starData.mass = 18f + (float)(num3 * num4) * 30f;
                break;
            case EStarType.NeutronStar:
                starData.mass = 7f + (float)num3 * 11f;
                break;
            case EStarType.WhiteDwarf:
                starData.mass = 1f + (float)num4 * 5f;
                break;
            default:
                starData.mass = Mathf.Pow(2f, num11);
                break;
        }

        double d = 5.0;
        if (starData.mass < 2f)
        {
            d = 2.0 + 0.4 * (1.0 - (double)starData.mass);
        }

        starData.lifetime = (float)(10000.0 * Math.Pow(0.1, Math.Log10((double)starData.mass * 0.5) / Math.Log10(d) + 1.0) * num7);
        switch (needtype)
        {
            case EStarType.GiantStar:
                starData.lifetime = (float)(10000.0 * Math.Pow(0.1, Math.Log10((double)starData.mass * 0.58) / Math.Log10(d) + 1.0) * num7);
                starData.age = (float)num5 * 0.04f + 0.96f;
                break;
            case EStarType.WhiteDwarf:
            case EStarType.NeutronStar:
            case EStarType.BlackHole:
                starData.age = (float)num5 * 0.4f + 1f;
                switch (needtype)
                {
                    case EStarType.WhiteDwarf:
                        starData.lifetime += 10000f;
                        break;
                    case EStarType.NeutronStar:
                        starData.lifetime += 1000f;
                        break;
                }

                break;
            default:
                if ((double)starData.mass < 0.5)
                {
                    starData.age = (float)num5 * 0.12f + 0.02f;
                }
                else if ((double)starData.mass < 0.8)
                {
                    starData.age = (float)num5 * 0.4f + 0.1f;
                }
                else
                {
                    starData.age = (float)num5 * 0.7f + 0.2f;
                }

                break;
        }

        float num12 = starData.lifetime * starData.age;
        if (num12 > 5000f)
        {
            num12 = (Mathf.Log(num12 / 5000f) + 1f) * 5000f;
        }

        if (num12 > 8000f)
        {
            float f = num12 / 8000f;
            f = Mathf.Log(f) + 1f;
            f = Mathf.Log(f) + 1f;
            f = Mathf.Log(f) + 1f;
            num12 = f * 8000f;
        }

        starData.lifetime = num12 / starData.age;
        float num13 = (1f - Mathf.Pow(Mathf.Clamp01(starData.age), 20f) * 0.5f) * starData.mass;
        starData.temperature = (float)(Math.Pow(num13, 0.56 + 0.14 / (Math.Log10(num13 + 4f) / Math.Log10(5.0))) * 4450.0 + 1300.0);
        double num14 = Math.Log10(((double)starData.temperature - 1300.0) / 4500.0) / Math.Log10(2.6) - 0.5;
        if (num14 < 0.0)
        {
            num14 *= 4.0;
        }

        if (num14 > 2.0)
        {
            num14 = 2.0;
        }
        else if (num14 < -4.0)
        {
            num14 = -4.0;
        }

        starData.spectr = (ESpectrType)Mathf.RoundToInt((float)num14 + 4f);
        starData.color = Mathf.Clamp01(((float)num14 + 3.5f) * 0.2f);
        starData.classFactor = (float)num14;
        starData.luminosity = Mathf.Pow(num13, 0.7f);
        starData.radius = (float)(Math.Pow(starData.mass, 0.4) * num9);
        starData.acdiskRadius = 0f;
        float p = (float)num14 + 2f;
        starData.habitableRadius = Mathf.Pow(1.7f, p) + 0.25f * Mathf.Min(1f, starData.orbitScaler);
        starData.lightBalanceRadius = Mathf.Pow(1.7f, p);
        starData.orbitScaler = Mathf.Pow(1.35f, p);
        if (starData.orbitScaler < 1f)
        {
            starData.orbitScaler = Mathf.Lerp(starData.orbitScaler, 1f, 0.6f);
        }

        SetStarAge(starData, starData.age, rn, rt);
        starData.dysonRadius = starData.orbitScaler * 0.28f;
        if ((double)starData.dysonRadius * 40000.0 < (double)(starData.physicsRadius * 1.5f))
        {
            starData.dysonRadius = (float)((double)(starData.physicsRadius * 1.5f) / 40000.0);
        }

        starData.uPosition = starData.position * 2400000.0;
        starData.name = NameGen.RandomStarName(seed2, starData, galaxy);
        starData.overrideName = string.Empty;
        return starData;
    }

    public StarData CreateBirthStar(GalaxyData galaxy, int seed)
    {
        StarData starData = new StarData();
        starData.galaxy = galaxy;
        starData.index = 0;
        starData.level = 0f;
        starData.id = 1;
        starData.seed = seed;
        starData.resourceCoef = 0.6f;
        System.Random random = new System.Random(seed);
        int seed2 = random.Next();
        int seed3 = random.Next();
        starData.name = NameGen.RandomName(seed2);
        starData.overrideName = string.Empty;
        starData.position = VectorLF3.zero;
        System.Random random2 = new System.Random(seed3);
        double r = random2.NextDouble();
        double r2 = random2.NextDouble();
        double num = random2.NextDouble();
        double rn = random2.NextDouble();
        double rt = random2.NextDouble();
        double num2 = random2.NextDouble() * 0.2 + 0.9;
        double y = random2.NextDouble() * 0.4 - 0.2;
        double num3 = Math.Pow(2.0, y);
        float value = RandNormal(0f, 0.08f, r, r2);
        value = Mathf.Clamp(value, -0.2f, 0.2f);
        starData.mass = Mathf.Pow(2f, value);
        if (specifyBirthStarMass > 0.1f)
        {
            starData.mass = specifyBirthStarMass;
        }

        if (specifyBirthStarAge > 1E-05f)
        {
            starData.age = specifyBirthStarAge;
        }

        double num4 = 5.0;
        num4 = 2.0 + 0.4 * (1.0 - (double)starData.mass);
        starData.lifetime = (float)(10000.0 * Math.Pow(0.1, Math.Log10((double)starData.mass * 0.5) / Math.Log10(num4) + 1.0) * num2);
        starData.age = (float)(num * 0.4 + 0.3);
        if (specifyBirthStarAge > 1E-05f)
        {
            starData.age = specifyBirthStarAge;
        }

        float num5 = (1f - Mathf.Pow(Mathf.Clamp01(starData.age), 20f) * 0.5f) * starData.mass;
        starData.temperature = (float)(Math.Pow(num5, 0.56 + 0.14 / (Math.Log10(num5 + 4f) / Math.Log10(5.0))) * 4450.0 + 1300.0);
        double num6 = Math.Log10(((double)starData.temperature - 1300.0) / 4500.0) / Math.Log10(2.6) - 0.5;
        if (num6 < 0.0)
        {
            num6 *= 4.0;
        }

        if (num6 > 2.0)
        {
            num6 = 2.0;
        }
        else if (num6 < -4.0)
        {
            num6 = -4.0;
        }

        starData.spectr = (ESpectrType)Mathf.RoundToInt((float)num6 + 4f);
        starData.color = Mathf.Clamp01(((float)num6 + 3.5f) * 0.2f);
        starData.classFactor = (float)num6;
        starData.luminosity = Mathf.Pow(num5, 0.7f);
        starData.radius = (float)(Math.Pow(starData.mass, 0.4) * num3);
        starData.acdiskRadius = 0f;
        float p = (float)num6 + 2f;
        starData.habitableRadius = Mathf.Pow(1.7f, p) + 0.2f * Mathf.Min(1f, starData.orbitScaler);
        starData.lightBalanceRadius = Mathf.Pow(1.7f, p);
        starData.orbitScaler = Mathf.Pow(1.35f, p);
        if (starData.orbitScaler < 1f)
        {
            starData.orbitScaler = Mathf.Lerp(starData.orbitScaler, 1f, 0.6f);
        }

        SetStarAge(starData, starData.age, rn, rt);
        starData.dysonRadius = starData.orbitScaler * 0.28f;
        if ((double)starData.dysonRadius * 40000.0 < (double)(starData.physicsRadius * 1.5f))
        {
            starData.dysonRadius = (float)((double)(starData.physicsRadius * 1.5f) / 40000.0);
        }

        starData.uPosition = VectorLF3.zero;
        starData.name = NameGen.RandomStarName(seed2, starData, galaxy);
        starData.overrideName = string.Empty;
        return starData;
    }

    private double _signpow(double x, double pow)
    {
        double num = (!(x > 0.0)) ? (-1.0) : 1.0;
        return Math.Abs(Math.Pow(x, pow)) * num;
    }

    public void CreateStarPlanets(GalaxyData galaxy, StarData star, GameDesc gameDesc)
    {
        System.Random random = new System.Random(star.seed);
        random.Next();
        random.Next();
        random.Next();
        int seed = random.Next();
        System.Random random2 = new System.Random(seed);
        double num = random2.NextDouble();
        double num2 = random2.NextDouble();
        double num3 = random2.NextDouble();
        double num4 = random2.NextDouble();
        double num5 = random2.NextDouble();
        double num6 = random2.NextDouble() * 0.2 + 0.9;
        double num7 = random2.NextDouble() * 0.2 + 0.9;
        if (star.type == EStarType.BlackHole)
        {
            star.planetCount = 1;
            star.planets = new PlanetData[star.planetCount];
            int info_seed = random2.Next();
            int gen_seed = random2.Next();
            star.planets[0] = planetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, 3, 1, gasGiant: false, info_seed, gen_seed);
        }
        else if (star.type == EStarType.NeutronStar)
        {
            star.planetCount = 1;
            star.planets = new PlanetData[star.planetCount];
            int info_seed2 = random2.Next();
            int gen_seed2 = random2.Next();
            star.planets[0] = planetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, 3, 1, gasGiant: false, info_seed2, gen_seed2);
        }
        else if (star.type == EStarType.WhiteDwarf)
        {
            if (num < 0.699999988079071)
            {
                star.planetCount = 1;
                star.planets = new PlanetData[star.planetCount];
                int info_seed3 = random2.Next();
                int gen_seed3 = random2.Next();
                star.planets[0] = planetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, 3, 1, gasGiant: false, info_seed3, gen_seed3);
            }
            else
            {
                star.planetCount = 2;
                star.planets = new PlanetData[star.planetCount];
                int num8 = 0;
                int num9 = 0;
                if (num2 < 0.30000001192092896)
                {
                    num8 = random2.Next();
                    num9 = random2.Next();
                    star.planets[0] = planetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, 3, 1, gasGiant: false, num8, num9);
                    num8 = random2.Next();
                    num9 = random2.Next();
                    star.planets[1] = planetGen.CreatePlanet(galaxy, star, gameDesc, 1, 0, 4, 2, gasGiant: false, num8, num9);
                }
                else
                {
                    num8 = random2.Next();
                    num9 = random2.Next();
                    star.planets[0] = planetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, 4, 1, gasGiant: true, num8, num9);
                    num8 = random2.Next();
                    num9 = random2.Next();
                    star.planets[1] = planetGen.CreatePlanet(galaxy, star, gameDesc, 1, 1, 1, 1, gasGiant: false, num8, num9);
                }
            }
        }
        else if (star.type == EStarType.GiantStar)
        {
            if (num < 0.30000001192092896)
            {
                star.planetCount = 1;
                star.planets = new PlanetData[star.planetCount];
                int info_seed4 = random2.Next();
                int gen_seed4 = random2.Next();
                star.planets[0] = planetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, (!(num3 > 0.5)) ? 2 : 3, 1, gasGiant: false, info_seed4, gen_seed4);
            }
            else if (num < 0.800000011920929)
            {
                star.planetCount = 2;
                star.planets = new PlanetData[star.planetCount];
                int num10 = 0;
                int num11 = 0;
                if (num2 < 0.25)
                {
                    num10 = random2.Next();
                    num11 = random2.Next();
                    star.planets[0] = planetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, (!(num3 > 0.5)) ? 2 : 3, 1, gasGiant: false, num10, num11);
                    num10 = random2.Next();
                    num11 = random2.Next();
                    star.planets[1] = planetGen.CreatePlanet(galaxy, star, gameDesc, 1, 0, (!(num3 > 0.5)) ? 3 : 4, 2, gasGiant: false, num10, num11);
                }
                else
                {
                    num10 = random2.Next();
                    num11 = random2.Next();
                    star.planets[0] = planetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, 3, 1, gasGiant: true, num10, num11);
                    num10 = random2.Next();
                    num11 = random2.Next();
                    star.planets[1] = planetGen.CreatePlanet(galaxy, star, gameDesc, 1, 1, 1, 1, gasGiant: false, num10, num11);
                }
            }
            else
            {
                star.planetCount = 3;
                star.planets = new PlanetData[star.planetCount];
                int num12 = 0;
                int num13 = 0;
                if (num2 < 0.15000000596046448)
                {
                    num12 = random2.Next();
                    num13 = random2.Next();
                    star.planets[0] = planetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, (!(num3 > 0.5)) ? 2 : 3, 1, gasGiant: false, num12, num13);
                    num12 = random2.Next();
                    num13 = random2.Next();
                    star.planets[1] = planetGen.CreatePlanet(galaxy, star, gameDesc, 1, 0, (!(num3 > 0.5)) ? 3 : 4, 2, gasGiant: false, num12, num13);
                    num12 = random2.Next();
                    num13 = random2.Next();
                    star.planets[2] = planetGen.CreatePlanet(galaxy, star, gameDesc, 2, 0, (!(num3 > 0.5)) ? 4 : 5, 3, gasGiant: false, num12, num13);
                }
                else if (num2 < 0.75)
                {
                    num12 = random2.Next();
                    num13 = random2.Next();
                    star.planets[0] = planetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, (!(num3 > 0.5)) ? 2 : 3, 1, gasGiant: false, num12, num13);
                    num12 = random2.Next();
                    num13 = random2.Next();
                    star.planets[1] = planetGen.CreatePlanet(galaxy, star, gameDesc, 1, 0, 4, 2, gasGiant: true, num12, num13);
                    num12 = random2.Next();
                    num13 = random2.Next();
                    star.planets[2] = planetGen.CreatePlanet(galaxy, star, gameDesc, 2, 2, 1, 1, gasGiant: false, num12, num13);
                }
                else
                {
                    num12 = random2.Next();
                    num13 = random2.Next();
                    star.planets[0] = planetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, (!(num3 > 0.5)) ? 3 : 4, 1, gasGiant: true, num12, num13);
                    num12 = random2.Next();
                    num13 = random2.Next();
                    star.planets[1] = planetGen.CreatePlanet(galaxy, star, gameDesc, 1, 1, 1, 1, gasGiant: false, num12, num13);
                    num12 = random2.Next();
                    num13 = random2.Next();
                    star.planets[2] = planetGen.CreatePlanet(galaxy, star, gameDesc, 2, 1, 2, 2, gasGiant: false, num12, num13);
                }
            }
        }
        else
        {
            Array.Clear(pGas, 0, pGas.Length);
            if (star.index == 0)
            {
                star.planetCount = 4;
                pGas[0] = 0.0;
                pGas[1] = 0.0;
                pGas[2] = 0.0;
            }
            else if (star.spectr == ESpectrType.M)
            {
                if (num < 0.1)
                {
                    star.planetCount = 1;
                }
                else if (num < 0.3)
                {
                    star.planetCount = 2;
                }
                else if (num < 0.8)
                {
                    star.planetCount = 3;
                }
                else
                {
                    star.planetCount = 4;
                }

                if (star.planetCount <= 3)
                {
                    pGas[0] = 0.2;
                    pGas[1] = 0.2;
                }
                else
                {
                    pGas[0] = 0.0;
                    pGas[1] = 0.2;
                    pGas[2] = 0.3;
                }
            }
            else if (star.spectr == ESpectrType.K)
            {
                if (num < 0.1)
                {
                    star.planetCount = 1;
                }
                else if (num < 0.2)
                {
                    star.planetCount = 2;
                }
                else if (num < 0.7)
                {
                    star.planetCount = 3;
                }
                else if (num < 0.95)
                {
                    star.planetCount = 4;
                }
                else
                {
                    star.planetCount = 5;
                }

                if (star.planetCount <= 3)
                {
                    pGas[0] = 0.18;
                    pGas[1] = 0.18;
                }
                else
                {
                    pGas[0] = 0.0;
                    pGas[1] = 0.18;
                    pGas[2] = 0.28;
                    pGas[3] = 0.28;
                }
            }
            else if (star.spectr == ESpectrType.G)
            {
                if (num < 0.4)
                {
                    star.planetCount = 3;
                }
                else if (num < 0.9)
                {
                    star.planetCount = 4;
                }
                else
                {
                    star.planetCount = 5;
                }

                if (star.planetCount <= 3)
                {
                    pGas[0] = 0.18;
                    pGas[1] = 0.18;
                }
                else
                {
                    pGas[0] = 0.0;
                    pGas[1] = 0.2;
                    pGas[2] = 0.3;
                    pGas[3] = 0.3;
                }
            }
            else if (star.spectr == ESpectrType.F)
            {
                if (num < 0.35)
                {
                    star.planetCount = 3;
                }
                else if (num < 0.8)
                {
                    star.planetCount = 4;
                }
                else
                {
                    star.planetCount = 5;
                }

                if (star.planetCount <= 3)
                {
                    pGas[0] = 0.2;
                    pGas[1] = 0.2;
                }
                else
                {
                    pGas[0] = 0.0;
                    pGas[1] = 0.22;
                    pGas[2] = 0.31;
                    pGas[3] = 0.31;
                }
            }
            else if (star.spectr == ESpectrType.A)
            {
                if (num < 0.3)
                {
                    star.planetCount = 3;
                }
                else if (num < 0.75)
                {
                    star.planetCount = 4;
                }
                else
                {
                    star.planetCount = 5;
                }

                if (star.planetCount <= 3)
                {
                    pGas[0] = 0.2;
                    pGas[1] = 0.2;
                }
                else
                {
                    pGas[0] = 0.1;
                    pGas[1] = 0.28;
                    pGas[2] = 0.3;
                    pGas[3] = 0.35;
                }
            }
            else if (star.spectr == ESpectrType.B)
            {
                if (num < 0.3)
                {
                    star.planetCount = 4;
                }
                else if (num < 0.75)
                {
                    star.planetCount = 5;
                }
                else
                {
                    star.planetCount = 6;
                }

                if (star.planetCount <= 3)
                {
                    pGas[0] = 0.2;
                    pGas[1] = 0.2;
                }
                else
                {
                    pGas[0] = 0.1;
                    pGas[1] = 0.22;
                    pGas[2] = 0.28;
                    pGas[3] = 0.35;
                    pGas[4] = 0.35;
                }
            }
            else if (star.spectr == ESpectrType.O)
            {
                if (num < 0.5)
                {
                    star.planetCount = 5;
                }
                else
                {
                    star.planetCount = 6;
                }

                pGas[0] = 0.1;
                pGas[1] = 0.2;
                pGas[2] = 0.25;
                pGas[3] = 0.3;
                pGas[4] = 0.32;
                pGas[5] = 0.35;
            }
            else
            {
                star.planetCount = 1;
            }

            star.planets = new PlanetData[star.planetCount];
            int num14 = 0;
            int num15 = 0;
            int num16 = 0;
            int num17 = 1;
            for (int i = 0; i < star.planetCount; i++)
            {
                int info_seed5 = random2.Next();
                int gen_seed5 = random2.Next();
                double num18 = random2.NextDouble();
                double num19 = random2.NextDouble();
                bool flag = false;
                if (num16 == 0)
                {
                    num14++;
                    if (i < star.planetCount - 1 && num18 < pGas[i])
                    {
                        flag = true;
                        if (num17 < 3)
                        {
                            num17 = 3;
                        }
                    }

                    while (true)
                    {
                        if (star.index == 0 && num17 == 3)
                        {
                            flag = true;
                            break;
                        }

                        int num20 = star.planetCount - i;
                        int num21 = 9 - num17;
                        if (num21 <= num20)
                        {
                            break;
                        }

                        float a = (float)num20 / (float)num21;
                        a = ((num17 <= 3) ? (Mathf.Lerp(a, 1f, 0.15f) + 0.01f) : (Mathf.Lerp(a, 1f, 0.45f) + 0.01f));
                        double num22 = random2.NextDouble();
                        if (num22 < (double)a)
                        {
                            break;
                        }

                        num17++;
                    }
                }
                else
                {
                    num15++;
                    flag = false;
                }

                star.planets[i] = planetGen.CreatePlanet(galaxy, star, gameDesc, i, num16, (num16 != 0) ? num15 : num17, (num16 != 0) ? num15 : num14, flag, info_seed5, gen_seed5);
                num17++;
                if (flag)
                {
                    num16 = num14;
                    num15 = 0;
                }

                if (num15 >= 1 && num19 < 0.8)
                {
                    num16 = 0;
                    num15 = 0;
                }
            }
        }

        int num23 = 0;
        int num24 = 0;
        int num25 = 0;
        int num26 = 0;
        for (int j = 0; j < star.planetCount; j++)
        {
            if (star.planets[j].type == EPlanetType.Gas)
            {
                num23 = star.planets[j].orbitIndex;
                break;
            }
        }

        for (int k = 0; k < star.planetCount; k++)
        {
            if (star.planets[k].orbitAround == 0)
            {
                num24 = star.planets[k].orbitIndex;
            }
        }

        if (num23 > 0)
        {
            int num27 = num23 - 1;
            bool flag2 = true;
            for (int l = 0; l < star.planetCount; l++)
            {
                if (star.planets[l].orbitAround == 0 && star.planets[l].orbitIndex == num23 - 1)
                {
                    flag2 = false;
                    break;
                }
            }

            if (flag2 && num4 < 0.2 + (double)num27 * 0.2)
            {
                num25 = num27;
            }
        }

        num26 = ((num5 < 0.2) ? (num24 + 3) : ((num5 < 0.4) ? (num24 + 2) : ((num5 < 0.8) ? (num24 + 1) : 0)));
        if (num26 != 0 && num26 < 5)
        {
            num26 = 5;
        }

        star.asterBelt1OrbitIndex = num25;
        star.asterBelt2OrbitIndex = num26;
        if (num25 > 0)
        {
            star.asterBelt1Radius = orbitRadius[num25] * (float)num6 * star.orbitScaler;
        }

        if (num26 > 0)
        {
            star.asterBelt2Radius = orbitRadius[num26] * (float)num7 * star.orbitScaler;
        }
    }

    public void SetStarAge(StarData star, float age, double rn, double rt)
    {
        float num = (float)(rn * 0.1 + 0.95);
        float num2 = (float)(rt * 0.4 + 0.8);
        float num3 = (float)(rt * 9.0 + 1.0);
        star.age = age;
        if (age >= 1f)
        {
            if (star.mass >= 18f)
            {
                star.type = EStarType.BlackHole;
                star.spectr = ESpectrType.X;
                star.mass *= 2.5f * num2;
                star.radius *= 1f;
                star.acdiskRadius = star.radius * 5f;
                star.temperature = 0f;
                star.luminosity *= 0.001f * num;
                star.habitableRadius = 0f;
                star.lightBalanceRadius *= 0.4f * num;
            }
            else if (star.mass >= 7f)
            {
                star.type = EStarType.NeutronStar;
                star.spectr = ESpectrType.X;
                star.mass *= 0.2f * num;
                star.radius *= 0.15f;
                star.acdiskRadius = star.radius * 9f;
                star.temperature = num3 * 1E+07f;
                star.luminosity *= 0.1f * num;
                star.habitableRadius = 0f;
                star.lightBalanceRadius *= 3f * num;
                star.orbitScaler *= 1.5f * num;
            }
            else
            {
                star.type = EStarType.WhiteDwarf;
                star.spectr = ESpectrType.X;
                star.mass *= 0.2f * num;
                star.radius *= 0.2f;
                star.acdiskRadius = 0f;
                star.temperature = num2 * 150000f;
                star.luminosity *= 0.04f * num2;
                star.habitableRadius *= 0.15f * num2;
                star.lightBalanceRadius *= 0.2f * num;
            }
        }
        else if (age >= 0.96f)
        {
            float num4 = (float)(Math.Pow(5.0, Math.Abs(Math.Log10(star.mass) - 0.7)) * 5.0);
            if (num4 > 10f)
            {
                num4 = (Mathf.Log(num4 * 0.1f) + 1f) * 10f;
            }

            float num5 = 1f - Mathf.Pow(star.age, 30f) * 0.5f;
            star.type = EStarType.GiantStar;
            star.mass = num5 * star.mass;
            star.radius = num4 * num2;
            star.acdiskRadius = 0f;
            star.temperature = num5 * star.temperature;
            star.luminosity = 1.6f * star.luminosity;
            star.habitableRadius = 9f * star.habitableRadius;
            star.lightBalanceRadius = 3f * star.habitableRadius;
            star.orbitScaler = 3.3f * star.orbitScaler;
        }
    }

    private float RandNormal(float averageValue, float standardDeviation, double r1, double r2)
    {
        return averageValue + standardDeviation * (float)(Math.Sqrt(-2.0 * Math.Log(1.0 - r1)) * Math.Sin(Math.PI * 2.0 * r2));
    }
}