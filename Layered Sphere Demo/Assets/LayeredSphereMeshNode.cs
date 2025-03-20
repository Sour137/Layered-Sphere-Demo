using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using System.Net;

public class LayeredSphereMeshNode
{
    // object to build a vertical slice of the layeredSphereMesh for localization of the world render
    GameObject landNode;
    GameObject oceanNode;
    Point2D location;

    //public LayeredSphereMeshNode()
    //{
    //    node = null;
    //}

    public void Disable()
    {
        Object.Destroy(landNode);
        Object.Destroy(oceanNode);
    }

    public LayeredSphereMeshNode(WorldGrid in_grid, Point2D in_point, float[] in_layers, float[] in_radii, float[,] in_oceanUVMap, GameData in_data, ColorSettings in_colors)
    {
        location = in_point;

        landNode = new GameObject();
        landNode.name = "Land Node " + location.GetStringForm();

        oceanNode = new GameObject();
        oceanNode.name = "Ocean Node " + location.GetStringForm();

        BuildMesh(in_grid, in_point, in_layers, in_radii, in_data, in_colors);
        BuildOceanMesh(in_grid, in_point, in_layers, in_radii, in_oceanUVMap, in_data, in_colors);
        BuildColliders(8, in_grid, in_point, in_layers, in_radii, in_data);
    }

    private void BuildMesh(WorldGrid in_grid, Point2D in_point, float[] in_layers, float[] in_radii, GameData in_data, ColorSettings in_colors)
    {
        WorldNode[,] worldNodes = in_grid.Get_Nodes();

        landNode.AddComponent<MeshRenderer>();
        landNode.GetComponent<MeshRenderer>().sharedMaterial = in_colors.terrainMaterial;
        landNode.AddComponent<MeshFilter>();

        // for the ocean rehash I think we need to duplicate most of below but assign it to the oceanNode mesh
        // GetConfigs will need a special variation but I think everything else may be fine?? Unless I want to optimize??

        //BuildOceanMesh(in_grid, in_point, in_layers, in_radii, in_settings, in_colors);

        Mesh nodeMesh = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        nodeMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        int maxX = worldNodes.GetLength(0);
        Point2D[] perimeter = location.GetValidatedPerimeter(new PointValidator(worldNodes));
        int oceanPos = in_grid.Get_oceanPos();
        int pos = oceanPos + 1; //0;
        while (pos < in_layers.Length)
        {
            // as copied from LayeredSphereMesh, but completed as a vertical slice in one object.
            bool occluded = false;
            if (worldNodes[location.x, location.y].Get_layerElev() != in_layers[pos])
                occluded = true;
            float thisRadius = in_radii[pos];
            float lowRadius;
            if (pos == 0)
                lowRadius = in_radii[0];
            else
                lowRadius = in_radii[pos - 1];

            // config for this layer
            WorldNode[] localNodes = GetLocalNodes(location, worldNodes, perimeter);
            int[] configs = GetConfigs(in_layers[pos], location, localNodes, worldNodes.GetLength(1)); // this needs to be sent to the collider

            // configs to mesh, verts first
            Vector3 center = GetSphericalVector(location, thisRadius, worldNodes, in_data);
            Vector3[] upperVerts = GetVertices(location, thisRadius, maxX, worldNodes, in_data);
            Vector3[] lowerVerts = GetVertices(location, lowRadius, maxX, worldNodes, in_data);
            // if we want to merge the collider funk, we would do it here
            List<Vector3> locVerts = VertsFromConfigs(localNodes, upperVerts, lowerVerts, center, occluded, configs, in_layers[pos]);
            verts.AddRange(locVerts);

            // tris
            List<int> locTris = TrisFromConfigs(verts.Count, occluded, configs, localNodes, in_layers[pos]);
            tris.AddRange(locTris);

            // normals
            if (in_data.settings.smootheNormals)
            {
                List<Vector3> locNormals = NormalsFromConfigs(upperVerts, lowerVerts, center, occluded, configs, localNodes, in_layers[pos]);
                normals.AddRange(locNormals);
            }

            // probably needs an ocean variant
            List<Vector2> locUVS = UVFromConfigs(thisRadius, in_radii, occluded, configs, localNodes, in_layers[pos]);
            uvs.AddRange(locUVS);

            pos++;

        }

        // mesh assignment
        nodeMesh.vertices = verts.ToArray();
        nodeMesh.triangles = tris.ToArray();
        if (in_data.settings.smootheNormals)
            nodeMesh.normals = normals.ToArray();
        else
            nodeMesh.RecalculateNormals();
        nodeMesh.uv = uvs.ToArray();

        // GameObject assignment
        landNode.GetComponent<MeshFilter>().mesh = nodeMesh;
    }

    private void BuildOceanMesh(WorldGrid in_grid, Point2D in_point, float[] in_layers, float[] in_radii, float[,] in_oceanUVMap, GameData in_data, ColorSettings in_colors)
    {
        WorldNode[,] worldNodes = in_grid.Get_Nodes();

        oceanNode.AddComponent<MeshRenderer>();
        oceanNode.GetComponent<MeshRenderer>().sharedMaterial = in_colors.OceanMaterial;
        oceanNode.AddComponent<MeshFilter>();

        Mesh nodeMesh = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        nodeMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        int maxX = worldNodes.GetLength(0);
        Point2D[] perimeter = location.GetValidatedPerimeter(new PointValidator(worldNodes));
        int oceanPos = in_grid.Get_oceanPos();

        bool occluded = false;
        if (worldNodes[location.x, location.y].Get_layerElev() > in_layers[oceanPos])
            occluded = true;
        float thisRadius = in_radii[oceanPos];
        // optimally we shouldn't need the lower radius
        float lowRadius;
        if (oceanPos == 0)
            lowRadius = in_radii[0];
        else
            lowRadius = in_radii[oceanPos - 1];

        // config for this layer
        WorldNode[] localNodes = GetLocalNodes(location, worldNodes, perimeter);
        int[] configs = GetOceanConfigs(in_layers[oceanPos], in_layers[oceanPos], location, localNodes, worldNodes.GetLength(1));

        // configs to mesh, verts first
        Vector3 center = GetSphericalVector(location, thisRadius, worldNodes, in_data);
        Vector3[] upperVerts = GetVertices(location, thisRadius, maxX, worldNodes, in_data);
        Vector3[] lowerVerts = GetVertices(location, lowRadius, maxX, worldNodes, in_data);
        List<Vector3> locVerts = VertsFromConfigs(localNodes, upperVerts, lowerVerts, center, occluded, configs, in_layers[oceanPos]);
        verts.AddRange(locVerts);

        // tris
        List<int> locTris = TrisFromConfigs(verts.Count, occluded, configs, localNodes, in_layers[oceanPos]);
        tris.AddRange(locTris);

        // normals
        if (in_data.settings.smootheNormals)
        {
            List<Vector3> locNormals = NormalsFromConfigs(upperVerts, lowerVerts, center, occluded, configs, localNodes, in_layers[oceanPos]);
            normals.AddRange(locNormals);
        }

        //List<Vector2> locUVS = OceanUVFromConfigs(thisRadius, in_radii[oceanPos], in_radii, occluded, configs, localNodes, in_layers[oceanPos]);
        List<Vector2> locUVS = OceanUVFromConfigs(in_point, in_oceanUVMap, occluded, configs, in_layers[oceanPos], in_grid);
        uvs.AddRange(locUVS);

        // mesh assignment
        nodeMesh.vertices = verts.ToArray();
        nodeMesh.triangles = tris.ToArray();
        if (in_data.settings.smootheNormals)
            nodeMesh.normals = normals.ToArray();
        else
            nodeMesh.RecalculateNormals();
        nodeMesh.uv = uvs.ToArray();

        // GameObject assignment
        oceanNode.GetComponent<MeshFilter>().mesh = nodeMesh;
    }

    private void BuildColliders(int in_resolution, WorldGrid in_grid, Point2D in_point, float[] in_layers, float[] in_radii, GameData in_data)
    {
        if (in_resolution == 1)
        {
            GameObject colliderObject = new GameObject();
            NodeData data = colliderObject.AddComponent<NodeData>();
            data.location = location;
            data.subNode = 0;
            MeshCollider locCollider = colliderObject.AddComponent<MeshCollider>();
            locCollider.name = location.GetStringForm();
            locCollider.sharedMesh = landNode.GetComponent<MeshFilter>().mesh;
            locCollider.transform.parent = landNode.transform;
        }
        else
        {
            List<Vector3>[] verts = InitializeColliderVector3();
            List<int>[] tris = InitializeColliderInt();

            WorldNode[,] worldNodes = in_grid.Get_Nodes();
            int maxX = worldNodes.GetLength(0);
            Point2D[] perimeter = location.GetValidatedPerimeter(new PointValidator(worldNodes));
            int pos = 0;
            while (pos < in_layers.Length)
            {
                bool occluded = false;
                if (worldNodes[location.x, location.y].Get_layerElev() != in_layers[pos])
                    occluded = true;
                float thisRadius = in_radii[pos];
                float lowRadius;
                if (pos == 0)
                    lowRadius = in_radii[0];
                else
                    lowRadius = in_radii[pos - 1];

                // config for this layer
                WorldNode[] localNodes = GetLocalNodes(location, worldNodes, perimeter);
                int[] configs = GetConfigs(in_layers[pos], location, localNodes, worldNodes.GetLength(1)); // this needs to be sent to the collider

                // configs to mesh, verts first
                Vector3 center = GetSphericalVector(location, thisRadius, worldNodes, in_data);
                Vector3[] upperVerts = GetVertices(location, thisRadius, maxX, worldNodes, in_data);
                Vector3[] lowerVerts = GetVertices(location, lowRadius, maxX, worldNodes, in_data);
                verts[0].AddRange(VertsFromConfigs(0, localNodes, upperVerts, lowerVerts, center, occluded, configs, in_layers[pos]));
                verts[1].AddRange(VertsFromConfigs(1, localNodes, upperVerts, lowerVerts, center, occluded, configs, in_layers[pos]));
                verts[2].AddRange(VertsFromConfigs(2, localNodes, upperVerts, lowerVerts, center, occluded, configs, in_layers[pos]));
                verts[3].AddRange(VertsFromConfigs(3, localNodes, upperVerts, lowerVerts, center, occluded, configs, in_layers[pos]));
                verts[4].AddRange(VertsFromConfigs(4, localNodes, upperVerts, lowerVerts, center, occluded, configs, in_layers[pos]));
                verts[5].AddRange(VertsFromConfigs(5, localNodes, upperVerts, lowerVerts, center, occluded, configs, in_layers[pos]));
                verts[6].AddRange(VertsFromConfigs(6, localNodes, upperVerts, lowerVerts, center, occluded, configs, in_layers[pos]));
                verts[7].AddRange(VertsFromConfigs(7, localNodes, upperVerts, lowerVerts, center, occluded, configs, in_layers[pos]));

                // tris
                tris[0].AddRange(TrisFromConfigs(0, verts[0].Count, occluded, configs, localNodes, in_layers[pos]));
                tris[1].AddRange(TrisFromConfigs(1, verts[1].Count, occluded, configs, localNodes, in_layers[pos]));
                tris[2].AddRange(TrisFromConfigs(2, verts[2].Count, occluded, configs, localNodes, in_layers[pos]));
                tris[3].AddRange(TrisFromConfigs(3, verts[3].Count, occluded, configs, localNodes, in_layers[pos]));
                tris[4].AddRange(TrisFromConfigs(4, verts[4].Count, occluded, configs, localNodes, in_layers[pos]));
                tris[5].AddRange(TrisFromConfigs(5, verts[5].Count, occluded, configs, localNodes, in_layers[pos]));
                tris[6].AddRange(TrisFromConfigs(6, verts[6].Count, occluded, configs, localNodes, in_layers[pos]));
                tris[7].AddRange(TrisFromConfigs(7, verts[7].Count, occluded, configs, localNodes, in_layers[pos]));

                // shouldn't need to smoothe normals for the collider

                // uvs may not be needed?


                pos++;
            }

            pos = 0;
            while (pos < 8)
            {
                GameObject colliderObject = new GameObject();
                NodeData data = colliderObject.AddComponent<NodeData>();
                data.location = location;
                data.subNode = pos;
                Mesh colliderMesh = new Mesh();
                colliderMesh.vertices = verts[pos].ToArray();
                colliderMesh.triangles = tris[pos].ToArray();
                colliderMesh.RecalculateNormals();
                MeshCollider locCollider = colliderObject.AddComponent<MeshCollider>();
                locCollider.name = in_point.GetStringForm() + ":" + pos.ToString();
                locCollider.sharedMesh = colliderMesh;
                locCollider.transform.parent = landNode.transform;
                pos++;
            }
        }


    }

    private List<Vector3>[] InitializeColliderVector3()
    {
        List<Vector3>[] toRet = new List<Vector3>[8];
        int pos = 0;
        while (pos < 8)
        {
            toRet[pos] = new List<Vector3>();
            pos++;
        }
        return toRet;
    }

    private List<Vector2>[] InitializeColliderVector2()
    {
        List<Vector2>[] toRet = new List<Vector2>[8];
        int pos = 0;
        while (pos < 8)
        {
            toRet[pos] = new List<Vector2>();
            pos++;
        }
        return toRet;
    }

    private List<int>[] InitializeColliderInt()
    {
        List<int>[] toRet = new List<int>[8];
        int pos = 0;
        while (pos < 8)
        {
            toRet[pos] = new List<int>();
            pos++;
        }
        return toRet;
    }



    // UVs
    private List<Vector2> OceanUVFromConfigs(Point2D in_loc, float[,] in_oceanUVMap, bool in_coreOccluded, int[] in_configs, float in_layerElev, WorldGrid in_grid)
    {
        List<Vector2> toRet = new List<Vector2>();

        WorldNode[] localNodes = GetLocalNodes(in_loc, in_grid.Get_Nodes(), in_loc.GetValidatedPerimeter(new PointValidator(in_grid.Get_Nodes())));
        float[] periUV = GetPeriUVS(in_loc.GetValidatedPerimeter(new PointValidator(in_grid.Get_Nodes())), in_oceanUVMap);
        float centerUV = in_oceanUVMap[in_loc.x, in_loc.y];

        Vector2 center = new Vector2(centerUV, 0);
        Vector2 peri_n = new Vector2(periUV[0], 0);
        Vector2 peri_ne = new Vector2(periUV[1], 0);
        Vector2 peri_e = new Vector2(periUV[2], 0);
        Vector2 peri_se = new Vector2(periUV[3], 0);
        Vector2 peri_s = new Vector2(periUV[4], 0);
        Vector2 peri_sw = new Vector2(periUV[5], 0);
        Vector2 peri_w = new Vector2(periUV[6], 0);
        Vector2 peri_nw = new Vector2(periUV[7], 0);

        Vector2 n = Vector2.Lerp(center, peri_n, 0.5f);
        Vector2 e = Vector2.Lerp(center, peri_e, 0.5f);
        Vector2 s = Vector2.Lerp(center, peri_s, 0.5f);
        Vector2 w = Vector2.Lerp(center, peri_w, 0.5f);

        Vector2 ne = QuadLerp(center, peri_n, peri_ne, peri_e, 0.5f, 0.5f);
        Vector2 se = QuadLerp(center, peri_e, peri_se, peri_s, 0.5f, 0.5f);
        Vector2 sw = QuadLerp(center, peri_s, peri_sw, peri_w, 0.5f, 0.5f);
        Vector2 nw = QuadLerp(center, peri_w, peri_nw, peri_n, 0.5f, 0.5f);


        if (!in_coreOccluded)
        {
            // nw
            toRet.Add(n);
            toRet.Add(center);
            toRet.Add(w);
            // ne
            toRet.Add(n);
            toRet.Add(e);
            toRet.Add(center);
            // se
            toRet.Add(center);
            toRet.Add(e);
            toRet.Add(s);
            // sw
            toRet.Add(w);
            toRet.Add(center);
            toRet.Add(s);
        }

        bool cornerOccluded = false;
        if (Corner_NW(ref cornerOccluded, in_configs[0], in_layerElev, localNodes))
        {
            if (!cornerOccluded)
            {
                toRet.Add(n);
                toRet.Add(w);
                toRet.Add(nw);
            }
        }
        if (Corner_NE(ref cornerOccluded, in_configs[1], in_layerElev, localNodes))
        {
            if (!cornerOccluded)
            {
                toRet.Add(n);
                toRet.Add(ne);
                toRet.Add(e);
            }
        }
        if (Corner_SE(ref cornerOccluded, in_configs[2], in_layerElev, localNodes))
        {
            if (!cornerOccluded)
            {
                toRet.Add(e);
                toRet.Add(se);
                toRet.Add(s);
            }
        }
        if (Corner_SW(ref cornerOccluded, in_configs[3], in_layerElev, localNodes))
        {
            if (!cornerOccluded)
            {
                toRet.Add(w);
                toRet.Add(s);
                toRet.Add(sw);
            }
        }

        return toRet;
    }

    private Vector2 QuadLerp(Vector2 in_a, Vector2 in_b, Vector2 in_c, Vector2 in_d, float in_x, float in_y)
    {
        Vector2 abu = Vector2.Lerp(in_a, in_b, in_x);
        Vector2 dcu = Vector2.Lerp(in_d, in_c, in_x);
        return Vector2.Lerp(abu, dcu, in_y);
    }

    private float[] GetPeriUVS(Point2D[] in_peri, float[,] in_OceanUVMap)
    {
        float[] toRet = new float[in_peri.Length];
        int pos = 0;
        while (pos < in_peri.Length)
        {
            Point2D loc = in_peri[pos];
            toRet[pos] = in_OceanUVMap[loc.x, loc.y];
            pos++;
        }
        return toRet;
    }

    private List<Vector2> OceanUVFromConfigs(float in_thisRadius, float in_sealevelRadius, float[] in_radii, bool in_coreOccluded, int[] in_configs, WorldNode[] in_localNodes, float in_layerElev)
    {
        List<Vector2> toRet = new List<Vector2>();
        float score = CalculateTimeFromRadius(in_thisRadius, in_sealevelRadius, in_radii);
        int offset = GetCountOffset(in_coreOccluded, in_configs, in_localNodes, in_layerElev);
        int pos = 0;
        while (pos < offset)
        {
            toRet.Add(new Vector2(score, 0));
            pos++;
        }
        return toRet;
    }

    private List<Vector2> UVFromConfigs(float in_thisRadius, float[] in_radii, bool in_coreOccluded, int[] in_configs, WorldNode[] in_localNodes, float in_layerElev)
    {
        List<Vector2> toRet = new List<Vector2>();
        float score = CalculateTimeFromRadius(in_thisRadius, in_radii);
        int offset = GetCountOffset(in_coreOccluded, in_configs, in_localNodes, in_layerElev);
        int pos = 0;
        while (pos < offset)
        {
            toRet.Add(new Vector2(score, 0));
            pos++;
        }
        return toRet;
    }

    private float CalculateTimeFromRadius(float in_radius, float[] in_radii)
    {
        float loRadius = in_radii[0];
        float hiRadius = in_radii[in_radii.Length - 1];
        float toRet = (in_radius - loRadius) / (hiRadius - loRadius);
        return toRet;
    }

    private float CalculateTimeFromRadius(float in_radius, float in_sealevelRadius, float[] in_radii)
    {
        float loRadius = in_radii[0];
        float toRet = (in_radius - loRadius) / (in_sealevelRadius - loRadius);
        return toRet;
    }

    public void InjectUVs(Vector2[] in_LandUVSet, Vector2[] in_OceanUVSet, float[] in_layers, float[] in_radii, WorldGrid in_grid)
    {
        // each position in the array is assumed to be a city level coordinate, currently 8, may be 5 in the future
        InjectOcaenUVs(in_OceanUVSet, in_layers, in_radii, in_grid);
        InjectLandUVs(in_LandUVSet, in_layers, in_grid);
    }

    private void InjectOcaenUVs(Vector2[] in_uvSet, float[] in_layers, float[] in_radii, WorldGrid in_grid)
    {
        List<Vector2> oceanUV = new List<Vector2>();
        int oceanPos = in_grid.Get_oceanPos();
        bool occluded = false;
        if (in_grid.Get_node(location).Get_layerElev() > in_layers[oceanPos])
            occluded = true;

        Point2D[] perimeter = location.GetValidatedPerimeter(new PointValidator(in_grid.Get_Nodes()));

        WorldNode[] localNodes = GetLocalNodes(location, in_grid.Get_Nodes(), perimeter);
        int[] configs = GetOceanConfigs(in_layers[oceanPos], in_layers[oceanPos], location, localNodes, in_grid.Get_Nodes().GetLength(1));

        if (in_uvSet.Length == oceanNode.GetComponent<MeshFilter>().mesh.uv.Length)
        {
            for (int i = 0; i < in_uvSet.Length; i++)
                oceanUV.Add(in_uvSet[i]);
        }
        else
        {
            if (!occluded)
            {
                // nw
                for (int i = 0; i < 3; i++)
                    oceanUV.Add(in_uvSet[1]);
                // ne
                for (int i = 0; i < 3; i++)
                    oceanUV.Add(in_uvSet[2]);
                // se
                for (int i = 0; i < 3; i++)
                    oceanUV.Add(in_uvSet[6]);
                // sw
                for (int i = 0; i < 3; i++)
                    oceanUV.Add(in_uvSet[5]);
            }

            bool cornerOccluded = false;
            if (Corner_NW(ref cornerOccluded, configs[0], in_layers[oceanPos], localNodes))
            {
                if (!cornerOccluded)
                {
                    for (int i = 0; i < 3; i++)
                        oceanUV.Add(in_uvSet[0]);
                }
            }
            if (Corner_NE(ref cornerOccluded, configs[1], in_layers[oceanPos], localNodes))
            {
                if (!cornerOccluded)
                {
                    for (int i = 0; i < 3; i++)
                        oceanUV.Add(in_uvSet[3]);
                }
            }
            if (Corner_SE(ref cornerOccluded, configs[2], in_layers[oceanPos], localNodes))
            {
                if (!cornerOccluded)
                {
                    for (int i = 0; i < 3; i++)
                        oceanUV.Add(in_uvSet[7]);
                }
            }
            if (Corner_SW(ref cornerOccluded, configs[3], in_layers[oceanPos], localNodes))
            {
                if (!cornerOccluded)
                {
                    for (int i = 0; i < 3; i++)
                        oceanUV.Add(in_uvSet[4]);
                }
            }
        }

        if (oceanNode.GetComponent<MeshFilter>().mesh.uv.Length == oceanUV.Count)
        {
            oceanNode.GetComponent<MeshFilter>().mesh.uv3 = oceanUV.ToArray();
        }
        else
            Debug.Log(location.GetStringForm() + " had the incorrect UV size : " + oceanUV.Count + "/" + oceanNode.GetComponent<MeshFilter>().mesh.uv.Length);

        //oceanNode.GetComponent<MeshFilter>().mesh.uv3 = in_uvSet;
    }

    private void InjectLandUVs(Vector2[] in_uvSet, float[] in_layers, WorldGrid in_grid)
    {
        // each position in the array is assumed to be a city level coordinate, currently 8, may be 5 in the future
        int pos = in_grid.Get_oceanPos() + 1; // 0;
        WorldNode[,] nodes = in_grid.Get_Nodes();
        Point2D[] perimeter = location.GetValidatedPerimeter(new PointValidator(nodes));
        List<Vector2> uvs = new List<Vector2>();

        while (pos < in_layers.Length)
        {
            bool coreOccluded = false;
            bool cornerOccluded = false;
            if (nodes[location.x, location.y].Get_layerElev() != in_layers[pos])
                coreOccluded = true;

            // config for this layer
            WorldNode[] localNodes = GetLocalNodes(location, nodes, perimeter);
            int[] configs = GetConfigs(in_layers[pos], location, localNodes, nodes.GetLength(1)); // this needs to be sent to the collider

            if (!coreOccluded)
            {
                for (int i = 0; i < 3; i++)
                    uvs.Add(in_uvSet[1]);
                for (int i = 0; i < 3; i++)
                    uvs.Add(in_uvSet[2]);
                for (int i = 0; i < 3; i++)
                    uvs.Add(in_uvSet[6]);
                for (int i = 0; i < 3; i++)
                    uvs.Add(in_uvSet[5]);
            }

            if (Corner_NW(ref cornerOccluded, configs[0], in_layers[pos], localNodes))
                uvs.AddRange(InjectNW(in_uvSet[0], cornerOccluded, configs[0]));
            else if (configs[0] == 2)
                for (int i = 0; i < 6; i++)
                    uvs.Add(in_uvSet[1]);

            if (Corner_NE(ref cornerOccluded, configs[1], in_layers[pos], localNodes))
                uvs.AddRange(InjectNE(in_uvSet[3], cornerOccluded, configs[1]));
            else if (configs[1] == 1)
                for (int i = 0; i < 6; i++)
                    uvs.Add(in_uvSet[2]);

            if (Corner_SE(ref cornerOccluded, configs[2], in_layers[pos], localNodes))
                uvs.AddRange(InjectSE(in_uvSet[7], cornerOccluded, configs[2]));
            else if (configs[2] == 8)
                for (int i = 0; i < 6; i++)
                    uvs.Add(in_uvSet[6]);

            if (Corner_SW(ref cornerOccluded, configs[3], in_layers[pos], localNodes))
                uvs.AddRange(InjectSW(in_uvSet[4], cornerOccluded, configs[3]));
            else if (configs[3] == 4)
                for (int i = 0; i < 6; i++)
                    uvs.Add(in_uvSet[5]);
            pos++;
        }

        landNode.GetComponent<MeshFilter>().mesh.uv3 = uvs.ToArray();
    }

    private List<Vector2> InjectNW(Vector2 in_toInject, bool in_occluded, int in_config)
    {
        List<Vector2> toRet = new List<Vector2>();
        if (!in_occluded)
            for (int i = 0; i < 3; i++)
                toRet.Add(in_toInject);
        if ((in_config == 3) ||
            (in_config == 5) ||
            (in_config == 6) ||
            (in_config == 13))
            for (int i = 0; i < 6; i++)
                toRet.Add(in_toInject);
        return toRet;
    }
    private List<Vector2> InjectNE(Vector2 in_toInject, bool in_occluded, int in_config)
    {
        List<Vector2> toRet = new List<Vector2>();
        if (!in_occluded)
            for (int i = 0; i < 3; i++)
                toRet.Add(in_toInject);
        if ((in_config == 3) ||
            (in_config == 9) ||
            (in_config == 10) ||
            (in_config == 14))
            for (int i = 0; i < 6; i++)
                toRet.Add(in_toInject);
        return toRet;
    }
    private List<Vector2> InjectSE(Vector2 in_toInject, bool in_occluded, int in_config)
    {
        List<Vector2> toRet = new List<Vector2>();
        if (!in_occluded)
            for (int i = 0; i < 3; i++)
                toRet.Add(in_toInject);
        if ((in_config == 5) ||
            (in_config == 7) ||
            (in_config == 9) ||
            (in_config == 12))
            for (int i = 0; i < 6; i++)
                toRet.Add(in_toInject);
        return toRet;
    }
    private List<Vector2> InjectSW(Vector2 in_toInject, bool in_occluded, int in_config)
    {
        List<Vector2> toRet = new List<Vector2>();
        if (!in_occluded)
            for (int i = 0; i < 3; i++)
                toRet.Add(in_toInject);
        if ((in_config == 6) ||
            (in_config == 10) ||
            (in_config == 11) ||
            (in_config == 12))
            for (int i = 0; i < 6; i++)
                toRet.Add(in_toInject);
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

    private List<int> TrisFromConfigs(int in_subNode, int in_count, bool in_coreOccluded, int[] in_configs, WorldNode[] in_nodes, float in_layerElev)
    {
        List<int> toRet = new List<int>();
        int offset = GetCountOffset(in_subNode, in_coreOccluded, in_configs, in_nodes, in_layerElev);
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

    private int GetCountOffset(int in_subNode, bool in_coreOccluded, int[] in_configs, WorldNode[] in_localNodes, float in_layerElev)
    {
        bool cornerOccluded = false;
        int toRet = 0;
        switch (in_subNode)
        {
            case 0:
                if (Corner_NW(ref cornerOccluded, in_configs[0], in_layerElev, in_localNodes))
                {
                    if (!cornerOccluded)
                        toRet += 3;
                    if ((in_configs[0] == 3) ||
                        (in_configs[0] == 5) ||
                        (in_configs[0] == 6) ||
                        (in_configs[0] == 13))
                        toRet += 6;
                }
                break;
            case 1:
                if (!in_coreOccluded)
                    toRet += 3;
                if ((in_configs[0] == 2) && (!Corner_NW(ref cornerOccluded, in_configs[0], in_layerElev, in_localNodes)))
                    toRet += 6;
                break;
            case 2:
                if (!in_coreOccluded)
                    toRet += 3;
                if ((in_configs[1] == 1) && (!Corner_NE(ref cornerOccluded, in_configs[1], in_layerElev, in_localNodes)))
                    toRet += 6;
                break;
            case 3:
                if (Corner_NE(ref cornerOccluded, in_configs[1], in_layerElev, in_localNodes))
                {
                    if (!cornerOccluded)
                        toRet += 3;
                    if ((in_configs[1] == 3) ||
                        (in_configs[1] == 9) ||
                        (in_configs[1] == 10) ||
                        (in_configs[1] == 14))
                        toRet += 6;
                }
                break;
            case 4:
                if (Corner_SW(ref cornerOccluded, in_configs[3], in_layerElev, in_localNodes))
                {
                    if (!cornerOccluded)
                        toRet += 3;
                    if ((in_configs[3] == 6) ||
                        (in_configs[3] == 10) ||
                        (in_configs[3] == 11) ||
                        (in_configs[3] == 12))
                        toRet += 6;
                }
                break;
            case 5:
                if (!in_coreOccluded)
                    toRet += 3;
                if ((in_configs[3] == 4) && (!Corner_SW(ref cornerOccluded, in_configs[3], in_layerElev, in_localNodes)))
                    toRet += 6;
                break;
            case 6:
                if (!in_coreOccluded)
                    toRet += 3;
                if ((in_configs[2] == 8) && (!Corner_SE(ref cornerOccluded, in_configs[2], in_layerElev, in_localNodes)))
                    toRet += 6;
                break;
            case 7:
                if (Corner_SE(ref cornerOccluded, in_configs[2], in_layerElev, in_localNodes))
                {
                    if (!cornerOccluded)
                        toRet += 3;
                    if ((in_configs[2] == 5) ||
                        (in_configs[2] == 7) ||
                        (in_configs[2] == 9) ||
                        (in_configs[2] == 12))
                        toRet += 6;
                }
                break;
            default:
                break;
        }
        return toRet;
    }

    // verts section
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

    private List<Vector3> VertsFromConfigs(int in_subNode, WorldNode[] in_localNodes, Vector3[] in_upperVertices, Vector3[] in_lowerVertices, Vector3 in_center, bool in_coreOcculuded, int[] in_configs, float in_layerElev)
    {
        List<Vector3> toRet = new List<Vector3>();
        bool cornerOccluded = false;
        switch (in_subNode)
        {
            case 0: // far nw
                if (Corner_NW(ref cornerOccluded, in_configs[0], in_layerElev, in_localNodes))
                    toRet = Quadrant_NW(toRet, cornerOccluded, in_configs, in_upperVertices, in_lowerVertices);
                break;
            case 1: // central nw
                if (!in_coreOcculuded)
                {
                    Vector3 a = in_upperVertices[0];
                    Vector3 b = in_center;
                    Vector3 c = in_upperVertices[6];

                    toRet.Add(a);
                    toRet.Add(b);
                    toRet.Add(c);
                }
                if ((in_configs[0] == 2) && (!Corner_NW(ref cornerOccluded, in_configs[0], in_layerElev, in_localNodes)))
                    toRet = Diamond_NW(toRet, in_upperVertices, in_lowerVertices);
                break;
            case 2: // central ne
                if (!in_coreOcculuded)
                {
                    Vector3 a = in_upperVertices[0];
                    Vector3 b = in_upperVertices[2];
                    Vector3 c = in_center;

                    toRet.Add(a);
                    toRet.Add(b);
                    toRet.Add(c);
                }
                if ((in_configs[1] == 1) && (!Corner_NE(ref cornerOccluded, in_configs[1], in_layerElev, in_localNodes)))
                    toRet = Diamond_NE(toRet, in_upperVertices, in_lowerVertices);
                break;
            case 3: // far ne
                if (Corner_NE(ref cornerOccluded, in_configs[1], in_layerElev, in_localNodes))
                    toRet.AddRange(Quadrant_NE(toRet, cornerOccluded, in_configs, in_upperVertices, in_lowerVertices));
                break;
            case 4: // far sw
                if (Corner_SW(ref cornerOccluded, in_configs[3], in_layerElev, in_localNodes))
                    toRet.AddRange(Quadrant_SW(toRet, cornerOccluded, in_configs, in_upperVertices, in_lowerVertices));
                break;
            case 5: // central sw
                if (!in_coreOcculuded)
                {
                    Vector3 a = in_upperVertices[6];
                    Vector3 b = in_center;
                    Vector3 c = in_upperVertices[4];

                    toRet.Add(a);
                    toRet.Add(b);
                    toRet.Add(c);
                }
                if ((in_configs[3] == 4) && (!Corner_SW(ref cornerOccluded, in_configs[3], in_layerElev, in_localNodes)))
                    toRet = Diamond_SW(toRet, in_upperVertices, in_lowerVertices);
                break;
            case 6: // central se
                if (!in_coreOcculuded)
                {
                    Vector3 a = in_center;
                    Vector3 b = in_upperVertices[2];
                    Vector3 c = in_upperVertices[4];

                    toRet.Add(a);
                    toRet.Add(b);
                    toRet.Add(c);
                }
                if ((in_configs[2] == 8) && (!Corner_SE(ref cornerOccluded, in_configs[2], in_layerElev, in_localNodes)))
                    toRet = Diamond_SE(toRet, in_upperVertices, in_lowerVertices);
                break;
            case 7: // far se
                if (Corner_SE(ref cornerOccluded, in_configs[2], in_layerElev, in_localNodes))
                    toRet.AddRange(Quadrant_SE(toRet, cornerOccluded, in_configs, in_upperVertices, in_lowerVertices));
                break;
            default:
                break;

        }
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

    private Vector3[] GetVertices(Point2D in_point, float in_thisRadius, int in_maxX, WorldNode[,] in_nodes, GameData in_data)
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

    // config section

    private int[] GetOceanConfigs(float in_oceanElev, float in_layerElev, Point2D in_center, WorldNode[] in_nodes, int in_maxY)
    {
        int[] toRet = new int[4];
        int nw;
        int ne;
        int se;
        int sw;

        if (in_center.y == 0)
        {
            nw = OceanConfigFromNodes(in_layerElev, in_oceanElev, in_nodes[8], in_nodes[8], in_nodes[8], in_nodes[6]); // c c C w
            ne = OceanConfigFromNodes(in_layerElev, in_oceanElev, in_nodes[8], in_nodes[8], in_nodes[2], in_nodes[8]); // c c e C
            se = OceanConfigFromNodes(in_layerElev, in_oceanElev, in_nodes[8], in_nodes[2], in_nodes[3], in_nodes[4]); // C e se s
            sw = OceanConfigFromNodes(in_layerElev, in_oceanElev, in_nodes[6], in_nodes[8], in_nodes[4], in_nodes[5]); // w C s sw
        }
        else if ((in_center.y + 1) == in_maxY)
        {
            nw = OceanConfigFromNodes(in_layerElev, in_oceanElev, in_nodes[7], in_nodes[0], in_nodes[8], in_nodes[6]); // nw n C w
            ne = OceanConfigFromNodes(in_layerElev, in_oceanElev, in_nodes[0], in_nodes[1], in_nodes[2], in_nodes[8]); // n ne e C
            se = OceanConfigFromNodes(in_layerElev, in_oceanElev, in_nodes[8], in_nodes[2], in_nodes[8], in_nodes[8]); // C e c c
            sw = OceanConfigFromNodes(in_layerElev, in_oceanElev, in_nodes[6], in_nodes[8], in_nodes[8], in_nodes[8]); // w C c c
        }
        else
        {
            nw = OceanConfigFromNodes(in_layerElev, in_oceanElev, in_nodes[7], in_nodes[0], in_nodes[8], in_nodes[6]); // nw n C w
            ne = OceanConfigFromNodes(in_layerElev, in_oceanElev, in_nodes[0], in_nodes[1], in_nodes[2], in_nodes[8]); // n ne e C
            se = OceanConfigFromNodes(in_layerElev, in_oceanElev, in_nodes[8], in_nodes[2], in_nodes[3], in_nodes[4]); // C e se s
            sw = OceanConfigFromNodes(in_layerElev, in_oceanElev, in_nodes[6], in_nodes[8], in_nodes[4], in_nodes[5]); // w C s sw
        }

        toRet[0] = nw;
        toRet[1] = ne;
        toRet[2] = se;
        toRet[3] = sw;

        return toRet;
    }

    private int[] GetConfigs(float in_layerElev, Point2D in_center, WorldNode[] in_nodes, int in_maxY)
    {
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

    private int OceanConfigFromNodes(float in_layerElev, float in_oceanElev, WorldNode in_nw, WorldNode in_ne, WorldNode in_se, WorldNode in_sw)
    {
        int configuration = 0;
        if ((OceanNodeActive(in_layerElev, in_oceanElev, in_nw)))
            configuration += 8;
        if ((OceanNodeActive(in_layerElev, in_oceanElev, in_ne)))
            configuration += 4;
        if ((OceanNodeActive(in_layerElev, in_oceanElev, in_se)))
            configuration += 2;
        if ((OceanNodeActive(in_layerElev, in_oceanElev, in_sw)))
            configuration += 1;

        if ((configuration == 15) && ((NodeOccluded(in_layerElev, in_nw.Get_layerElev())) && (NodeOccluded(in_layerElev, in_ne.Get_layerElev())) && (NodeOccluded(in_layerElev, in_se.Get_layerElev())) && (NodeOccluded(in_layerElev, in_sw.Get_layerElev()))))
            configuration = 0;

        return configuration;
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

    private bool OceanNodeActive(float in_thisLayer, float in_oceanLayer, WorldNode in_node)
    {
        bool toRet = false;
        float locElev = in_node.Get_layerElev();
        if (locElev >= in_oceanLayer)
        {
            if (locElev >= in_thisLayer)
                toRet = true;
        }
        else
            return true;
        return toRet;
    }

    private WorldNode[] GetLocalNodes(Point2D in_loc, WorldNode[,] in_nodes, Point2D[] in_peri)
    {
        WorldNode[] toRet = new WorldNode[9];
        int pos = 0;
        while (pos < in_peri.Length)
        {
            toRet[pos] = in_nodes[in_peri[pos].x, in_peri[pos].y];
            pos++;
        }
        toRet[8] = in_nodes[in_loc.x, in_loc.y];
        return toRet;
    }

    public Point2D Get_location() { return location; }
    public GameObject Get_landNode() { return landNode; }
    public GameObject Get_oceanNode() { return oceanNode; }
}
