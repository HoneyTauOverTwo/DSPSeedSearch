using System;
using System.Collections.Generic;
using UnityEngine;

public class ThreadSafeUniverseGen
{
    public static int algoVersion = 20200403;

    private List<VectorLF3> tmp_poses;

    private List<VectorLF3> tmp_drunk;

    private int[] tmp_state;

    ThreadSafeStarGen starGen = new ThreadSafeStarGen();

    public void Start()
    {
        PlanetModelingManager.Start();
    }

    public void End()
    {
        PlanetModelingManager.End();
    }

    public void Update()
    {
        PlanetModelingManager.Update();
    }

    public GalaxyData CreateGalaxy(GameDesc gameDesc)
    {
        int galaxyAlgo = gameDesc.galaxyAlgo;
        int galaxySeed = gameDesc.galaxySeed;
        int starCount = gameDesc.starCount;
        if (galaxyAlgo < 20200101 || galaxyAlgo > 20591231)
        {
            throw new Exception("Wrong version of unigen algorithm!");
        }

        System.Random random = new System.Random(galaxySeed);
        int seed = random.Next();
        starCount = GenerateTempPoses(seed, starCount, 4, 2.0, 2.3, 3.5, 0.18);
        GalaxyData galaxyData = new GalaxyData();
        galaxyData.seed = galaxySeed;
        galaxyData.starCount = starCount;
        galaxyData.stars = new StarData[starCount];
        Assert.Positive(starCount);
        if (starCount <= 0)
        {
            return galaxyData;
        }

        float num = (float)random.NextDouble();
        float num2 = (float)random.NextDouble();
        float num3 = (float)random.NextDouble();
        float num4 = (float)random.NextDouble();
        int num5 = Mathf.CeilToInt(0.01f * (float)starCount + num * 0.3f);
        int num6 = Mathf.CeilToInt(0.01f * (float)starCount + num2 * 0.3f);
        int num7 = Mathf.CeilToInt(0.016f * (float)starCount + num3 * 0.4f);
        int num8 = Mathf.CeilToInt(0.013f * (float)starCount + num4 * 1.4f);
        int num9 = starCount - num5;
        int num10 = num9 - num6;
        int num11 = num10 - num7;
        int num12 = (num11 - 1) / num8;
        int num13 = num12 / 2;
        for (int i = 0; i < starCount; i++)
        {
            int seed2 = random.Next();
            if (i == 0)
            {
                galaxyData.stars[i] = starGen.CreateBirthStar(galaxyData, seed2);
                continue;
            }

            ESpectrType needSpectr = ESpectrType.X;
            if (i == 3)
            {
                needSpectr = ESpectrType.M;
            }
            else if (i == num11 - 1)
            {
                needSpectr = ESpectrType.O;
            }

            EStarType needtype = EStarType.MainSeqStar;
            if (i % num12 == num13)
            {
                needtype = EStarType.GiantStar;
            }

            if (i >= num9)
            {
                needtype = EStarType.BlackHole;
            }
            else if (i >= num10)
            {
                needtype = EStarType.NeutronStar;
            }
            else if (i >= num11)
            {
                needtype = EStarType.WhiteDwarf;
            }

            galaxyData.stars[i] = starGen.CreateStar(galaxyData, tmp_poses[i], i + 1, seed2, needtype, needSpectr);
        }

        AstroPose[] astroPoses = galaxyData.astroPoses;
        StarData[] stars = galaxyData.stars;
        for (int j = 0; j < galaxyData.astroPoses.Length; j++)
        {
            astroPoses[j].uRot.w = 1f;
            astroPoses[j].uRotNext.w = 1f;
        }

        for (int k = 0; k < starCount; k++)
        {
            starGen.CreateStarPlanets(galaxyData, stars[k], gameDesc);
            astroPoses[stars[k].id * 100].uPos = (astroPoses[stars[k].id * 100].uPosNext = stars[k].uPosition);
            astroPoses[stars[k].id * 100].uRot = (astroPoses[stars[k].id * 100].uRotNext = Quaternion.identity);
            astroPoses[stars[k].id * 100].uRadius = stars[k].physicsRadius;
        }

        galaxyData.UpdatePoses(0.0);
        galaxyData.birthPlanetId = 0;
        if (starCount > 0)
        {
            StarData starData = stars[0];
            for (int l = 0; l < starData.planetCount; l++)
            {
                PlanetData planetData = starData.planets[l];
                ThemeProto themeProto = LDB.themes.Select(planetData.theme);
                if (themeProto != null && themeProto.Distribute == EThemeDistribute.Birth)
                {
                    galaxyData.birthPlanetId = planetData.id;
                    galaxyData.birthStarId = starData.id;
                    break;
                }
            }
        }

        Assert.Positive(galaxyData.birthPlanetId);
        for (int m = 0; m < starCount; m++)
        {
            StarData starData2 = galaxyData.stars[m];
            for (int n = 0; n < starData2.planetCount; n++)
            {
                PlanetData planet = starData2.planets[n];
                PlanetAlgorithm planetAlgorithm = PlanetModelingManager.Algorithm(planet);
                planetAlgorithm.GenerateVeins(sketchOnly: true);
            }
        }

        CreateGalaxyStarGraph(galaxyData);
        return galaxyData;
    }

    private int GenerateTempPoses(int seed, int targetCount, int iterCount, double minDist, double minStepLen, double maxStepLen, double flatten)
    {
        if (tmp_poses == null)
        {
            tmp_poses = new List<VectorLF3>();
            tmp_drunk = new List<VectorLF3>();
        }
        else
        {
            tmp_poses.Clear();
            tmp_drunk.Clear();
        }

        if (iterCount < 1)
        {
            iterCount = 1;
        }
        else if (iterCount > 16)
        {
            iterCount = 16;
        }

        RandomPoses(seed, targetCount * iterCount, minDist, minStepLen, maxStepLen, flatten);
        for (int num = tmp_poses.Count - 1; num >= 0; num--)
        {
            if (num % iterCount != 0)
            {
                tmp_poses.RemoveAt(num);
            }

            if (tmp_poses.Count <= targetCount)
            {
                break;
            }
        }

        return tmp_poses.Count;
    }

    private void RandomPoses(int seed, int maxCount, double minDist, double minStepLen, double maxStepLen, double flatten)
    {
        System.Random random = new System.Random(seed);
        double num = random.NextDouble();
        tmp_poses.Add(VectorLF3.zero);
        int num2 = 6;
        int num3 = 8;
        if (num2 < 1)
        {
            num2 = 1;
        }

        if (num3 < 1)
        {
            num3 = 1;
        }

        int num4 = (int)(num * (double)(num3 - num2) + (double)num2);
        for (int i = 0; i < num4; i++)
        {
            int num5 = 0;
            while (num5++ < 256)
            {
                double num6 = random.NextDouble() * 2.0 - 1.0;
                double num7 = (random.NextDouble() * 2.0 - 1.0) * flatten;
                double num8 = random.NextDouble() * 2.0 - 1.0;
                double num9 = random.NextDouble();
                double num10 = num6 * num6 + num7 * num7 + num8 * num8;
                if (num10 > 1.0 || num10 < 1E-08)
                {
                    continue;
                }

                double num11 = Math.Sqrt(num10);
                num9 = (num9 * (maxStepLen - minStepLen) + minDist) / num11;
                VectorLF3 vectorLF = new VectorLF3(num6 * num9, num7 * num9, num8 * num9);
                if (CheckCollision(tmp_poses, vectorLF, minDist))
                {
                    continue;
                }

                tmp_drunk.Add(vectorLF);
                tmp_poses.Add(vectorLF);
                if (tmp_poses.Count >= maxCount)
                {
                    return;
                }

                break;
            }
        }

        int num12 = 0;
        while (num12++ < 256)
        {
            for (int j = 0; j < tmp_drunk.Count; j++)
            {
                double num13 = random.NextDouble();
                if (num13 > 0.7)
                {
                    continue;
                }

                int num14 = 0;
                while (num14++ < 256)
                {
                    double num15 = random.NextDouble() * 2.0 - 1.0;
                    double num16 = (random.NextDouble() * 2.0 - 1.0) * flatten;
                    double num17 = random.NextDouble() * 2.0 - 1.0;
                    double num18 = random.NextDouble();
                    double num19 = num15 * num15 + num16 * num16 + num17 * num17;
                    if (num19 > 1.0 || num19 < 1E-08)
                    {
                        continue;
                    }

                    double num20 = Math.Sqrt(num19);
                    num18 = (num18 * (maxStepLen - minStepLen) + minDist) / num20;
                    VectorLF3 vectorLF2 = tmp_drunk[j];
                    double x_ = vectorLF2.x + num15 * num18;
                    VectorLF3 vectorLF3 = tmp_drunk[j];
                    double y_ = vectorLF3.y + num16 * num18;
                    VectorLF3 vectorLF4 = tmp_drunk[j];
                    VectorLF3 vectorLF5 = new VectorLF3(x_, y_, vectorLF4.z + num17 * num18);
                    if (CheckCollision(tmp_poses, vectorLF5, minDist))
                    {
                        continue;
                    }

                    tmp_drunk[j] = vectorLF5;
                    tmp_poses.Add(vectorLF5);
                    if (tmp_poses.Count >= maxCount)
                    {
                        return;
                    }

                    break;
                }
            }
        }
    }

    private bool CheckCollision(List<VectorLF3> pts, VectorLF3 pt, double min_dist)
    {
        double num = min_dist * min_dist;
        foreach (VectorLF3 pt2 in pts)
        {
            double num2 = pt.x - pt2.x;
            double num3 = pt.y - pt2.y;
            double num4 = pt.z - pt2.z;
            double num5 = num2 * num2 + num3 * num3 + num4 * num4;
            if (num5 < num)
            {
                return true;
            }
        }

        return false;
    }

    public void CreateGalaxyStarGraph(GalaxyData galaxy)
    {
        galaxy.graphNodes = new StarGraphNode[galaxy.starCount];
        for (int i = 0; i < galaxy.starCount; i++)
        {
            galaxy.graphNodes[i] = new StarGraphNode(galaxy.stars[i]);
            StarGraphNode starGraphNode = galaxy.graphNodes[i];
            for (int j = 0; j < i; j++)
            {
                StarGraphNode starGraphNode2 = galaxy.graphNodes[j];
                VectorLF3 pos = starGraphNode.pos;
                VectorLF3 pos2 = starGraphNode2.pos;
                if ((pos - pos2).sqrMagnitude < 64.0)
                {
                    list_sorted_add(starGraphNode.conns, starGraphNode2);
                    list_sorted_add(starGraphNode2.conns, starGraphNode);
                }
            }

            line_arragement_for_add_node(starGraphNode);
        }
    }

    private void list_sorted_add(List<StarGraphNode> l, StarGraphNode n)
    {
        int count = l.Count;
        bool flag = false;
        for (int i = 0; i < count; i++)
        {
            if (l[i].index == n.index)
            {
                flag = true;
                break;
            }

            if (l[i].index > n.index)
            {
                l.Insert(i, n);
                flag = true;
                break;
            }
        }

        if (!flag)
        {
            l.Add(n);
        }
    }

    private void line_arragement_for_add_node(StarGraphNode node)
    {
        if (tmp_state == null)
        {
            tmp_state = new int[128];
        }

        Array.Clear(tmp_state, 0, tmp_state.Length);
        Vector3 vector = node.pos;
        for (int i = 0; i < node.conns.Count; i++)
        {
            StarGraphNode starGraphNode = node.conns[i];
            Vector3 vector2 = starGraphNode.pos;
            for (int j = i + 1; j < node.conns.Count; j++)
            {
                StarGraphNode starGraphNode2 = node.conns[j];
                Vector3 vector3 = starGraphNode2.pos;
                bool flag = false;
                for (int k = 0; k < starGraphNode.conns.Count; k++)
                {
                    if (starGraphNode.conns[k] == starGraphNode2)
                    {
                        flag = true;
                        break;
                    }
                }

                if (!flag)
                {
                    continue;
                }

                float num = (vector2.x - vector.x) * (vector2.x - vector.x) + (vector2.y - vector.y) * (vector2.y - vector.y) + (vector2.z - vector.z) * (vector2.z - vector.z);
                float num2 = (vector3.x - vector.x) * (vector3.x - vector.x) + (vector3.y - vector.y) * (vector3.y - vector.y) + (vector3.z - vector.z) * (vector3.z - vector.z);
                float num3 = (vector2.x - vector3.x) * (vector2.x - vector3.x) + (vector2.y - vector3.y) * (vector2.y - vector3.y) + (vector2.z - vector3.z) * (vector2.z - vector3.z);
                float num4 = (num > num2) ? ((!(num > num3)) ? num3 : num) : ((!(num2 > num3)) ? num3 : num2);
                float num5 = (num < num2) ? ((!(num < num3)) ? num3 : num) : ((!(num2 < num3)) ? num3 : num2);
                float num6 = (num + num2 + num3 - num4 - num5) * 1.001f;
                num5 *= 1.01f;
                if (num <= num6 || num <= num5)
                {
                    if (tmp_state[i] == 0)
                    {
                        list_sorted_add(node.lines, starGraphNode);
                        list_sorted_add(starGraphNode.lines, node);
                        tmp_state[i] = 1;
                    }
                }
                else
                {
                    tmp_state[i] = -1;
                    node.lines.Remove(starGraphNode);
                    starGraphNode.lines.Remove(node);
                }

                if (num2 <= num6 || num2 <= num5)
                {
                    if (tmp_state[j] == 0)
                    {
                        list_sorted_add(node.lines, starGraphNode2);
                        list_sorted_add(starGraphNode2.lines, node);
                        tmp_state[j] = 1;
                    }
                }
                else
                {
                    tmp_state[j] = -1;
                    node.lines.Remove(starGraphNode2);
                    starGraphNode2.lines.Remove(node);
                }

                if (num3 > num6 && num3 > num5)
                {
                    starGraphNode.lines.Remove(starGraphNode2);
                    starGraphNode2.lines.Remove(starGraphNode);
                }
            }

            if (tmp_state[i] == 0)
            {
                list_sorted_add(node.lines, starGraphNode);
                list_sorted_add(starGraphNode.lines, node);
                tmp_state[i] = 1;
            }
        }

        Array.Clear(tmp_state, 0, tmp_state.Length);
    }
}