using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.VisualScripting;
using UnityEngine.Playables;

public class LayeredSphereMesh
{
    // Copied from LayeredSphere which will be merged into GameControl eventually(7/23):
    // Time to synthesize my Unity knowledge. Looks tough, but I think It'd look dope as hell.
    // It does look dope as hell 9/21
    LayeredSphereColorGenerator colorGenerator;
    ColorSettings colorSettings;
    //EditorSettings worldSettings;
    //MinMax elevMinMax;
    GameObject builtWorld;
    //GameObject builtOcean;
    //Vector3[,] spherePositions;
    float[] actualLayers;
    float[] actualRadii;
    float finalOceanRadius;
    float[,] oceanUVMap;

    // vars for localized rendering
    bool[,] activeMap;
    List<LayeredSphereMeshNode> activeNodes;
    NormalizedFloat[,] heatmap;
    int[,] regionmap;
    int mapType;
    int regionCount;
    //int variantCount;

    //public LayeredSphereMesh(Game in_game, EditorSettings in_worldSettings, ColorSettings in_ColorSettings)
    //{
    //    //instance = in_game;
    //    worldSettings = in_worldSettings;
    //    colorSettings = in_ColorSettings;
    //    mapType = 0;
    //    regionCount = 1;
    //}

    public LayeredSphereMesh(GameData in_data, ColorSettings in_colorSettings)
    {
        //worldSettings = in_worldSettings;
        colorSettings = in_colorSettings;
        mapType = 0;
        regionCount = 1;
    }

    public void PrepSmallMesh(World in_world, GameData in_data)
    {
        actualRadii = in_world.Get_grid().Get_actualradii();
        actualLayers = in_world.Get_grid().Get_actualLayers();
        oceanUVMap = CalculateOceanUVMap(in_world.Get_grid());

        Point2D testCenter = new Point2D(in_world.Get_grid().Get_Nodes().GetLength(0) / 2, in_world.Get_grid().Get_Nodes().GetLength(1) / 2);
        Point2D[] testPeri = testCenter.GetValidatedPerimeter(new PointValidator(in_world.Get_grid().Get_Nodes()));
        List<Point2D> testPoints = new List<Point2D>();
        testPoints.Add(testCenter);
        testPoints.AddRange(testPeri);

        //builtOcean = BuildOceanMesh(in_world);
        activeMap = new bool[in_world.Get_grid().Get_Nodes().GetLength(0), in_world.Get_grid().Get_Nodes().GetLength(1)];
        activeNodes = new List<LayeredSphereMeshNode>();
        builtWorld = new GameObject();
        builtWorld.name = "World from seed: " + in_data.settings.worldSeed;

        //PointFeeder initial = new PointFeeder(testCenter, 2, new PointValidator(activeMap));
        //UpdateLocalMesh(initial, in_world.Get_grid());

        colorGenerator = new LayeredSphereColorGenerator(actualRadii[actualRadii.Length - 1], actualRadii[0]);
        colorGenerator.UpdateColors(colorSettings, in_world);
        //colorGenerator.UpdateRegionColors(in_world.Get_rivers().Get_drainages().Get_rawCells());
        //colorGenerator.UpdateHeatMapColors();
    }

    public void BuildMesh(World in_world, GameData in_data)
    {
        // This method will be deprecated

        //elevMinMax = in_world.Get_grid().Get_layerElevMinMax();
        //spherePositions = RadiiToSpherePositions(in_world.Get_grid().Get_Nodes());
        actualRadii = in_world.Get_grid().Get_actualradii();
        actualLayers = in_world.Get_grid().Get_actualLayers();

        builtWorld = BuildWorldCoreMesh(in_world, in_data);
        //builtOcean = BuildOceanMesh(in_world);

        InjectUVFromRegions(in_world.Get_rivers().Get_drainages().Get_rawCells(), in_world.Get_grid().Get_Nodes());
        //InjectUVFromRegions(in_world.Get_landBodies().Get_rawCells(), in_world.Get_grid().Get_Nodes());

        colorGenerator = new LayeredSphereColorGenerator(actualRadii[actualRadii.Length - 1], actualRadii[0]);
        colorGenerator.UpdateColors(colorSettings, in_world);
        colorGenerator.UpdateRegionColors(in_world.Get_rivers().Get_drainages().Get_rawCells());
        //colorGenerator.ToggleRegionalShading();
    }

    public void BuildMeshFromNodes()
    {
        //elevMinMax = instance.Get_homeworld().Get_grid().Get_layerElevMinMax();
        //Square[,] squares = GetSquaresFromNodes(instance.Get_homeworld().Get_grid().Get_Nodes());
        //spherePositions = RadiiToSpherePositions(instance.Get_homeworld().Get_grid().Get_Nodes());

        //builtWorld = BuildWorldCoreMesh();
        //builtOcean = BuildOceanMesh(instance);

        //colorGenerator = new LayeredSphereColorGenerator(actualRadii[actualRadii.Length - 1], actualRadii[0]);
        //colorGenerator.UpdateColors(colorSettings, instance);
    }

    public GameObject Get_builtWorld() { return builtWorld; }
    //public GameObject Get_builtOcean() { return builtOcean; }
    public Vector3[,] Get_spherePositions(World in_world, GameData in_data) { return RadiiToSpherePositions(in_world.Get_grid().Get_Nodes(), in_data); }
    public float Get_finalOceanRadius() { return finalOceanRadius; }
    public LayeredSphereColorGenerator Get_colorGenerator() { return colorGenerator; }
    public bool[,] Get_activeMap() { return activeMap; }

    private float[,] CalculateOceanUVMap(WorldGrid in_grid)
    {
        // this is a double pass
        // since we don't capture elevs in an array like we do layers and radii we have to manually find
        // the min and max elevations for ocean nodes
        WorldNode[,] nodes = in_grid.Get_Nodes();
        int x = 0;
        int y = 0;
        int maxX = nodes.GetLength(0);
        int maxY = nodes.GetLength(1);
        float minElev = float.MaxValue;
        float maxElev = float.MinValue;
        while (y < maxY)
        {
            while (x < maxX)
            {
                if (!nodes[x, y].IsLand())
                {
                    float locElev = nodes[x, y].Get_elev();
                    if (locElev < minElev)
                        minElev = locElev;
                    if (locElev > maxElev)
                        maxElev = locElev;
                }
                x++;
            }
            x = 0;
            y++;
        }

        x = 0;
        y = 0;
        float[,] toRet = new float[maxX, maxY];
        while (y < maxY)
        {
            while (x < maxX)
            {
                float locElev = nodes[x, y].Get_elev();
                if (locElev > maxElev)
                    locElev = maxElev;
                float normalized = (locElev - minElev) / (maxElev - minElev);
                toRet[x, y] = normalized;
                x++;
            }
            x = 0;
            y++;
        }

        return toRet;
    }

    // Mesh Building Functions

    public void DeltaUpdateLocalMesh(PointFeeder in_feeder, WorldGrid in_grid, GameData in_data)
    {
        if (in_feeder.Get_newlyActivePoints().Count > 0)
        {
            Point2D toActivate = in_feeder.Get_newlyActivePoints()[0];
            if (!activeMap[toActivate.x, toActivate.y])
            {
                LayeredSphereMeshNode newActiveNode = new LayeredSphereMeshNode(in_grid, toActivate, actualLayers, actualRadii, oceanUVMap, in_data, colorSettings);
                newActiveNode.Get_landNode().transform.parent = builtWorld.transform;
                newActiveNode.Get_oceanNode().transform.parent = builtWorld.transform;
                if (mapType != 0)
                {
                    if (mapType == 1)
                    {
                        Vector2[] landUVS = GetLocalLerpedUVs(heatmap, toActivate, in_grid.Get_Nodes());
                        newActiveNode.InjectUVs(landUVS, landUVS, actualLayers, actualRadii, in_grid);
                    }
                    else if (mapType == 2)
                    {
                        List<Vector2> landUVS = new List<Vector2>();
                        List<Vector2> oceanUVS = new List<Vector2>();
                        float locScore = ((float)regionmap[toActivate.x, toActivate.y]) / ((float)regionCount);
                        locScore += 0.001f;
                        int landUVLim = newActiveNode.Get_landNode().GetComponent<MeshFilter>().sharedMesh.uv.Length;
                        for (int i = 0; i < landUVLim; i++)
                            landUVS.Add(new Vector2(locScore, 0));
                        int oceanUVLim = newActiveNode.Get_oceanNode().GetComponent<MeshFilter>().sharedMesh.uv.Length;
                        for (int i = 0; i < oceanUVLim; i++)
                            oceanUVS.Add(new Vector2(locScore, 0));
                        newActiveNode.InjectUVs(landUVS.ToArray(), oceanUVS.ToArray(), actualLayers, actualRadii, in_grid);
                    }
                    else if (mapType == 3)
                    {
                        List<Vector2> landUVS = new List<Vector2>();
                        List<Vector2> oceanUVS = new List<Vector2>();
                        float locScore = heatmap[toActivate.x, toActivate.y].Get_value();
                        int landUVLim = newActiveNode.Get_landNode().GetComponent<MeshFilter>().sharedMesh.uv.Length;
                        for (int i = 0; i < landUVLim; i++)
                            landUVS.Add(new Vector2(locScore, 0));
                        int oceanUVLim = newActiveNode.Get_oceanNode().GetComponent<MeshFilter>().sharedMesh.uv.Length;
                        for (int i = 0; i < oceanUVLim; i++)
                            oceanUVS.Add(new Vector2(locScore, 0));

                        newActiveNode.InjectUVs(landUVS.ToArray(), oceanUVS.ToArray(), actualLayers, actualRadii, in_grid);
                    }
                }

                activeNodes.Add(newActiveNode);
                activeMap[toActivate.x, toActivate.y] = true;
                in_feeder.RemoveFromActive(toActivate);
            }
            else
                in_feeder.RemoveFromActive(toActivate);
        }

        if (in_feeder.Get_newlyInactivePoints().Count > 0)
        {
            Point2D toDeactivate = in_feeder.Get_newlyInactivePoints()[0];
            int pos = 0;
            while (pos < activeNodes.Count)
            {
                if (activeNodes[pos].Get_location().IsEqualTo(toDeactivate))
                {
                    //activeNodes[pos].Disable();
                    //activeNodes.RemoveAt(pos);
                    //activeMap[toDeactivate.x, toDeactivate.y] = false;
                    LayeredSphereMeshNode toRemove = activeNodes[pos];
                    activeNodes.RemoveAt(pos);
                    toRemove.Disable();
                    activeMap[toDeactivate.x, toDeactivate.y] = false;
                    in_feeder.RemoveFromInactive(toDeactivate);
                    break;
                }
                pos++;
            }
            if (pos >= activeNodes.Count)
                in_feeder.RemoveFromInactive(toDeactivate);
        }
    }

    public void CleanupMesh(PointFeeder in_feeder)
    {
        int pos = 0;
        List<Point2D> currentActive = in_feeder.Get_currentActive();
        while (pos < activeNodes.Count)
        {
            Point2D activeLoc = activeNodes[pos].Get_location();
            int subPos = 0;
            bool found = false;
            while (subPos < currentActive.Count)
            {
                if (currentActive[subPos].IsEqualTo(activeLoc))
                {
                    found = true;
                    break;
                }
                subPos++;
            }
            if (!found)
            {
                LayeredSphereMeshNode toRemove = activeNodes[pos];
                activeNodes.RemoveAt(pos);
                toRemove.Disable();
                activeMap[activeLoc.x, activeLoc.y] = false;
            }
            pos++;
        }
    }

    public void UpdateLocalMesh(PointFeeder in_feeder, WorldGrid in_grid, GameData in_data)
    {
        int pos = 0;
        List<Point2D> active = in_feeder.Get_newlyActivePoints();
        List<Point2D> newInactive = in_feeder.Get_newlyInactivePoints();
        while (pos < active.Count)
        {
            // active pass, probs will be a funk
            Point2D loc = active[pos];
            if (!activeMap[loc.x, loc.y])
            {
                if (in_grid.Get_node(loc).IsLand())
                {
                    LayeredSphereMeshNode newActive = new LayeredSphereMeshNode(in_grid, loc, actualLayers, actualRadii, oceanUVMap, in_data, colorSettings);
                    newActive.Get_landNode().transform.parent = builtWorld.transform;
                    activeNodes.Add(newActive);
                    activeMap[loc.x, loc.y] = true;
                }

            }
            pos++;
        }

        pos = 0;
        while (pos < newInactive.Count)
        {
            int subPos = 0;
            Point2D inactive = newInactive[pos];

            while (subPos < activeNodes.Count)
            {
                Point2D loc = activeNodes[subPos].Get_location();
                if (loc.IsEqualTo(inactive))
                {
                    activeNodes[subPos].Disable();
                    activeNodes.RemoveAt(subPos);
                    activeMap[loc.x, loc.y] = false;
                    break;
                }
                subPos++;
            }
            pos++;
        }
    }

    private GameObject BuildWorldCoreMesh(World in_world, GameData in_data)
    {
        GameObject toRet = new GameObject();
        toRet.name = "World From Seed : " + in_data.settings.worldSeed;
        toRet.AddComponent<MeshRenderer>();
        toRet.GetComponent<MeshRenderer>().sharedMaterial = colorSettings.terrainMaterial;
        toRet.AddComponent<MeshFilter>();
        Mesh worldMesh = new Mesh();
        worldMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        WorldNode[,] nodes = in_world.Get_grid().Get_Nodes();
        int x = 0;
        int maxX = nodes.GetLength(0);
        int y = 1;
        int maxY = nodes.GetLength(1);
        int pos = 0;
        int lim = actualLayers.Length;
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        while (pos < lim)
        {
            float layerElev = actualLayers[pos];
            while (y < maxY - 1)
            {
                while (x < maxX)
                {
                    Point2D loc = new Point2D(x, y);
                    bool occluded = false;
                    if (nodes[loc.x, loc.y].Get_layerElev() != layerElev)
                        occluded = true;
                    float thisRadius = actualRadii[pos];
                    float lowRadius;
                    if (pos == 0)
                        lowRadius = actualRadii[0];
                    else
                        lowRadius = actualRadii[pos - 1];

                    // to modify the mesh generation so we can use surface shaders to visually debug regions in the game
                    // every node needs to have the same diamond at the center and the corners can be determined by the square
                    // Vertices are calculated for this layer
                    Vector3 center = GetSphericalVector(loc, thisRadius, nodes, in_data);
                    Vector3[] upperVertices = GetCloseVertices(loc, thisRadius, maxX, nodes, in_data);
                    Vector3[] lowerVertices = GetCloseVertices(loc, lowRadius, maxX, nodes, in_data);

                    // core verts always active, edges determined by the square configurations
                    // each node will need to have the 4 quadrants configuration determined and passed to vertsfromconfig
                    // vertices are then selected from the configuration determined by neighboring nodes.
                    WorldNode[] localNodes = GetLocalNodes(loc, nodes);
                    int[] configs = GetConfigs(layerElev, loc, localNodes, maxY);

                    // once we have the config we simply select from the predetermined positions
                    List<Vector3> locVerts = VertsFromConfigs(localNodes, upperVertices, lowerVertices, center, occluded, configs, layerElev);
                    verts.AddRange(locVerts);

                    List<int> locTris = TrisFromConfigs(verts.Count, occluded, configs, localNodes, layerElev);
                    tris.AddRange(locTris);

                    if (in_data.settings.smootheNormals)
                    {
                        List<Vector3> locNormals = NormalsFromConfigs(upperVertices, lowerVertices, center, occluded, configs, localNodes, layerElev);
                        normals.AddRange(locNormals);
                    }

                    List<Vector2> locUV = UVFromConfigs(thisRadius, occluded, configs, localNodes, layerElev);
                    //Debug.Log("UVs : " + locUV.Count + " / " + uv.Count + ":" + normals.Count);

                    uv.AddRange(locUV); // we will need to inject more uvs for vegetaion, terrain, snow coverage, and random offest
                    x++;
                }
                x = 0;
                y++;
            }
            x = 0;
            y = 1;
            pos++;
        }

        worldMesh.vertices = verts.ToArray();
        worldMesh.triangles = tris.ToArray();
        if (in_data.settings.smootheNormals)
            worldMesh.normals = normals.ToArray();
        else
            worldMesh.RecalculateNormals();
        worldMesh.uv = uv.ToArray();

        toRet.GetComponent<MeshFilter>().mesh = worldMesh;
        return toRet;
    }

    private WorldNode[] GetLocalNodes(Point2D in_loc, WorldNode[,] in_nodes)
    {
        Point2D[] peri = in_loc.GetValidatedPerimeter(new PointValidator(in_nodes));
        WorldNode[] toRet = new WorldNode[9];
        int pos = 0;
        while (pos < peri.Length)
        {
            toRet[pos] = in_nodes[peri[pos].x, peri[pos].y];
            pos++;
        }
        toRet[8] = in_nodes[in_loc.x, in_loc.y];
        return toRet;
    }

    private int[] GetConfigs(float in_layerElev, Point2D in_center, WorldNode[] in_nodes, int in_maxY)
    {
        // marker can delete
        int[] toRet = new int[4];
        int nw;
        int ne;
        int se;
        int sw;

        if (in_center.y == 0)
        {
            nw = ConfigFromNodes(in_layerElev, in_nodes[8], in_nodes[8], in_nodes[8], in_nodes[6]); // c c C w
            ne = ConfigFromNodes(in_layerElev, in_nodes[8], in_nodes[8], in_nodes[2], in_nodes[8]); // c c e C
            se = ConfigFromNodes(in_layerElev, in_nodes[8], in_nodes[2], in_nodes[3], in_nodes[4]); // C e se s
            sw = ConfigFromNodes(in_layerElev, in_nodes[6], in_nodes[8], in_nodes[4], in_nodes[5]); // w C s sw
        }
        else if ((in_center.y + 1) == in_maxY)
        {
            nw = ConfigFromNodes(in_layerElev, in_nodes[7], in_nodes[0], in_nodes[8], in_nodes[6]); // nw n C w
            ne = ConfigFromNodes(in_layerElev, in_nodes[0], in_nodes[1], in_nodes[2], in_nodes[8]); // n ne e C
            se = ConfigFromNodes(in_layerElev, in_nodes[8], in_nodes[2], in_nodes[8], in_nodes[8]); // C e c c
            sw = ConfigFromNodes(in_layerElev, in_nodes[6], in_nodes[8], in_nodes[8], in_nodes[8]); // w C c c
        }
        else
        {
            nw = ConfigFromNodes(in_layerElev, in_nodes[7], in_nodes[0], in_nodes[8], in_nodes[6]); // nw n C w
            ne = ConfigFromNodes(in_layerElev, in_nodes[0], in_nodes[1], in_nodes[2], in_nodes[8]); // n ne e C
            se = ConfigFromNodes(in_layerElev, in_nodes[8], in_nodes[2], in_nodes[3], in_nodes[4]); // C e se s
            sw = ConfigFromNodes(in_layerElev, in_nodes[6], in_nodes[8], in_nodes[4], in_nodes[5]); // w C s sw
        }

        toRet[0] = nw;
        toRet[1] = ne;
        toRet[2] = se;
        toRet[3] = sw;


        //toRet[0] = 3;
        //toRet[1] = 3;
        //toRet[2] = 3;
        //toRet[3] = 3;

        return toRet;
    }

    private int ConfigFromNodes(float in_layerElev, WorldNode in_nw, WorldNode in_ne, WorldNode in_se, WorldNode in_sw)
    {
        int configuration = 0;
        if ((NodeActive(in_layerElev, in_nw))/* && (in_nw.Get_layerElev() == in_layerElev)*/)
            configuration += 8;
        if ((NodeActive(in_layerElev, in_ne))/* && (in_ne.Get_layerElev() == in_layerElev)*/)
            configuration += 4;
        if ((NodeActive(in_layerElev, in_se))/* && (in_se.Get_layerElev() == in_layerElev)*/)
            configuration += 2;
        if ((NodeActive(in_layerElev, in_sw))/* && (in_sw.Get_layerElev() == in_layerElev)*/)
            configuration += 1;

        if ((configuration == 15) && ((NodeOccluded(in_layerElev, in_nw.Get_layerElev())) && (NodeOccluded(in_layerElev, in_ne.Get_layerElev())) && (NodeOccluded(in_layerElev, in_se.Get_layerElev())) && (NodeOccluded(in_layerElev, in_sw.Get_layerElev()))))
            configuration = 0;

        return configuration;
    }

    private bool NodeOccluded(float in_elev, float in_locLayerElev)
    {
        bool toRet = false;
        if (in_locLayerElev > in_elev)
            toRet = true;
        return toRet;
    }

    private bool NodeActive(float in_thisLayer, WorldNode in_node)
    {
        bool toRet = false;
        if (in_node.Get_layerElev() >= in_thisLayer)
            toRet = true;
        return toRet;
    }

    //private GameObject BuildOceanMesh(Game in_game)
    //{
    //    WorldGrid grid = in_game.Get_homeworld().Get_grid();
    //    WorldNode[,] nodes = grid.Get_Nodes();
    //    GameObject toRet = new GameObject();
    //    toRet.name = "World Oceans From Seed : " + worldSettings.worldSeed;
    //    toRet.AddComponent<MeshRenderer>();
    //    toRet.GetComponent<MeshRenderer>().sharedMaterial = colorSettings.OceanMaterial;
    //    toRet.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    //    toRet.AddComponent<MeshFilter>();
    //    Mesh oceanMesh = new Mesh();
    //    oceanMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    //    int x = 0;
    //    int maxX = nodes.GetLength(0);
    //    int y = 0;
    //    int maxY = nodes.GetLength(1);
    //    List<Vector3> verts = new List<Vector3>();
    //    List<int> tris = new List<int>();
    //    List<Vector3> normals = new List<Vector3>();
    //    List<Vector2> uv = new List<Vector2>();
    //    //float oceanElev = (float)grid.Get_oceanElevation();
    //    //float elev = oceanElev + worldSettings.sealevelOffset;
    //    //float adjusted = ((float)elev / (float)grid.Get_highest()) * (float)worldSettings.layers;
    //    //float layerElev = adjusted / (float)worldSettings.layers;
    //    //float actualElev = layerElev * worldSettings.elevScale;
    //    //float oceanRadius = actualElev - (elevMinMax.Min + (elevMinMax.Max - elevMinMax.Min) / 2.0f) + worldSettings.radius;
    //    //float seafloorRadius = actualRadii[0];
    //    int oceanPos = grid.Get_oceanPos();
    //    float oceanRadius = actualRadii[oceanPos];
    //    float seafloorRadius = actualRadii[0];
    //    float offsetRange = actualRadii[oceanPos + 1] - oceanRadius;
    //    float offset = offsetRange * worldSettings.sealevelOffset;
    //    finalOceanRadius = oceanRadius;
    //    Vector3[] lowerVertices = GetPossibleVerticesFromSphericalSquare(x, y, 0.0f, maxX, nodes);
    //    while (y < maxY)
    //    {
    //        while (x < maxX)
    //        {
    //            float currentRadius = nodes[x, y].Get_meshRadius();
    //            Vector3[] possibleVertices = GetPossibleVerticesFromSphericalSquare(x, y, oceanRadius + offset, maxX, nodes);
    //            List<Vector3> thisVerts = VertsFromConfig(possibleVertices, lowerVertices, 15);
    //            verts.AddRange(thisVerts);
    //            List<int> thisTris = TrisFromConfig(15, verts.Count);
    //            tris.AddRange(thisTris);
    //            if (worldSettings.smootheNormals)
    //            {
    //                List<Vector3> thisNormals = NormalsFromConfig(possibleVertices, lowerVertices, 15);
    //                normals.AddRange(thisNormals);
    //            }
    //            List<Vector2> thisUV = TimeUV(oceanRadius, seafloorRadius, nodes, x, y);
    //            uv.AddRange(thisUV);

    //            x++;
    //        }
    //        x = 0;
    //        y++;
    //    }
    //    colorSettings.Set_oceanRadius(oceanRadius + offset);
    //    oceanMesh.vertices = verts.ToArray();
    //    oceanMesh.triangles = tris.ToArray();
    //    if (worldSettings.smootheNormals)
    //        oceanMesh.normals = normals.ToArray();
    //    else
    //        oceanMesh.RecalculateNormals();
    //    oceanMesh.uv = uv.ToArray();
    //    toRet.GetComponent<MeshFilter>().mesh = oceanMesh;
    //    return toRet;
    //}

    //private GameObject BuildOceanMesh(World in_world)
    //{
    //    // here
    //    WorldGrid grid = in_world.Get_grid();
    //    WorldNode[,] nodes = grid.Get_Nodes();
    //    GameObject toRet = new GameObject();
    //    toRet.name = "World Oceans From Seed : " + worldSettings.worldSeed;
    //    toRet.AddComponent<MeshRenderer>();
    //    toRet.GetComponent<MeshRenderer>().sharedMaterial = colorSettings.OceanMaterial;
    //    toRet.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    //    toRet.AddComponent<MeshFilter>();
    //    Mesh oceanMesh = new Mesh();
    //    oceanMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    //    int x = 0;
    //    int maxX = nodes.GetLength(0);
    //    int y = 0;
    //    int maxY = nodes.GetLength(1);
    //    List<Vector3> verts = new List<Vector3>();
    //    List<int> tris = new List<int>();
    //    List<Vector3> normals = new List<Vector3>();
    //    List<Vector2> uv = new List<Vector2>();
    //    //float oceanElev = (float)grid.Get_oceanElevation();
    //    //float elev = oceanElev + worldSettings.sealevelOffset;
    //    //float adjusted = ((float)elev / (float)grid.Get_highest()) * (float)worldSettings.layers;
    //    //float layerElev = adjusted / (float)worldSettings.layers;
    //    //float actualElev = layerElev * worldSettings.elevScale;
    //    //float oceanRadius = actualElev - (elevMinMax.Min + (elevMinMax.Max - elevMinMax.Min) / 2.0f) + worldSettings.radius;
    //    //float seafloorRadius = actualRadii[0];
    //    int oceanPos = grid.Get_oceanPos();
    //    float oceanRadius = actualRadii[oceanPos];
    //    float seafloorRadius = actualRadii[0];
    //    float offsetRange = actualRadii[oceanPos + 1] - oceanRadius;
    //    float offset = offsetRange * worldSettings.sealevelOffset;
    //    finalOceanRadius = oceanRadius;
    //    Vector3[] lowerVertices = GetPossibleVerticesFromSphericalSquare(x, y, 0.0f, maxX, nodes);
    //    while (y < maxY)
    //    {
    //        while (x < maxX)
    //        {
    //            float currentRadius = nodes[x, y].Get_meshRadius();
    //            Vector3[] possibleVertices = GetPossibleVerticesFromSphericalSquare(x, y, oceanRadius + offset, maxX, nodes);
    //            List<Vector3> thisVerts = VertsFromConfig(possibleVertices, lowerVertices, 15);
    //            verts.AddRange(thisVerts);
    //            List<int> thisTris = TrisFromConfig(15, verts.Count);
    //            tris.AddRange(thisTris);
    //            if (worldSettings.smootheNormals)
    //            {
    //                List<Vector3> thisNormals = NormalsFromConfig(possibleVertices, lowerVertices, 15);
    //                normals.AddRange(thisNormals);
    //            }
    //            List<Vector2> thisUV = TimeUV(oceanRadius, seafloorRadius, nodes, x, y);
    //            uv.AddRange(thisUV);

    //            x++;
    //        }
    //        x = 0;
    //        y++;
    //    }
    //    colorSettings.Set_oceanRadius(oceanRadius + offset);
    //    oceanMesh.vertices = verts.ToArray();
    //    oceanMesh.triangles = tris.ToArray();
    //    if (worldSettings.smootheNormals)
    //        oceanMesh.normals = normals.ToArray();
    //    else
    //        oceanMesh.RecalculateNormals();
    //    oceanMesh.uv = uv.ToArray();
    //    toRet.GetComponent<MeshFilter>().mesh = oceanMesh;
    //    return toRet;
    //}

    // Vertices Section

    private List<Vector3> VertsFromConfigs(WorldNode[] in_localNodes, Vector3[] in_upperVertices, Vector3[] in_lowerVertices, Vector3 in_center, bool in_coreOcculuded, int[] in_configs, float in_layerElev)
    {
        List<Vector3> toRet = new List<Vector3>();

        // do core if not occluded
        if (!in_coreOcculuded)
            toRet.AddRange(CoreVerts(in_upperVertices, in_center));

        bool cornerOccluded = false;
        // then do quadrants
        // check marching square config to reference these seemingly random numbers
        if (Corner_NW(ref cornerOccluded, in_configs[0], in_layerElev, in_localNodes))
            toRet = Quadrant_NW(toRet, cornerOccluded, in_configs, in_upperVertices, in_lowerVertices);
        else if (in_configs[0] == 2)
            toRet = Diamond_NW(toRet, in_upperVertices, in_lowerVertices);

        if (Corner_NE(ref cornerOccluded, in_configs[1], in_layerElev, in_localNodes))
            toRet = Quadrant_NE(toRet, cornerOccluded, in_configs, in_upperVertices, in_lowerVertices);
        else if (in_configs[1] == 1)
            toRet = Diamond_NE(toRet, in_upperVertices, in_lowerVertices);

        if (Corner_SE(ref cornerOccluded, in_configs[2], in_layerElev, in_localNodes))
            toRet = Quadrant_SE(toRet, cornerOccluded, in_configs, in_upperVertices, in_lowerVertices);
        else if (in_configs[2] == 8)
            toRet = Diamond_SE(toRet, in_upperVertices, in_lowerVertices);

        if (Corner_SW(ref cornerOccluded, in_configs[3], in_layerElev, in_localNodes))
            toRet = Quadrant_SW(toRet, cornerOccluded, in_configs, in_upperVertices, in_lowerVertices);
        else if (in_configs[3] == 4)
            toRet = Diamond_SW(toRet, in_upperVertices, in_lowerVertices);

        return toRet;
    }

    // NW verts

    private List<Vector3> Quadrant_NW(List<Vector3> in_vertices, bool in_cornerOccluded, int[] in_configs, Vector3[] in_upperVertices, Vector3[] in_lowerVertices)
    {
        List<Vector3> toRet = in_vertices;
        // nw quadrant but we want SE active
        Vector3 a = in_upperVertices[0];
        Vector3 b = in_upperVertices[6];
        Vector3 c = in_upperVertices[7];
        if (!in_cornerOccluded)
        {
            toRet.Add(a);
            toRet.Add(b);
            toRet.Add(c);
        }

        Vector3 d = in_lowerVertices[0];
        Vector3 e = in_lowerVertices[6];
        Vector3 f = in_lowerVertices[7];

        switch (in_configs[0])
        {
            case 2:
                // upper
                toRet.Add(a);
                toRet.Add(b);
                toRet.Add(e);
                // lower
                toRet.Add(a);
                toRet.Add(e);
                toRet.Add(d);
                break;
            case 3:
                // upper
                toRet.Add(a);
                toRet.Add(c);
                toRet.Add(f);
                // lower
                toRet.Add(a);
                toRet.Add(f);
                toRet.Add(d);
                break;
            case 6:
                // upper
                toRet.Add(c);
                toRet.Add(b);
                toRet.Add(e);
                // lower
                toRet.Add(c);
                toRet.Add(e);
                toRet.Add(f);
                break;
            case 5:
            case 13:
                // upper
                toRet.Add(b);
                toRet.Add(a);
                toRet.Add(d);
                // lower
                toRet.Add(b);
                toRet.Add(d);
                toRet.Add(e);
                break;
            default:
                break;
        }
        return toRet;
    }

    private List<Vector3> Diamond_NW(List<Vector3> in_vertices, Vector3[] in_upperVertices, Vector3[] in_lowerVertices)
    {
        List<Vector3> toRet = in_vertices;

        Vector3 a = in_upperVertices[0];
        Vector3 b = in_upperVertices[6];

        Vector3 d = in_lowerVertices[0];
        Vector3 e = in_lowerVertices[6];

        // upper
        toRet.Add(a);
        toRet.Add(b);
        toRet.Add(e);
        // lower
        toRet.Add(a);
        toRet.Add(e);
        toRet.Add(d);
        return toRet;
    }

    private bool Corner_NW(ref bool occluded, int in_config, float in_layerElev, WorldNode[] in_nodes)
    {
        bool toRet;
        occluded = false;
        float nw = in_nodes[7].Get_layerElev();
        float n = in_nodes[0].Get_layerElev();
        float c = in_nodes[8].Get_layerElev();
        float w = in_nodes[6].Get_layerElev();
        switch (in_config)
        {
            case 3:
                toRet = true;
                if ((w > in_layerElev) && (c > in_layerElev))
                    occluded = true;
                break;
            case 5:
                toRet = true;
                if ((w > in_layerElev) && (n > in_layerElev))
                    occluded = true;
                break;
            case 6:
                toRet = true;
                if ((n > in_layerElev) && (c > in_layerElev))
                    occluded = true;
                break;
            case 7:
                toRet = true;
                if (((n > in_layerElev) && (c > in_layerElev)) ||
                    ((n > in_layerElev) && (w > in_layerElev)) ||
                    ((w > in_layerElev) && (c > in_layerElev)) ||
                    ((n > in_layerElev) && (c > in_layerElev) && (w > in_layerElev)))
                    occluded = true;
                break;
            case 10:
                toRet = true;
                if ((c > in_layerElev) && (nw > in_layerElev))
                    occluded = true;
                break;
            case 11:
                toRet = true;
                if (((nw > in_layerElev) && (c > in_layerElev)) ||
                    ((w > in_layerElev) && (c > in_layerElev)) ||
                    ((nw > in_layerElev) && (c > in_layerElev) && (w > in_layerElev)))
                    occluded = true;
                break;
            case 13:
                toRet = true;
                if (((n > in_layerElev) && (w > in_layerElev)) ||
                    ((nw > in_layerElev) && (n > in_layerElev) && (w > in_layerElev)))
                    occluded = true;
                break;
            case 14:
                toRet = true;
                if (((n > in_layerElev) && (c > in_layerElev)) ||
                    ((nw > in_layerElev) && (c > in_layerElev)) ||
                    ((nw > in_layerElev) && (c > in_layerElev) && (n > in_layerElev)))
                    occluded = true;
                break;
            case 15:
                toRet = true;
                if (((n > in_layerElev) && (c > in_layerElev)) ||
                    ((n > in_layerElev) && (w > in_layerElev)) ||
                    ((w > in_layerElev) && (c > in_layerElev)) ||
                    ((nw > in_layerElev) && (c > in_layerElev)) ||
                    ((n > in_layerElev) && (c > in_layerElev) && (w > in_layerElev)) ||
                    ((nw > in_layerElev) && (c > in_layerElev) && (w > in_layerElev)) ||
                    ((nw > in_layerElev) && (n > in_layerElev) && (w > in_layerElev)) ||
                    ((nw > in_layerElev) && (n > in_layerElev) && (c > in_layerElev)) ||
                    ((nw > in_layerElev) && (n > in_layerElev) && (c > in_layerElev) && (w > in_layerElev)))
                    occluded = true;
                break;
            default:
                toRet = false;
                break;
        }
        //if (occluded)
        //    toRet = false;
        return toRet;
    }

    // NE verts

    private List<Vector3> Quadrant_NE(List<Vector3> in_vertices, bool in_cornerOccluded, int[] in_configs, Vector3[] in_upperVertices, Vector3[] in_lowerVertices)
    {
        List<Vector3> toRet = in_vertices;
        // ne quadrant but we want SW active
        Vector3 a = in_upperVertices[0];
        Vector3 b = in_upperVertices[1];
        Vector3 c = in_upperVertices[2];
        if (!in_cornerOccluded)
        {
            toRet.Add(a);
            toRet.Add(b);
            toRet.Add(c);
        }


        Vector3 d = in_lowerVertices[0];
        Vector3 e = in_lowerVertices[1];
        Vector3 f = in_lowerVertices[2];

        switch (in_configs[1])
        {
            case 3:
                // upper
                toRet.Add(b);
                toRet.Add(a);
                toRet.Add(e);
                // lower
                toRet.Add(e);
                toRet.Add(a);
                toRet.Add(d);
                break;
            case 9:
                // upper
                toRet.Add(c);
                toRet.Add(b);
                toRet.Add(e);
                // lower
                toRet.Add(c);
                toRet.Add(e);
                toRet.Add(f);
                break;
            case 10:
            case 14:
                // upper
                toRet.Add(a);
                toRet.Add(c);
                toRet.Add(f);
                // lower
                toRet.Add(a);
                toRet.Add(f);
                toRet.Add(d);
                break;
            default:
                break;
        }
        return toRet;
    }

    private List<Vector3> Diamond_NE(List<Vector3> in_vertices, Vector3[] in_upperVertices, Vector3[] in_lowerVertices)
    {
        List<Vector3> toRet = in_vertices;

        Vector3 a = in_upperVertices[0];
        Vector3 c = in_upperVertices[2];
        Vector3 d = in_lowerVertices[0];
        Vector3 f = in_lowerVertices[2];
        // upper
        toRet.Add(c);
        toRet.Add(a);
        toRet.Add(d);
        // lower
        toRet.Add(c);
        toRet.Add(d);
        toRet.Add(f);
        return toRet;
    }

    private bool Corner_NE(ref bool occluded, int in_config, float in_layerElev, WorldNode[] in_nodes)
    {
        bool toRet;
        occluded = false;
        float n = in_nodes[0].Get_layerElev();
        float ne = in_nodes[1].Get_layerElev();
        float e = in_nodes[2].Get_layerElev();
        float c = in_nodes[8].Get_layerElev();
        switch (in_config)
        {
            case 3:
                toRet = true;
                if ((c > in_layerElev) && (e > in_layerElev))
                    occluded = true;
                break;
            case 5:
                toRet = true;
                if ((c > in_layerElev) && (ne > in_layerElev))
                    occluded = true;
                break;
            case 7:
                toRet = true;
                if (((e > in_layerElev) && (c > in_layerElev)) ||
                    ((ne > in_layerElev) && (c > in_layerElev)) ||
                    ((ne > in_layerElev) && (c > in_layerElev) && (e > in_layerElev)))
                    occluded = true;
                break;
            case 9:
                toRet = true;
                if ((c > in_layerElev) && (n > in_layerElev))
                    occluded = true;
                break;
            case 10:
                toRet = true;
                if ((n > in_layerElev) && (e > in_layerElev))
                    occluded = true;
                break;
            case 11:
                toRet = true;
                if (((n > in_layerElev) && (c > in_layerElev)) ||
                    ((n > in_layerElev) && (e > in_layerElev)) ||
                    ((e > in_layerElev) && (c > in_layerElev)) ||
                    ((n > in_layerElev) && (c > in_layerElev) && (e > in_layerElev)))
                    occluded = true;
                break;
            case 13:
                toRet = true;
                if (((n > in_layerElev) && (c > in_layerElev)) ||
                    ((ne > in_layerElev) && (c > in_layerElev)) ||
                    ((n > in_layerElev) && (c > in_layerElev) && (ne > in_layerElev)))
                    occluded = true;
                break;
            case 14:
                toRet = true;
                if (((n > in_layerElev) && (e > in_layerElev)) ||
                    ((n > in_layerElev) && (e > in_layerElev) && (ne > in_layerElev)))
                    occluded = true;
                break;
            case 15:
                toRet = true;
                if (((n > in_layerElev) && (c > in_layerElev)) ||
                    ((n > in_layerElev) && (e > in_layerElev)) ||
                    ((e > in_layerElev) && (c > in_layerElev)) ||
                    ((ne > in_layerElev) && (c > in_layerElev)) ||
                    ((n > in_layerElev) && (c > in_layerElev) && (e > in_layerElev)) ||
                    ((ne > in_layerElev) && (e > in_layerElev) && (c > in_layerElev)) ||
                    ((ne > in_layerElev) && (n > in_layerElev) && (e > in_layerElev)) ||
                    ((ne > in_layerElev) && (n > in_layerElev) && (c > in_layerElev)) ||
                    ((ne > in_layerElev) && (n > in_layerElev) && (c > in_layerElev) && (e > in_layerElev)))
                    occluded = true;
                break;
            default:
                toRet = false;
                break;
        }

        //if (occluded)
        //    toRet = false;

        return toRet;
    }

    // SE verts

    private List<Vector3> Quadrant_SE(List<Vector3> in_vertices, bool in_cornerOccluded, int[] in_configs, Vector3[] in_upperVertices, Vector3[] in_lowerVertices)
    {
        List<Vector3> toRet = in_vertices;
        // se quadrant but we want NW active
        Vector3 a = in_upperVertices[2];
        Vector3 b = in_upperVertices[3];
        Vector3 c = in_upperVertices[4];
        if (!in_cornerOccluded)
        {
            toRet.Add(a);
            toRet.Add(b);
            toRet.Add(c);
        }

        Vector3 d = in_lowerVertices[2];
        Vector3 e = in_lowerVertices[3];
        Vector3 f = in_lowerVertices[4];

        switch (in_configs[2])
        {
            case 9:
                // upper
                toRet.Add(b);
                toRet.Add(a);
                toRet.Add(d);
                // lower
                toRet.Add(b);
                toRet.Add(d);
                toRet.Add(e);
                break;
            case 12:
                // upper
                toRet.Add(c);
                toRet.Add(b);
                toRet.Add(e);
                // lower
                toRet.Add(c);
                toRet.Add(e);
                toRet.Add(f);
                break;
            case 5:
            case 7:
                // upper
                toRet.Add(a);
                toRet.Add(c);
                toRet.Add(f);
                // lower
                toRet.Add(a);
                toRet.Add(f);
                toRet.Add(d);
                break;
            default:
                break;
        }
        return toRet;
    }

    private List<Vector3> Diamond_SE(List<Vector3> in_vertices, Vector3[] in_upperVertices, Vector3[] in_lowerVertices)
    {
        List<Vector3> toRet = in_vertices;

        Vector3 a = in_upperVertices[2];
        Vector3 c = in_upperVertices[4];

        Vector3 d = in_lowerVertices[2];
        Vector3 f = in_lowerVertices[4];
        // upper
        toRet.Add(c);
        toRet.Add(a);
        toRet.Add(d);
        // lower
        toRet.Add(c);
        toRet.Add(d);
        toRet.Add(f);

        return toRet;
    }

    private bool Corner_SE(ref bool occluded, int in_config, float in_layerElev, WorldNode[] in_nodes)
    {
        bool toRet;
        occluded = false;
        float c = in_nodes[8].Get_layerElev();
        float e = in_nodes[2].Get_layerElev();
        float se = in_nodes[3].Get_layerElev();
        float s = in_nodes[4].Get_layerElev();
        switch (in_config)
        {
            case 5:
                toRet = true;
                if ((e > in_layerElev) && (s > in_layerElev))
                    occluded = true;
                break;
            case 7:
                toRet = true;
                if (((e > in_layerElev) && (s > in_layerElev)) ||
                    ((e > in_layerElev) && (se > in_layerElev) && (s > in_layerElev)))
                    occluded = true;
                break;
            case 9:
                toRet = true;
                if ((c > in_layerElev) && (s > in_layerElev))
                    occluded = true;
                break;
            case 10:
                toRet = true;
                if ((c > in_layerElev) && (se > in_layerElev))
                    occluded = true;
                break;
            case 11:
                toRet = true;
                if (((c > in_layerElev) && (s > in_layerElev)) ||
                    ((c > in_layerElev) && (se > in_layerElev)) ||
                    ((c > in_layerElev) && (s > in_layerElev) && (se > in_layerElev)))
                    occluded = true;
                break;
            case 12:
                toRet = true;
                if ((c > in_layerElev) && (e > in_layerElev))
                    occluded = true;
                break;
            case 13:
                toRet = true;
                if (((c > in_layerElev) && (e > in_layerElev)) ||
                    ((c > in_layerElev) && (s > in_layerElev)) ||
                    ((s > in_layerElev) && (e > in_layerElev)) ||
                    ((c > in_layerElev) && (e > in_layerElev) && (s > in_layerElev)))
                    occluded = true;
                break;
            case 14:
                toRet = true;
                if (((c > in_layerElev) && (e > in_layerElev)) ||
                    ((c > in_layerElev) && (se > in_layerElev)) ||
                    ((c > in_layerElev) && (e > in_layerElev) && (s > in_layerElev)))
                    occluded = true;
                break;
            case 15:
                toRet = true;
                if (((c > in_layerElev) && (e > in_layerElev)) ||
                    ((c > in_layerElev) && (s > in_layerElev)) ||
                    ((e > in_layerElev) && (s > in_layerElev)) ||
                    ((se > in_layerElev) && (c > in_layerElev)) ||
                    ((s > in_layerElev) && (c > in_layerElev) && (e > in_layerElev)) ||
                    ((se > in_layerElev) && (e > in_layerElev) && (c > in_layerElev)) ||
                    ((se > in_layerElev) && (s > in_layerElev) && (e > in_layerElev)) ||
                    ((se > in_layerElev) && (s > in_layerElev) && (c > in_layerElev)) ||
                    ((se > in_layerElev) && (s > in_layerElev) && (c > in_layerElev) && (e > in_layerElev)))
                    occluded = true;
                break;
            default:
                toRet = false;
                break;
        }

        //if (occluded)
        //    toRet = false;

        return toRet;
    }

    // SE verts

    private List<Vector3> Quadrant_SW(List<Vector3> in_vertices, bool in_cornerOccluded, int[] in_configs, Vector3[] in_upperVertices, Vector3[] in_lowerVertices)
    {
        List<Vector3> toRet = in_vertices;
        // sw quadrant but we want NE active
        Vector3 a = in_upperVertices[6];
        Vector3 b = in_upperVertices[4];
        Vector3 c = in_upperVertices[5];
        if (!in_cornerOccluded)
        {
            toRet.Add(a);
            toRet.Add(b);
            toRet.Add(c);
        }

        Vector3 d = in_lowerVertices[6];
        Vector3 e = in_lowerVertices[4];
        Vector3 f = in_lowerVertices[5];

        switch (in_configs[3])
        {
            case 6:
                // upper
                toRet.Add(a);
                toRet.Add(c);
                toRet.Add(f);
                // lower
                toRet.Add(a);
                toRet.Add(f);
                toRet.Add(d);
                break;
            case 12:
                // upper
                toRet.Add(c);
                toRet.Add(b);
                toRet.Add(e);
                // lower
                toRet.Add(c);
                toRet.Add(e);
                toRet.Add(f);
                break;
            case 10:
            case 11:
                // upper
                toRet.Add(b);
                toRet.Add(a);
                toRet.Add(d);
                // lower
                toRet.Add(b);
                toRet.Add(d);
                toRet.Add(e);
                break;
            default:
                break;
        }
        return toRet;
    }

    private List<Vector3> Diamond_SW(List<Vector3> in_vertices, Vector3[] in_upperVertices, Vector3[] in_lowerVertices)
    {
        List<Vector3> toRet = in_vertices;

        // sw quadrant but we want NE active
        Vector3 a = in_upperVertices[6];
        Vector3 b = in_upperVertices[4];

        Vector3 d = in_lowerVertices[6];
        Vector3 e = in_lowerVertices[4];
        // upper
        toRet.Add(a);
        toRet.Add(b);
        toRet.Add(e);
        // lower
        toRet.Add(a);
        toRet.Add(e);
        toRet.Add(d);

        return toRet;
    }

    private bool Corner_SW(ref bool occluded, int in_config, float in_layerElev, WorldNode[] in_nodes)
    {
        bool toRet;
        occluded = false;
        float w = in_nodes[6].Get_layerElev();
        float c = in_nodes[8].Get_layerElev();
        float s = in_nodes[4].Get_layerElev();
        float sw = in_nodes[5].Get_layerElev();
        switch (in_config)
        {
            case 5:
                toRet = true;
                if ((c > in_layerElev) && (sw > in_layerElev))
                    occluded = true;
                break;
            case 6:
                toRet = true;
                if ((c > in_layerElev) && (s > in_layerElev))
                    occluded = true;
                break;
            case 7:
                toRet = true;
                if (((c > in_layerElev) && (s > in_layerElev)) ||
                    ((c > in_layerElev) && (sw > in_layerElev)) ||
                    ((c > in_layerElev) && (s > in_layerElev) && (sw > in_layerElev)))
                    occluded = true;
                break;
            case 10:
                toRet = true;
                if ((s > in_layerElev) && (w > in_layerElev))
                    occluded = true;
                break;
            case 11:
                toRet = true;
                if (((w > in_layerElev) && (s > in_layerElev)) ||
                    ((w > in_layerElev) && (s > in_layerElev) && (sw > in_layerElev)))
                    occluded = true;
                break;
            case 12:
                toRet = true;
                if ((c > in_layerElev) && (w > in_layerElev))
                    occluded = true;
                break;
            case 13:
                toRet = true;
                if (((c > in_layerElev) && (w > in_layerElev)) ||
                    ((c > in_layerElev) && (sw > in_layerElev)) ||
                    ((c > in_layerElev) && (w > in_layerElev) && (sw > in_layerElev)))
                    occluded = true;
                break;
            case 14:
                toRet = true;
                toRet = true;
                if (((c > in_layerElev) && (w > in_layerElev)) ||
                    ((c > in_layerElev) && (s > in_layerElev)) ||
                    ((s > in_layerElev) && (w > in_layerElev)) ||
                    ((c > in_layerElev) && (s > in_layerElev) && (w > in_layerElev)))
                    occluded = true;
                break;
            case 15:
                toRet = true;
                if (((c > in_layerElev) && (w > in_layerElev)) ||
                    ((c > in_layerElev) && (s > in_layerElev)) ||
                    ((w > in_layerElev) && (s > in_layerElev)) ||
                    ((sw > in_layerElev) && (c > in_layerElev)) ||
                    ((s > in_layerElev) && (c > in_layerElev) && (w > in_layerElev)) ||
                    ((sw > in_layerElev) && (w > in_layerElev) && (c > in_layerElev)) ||
                    ((sw > in_layerElev) && (s > in_layerElev) && (w > in_layerElev)) ||
                    ((sw > in_layerElev) && (s > in_layerElev) && (c > in_layerElev)) ||
                    ((sw > in_layerElev) && (s > in_layerElev) && (c > in_layerElev) && (w > in_layerElev)))
                    occluded = true;
                break;
            default:
                toRet = false;
                break;
        }

        //if (occluded)
        //    toRet = false;

        return toRet;
    }

    private Vector3[] CoreVerts(Vector3[] in_upperVertices, Vector3 in_center)
    {
        // cores are always the same
        Vector3[] toRet = new Vector3[12];
        // nw tris 
        toRet[0] = in_upperVertices[0];
        toRet[1] = in_center;
        toRet[2] = in_upperVertices[6];
        // ne tris
        toRet[3] = in_upperVertices[0];
        toRet[4] = in_upperVertices[2];
        toRet[5] = in_center;
        // se tris
        toRet[6] = in_center;
        toRet[7] = in_upperVertices[2];
        toRet[8] = in_upperVertices[4];
        // sw tris
        toRet[9] = in_upperVertices[6];
        toRet[10] = in_center;
        toRet[11] = in_upperVertices[4];
        return toRet;
    }

    private List<Vector3> VertsFromConfig(Vector3[] in_possibleVertices, Vector3[] in_lowerVertices, int in_config)
    {
        List<Vector3> toRet = new List<Vector3>();
        switch (in_config)
        {
            case 1:
                toRet.Add(in_possibleVertices[7]);
                toRet.Add(in_possibleVertices[5]);
                toRet.Add(in_possibleVertices[6]);
                toRet.Add(in_possibleVertices[7]);
                toRet.Add(in_possibleVertices[5]);
                toRet.Add(in_lowerVertices[5]);
                toRet.Add(in_lowerVertices[7]);
                break;
            case 2:
                toRet.Add(in_possibleVertices[3]);
                toRet.Add(in_possibleVertices[4]);
                toRet.Add(in_possibleVertices[5]);
                toRet.Add(in_possibleVertices[3]);
                toRet.Add(in_possibleVertices[5]);
                toRet.Add(in_lowerVertices[5]);
                toRet.Add(in_lowerVertices[3]);
                break;
            case 3:
                toRet.Add(in_possibleVertices[3]);
                toRet.Add(in_possibleVertices[4]);
                toRet.Add(in_possibleVertices[6]);
                toRet.Add(in_possibleVertices[7]);
                toRet.Add(in_possibleVertices[3]);
                toRet.Add(in_possibleVertices[7]);
                toRet.Add(in_lowerVertices[7]);
                toRet.Add(in_lowerVertices[3]);
                break;
            case 4:
                toRet.Add(in_possibleVertices[1]);
                toRet.Add(in_possibleVertices[2]);
                toRet.Add(in_possibleVertices[3]);
                toRet.Add(in_possibleVertices[1]);
                toRet.Add(in_possibleVertices[3]);
                toRet.Add(in_lowerVertices[3]);
                toRet.Add(in_lowerVertices[1]);
                break;
            case 5:
                toRet.Add(in_possibleVertices[1]);
                toRet.Add(in_possibleVertices[2]);
                toRet.Add(in_possibleVertices[3]);
                toRet.Add(in_possibleVertices[5]);
                toRet.Add(in_possibleVertices[6]);
                toRet.Add(in_possibleVertices[7]);
                toRet.Add(in_possibleVertices[1]);
                toRet.Add(in_possibleVertices[7]);
                toRet.Add(in_lowerVertices[7]);
                toRet.Add(in_lowerVertices[1]);
                toRet.Add(in_possibleVertices[5]);
                toRet.Add(in_possibleVertices[3]);
                toRet.Add(in_lowerVertices[5]);
                toRet.Add(in_lowerVertices[3]);
                break;
            case 6:
                toRet.Add(in_possibleVertices[1]);
                toRet.Add(in_possibleVertices[2]);
                toRet.Add(in_possibleVertices[4]);
                toRet.Add(in_possibleVertices[5]);
                toRet.Add(in_possibleVertices[1]);
                toRet.Add(in_possibleVertices[5]);
                toRet.Add(in_lowerVertices[5]);
                toRet.Add(in_lowerVertices[1]);
                break;
            case 7:
                toRet.Add(in_possibleVertices[1]);
                toRet.Add(in_possibleVertices[2]);
                toRet.Add(in_possibleVertices[4]);
                toRet.Add(in_possibleVertices[6]);
                toRet.Add(in_possibleVertices[7]);
                toRet.Add(in_possibleVertices[1]);
                toRet.Add(in_possibleVertices[7]);
                toRet.Add(in_lowerVertices[7]);
                toRet.Add(in_lowerVertices[1]);
                break;
            case 8:
                toRet.Add(in_possibleVertices[0]);
                toRet.Add(in_possibleVertices[1]);
                toRet.Add(in_possibleVertices[7]);
                toRet.Add(in_possibleVertices[1]);
                toRet.Add(in_possibleVertices[7]);
                toRet.Add(in_lowerVertices[7]);
                toRet.Add(in_lowerVertices[1]);
                break;
            case 9:
                toRet.Add(in_possibleVertices[0]);
                toRet.Add(in_possibleVertices[1]);
                toRet.Add(in_possibleVertices[5]);
                toRet.Add(in_possibleVertices[6]);
                toRet.Add(in_possibleVertices[1]);
                toRet.Add(in_possibleVertices[5]);
                toRet.Add(in_lowerVertices[5]);
                toRet.Add(in_lowerVertices[1]);
                break;
            case 10:
                toRet.Add(in_possibleVertices[0]);
                toRet.Add(in_possibleVertices[1]);
                toRet.Add(in_possibleVertices[3]);
                toRet.Add(in_possibleVertices[4]);
                toRet.Add(in_possibleVertices[5]);
                toRet.Add(in_possibleVertices[7]);
                toRet.Add(in_possibleVertices[1]);
                toRet.Add(in_possibleVertices[3]);
                toRet.Add(in_lowerVertices[3]);
                toRet.Add(in_lowerVertices[1]);
                toRet.Add(in_possibleVertices[5]);
                toRet.Add(in_possibleVertices[7]);
                toRet.Add(in_lowerVertices[7]);
                toRet.Add(in_lowerVertices[5]);
                break;
            case 11:
                toRet.Add(in_possibleVertices[0]);
                toRet.Add(in_possibleVertices[1]);
                toRet.Add(in_possibleVertices[3]);
                toRet.Add(in_possibleVertices[4]);
                toRet.Add(in_possibleVertices[6]);
                toRet.Add(in_possibleVertices[1]);
                toRet.Add(in_possibleVertices[3]);
                toRet.Add(in_lowerVertices[3]);
                toRet.Add(in_lowerVertices[1]);
                break;
            case 12:
                toRet.Add(in_possibleVertices[0]);
                toRet.Add(in_possibleVertices[2]);
                toRet.Add(in_possibleVertices[3]);
                toRet.Add(in_possibleVertices[7]);
                toRet.Add(in_possibleVertices[3]);
                toRet.Add(in_possibleVertices[7]);
                toRet.Add(in_lowerVertices[7]);
                toRet.Add(in_lowerVertices[3]);
                break;
            case 13:
                toRet.Add(in_possibleVertices[0]);
                toRet.Add(in_possibleVertices[2]);
                toRet.Add(in_possibleVertices[3]);
                toRet.Add(in_possibleVertices[5]);
                toRet.Add(in_possibleVertices[6]);
                toRet.Add(in_possibleVertices[3]);
                toRet.Add(in_possibleVertices[5]);
                toRet.Add(in_lowerVertices[5]);
                toRet.Add(in_lowerVertices[3]);
                break;
            case 14:
                toRet.Add(in_possibleVertices[0]);
                toRet.Add(in_possibleVertices[2]);
                toRet.Add(in_possibleVertices[4]);
                toRet.Add(in_possibleVertices[5]);
                toRet.Add(in_possibleVertices[7]);
                toRet.Add(in_possibleVertices[5]);
                toRet.Add(in_possibleVertices[7]);
                toRet.Add(in_lowerVertices[7]);
                toRet.Add(in_lowerVertices[5]);
                break;
            case 15:
                toRet.Add(in_possibleVertices[0]);
                toRet.Add(in_possibleVertices[2]);
                toRet.Add(in_possibleVertices[4]);
                toRet.Add(in_possibleVertices[6]);
                break;
            default:
                break;
        }
        return toRet;
    }

    // tris section

    private List<int> TrisFromConfigs(int in_count, bool in_coreOccluded, int[] in_configs, WorldNode[] in_nodes, float in_layerElev)
    {
        // please let this be a functional simplification. IT IS!
        List<int> toRet = new List<int>();
        int offset = GetCountOffset(in_coreOccluded, in_configs, in_nodes, in_layerElev);
        int count = in_count - offset;
        int pos = 0;
        while (pos < offset)
        {
            toRet.Add(count);
            count++;
            pos++;
        }
        return toRet;
    }

    private int GetCountOffset(bool in_coreOccluded, int[] in_configs, WorldNode[] in_localNodes, float in_layerElev)
    {
        int toRet = 0;
        if (!in_coreOccluded)
            toRet += 12;

        bool cornerOccluded = false;

        if (Corner_NW(ref cornerOccluded, in_configs[0], in_layerElev, in_localNodes))
        {
            // nw quadrant but we want se active
            if (!cornerOccluded)
                toRet += 3;
            if ((in_configs[0] == 3) ||
                (in_configs[0] == 5) ||
                (in_configs[0] == 6) ||
                (in_configs[0] == 13))
                toRet += 6;
        }
        else if (in_configs[0] == 2)
            toRet += 6;

        if (Corner_NE(ref cornerOccluded, in_configs[1], in_layerElev, in_localNodes))
        {
            // ne quadrant but we want sw active
            if (!cornerOccluded)
                toRet += 3;
            if ((in_configs[1] == 3) ||
                (in_configs[1] == 9) ||
                (in_configs[1] == 10) ||
                (in_configs[1] == 14))
                toRet += 6;
        }
        else if (in_configs[1] == 1)
            toRet += 6;

        if (Corner_SE(ref cornerOccluded, in_configs[2], in_layerElev, in_localNodes))
        {
            // se quadrant but we want nw active
            if (!cornerOccluded)
                toRet += 3;
            if ((in_configs[2] == 5) ||
                (in_configs[2] == 7) ||
                (in_configs[2] == 9) ||
                (in_configs[2] == 12))
                toRet += 6;
        }
        else if (in_configs[2] == 8)
            toRet += 6;

        if (Corner_SW(ref cornerOccluded, in_configs[3], in_layerElev, in_localNodes))
        {
            // sw quadrant but we want ne active
            if (!cornerOccluded)
                toRet += 3;
            if ((in_configs[3] == 6) ||
                (in_configs[3] == 10) ||
                (in_configs[3] == 11) ||
                (in_configs[3] == 12))
                toRet += 6;
        }
        else if (in_configs[3] == 4)
            toRet += 6;

        return toRet;
    }

    private List<int> TrisFromConfig(int in_config, int in_count)
    {
        List<int> toRet = new List<int>();
        int pos;
        switch (in_config)
        {
            case 1:
                pos = in_count - 7;
                toRet.Add(pos);
                toRet.Add(pos + 2);
                toRet.Add(pos + 1);
                toRet.Add(pos + 3);
                toRet.Add(pos + 4);
                toRet.Add(pos + 5);
                toRet.Add(pos + 3);
                toRet.Add(pos + 5);
                toRet.Add(pos + 6);
                break;
            case 2:
                pos = in_count - 7;
                toRet.Add(pos);
                toRet.Add(pos + 2);
                toRet.Add(pos + 1);
                toRet.Add(pos + 3);
                toRet.Add(pos + 5);
                toRet.Add(pos + 4);
                toRet.Add(pos + 3);
                toRet.Add(pos + 6);
                toRet.Add(pos + 5);
                break;
            case 3:
                pos = in_count - 8;
                toRet.Add(pos);
                toRet.Add(pos + 2);
                toRet.Add(pos + 1);
                toRet.Add(pos);
                toRet.Add(pos + 3);
                toRet.Add(pos + 2);
                toRet.Add(pos + 4);
                toRet.Add(pos + 6);
                toRet.Add(pos + 5);
                toRet.Add(pos + 4);
                toRet.Add(pos + 7);
                toRet.Add(pos + 6);
                break;
            case 4:
                pos = in_count - 7;
                toRet.Add(pos);
                toRet.Add(pos + 2);
                toRet.Add(pos + 1);
                toRet.Add(pos + 3);
                toRet.Add(pos + 5);
                toRet.Add(pos + 4);
                toRet.Add(pos + 3);
                toRet.Add(pos + 6);
                toRet.Add(pos + 5);
                break;
            case 5:
                pos = in_count - 14;
                toRet.Add(pos);
                toRet.Add(pos + 2);
                toRet.Add(pos + 1);
                toRet.Add(pos);
                toRet.Add(pos + 3);
                toRet.Add(pos + 2);
                toRet.Add(pos);
                toRet.Add(pos + 4);
                toRet.Add(pos + 3);
                toRet.Add(pos);
                toRet.Add(pos + 5);
                toRet.Add(pos + 4);
                toRet.Add(pos + 6);
                toRet.Add(pos + 8);
                toRet.Add(pos + 7);
                toRet.Add(pos + 6);
                toRet.Add(pos + 9);
                toRet.Add(pos + 8);
                toRet.Add(pos + 10);
                toRet.Add(pos + 12);
                toRet.Add(pos + 11);
                toRet.Add(pos + 11);
                toRet.Add(pos + 12);
                toRet.Add(pos + 13);
                break;
            case 6:
                pos = in_count - 8;
                toRet.Add(pos);
                toRet.Add(pos + 2);
                toRet.Add(pos + 1);
                toRet.Add(pos);
                toRet.Add(pos + 3);
                toRet.Add(pos + 2);
                toRet.Add(pos + 4);
                toRet.Add(pos + 6);
                toRet.Add(pos + 5);
                toRet.Add(pos + 4);
                toRet.Add(pos + 7);
                toRet.Add(pos + 6);
                break;
            case 7:
                pos = in_count - 9;
                toRet.Add(pos);
                toRet.Add(pos + 2);
                toRet.Add(pos + 1);
                toRet.Add(pos);
                toRet.Add(pos + 3);
                toRet.Add(pos + 2);
                toRet.Add(pos);
                toRet.Add(pos + 4);
                toRet.Add(pos + 3);
                toRet.Add(pos + 5);
                toRet.Add(pos + 7);
                toRet.Add(pos + 6);
                toRet.Add(pos + 5);
                toRet.Add(pos + 8);
                toRet.Add(pos + 7);
                break;
            case 8:
                pos = in_count - 7;
                toRet.Add(pos);
                toRet.Add(pos + 2);
                toRet.Add(pos + 1);
                toRet.Add(pos + 3);
                toRet.Add(pos + 4);
                toRet.Add(pos + 5);
                toRet.Add(pos + 3);
                toRet.Add(pos + 5);
                toRet.Add(pos + 6);
                break;
            case 9:
                pos = in_count - 8;
                toRet.Add(pos);
                toRet.Add(pos + 2);
                toRet.Add(pos + 1);
                toRet.Add(pos);
                toRet.Add(pos + 3);
                toRet.Add(pos + 2);
                toRet.Add(pos + 4);
                toRet.Add(pos + 5);
                toRet.Add(pos + 6);
                toRet.Add(pos + 4);
                toRet.Add(pos + 6);
                toRet.Add(pos + 7);
                break;
            case 10:
                pos = in_count - 14;
                toRet.Add(pos);
                toRet.Add(pos + 2);
                toRet.Add(pos + 1);
                toRet.Add(pos);
                toRet.Add(pos + 3);
                toRet.Add(pos + 2);
                toRet.Add(pos);
                toRet.Add(pos + 4);
                toRet.Add(pos + 3);
                toRet.Add(pos);
                toRet.Add(pos + 5);
                toRet.Add(pos + 4);
                toRet.Add(pos + 6);
                toRet.Add(pos + 7);
                toRet.Add(pos + 8);
                toRet.Add(pos + 6);
                toRet.Add(pos + 8);
                toRet.Add(pos + 9);
                toRet.Add(pos + 10);
                toRet.Add(pos + 11);
                toRet.Add(pos + 12);
                toRet.Add(pos + 10);
                toRet.Add(pos + 12);
                toRet.Add(pos + 13);
                break;
            case 11:
                pos = in_count - 9;
                toRet.Add(pos);
                toRet.Add(pos + 2);
                toRet.Add(pos + 1);
                toRet.Add(pos);
                toRet.Add(pos + 3);
                toRet.Add(pos + 2);
                toRet.Add(pos);
                toRet.Add(pos + 4);
                toRet.Add(pos + 3);
                toRet.Add(pos + 5);
                toRet.Add(pos + 6);
                toRet.Add(pos + 7);
                toRet.Add(pos + 5);
                toRet.Add(pos + 7);
                toRet.Add(pos + 8);
                break;
            case 12:
                pos = in_count - 8;
                toRet.Add(pos);
                toRet.Add(pos + 2);
                toRet.Add(pos + 1);
                toRet.Add(pos);
                toRet.Add(pos + 3);
                toRet.Add(pos + 2);
                toRet.Add(pos + 4);
                toRet.Add(pos + 5);
                toRet.Add(pos + 6);
                toRet.Add(pos + 4);
                toRet.Add(pos + 6);
                toRet.Add(pos + 7);
                break;
            case 13:
                pos = in_count - 9;
                toRet.Add(pos);
                toRet.Add(pos + 2);
                toRet.Add(pos + 1);
                toRet.Add(pos);
                toRet.Add(pos + 3);
                toRet.Add(pos + 2);
                toRet.Add(pos);
                toRet.Add(pos + 4);
                toRet.Add(pos + 3);
                toRet.Add(pos + 5);
                toRet.Add(pos + 6);
                toRet.Add(pos + 7);
                toRet.Add(pos + 5);
                toRet.Add(pos + 7);
                toRet.Add(pos + 8);
                break;
            case 14:
                pos = in_count - 9;
                toRet.Add(pos);
                toRet.Add(pos + 2);
                toRet.Add(pos + 1);
                toRet.Add(pos);
                toRet.Add(pos + 3);
                toRet.Add(pos + 2);
                toRet.Add(pos);
                toRet.Add(pos + 4);
                toRet.Add(pos + 3);
                toRet.Add(pos + 5);
                toRet.Add(pos + 6);
                toRet.Add(pos + 7);
                toRet.Add(pos + 5);
                toRet.Add(pos + 7);
                toRet.Add(pos + 8);
                break;
            case 15:
                pos = in_count - 4;
                toRet.Add(pos);
                toRet.Add(pos + 2);
                toRet.Add(pos + 1);
                toRet.Add(pos);
                toRet.Add(pos + 3);
                toRet.Add(pos + 2);
                break;
            default:
                break;
        }
        return toRet;
    }

    // Normals

    private List<Vector3> NormalsFromConfigs(Vector3[] in_upperVertices, Vector3[] in_lowerVertices, Vector3 in_center, bool in_coreOccluded, int[] in_configs, WorldNode[] in_localNodes, float in_layerElev)
    {
        List<Vector3> toRet = new List<Vector3>();

        if (!in_coreOccluded)
            toRet.AddRange(CoreNormals(in_upperVertices, in_center));

        bool cornerOccluded = false;

        if (Corner_NW(ref cornerOccluded, in_configs[0], in_layerElev, in_localNodes))
            toRet = Normals_NW(toRet, cornerOccluded, in_configs, in_upperVertices, in_lowerVertices);
        else if (in_configs[0] == 2)
            toRet = DNormals_NW(toRet, in_upperVertices, in_lowerVertices);

        if (Corner_NE(ref cornerOccluded, in_configs[1], in_layerElev, in_localNodes))
            toRet = Normals_NE(toRet, cornerOccluded, in_configs, in_upperVertices, in_lowerVertices);
        else if (in_configs[1] == 1)
            toRet = DNormals_NE(toRet, in_upperVertices, in_lowerVertices);

        if (Corner_SE(ref cornerOccluded, in_configs[2], in_layerElev, in_localNodes))
            toRet = Normals_SE(toRet, cornerOccluded, in_configs, in_upperVertices, in_lowerVertices);
        else if (in_configs[2] == 8)
            toRet = DNormals_SE(toRet, in_upperVertices, in_lowerVertices);

        if (Corner_SW(ref cornerOccluded, in_configs[3], in_layerElev, in_localNodes))
            toRet = Normals_SW(toRet, cornerOccluded, in_configs, in_upperVertices, in_lowerVertices);
        else if (in_configs[3] == 4)
            toRet = DNormals_SW(toRet, in_upperVertices, in_lowerVertices);


        return toRet;
    }

    // nw normals

    private List<Vector3> Normals_NW(List<Vector3> in_normals, bool in_cornerOccluded, int[] in_configs, Vector3[] in_upperVertices, Vector3[] in_lowerVertices)
    {
        List<Vector3> toRet = in_normals;

        // nw quadrant but we want se active

        Vector3 a = in_upperVertices[0];
        Vector3 b = in_upperVertices[6];
        Vector3 c = in_upperVertices[7];
        if (!in_cornerOccluded)
        {
            toRet.Add(Vector3.Normalize(a));
            toRet.Add(Vector3.Normalize(b));
            toRet.Add(Vector3.Normalize(c));
        }

        Vector3 d = in_lowerVertices[0];
        Vector3 e = in_lowerVertices[6];
        Vector3 f = in_lowerVertices[7];

        Vector3 sideNormal;
        int sideCount = 0;

        switch (in_configs[0])
        {
            case 2:
                sideNormal = Vector3.Cross(a, e);
                sideCount = 6;
                break;
            case 3:
                sideNormal = Vector3.Cross(a, f);
                sideCount = 6;
                break;
            case 6:
                sideNormal = Vector3.Cross(c, e);
                sideCount = 6;
                break;
            case 5:
            case 13:
                sideNormal = Vector3.Cross(b, d);
                sideCount = 6;
                break;
            default:
                sideNormal = Vector3.zero;
                break;
        }

        int count = 0;
        while (count < sideCount)
        {
            toRet.Add(sideNormal);
            count++;
        }

        return toRet;
    }

    private List<Vector3> DNormals_NW(List<Vector3> in_normals, Vector3[] in_upperVertices, Vector3[] in_lowerVertices)
    {
        List<Vector3> toRet = in_normals;

        Vector3 a = in_upperVertices[0];
        Vector3 e = in_lowerVertices[6];
        Vector3 sideNormal = Vector3.Cross(a, e);
        int count = 0;
        while (count < 6)
        {
            toRet.Add(sideNormal);
            count++;
        }

        return toRet;
    }

    //ne normals

    private List<Vector3> Normals_NE(List<Vector3> in_normals, bool in_cornerOccluded, int[] in_configs, Vector3[] in_upperVertices, Vector3[] in_lowerVertices)
    {
        List<Vector3> toRet = in_normals;

        // ne quadrant but we want sw active
        Vector3 a = in_upperVertices[0];
        Vector3 b = in_upperVertices[1];
        Vector3 c = in_upperVertices[2];
        if (!in_cornerOccluded)
        {
            toRet.Add(Vector3.Normalize(a));
            toRet.Add(Vector3.Normalize(b));
            toRet.Add(Vector3.Normalize(c));
        }

        Vector3 d = in_lowerVertices[0];
        Vector3 e = in_lowerVertices[1];
        Vector3 f = in_lowerVertices[2];

        Vector3 sideNormal;
        int sideCount = 0;

        switch (in_configs[1])
        {
            case 1:
                sideNormal = Vector3.Cross(c, d);
                sideCount = 6;
                break;
            case 3:
                sideNormal = Vector3.Cross(b, d);
                sideCount = 6;
                break;
            case 9:
                sideNormal = Vector3.Cross(c, e);
                sideCount = 6;
                break;
            case 10:
            case 14:
                sideNormal = Vector3.Cross(a, f);
                sideCount = 6;
                break;
            default:
                sideNormal = Vector3.zero;
                break;
        }

        int count = 0;
        while (count < sideCount)
        {
            toRet.Add(sideNormal);
            count++;
        }

        return toRet;
    }

    private List<Vector3> DNormals_NE(List<Vector3> in_normals, Vector3[] in_upperVertices, Vector3[] in_lowerVertices)
    {
        List<Vector3> toRet = in_normals;

        Vector3 a = in_upperVertices[0];
        Vector3 f = in_lowerVertices[2];

        Vector3 sideNormal = -Vector3.Cross(a, f);
        int count = 0;
        while (count < 6)
        {
            toRet.Add(sideNormal);
            count++;
        }
        return toRet;
    }

    // se normals

    private List<Vector3> Normals_SE(List<Vector3> in_normals, bool in_cornerOccluded, int[] in_configs, Vector3[] in_upperVertices, Vector3[] in_lowerVertices)
    {
        List<Vector3> toRet = in_normals;

        // se quadrant but we want nw active
        Vector3 a = in_upperVertices[2];
        Vector3 b = in_upperVertices[3];
        Vector3 c = in_upperVertices[4];
        if (!in_cornerOccluded)
        {
            toRet.Add(Vector3.Normalize(a));
            toRet.Add(Vector3.Normalize(b));
            toRet.Add(Vector3.Normalize(c));
        }

        Vector3 d = in_lowerVertices[2];
        Vector3 e = in_lowerVertices[3];
        Vector3 f = in_lowerVertices[4];

        Vector3 sideNormal;
        int sideCount = 0;

        switch (in_configs[2])
        {
            case 8:
                sideNormal = Vector3.Cross(c, d);
                sideCount = 6;
                break;
            case 9:
                sideNormal = Vector3.Cross(b, d);
                sideCount = 6;
                break;
            case 12:
                sideNormal = Vector3.Cross(c, e);
                sideCount = 6;
                break;
            case 5:
            case 7:
                sideNormal = Vector3.Cross(a, f);
                sideCount = 6;
                break;
            default:
                sideNormal = Vector3.zero;
                break;
        }

        int count = 0;
        while (count < sideCount)
        {
            toRet.Add(sideNormal);
            count++;
        }

        return toRet;
    }

    private List<Vector3> DNormals_SE(List<Vector3> in_normals, Vector3[] in_upperVertices, Vector3[] in_lowerVertices)
    {
        List<Vector3> toRet = in_normals;

        // se quadrant but we want nw active
        Vector3 a = in_upperVertices[2];
        Vector3 f = in_lowerVertices[4];

        Vector3 sideNormal = -Vector3.Cross(a, f);
        int count = 0;
        while (count < 6)
        {
            toRet.Add(sideNormal);
            count++;
        }

        return toRet;
    }

    // sw normals

    private List<Vector3> Normals_SW(List<Vector3> in_normals, bool in_cornerOccluded, int[] in_configs, Vector3[] in_upperVertices, Vector3[] in_lowerVertices)
    {
        List<Vector3> toRet = in_normals;

        // sw quadrant but we want ne active
        Vector3 a = in_upperVertices[6];
        Vector3 b = in_upperVertices[4];
        Vector3 c = in_upperVertices[5];
        if (!in_cornerOccluded)
        {
            toRet.Add(Vector3.Normalize(a));
            toRet.Add(Vector3.Normalize(b));
            toRet.Add(Vector3.Normalize(c));
        }

        Vector3 d = in_lowerVertices[6];
        Vector3 e = in_lowerVertices[4];
        Vector3 f = in_lowerVertices[5];

        Vector3 sideNormal;
        int sideCount = 0;

        switch (in_configs[3])
        {
            case 4:
                sideNormal = Vector3.Cross(a, e);
                sideCount = 6;
                break;
            case 6:
                sideNormal = Vector3.Cross(a, f);
                sideCount = 6;
                break;
            case 12:
                sideNormal = Vector3.Cross(c, e);
                sideCount = 6;
                break;
            case 10:
            case 11:
                sideNormal = Vector3.Cross(b, d);
                sideCount = 6;
                break;
            default:
                sideNormal = Vector3.zero;
                break;
        }

        int count = 0;
        while (count < sideCount)
        {
            toRet.Add(sideNormal);
            count++;
        }

        return toRet;
    }

    private List<Vector3> DNormals_SW(List<Vector3> in_normals, Vector3[] in_upperVertices, Vector3[] in_lowerVertices)
    {
        List<Vector3> toRet = in_normals;

        // sw quadrant but we want ne active
        Vector3 a = in_upperVertices[6];
        Vector3 e = in_lowerVertices[4];

        Vector3 sideNormal = Vector3.Cross(a, e);
        int count = 0;
        while (count < 6)
        {
            toRet.Add(sideNormal);
            count++;
        }

        return toRet;
    }

    private Vector3[] CoreNormals(Vector3[] in_upperVertices, Vector3 in_center)
    {
        // cores are always the same
        Vector3[] toRet = new Vector3[12];
        // nw tris 
        toRet[0] = Vector3.Normalize(in_upperVertices[0]);
        toRet[1] = Vector3.Normalize(in_center);
        toRet[2] = Vector3.Normalize(in_upperVertices[6]);
        // ne tris
        toRet[3] = Vector3.Normalize(in_upperVertices[0]);
        toRet[4] = Vector3.Normalize(in_upperVertices[2]);
        toRet[5] = Vector3.Normalize(in_center);
        // se tris
        toRet[6] = Vector3.Normalize(in_center);
        toRet[7] = Vector3.Normalize(in_upperVertices[2]);
        toRet[8] = Vector3.Normalize(in_upperVertices[4]);
        // sw tris
        toRet[9] = Vector3.Normalize(in_upperVertices[6]);
        toRet[10] = Vector3.Normalize(in_center);
        toRet[11] = Vector3.Normalize(in_upperVertices[4]);
        return toRet;
    }

    private List<Vector3> NormalsFromConfig(Vector3[] in_possibleVerts, Vector3[] in_lowerVerts, int in_config)
    {
        List<Vector3> toRet = new List<Vector3>();
        switch (in_config)
        {
            case 1:
                toRet.Add(Vector3.Normalize(in_possibleVerts[7]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[5]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[6]));

                toRet.Add(Vector3.Normalize(Vector3.Cross(in_possibleVerts[7], in_possibleVerts[5])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_possibleVerts[7], in_possibleVerts[5])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_lowerVerts[7], in_lowerVerts[5])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_lowerVerts[7], in_lowerVerts[5])));
                break;
            case 2:
                toRet.Add(Vector3.Normalize(in_possibleVerts[3]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[4]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[5]));

                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_possibleVerts[3], in_possibleVerts[5])));
                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_possibleVerts[3], in_possibleVerts[5])));
                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_lowerVerts[3], in_lowerVerts[5])));
                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_lowerVerts[3], in_lowerVerts[5])));
                break;
            case 3:
                toRet.Add(Vector3.Normalize(in_possibleVerts[3]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[4]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[6]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[7]));

                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_possibleVerts[3], in_possibleVerts[7])));
                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_possibleVerts[3], in_possibleVerts[7])));
                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_lowerVerts[3], in_lowerVerts[7])));
                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_lowerVerts[3], in_lowerVerts[7])));
                break;
            case 4:
                toRet.Add(Vector3.Normalize(in_possibleVerts[1]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[2]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[3]));

                toRet.Add(Vector3.Normalize(Vector3.Cross(in_possibleVerts[3], in_possibleVerts[1])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_possibleVerts[3], in_possibleVerts[1])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_lowerVerts[3], in_lowerVerts[1])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_lowerVerts[3], in_lowerVerts[1])));
                break;
            case 5:
                toRet.Add(Vector3.Normalize(in_possibleVerts[1]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[2]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[3]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[5]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[6]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[7]));

                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_possibleVerts[1], in_possibleVerts[7])));
                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_possibleVerts[1], in_possibleVerts[7])));
                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_lowerVerts[1], in_lowerVerts[7])));
                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_lowerVerts[1], in_lowerVerts[7])));
                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_possibleVerts[5], in_possibleVerts[3])));
                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_possibleVerts[5], in_possibleVerts[3])));
                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_lowerVerts[5], in_lowerVerts[3])));
                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_lowerVerts[5], in_lowerVerts[3])));
                break;
            case 6:
                toRet.Add(Vector3.Normalize(in_possibleVerts[1]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[2]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[4]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[5]));

                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_possibleVerts[1], in_possibleVerts[5])));
                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_possibleVerts[1], in_possibleVerts[5])));
                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_lowerVerts[1], in_lowerVerts[5])));
                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_lowerVerts[1], in_lowerVerts[5])));
                break;
            case 7:
                toRet.Add(Vector3.Normalize(in_possibleVerts[1]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[2]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[4]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[6]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[7]));

                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_possibleVerts[1], in_possibleVerts[7])));
                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_possibleVerts[1], in_possibleVerts[7])));
                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_lowerVerts[1], in_lowerVerts[7])));
                toRet.Add(Vector3.Normalize(-Vector3.Cross(in_lowerVerts[1], in_lowerVerts[7])));
                break;
            case 8:
                toRet.Add(Vector3.Normalize(in_possibleVerts[0]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[1]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[7]));

                toRet.Add(Vector3.Normalize(Vector3.Cross(in_possibleVerts[1], in_possibleVerts[7])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_possibleVerts[1], in_possibleVerts[7])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_lowerVerts[1], in_lowerVerts[7])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_lowerVerts[1], in_lowerVerts[7])));
                break;
            case 9:
                toRet.Add(Vector3.Normalize(in_possibleVerts[0]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[1]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[5]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[6]));

                toRet.Add(Vector3.Normalize(Vector3.Cross(in_possibleVerts[1], in_possibleVerts[5])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_possibleVerts[1], in_possibleVerts[5])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_lowerVerts[1], in_lowerVerts[5])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_lowerVerts[1], in_lowerVerts[5])));
                break;
            case 10:
                toRet.Add(Vector3.Normalize(in_possibleVerts[0]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[1]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[3]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[4]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[5]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[7]));

                toRet.Add(Vector3.Normalize(Vector3.Cross(in_possibleVerts[1], in_possibleVerts[3])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_possibleVerts[1], in_possibleVerts[3])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_lowerVerts[1], in_lowerVerts[3])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_lowerVerts[1], in_lowerVerts[3])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_possibleVerts[5], in_possibleVerts[7])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_possibleVerts[5], in_possibleVerts[7])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_lowerVerts[5], in_lowerVerts[7])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_lowerVerts[5], in_lowerVerts[7])));
                break;
            case 11:
                toRet.Add(Vector3.Normalize(in_possibleVerts[0]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[1]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[3]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[4]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[6]));

                toRet.Add(Vector3.Normalize(Vector3.Cross(in_possibleVerts[1], in_possibleVerts[3])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_possibleVerts[1], in_possibleVerts[3])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_lowerVerts[1], in_lowerVerts[3])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_lowerVerts[1], in_lowerVerts[3])));
                break;
            case 12:
                toRet.Add(Vector3.Normalize(in_possibleVerts[0]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[2]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[3]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[7]));

                toRet.Add(Vector3.Normalize(Vector3.Cross(in_possibleVerts[3], in_possibleVerts[7])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_possibleVerts[3], in_possibleVerts[7])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_lowerVerts[3], in_lowerVerts[7])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_lowerVerts[3], in_lowerVerts[7])));
                break;
            case 13:
                toRet.Add(Vector3.Normalize(in_possibleVerts[0]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[2]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[3]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[5]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[6]));

                toRet.Add(Vector3.Normalize(Vector3.Cross(in_possibleVerts[3], in_possibleVerts[5])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_possibleVerts[3], in_possibleVerts[5])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_lowerVerts[3], in_lowerVerts[5])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_lowerVerts[3], in_lowerVerts[5])));
                break;
            case 14:
                toRet.Add(Vector3.Normalize(in_possibleVerts[0]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[2]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[4]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[5]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[7]));

                toRet.Add(Vector3.Normalize(Vector3.Cross(in_possibleVerts[5], in_possibleVerts[7])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_possibleVerts[5], in_possibleVerts[7])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_lowerVerts[5], in_lowerVerts[7])));
                toRet.Add(Vector3.Normalize(Vector3.Cross(in_lowerVerts[5], in_lowerVerts[7])));
                break;
            case 15:
                toRet.Add(Vector3.Normalize(in_possibleVerts[0]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[2]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[4]));
                toRet.Add(Vector3.Normalize(in_possibleVerts[6]));
                break;
            default:
                break;
        }
        return toRet;
    }

    // UV section

    public void InjectOverlapUVS(PointFeeder in_feeder, int in_overlapType, WorldGrid in_grid, List<IterableBinaryBubbleCell> in_cells)
    {
        heatmap = GenerateOverlapMap(in_cells, in_grid.Get_Nodes());
        mapType = 3;
        List<Point2D> activePoints = in_feeder.Get_currentActive();
        int pos = 0;
        while (pos < activePoints.Count)
        {
            Point2D loc = activePoints[pos];
            if (activeMap[loc.x, loc.y])
            {
                Vector2[] locUVS = new Vector2[8];
                float locScore = heatmap[loc.x, loc.y].Get_value();
                for (int i = 0; i < 8; i++)
                    locUVS[i] = new Vector2(locScore, 0);
                int subPos = 0;
                while (subPos < activeNodes.Count)
                {
                    if (activeNodes[subPos].Get_location().IsEqualTo(loc))
                    {
                        activeNodes[subPos].InjectUVs(locUVS, locUVS, actualLayers, actualRadii, in_grid);
                        break;
                    }
                    subPos++;
                }
            }
            pos++;
        }
        colorGenerator.UpdateHeatMapColors();
    }

    public NormalizedFloat[,] GenerateOverlapMap(List<IterableBinaryBubbleCell> in_cells, WorldNode[,] in_nodes)
    {
        int x = 0;
        int maxX = in_nodes.GetLength(0);
        int y = 0;
        int maxY = in_nodes.GetLength(1);
        NormalizedFloat.maximum = 0;
        NormalizedFloat.minimum = 1;
        NormalizedFloat[,] toRet = new NormalizedFloat[maxX, maxY];
        while (y < maxY)
        {
            while (x < maxX)
            {
                toRet[x, y] = new NormalizedFloat(0f);
                x++;
            }
            x = 0;
            y++;
        }
        int pos = 0;
        float maxCount = 0f;
        while (pos < in_cells.Count)
        {
            List<Point2D> interior = in_cells[pos].Get_interior();
            int subPos = 0;
            while (subPos < interior.Count)
            {
                Point2D loc = interior[subPos];
                toRet[loc.x, loc.y].Set_value(toRet[loc.x, loc.y].Get_value() + 1f);
                if (toRet[loc.x, loc.y].Get_value() > maxCount)
                    maxCount = toRet[loc.x, loc.y].Get_value();
                subPos++;
            }
            pos++;
        }
        NormalizedFloat.maximum = maxCount;
        return toRet;
    }

    public void InjectHeatmapUVS(PointFeeder in_feeder, int in_heatMapType, WorldGrid in_grid)
    {
        List<Point2D> activePoints = in_feeder.Get_currentActive();
        heatmap = GenerateNormalizedHeatMap(in_heatMapType, in_grid.Get_Nodes());
        mapType = 1;
        int pos = 0;
        while (pos < activePoints.Count)
        {
            Point2D loc = activePoints[pos];
            if (activeMap[loc.x, loc.y])
            {
                Vector2[] localUVS = GetLocalLerpedUVs(heatmap, loc, in_grid.Get_Nodes());
                int subPos = 0;
                while (subPos < activeNodes.Count)
                {
                    if (activeNodes[subPos].Get_location().IsEqualTo(loc))
                    {
                        activeNodes[subPos].InjectUVs(localUVS, localUVS, actualLayers, actualRadii, in_grid);
                        break;
                    }
                    subPos++;
                }
            }
            pos++;
        }
        colorGenerator.UpdateHeatMapColors();
    }

    public void InjectLocalUVSFromHeatMapType(int in_heatMapType, WorldGrid in_grid)
    {
        WorldNode[,] nodes = in_grid.Get_Nodes();
        heatmap = GenerateNormalizedHeatMap(in_heatMapType, nodes);
        mapType = 1;
        int x = 0;
        int maxX = nodes.GetLength(0);
        int y = 1;
        int maxY = nodes.GetLength(1);
        while (y < maxY - 1)
        {
            while (x < maxX)
            {
                if (activeMap[x, y])
                {
                    // all 8 subnodes must be calculated here
                    Vector2[] localUVs = GetLocalLerpedUVs(heatmap, new Point2D(x, y), nodes);
                    int pos = 0;
                    while (pos < activeNodes.Count)
                    {
                        if (activeNodes[pos].Get_location().IsEqualTo(new Point2D(x, y)))
                        {
                            activeNodes[pos].InjectUVs(localUVs, localUVs, actualLayers, actualRadii, in_grid);
                            break;
                        }
                        pos++;
                    }
                }

                x++;
            }
            x = 0;
            y++;
        }
        colorGenerator.UpdateHeatMapColors();
    }

    public void InjectRegionUVS(PointFeeder in_feeder, List<IterableBinaryBubbleCell> in_cells, WorldGrid in_grid)
    {
        List<Point2D> activePoints = in_feeder.Get_currentActive();
        regionmap = GenerateRegionMap(in_cells, in_grid.Get_Nodes());
        mapType = 2;
        int pos = 0;
        regionCount = in_cells.Count;
        while (pos < activePoints.Count)
        {
            Point2D loc = activePoints[pos];
            if (activeMap[loc.x, loc.y])
            {
                List<Vector2> locUVS = new List<Vector2>();
                float locScore = ((float)regionmap[loc.x, loc.y]) / ((float)regionCount);
                locScore += 0.001f;
                for (int i = 0; i < 8; i++)
                    locUVS.Add(new Vector2(locScore, 0));
                int subPos = 0;
                while (subPos < activeNodes.Count)
                {
                    if (activeNodes[subPos].Get_location().IsEqualTo(loc))
                    {
                        activeNodes[subPos].InjectUVs(locUVS.ToArray(), locUVS.ToArray(), actualLayers, actualRadii, in_grid);
                        break;
                    }
                    subPos++;
                }
            }
            pos++;
        }
        colorGenerator.UpdateRegionColors(in_cells);
    }

    //public void InjectResourceUVS(PointFeeder in_feeder, Resource in_resource, WorldGrid in_grid)
    //{
    //    List<IterableBinaryBubbleCell> cells = CollectCellsFromresource(in_resource);
    //    InjectRegionUVS(in_feeder, cells, in_grid);
    //}

    //private List<IterableBinaryBubbleCell> CollectCellsFromresource(Resource in_resource)
    //{
    //    List<IterableBinaryBubbleCell> toRet = new List<IterableBinaryBubbleCell>();
    //    ResourceVariant[] variants = new ResourceVariant[0];
    //    if (in_resource.GetType() == typeof(Plant))
    //    {
    //        Plant plant = (Plant)in_resource;
    //        variants = plant.data.variants.ToArray();

    //    }
    //    else if (in_resource.GetType() == typeof(Animal))
    //    {
    //        Animal animal = (Animal)in_resource;
    //        variants = animal.data.variants.ToArray();
    //    }
    //    else if (in_resource.GetType() == typeof(MarineFish))
    //    {
    //        MarineFish saltFish = (MarineFish)in_resource;
    //        variants = saltFish.data.variants.ToArray();
    //        Debug.Log(saltFish.baseDetails.name + " has " + variants.Length + " variants");
    //    }
    //    else if (in_resource.GetType() == typeof(Insect))
    //    {
    //        Insect insect = (Insect)in_resource;
    //        variants = insect.data.variants.ToArray();
    //        Debug.Log(insect.baseDetails.name + " has " + variants.Length + " variants");
    //    }
    //    else
    //        Debug.Log("Resource does not have a terrestrial type");

    //    int pos = 0;
    //    while (pos < variants.Length)
    //    {
    //        toRet.Add(variants[pos].Get_cell());
    //        pos++;
    //    }
    //    return toRet;
    //}

    public void InjectLocalUVSFromRegions(List<IterableBinaryBubbleCell> in_cells, WorldGrid in_grid)
    {
        WorldNode[,] nodes = in_grid.Get_Nodes();
        regionmap = GenerateRegionMap(in_cells, nodes);
        mapType = 2;
        regionCount = in_cells.Count;
        int x = 0;
        int maxX = nodes.GetLength(0);
        int y = 1;
        int maxY = nodes.GetLength(1);
        while (y < maxY - 1)
        {
            while (x < maxX)
            {
                if (activeMap[x, y])
                {
                    List<Vector2> locUVS = new List<Vector2>();
                    float locScore = ((float)regionmap[x, y]) / ((float)regionCount);
                    locScore += 0.001f;
                    for (int i = 0; i < 8; i++)
                        locUVS.Add(new Vector2(locScore, 0));
                    int pos = 0;
                    while (pos < activeNodes.Count)
                    {
                        if (activeNodes[pos].Get_location().IsEqualTo(new Point2D(x, y)))
                        {
                            activeNodes[pos].InjectUVs(locUVS.ToArray(), locUVS.ToArray(), actualLayers, actualRadii, in_grid);
                            break;
                        }
                        pos++;
                    }
                }

                x++;
            }
            x = 0;
            y++;
        }
        colorGenerator.UpdateRegionColors(in_cells);
    }

    public void InjectUVFromRegions(List<IterableBinaryBubbleCell> in_cells, WorldNode[,] in_nodes)
    {
        // either scan line and complicated interpret
        // or make a xy map of region values and calculate configs as we do above
        // challenge node is 3/4 active but not the center node
        // xy map to configs would be == instead of >= as we do for elev

        List<Vector2> uvs = new List<Vector2>();
        int[,] regionMap = GenerateRegionMap(in_cells, in_nodes);
        //Debug.Log("There are " + in_cells.Count + " cells");
        int layerPos = 0;
        int counter = 0;
        while (layerPos < actualLayers.Length)
        {

            int x = 0;
            int maxX = in_nodes.GetLength(0);
            int y = 1;
            int maxY = in_nodes.GetLength(1);
            while (y < maxY - 1)
            {

                while (x < maxX)
                {

                    Point2D loc = new Point2D(x, y);
                    //int regionPos = FindRegion(loc, in_cells);
                    int regionPos = regionMap[x, y];
                    bool coreOccluded = false;
                    if (in_nodes[loc.x, loc.y].Get_layerElev() != actualLayers[layerPos])
                        coreOccluded = true;
                    int[] locRegion = GetLocalRegion(loc, regionMap);
                    WorldNode[] locNodes = GetLocalNodes(loc, in_nodes);
                    int[] cornerRegions = GetRegionConfigs(in_cells, loc, locRegion);
                    int[] meshConfigs = GetConfigs(actualLayers[layerPos], loc, locNodes, maxY);

                    //List<Vector2> locUVs = UVFromRegion(regionPos, in_cells.Count, coreOccluded, actualLayers[layerPos], meshConfigs, locNodes);
                    List<Vector2> locUVs = CornerUVs(regionPos, in_cells.Count, coreOccluded, actualLayers[layerPos], meshConfigs, cornerRegions, locNodes);
                    uvs.AddRange(locUVs);
                    counter++;
                    x++;
                }
                x = 0;
                y++;
            }
            layerPos++;
        }
        //Debug.Log("Counter reports " + counter + " visits");
        //Debug.Log(uvs.Count + " / " + builtWorld.GetComponent<MeshFilter>().sharedMesh.vertexCount);
        builtWorld.GetComponent<MeshFilter>().sharedMesh.uv3 = uvs.ToArray();
    }



    private int[] GetLocalRegion(Point2D in_center, int[,] in_map)
    {
        int[] toRet = new int[9];
        Point2D[] peri = in_center.GetValidatedPerimeter(new PointValidator(in_map));
        int pos = 0;
        while (pos < peri.Length)
        {
            Point2D loc = peri[pos];
            toRet[pos] = in_map[loc.x, loc.y];
            pos++;
        }
        toRet[8] = in_map[in_center.x, in_center.y];
        return toRet;
    }

    private int[] GetRegionConfigs(List<IterableBinaryBubbleCell> in_cells, Point2D in_loc, int[] in_regionConfigs)
    {
        //Debug.Log("Searching through " + in_cells.Count + " cells");
        int[] toRet = new int[4];

        toRet[0] = FindCornerInCells(new Vector3Int(in_loc.x, in_loc.y, 0), in_cells, GetCornerRegions(0, in_regionConfigs));
        toRet[1] = FindCornerInCells(new Vector3Int(in_loc.x, in_loc.y, 1), in_cells, GetCornerRegions(1, in_regionConfigs));
        toRet[2] = FindCornerInCells(new Vector3Int(in_loc.x, in_loc.y, 2), in_cells, GetCornerRegions(2, in_regionConfigs));
        toRet[3] = FindCornerInCells(new Vector3Int(in_loc.x, in_loc.y, 3), in_cells, GetCornerRegions(3, in_regionConfigs));

        return toRet;
    }

    private int[] GetCornerRegions(int in_corner, int[] in_regions)
    {
        int[] toRet = new int[4];
        int a;
        int b;
        int c;
        int d;
        switch (in_corner)
        {
            case 0:
                a = in_regions[7];
                b = in_regions[0];
                c = in_regions[8];
                d = in_regions[6];
                break;
            case 1:
                a = in_regions[0];
                b = in_regions[1];
                c = in_regions[2];
                d = in_regions[8];
                break;
            case 2:
                a = in_regions[2];
                b = in_regions[3];
                c = in_regions[4];
                d = in_regions[8];
                break;
            case 3:
                a = in_regions[6];
                b = in_regions[8];
                c = in_regions[4];
                d = in_regions[5];
                break;
            default:
                a = -1;
                b = -1;
                c = -1;
                d = -1;
                break;
        }
        toRet[0] = a;
        toRet[1] = b;
        toRet[2] = c;
        toRet[3] = d;
        return toRet;
    }

    private int FindCornerInCells(Vector3Int in_loc, List<IterableBinaryBubbleCell> in_cells, int[] in_cornerRegions)
    {

        int pos = 0;
        int toRet = -1;
        while (pos < in_cornerRegions.Length)
        {
            int searchPos = in_cornerRegions[pos];
            if (in_cells[searchPos].FindCorner(new Point2D(in_loc.x, in_loc.y), in_loc.z))
            {
                toRet = searchPos;
                break;
            }
            pos++;
        }

        return toRet;
    }

    private Vector2[] GetLocalLerpedUVs(NormalizedFloat[,] in_heatMap, Point2D in_point, WorldNode[,] in_nodes)
    {
        // here
        Point2D[] localPoints = in_point.GetValidatedPerimeter(new PointValidator(in_nodes));
        Vector3 centerUV = new Vector3(in_point.x, in_point.y, in_heatMap[in_point.x, in_point.y].Get_value());
        Vector3[] perimeterUVs = FillPerimeterWithUVs(in_heatMap, localPoints);
        Vector2[] toRet = new Vector2[localPoints.Length];

        float close = Mathf.Sqrt(2f * Mathf.Pow(1f / 6f, 2));
        float far = Mathf.Sqrt(2f * Mathf.Pow(1f / 3f, 2));

        Vector3 nwCornerNear = Vector3.Lerp(centerUV, perimeterUVs[7], close);
        Vector3 nwCornerFar = Vector3.Lerp(centerUV, perimeterUVs[7], far);
        Vector3 nwCornerCross = Vector3.Lerp(perimeterUVs[0], perimeterUVs[6], 0.5f);
        Vector3 nwDiamond = Vector3.Lerp(nwCornerNear, nwCornerCross, 0.5f);
        Vector3 nwCorner = Vector3.Lerp(nwCornerFar, nwCornerCross, 0.5f);

        Vector3 neCornerNear = Vector3.Lerp(centerUV, perimeterUVs[1], close);
        Vector3 neCornerFar = Vector3.Lerp(centerUV, perimeterUVs[1], far);
        Vector3 neCornerCross = Vector3.Lerp(perimeterUVs[0], perimeterUVs[2], 0.5f);
        Vector3 neDiamond = Vector3.Lerp(neCornerNear, neCornerCross, 0.5f);
        Vector3 neCorner = Vector3.Lerp(neCornerFar, neCornerCross, 0.5f);

        Vector3 seCornerNear = Vector3.Lerp(centerUV, perimeterUVs[3], close);
        Vector3 seCornerFar = Vector3.Lerp(centerUV, perimeterUVs[3], far);
        Vector3 seCornerCross = Vector3.Lerp(perimeterUVs[2], perimeterUVs[4], 0.5f);
        Vector3 seDiamond = Vector3.Lerp(seCornerNear, seCornerCross, 0.5f);
        Vector3 seCorner = Vector3.Lerp(seCornerFar, seCornerCross, 0.5f);

        Vector3 swCornerNear = Vector3.Lerp(centerUV, perimeterUVs[5], close);
        Vector3 swCornerFar = Vector3.Lerp(centerUV, perimeterUVs[5], far);
        Vector3 swCornerCross = Vector3.Lerp(perimeterUVs[4], perimeterUVs[6], 0.5f);
        Vector3 swDiamond = Vector3.Lerp(swCornerNear, swCornerCross, 0.5f);
        Vector3 swCorner = Vector3.Lerp(swCornerFar, swCornerCross, 0.5f);

        toRet[0] = new Vector2(nwCorner.z, 0);
        toRet[1] = new Vector2(nwDiamond.z, 0);
        toRet[2] = new Vector2(neDiamond.z, 0);
        toRet[3] = new Vector2(neCorner.z, 0);
        toRet[4] = new Vector2(swCorner.z, 0);
        toRet[5] = new Vector2(swDiamond.z, 0);
        toRet[6] = new Vector2(seDiamond.z, 0);
        toRet[7] = new Vector2(seCorner.z, 0);

        return toRet;
    }

    private Vector3[] FillPerimeterWithUVs(NormalizedFloat[,] in_heatMap, Point2D[] in_perimeter)
    {
        Vector3[] toRet = new Vector3[in_perimeter.Length];
        int pos = 0;
        while (pos < in_perimeter.Length)
        {
            Point2D loc = in_perimeter[pos];
            toRet[pos] = new Vector3(loc.x, loc.y, in_heatMap[loc.x, loc.y].Get_value());
            pos++;
        }
        return toRet;
    }

    private NormalizedFloat[,] GenerateNormalizedHeatMap(int in_heatMapType, WorldNode[,] in_nodes)
    {
        int x = 0;
        int maxX = in_nodes.GetLength(0);
        int y = 0;
        int maxY = in_nodes.GetLength(1);
        NormalizedFloat[,] toRet = new NormalizedFloat[maxX, maxY];
        float maximum = 0f;

        while (y < maxY)
        {
            while (x < maxX)
            {
                //float value = in_nodes[x, y].Get_meanHighTemperature();
                float value = 0f;
                switch (in_heatMapType)
                {
                    default:
                    case 0:
                        value = in_nodes[x, y].Get_meanHighTemperature();
                        break;
                    case 1:
                        value = in_nodes[x, y].Get_precipitation();
                        break;
                    case 2:
                        value = in_nodes[x, y].Get_oceanSurfaceTemperature();
                        break;
                }
                toRet[x, y] = new NormalizedFloat(value);

                if (value > maximum)
                    maximum = value;

                x++;
            }
            x = 0;
            y++;
        }

        NormalizedFloat.maximum = maximum;

        return toRet;
    }


    private int[,] GenerateRegionMap(List<IterableBinaryBubbleCell> in_cells, WorldNode[,] in_nodes)
    {
        int pos = 0;
        int[,] toRet = new int[in_nodes.GetLength(0), in_nodes.GetLength(1)];
        while (pos < in_cells.Count)
        {
            int subPos = 0;
            List<Point2D> inner = in_cells[pos].Get_interior();
            while (subPos < inner.Count)
            {
                Point2D loc = inner[subPos];
                toRet[loc.x, loc.y] = pos + 1;
                subPos++;
            }
            pos++;
        }

        return toRet;
    }

    private List<Vector2> CornerUVs(int in_coreRegion, int in_regionCount, bool in_coreOccluded, float in_layerElev, int[] in_meshConfigs, int[] in_UVconfigs, WorldNode[] in_localNodes)
    {
        List<Vector2> toRet = new List<Vector2>();
        // core first
        if (!in_coreOccluded)
            toRet.AddRange(CoreUVs(in_coreRegion, in_regionCount));
        // then corners
        bool cornerOccluded = false;
        if (Corner_NW(ref cornerOccluded, in_meshConfigs[0], in_layerElev, in_localNodes))
            toRet = UVQuadrant_NW(toRet, in_UVconfigs[0], in_regionCount, cornerOccluded, in_meshConfigs[0]);
        else if (in_meshConfigs[0] == 2)
            toRet = UVDiamond(toRet, in_coreRegion, in_regionCount);

        if (Corner_NE(ref cornerOccluded, in_meshConfigs[1], in_layerElev, in_localNodes))
            toRet = UVQuadrant_NE(toRet, in_UVconfigs[1], in_regionCount, cornerOccluded, in_meshConfigs[1]);
        else if (in_meshConfigs[1] == 1)
            toRet = UVDiamond(toRet, in_coreRegion, in_regionCount);

        if (Corner_SE(ref cornerOccluded, in_meshConfigs[2], in_layerElev, in_localNodes))
            toRet = UVQuadrant_SE(toRet, in_UVconfigs[2], in_regionCount, cornerOccluded, in_meshConfigs[2]);
        else if (in_meshConfigs[2] == 8)
            toRet = UVDiamond(toRet, in_coreRegion, in_regionCount);

        if (Corner_SW(ref cornerOccluded, in_meshConfigs[3], in_layerElev, in_localNodes))
            toRet = UVQuadrant_SW(toRet, in_UVconfigs[3], in_regionCount, cornerOccluded, in_meshConfigs[3]);
        else if (in_meshConfigs[3] == 4)
            toRet = UVDiamond(toRet, in_coreRegion, in_regionCount);

        return toRet;
    }

    private List<Vector2> UVDiamond(List<Vector2> in_uvs, int in_coreRegion, int in_regionCount)
    {
        List<Vector2> toRet = in_uvs;
        float time = CalculateTimeFromRegion(in_coreRegion, in_regionCount);
        Vector2 uv = new Vector2(time, 0);
        int pos = 0;
        while (pos < 6)
        {
            toRet.Add(uv);
            pos++;
        }
        return toRet;
    }

    private List<Vector2> UVQuadrant_NW(List<Vector2> in_uvs, int in_region, int in_regionCount, bool in_occluded, int in_meshConfig)
    {
        List<Vector2> toRet = in_uvs;
        int pos = 0;
        int limit = 0;
        float time = CalculateTimeFromRegion(in_region, in_regionCount);
        Vector2 uv = new Vector2(time, 0);
        if (!in_occluded)
            limit += 3;
        if ((in_meshConfig == 3) ||
            (in_meshConfig == 5) ||
            (in_meshConfig == 6) ||
            (in_meshConfig == 13))
            limit += 6;

        while (pos < limit)
        {
            toRet.Add(uv);
            pos++;
        }
        return toRet;
    }

    private List<Vector2> UVQuadrant_NE(List<Vector2> in_uvs, int in_region, int in_regionCount, bool in_occluded, int in_meshConfig)
    {
        List<Vector2> toRet = in_uvs;
        int pos = 0;
        int limit = 0;
        float time = CalculateTimeFromRegion(in_region, in_regionCount);
        Vector2 uv = new Vector2(time, 0);
        if (!in_occluded)
            limit += 3;
        if ((in_meshConfig == 3) ||
            (in_meshConfig == 9) ||
            (in_meshConfig == 10) ||
            (in_meshConfig == 14))
            limit += 6;

        while (pos < limit)
        {
            toRet.Add(uv);
            pos++;
        }
        return toRet;
    }

    private List<Vector2> UVQuadrant_SE(List<Vector2> in_uvs, int in_region, int in_regionCount, bool in_occluded, int in_meshConfig)
    {
        List<Vector2> toRet = in_uvs;
        int pos = 0;
        int limit = 0;
        float time = CalculateTimeFromRegion(in_region, in_regionCount);
        Vector2 uv = new Vector2(time, 0);
        if (!in_occluded)
            limit += 3;
        if ((in_meshConfig == 5) ||
            (in_meshConfig == 7) ||
            (in_meshConfig == 9) ||
            (in_meshConfig == 12))
            limit += 6;

        while (pos < limit)
        {
            toRet.Add(uv);
            pos++;
        }
        return toRet;
    }

    private List<Vector2> UVQuadrant_SW(List<Vector2> in_uvs, int in_region, int in_regionCount, bool in_occluded, int in_meshConfig)
    {
        List<Vector2> toRet = in_uvs;
        int pos = 0;
        int limit = 0;
        float time = CalculateTimeFromRegion(in_region, in_regionCount);
        Vector2 uv = new Vector2(time, 0);
        if (!in_occluded)
            limit += 3;
        if ((in_meshConfig == 6) ||
            (in_meshConfig == 10) ||
            (in_meshConfig == 11) ||
            (in_meshConfig == 12))
            limit += 6;

        while (pos < limit)
        {
            toRet.Add(uv);
            pos++;
        }
        return toRet;
    }

    private List<Vector2> CoreUVs(int in_coreRegion, int in_coreCount)
    {
        List<Vector2> toRet = new List<Vector2>();
        float time = CalculateTimeFromRegion(in_coreRegion, in_coreCount);
        Vector2 uv = new Vector2(time, 0);
        int pos = 0;
        while (pos < 12)
        {
            toRet.Add(uv);
            pos++;
        }
        return toRet;
    }

    private List<Vector2> UVFromRegion(int in_region, int in_regionCount, bool in_coreOccluded, float in_layerElev, int[] in_meshConfigs, WorldNode[] in_localNodes)
    {
        List<Vector2> toRet = new List<Vector2>();
        float time = CalculateTimeFromRegion(in_region, in_regionCount);
        int offSet = GetCountOffset(in_coreOccluded, in_meshConfigs, in_localNodes, in_layerElev);
        int pos = 0;
        while (pos < offSet)
        {
            toRet.Add(new Vector2(time, 0));
            pos++;
        }
        return toRet;
    }

    private List<Vector2> UVFromConfigs(float in_thisRadius, bool in_coreOccluded, int[] in_configs, WorldNode[] in_localNodes, float in_layerElev)
    {
        List<Vector2> toRet = new List<Vector2>();
        float score = CalculateTimeFromRadius(in_thisRadius);
        int offset = GetCountOffset(in_coreOccluded, in_configs, in_localNodes, in_layerElev);
        int pos = 0;
        while (pos < offset)
        {
            toRet.Add(new Vector2(score, 0));
            pos++;
        }
        return toRet;
    }

    private List<Vector2> TimeUV(float in_oceanRadius, float in_seafloorRadius, WorldNode[,] in_nodes, int in_x, int in_y)
    {
        List<Vector2> toRet = new List<Vector2>();
        Point2D thisPoint = new Point2D(in_x, in_y);
        Point2D[] peri = AdjustPoints(thisPoint.GetPerimeter(), in_nodes);
        Point2D n = peri[4];
        Point2D nw = peri[5];
        Point2D e = peri[2];
        Point2D ne = peri[3];
        Point2D s = peri[0];
        Point2D se = peri[1];
        Point2D w = peri[6];
        Point2D sw = peri[7];

        float thisTime = CalculateTime(in_nodes, thisPoint, in_oceanRadius);
        Vector2 thisUV = new Vector2(thisTime, 0);
        float nTime = CalculateTime(in_nodes, n, in_oceanRadius);
        float eTime = CalculateTime(in_nodes, e, in_oceanRadius);
        float sTime = CalculateTime(in_nodes, s, in_oceanRadius);
        float wTime = CalculateTime(in_nodes, w, in_oceanRadius);
        float nwTime = CalculateTime(in_nodes, nw, in_oceanRadius);
        float neTime = CalculateTime(in_nodes, ne, in_oceanRadius);
        float seTime = CalculateTime(in_nodes, se, in_oceanRadius);
        float swTime = CalculateTime(in_nodes, sw, in_oceanRadius);

        Vector2 nwOne = Vector2.Lerp(thisUV, new Vector2(nwTime, 0), GameSettings.BLENDPOSITION);
        Vector2 nwTwo = Vector2.Lerp(new Vector2(nTime, 0), new Vector2(wTime, 0), GameSettings.BLENDPOSITION);
        Vector2 neOne = Vector2.Lerp(thisUV, new Vector2(neTime, 0), GameSettings.BLENDPOSITION);
        Vector2 neTwo = Vector2.Lerp(new Vector2(nTime, 0), new Vector2(eTime, 0), GameSettings.BLENDPOSITION);
        Vector2 seOne = Vector2.Lerp(thisUV, new Vector2(seTime, 0), GameSettings.BLENDPOSITION);
        Vector2 seTwo = Vector2.Lerp(new Vector2(sTime, 0), new Vector2(eTime, 0), GameSettings.BLENDPOSITION);
        Vector2 swOne = Vector2.Lerp(thisUV, new Vector2(swTime, 0), GameSettings.BLENDPOSITION);
        Vector2 swTwo = Vector2.Lerp(new Vector2(sTime, 0), new Vector2(wTime, 0), GameSettings.BLENDPOSITION);

        Vector2 nwUV = Vector2.Lerp(nwOne, nwTwo, 0.5f);
        Vector2 neUV = Vector2.Lerp(neOne, neTwo, 0.5f);
        Vector2 seUV = Vector2.Lerp(seOne, seTwo, 0.5f);
        Vector2 swUV = Vector2.Lerp(swOne, swTwo, 0.5f);

        toRet.Add(nwUV);
        toRet.Add(neUV);
        toRet.Add(seUV);
        toRet.Add(swUV);
        return toRet;
    }

    private float CalculateTimeFromRegion(int in_region, int in_regionLimit)
    {
        float toRet = ((float)in_region + 1.5f) / (float)(in_regionLimit);
        //Debug.Log(in_region + " / " + in_regionLimit + " = " + toRet);
        return toRet;
    }

    private float CalculateTimeFromRadius(float in_radius)
    {
        float loRadius = actualRadii[0];
        float hiRadius = actualRadii[actualRadii.Length - 1];
        float toRet = (in_radius - loRadius) / (hiRadius - loRadius);
        return toRet;
    }

    private float CalculateTime(WorldNode[,] in_nodes, Point2D in_point, float in_oceanRadius)
    {
        float loRadius = actualRadii[0];
        float toRet = (in_nodes[in_point.x, in_point.y].Get_meshRadius() - loRadius) / (in_oceanRadius - loRadius);
        return toRet;
    }

    // Extra Funks

    private Point2D[] AdjustPoints(Point2D[] in_peri, WorldNode[,] in_nodes)
    {
        Point2D[] peri = in_peri;
        if (peri[5].x < 0)
        {
            int lastX = in_nodes.GetLength(0) - 1;
            peri[5].x = lastX;
            peri[6].x = lastX;
            peri[7].x = lastX;
        }
        if (peri[3].x == in_nodes.GetLength(0))
        {
            peri[3].x = 0;
            peri[2].x = 0;
            peri[1].x = 0;
        }

        if (peri[3].y < 0)
        {
            peri[3].y = 0;
            peri[4].y = 0;
            peri[5].y = 0;
        }
        else if (peri[1].y >= in_nodes.GetLength(1))
        {
            int lastY = in_nodes.GetLength(1) - 1;
            peri[1].y = lastY;
            peri[0].y = lastY;
            peri[7].y = lastY;
        }
        return peri;
    }

    private Vector3 GetSphericalVector(int in_x, int in_y, float in_radius, WorldNode[,] in_nodes, GameData in_data)
    {
        int maxX = in_nodes.GetLength(0);
        int maxY = in_nodes.GetLength(1);
        float polarOffset = (in_data.settings.polarBuffer * Mathf.PI * 2.0f) / 180.0f;
        float xIncrement = (float)((Mathf.PI * 2.0f) / (maxX));
        float yIncrement = (float)((Mathf.PI - polarOffset) / maxY);
        float xRadians = (float)in_x * xIncrement;
        float yRadians = (polarOffset / 2.0f) + (in_y * yIncrement);
        float radius = in_radius;
        float yPos = radius * Mathf.Cos(yRadians);
        float subRadius = radius * Mathf.Sin(yRadians);
        float xPos = subRadius * Mathf.Sin(xRadians);
        float zPos = subRadius * Mathf.Cos(xRadians);
        return new Vector3(xPos, yPos, zPos);
    }

    private Vector3 GetSphericalVector(Point2D in_point, float in_radius, WorldNode[,] in_nodes, GameData in_data)
    {
        int maxX = in_nodes.GetLength(0);
        int maxY = in_nodes.GetLength(1);
        float polarOffset = (in_data.settings.polarBuffer * Mathf.PI * 2.0f) / 180.0f;
        float xIncrement = (float)((Mathf.PI * 2.0f) / (maxX));
        float yIncrement = (float)((Mathf.PI - polarOffset) / maxY);
        float xRadians = (float)in_point.x * xIncrement;
        float yRadians = (polarOffset / 2.0f) + (in_point.y * yIncrement);
        float radius = in_radius;
        float yPos = radius * Mathf.Cos(yRadians);
        float subRadius = radius * Mathf.Sin(yRadians);
        float xPos = subRadius * Mathf.Sin(xRadians);
        float zPos = subRadius * Mathf.Cos(xRadians);
        return new Vector3(xPos, yPos, zPos);
    }

    private Vector3[] GetSphericalVectors(Point2D[] in_points, float in_radius, WorldNode[,] in_nodes, GameData in_data)
    {
        // validation will need to occur here, or will it??
        // x validation for sure but y validation may not be needed because of how it projects
        // not even sure we need x validation either
        //PointValidator validator = new PointValidator(in_nodes);
        Vector3[] toRet = new Vector3[in_points.Length];
        int pos = 0;
        while (pos < in_points.Length)
        {
            toRet[pos] = GetSphericalVector(in_points[pos], in_radius, in_nodes, in_data);
            pos++;
        }
        return toRet;
    }

    //
    private Vector3[] GetCloseVertices(Point2D in_point, float in_thisRadius, int in_maxX, WorldNode[,] in_nodes, GameData in_data)
    {
        Vector3[] toRet = new Vector3[8];
        Vector3 center = GetSphericalVector(in_point, in_thisRadius, in_nodes, in_data);
        Vector3[] neighbors = GetSphericalVectors(in_point.GetPerimeter(), in_thisRadius, in_nodes, in_data);

        Vector3 top = Vector3.Lerp(center, neighbors[0], 0.5f);
        Vector3 right = Vector3.Lerp(center, neighbors[2], 0.5f);
        Vector3 bottom = Vector3.Lerp(center, neighbors[4], 0.5f);
        Vector3 left = Vector3.Lerp(center, neighbors[6], 0.5f);

        Vector3 topLeft = BilinearLerp(neighbors[7], neighbors[0], center, neighbors[6]); // nw n c w
        Vector3 topRight = BilinearLerp(neighbors[0], neighbors[1], neighbors[2], center); // n ne e c
        Vector3 bottomRight = BilinearLerp(center, neighbors[2], neighbors[3], neighbors[4]); // c e se s
        Vector3 bottomLeft = BilinearLerp(neighbors[6], center, neighbors[4], neighbors[5]); // w c s sw

        toRet[0] = top;
        toRet[1] = topRight;
        toRet[2] = right;
        toRet[3] = bottomRight;
        toRet[4] = bottom;
        toRet[5] = bottomLeft;
        toRet[6] = left;
        toRet[7] = topLeft;

        return toRet;
    }

    private Vector3 BilinearLerp(Vector3 in_a, Vector3 in_b, Vector3 in_c, Vector3 in_d)
    {
        Vector3 ab = Vector3.Lerp(in_a, in_b, 0.5f);
        Vector3 cd = Vector3.Lerp(in_c, in_d, 0.5f);
        Vector3 toRet = Vector3.Lerp(ab, cd, 0.5f);
        return toRet;
    }

    private Vector3[] GetPossibleVerticesFromSphericalSquare(int in_x, int in_y, float in_thisRadius, int in_maxX, WorldNode[,] in_nodes, GameData in_data)
    {
        Vector3[] toRet = new Vector3[8];
        Vector3 topLeft = GetSphericalVector(in_x, in_y, in_thisRadius, in_nodes, in_data);
        Vector3 botLeft = GetSphericalVector(in_x, in_y + 1, in_thisRadius, in_nodes, in_data);
        Vector3 topRight;
        Vector3 botRight;
        if (in_x + 1 == in_maxX)
        {
            topRight = GetSphericalVector(0, in_y, in_thisRadius, in_nodes, in_data);
            botRight = GetSphericalVector(0, in_y + 1, in_thisRadius, in_nodes, in_data);
        }
        else
        {
            topRight = GetSphericalVector(in_x + 1, in_y, in_thisRadius, in_nodes, in_data);
            botRight = GetSphericalVector(in_x + 1, in_y + 1, in_thisRadius, in_nodes, in_data);
        }
        Vector3 topMid = Vector3.Lerp(topLeft, topRight, 0.5f);
        Vector3 rightMid = Vector3.Lerp(topRight, botRight, 0.5f);
        Vector3 botMid = Vector3.Lerp(botLeft, botRight, 0.5f);
        Vector3 leftMid = Vector3.Lerp(topLeft, botLeft, 0.5f);

        toRet[0] = topLeft;
        toRet[1] = topMid;
        toRet[2] = topRight;
        toRet[3] = rightMid;
        toRet[4] = botRight;
        toRet[5] = botMid;
        toRet[6] = botLeft;
        toRet[7] = leftMid;
        return toRet;
    }

    private Square[,] GetSquaresFromNodes(WorldNode[,] in_nodes)
    {
        int x = 0;
        int maxX = in_nodes.GetLength(0);
        int y = 0;
        int maxY = in_nodes.GetLength(1);
        Square[,] toRet = new Square[maxX, maxY];
        PointValidator validator = new PointValidator(in_nodes);
        while (y < maxY)
        {
            while (x < maxX)
            {
                Point2D posA = validator.ValidatePoint(x, y);
                Point2D posB = validator.ValidatePoint(x + 1, y);
                Point2D posC = validator.ValidatePoint(x + 1, y + 1);
                Point2D posD = validator.ValidatePoint(x, y + 1);
                Vector3 a = new Vector3(posA.x, in_nodes[posA.x, posA.y].Get_layerElev(), posA.y);
                Vector3 b = new Vector3(posB.x, in_nodes[posB.x, posB.y].Get_layerElev(), posB.y);
                Vector3 c = new Vector3(posC.x, in_nodes[posC.x, posC.y].Get_layerElev(), posC.y);
                Vector3 d = new Vector3(posD.x, in_nodes[posD.x, posD.y].Get_layerElev(), posD.y);
                Square loc = new Square(a, b, c, d);
                toRet[x, y] = loc;
                x++;
            }
            x = 0;
            y++;
        }
        return toRet;
    }

    private Vector3[,] RadiiToSpherePositions(WorldNode[,] in_nodes, GameData in_data)
    {
        float polarOffset = (in_data.settings.polarBuffer * Mathf.PI * 2.0f) / 180.0f;

        int x = 0;
        int maxX = in_nodes.GetLength(0);
        int y = 0;
        int maxY = in_nodes.GetLength(1);
        //y = maxY - 1;
        float xIncrement = (float)((Mathf.PI * 2.0) / (maxX));
        float yIncrement = (float)((Mathf.PI - polarOffset) / maxY);
        float yRadians = polarOffset / 2.0f;
        Vector3[,] toRet = new Vector3[maxX, maxY];
        while (y < maxY)
        {
            while (x < maxX)
            {
                float xRadians = (float)x * xIncrement;
                float radius = in_nodes[x, y].Get_meshRadius();
                float yPos = radius * Mathf.Cos(yRadians);
                float subRadius = radius * Mathf.Sin(yRadians);
                float xPos = subRadius * Mathf.Sin(xRadians);
                float zPos = subRadius * Mathf.Cos(xRadians);
                Vector3 temp = new Vector3(xPos, yPos, zPos);
                toRet[x, y] = temp;
                x++;
            }
            yRadians += yIncrement;
            x = 0;
            y++;
        }
        return toRet;
    }
}
