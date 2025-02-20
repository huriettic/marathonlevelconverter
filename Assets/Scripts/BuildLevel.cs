using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weland;
using System.IO;
using UnityEngine.XR;
using System;

public class BuildLevel : MonoBehaviour
{
    // Default scale is 2.5, but Unreal tournament is 128
    public float Scale = 2.5f;

    public string Name = "Tutorial";

    public Level level;

    public int LevelNumber;

    private Plane TopPlane;

    private Plane LeftPlane;

    private bool listsFilled = false;

    private List<int> triangles;

    private List<int> MeshTexture = new List<int>();

    private List<int> MeshTextureCollection = new List<int>();

    private List<int> Planes = new List<int>();

    private List<int> Portals = new List<int>();

    private List<int> Renders = new List<int>();

    private List<int> Collisions = new List<int>();

    private List<Mesh> meshes = new List<Mesh>();

    private List<Vector3> CW = new List<Vector3>();

    private List<Vector2> CWUV = new List<Vector2>();

    private List<Vector2> CWUVOffset = new List<Vector2>();

    private List<Vector3> CWUVOffsetZ = new List<Vector3>();

    private List<Vector3> CCW = new List<Vector3>();

    private List<Vector2> CCWUV = new List<Vector2>();

    private List<Vector2> CCWUVOffset = new List<Vector2>();

    private List<Vector3> CCWUVOffsetZ = new List<Vector3>();

    private List<Vector2> ceilinguvs = new List<Vector2>();

    private List<Vector3> ceilinguvsz = new List<Vector3>();

    private List<Vector2> flooruvs = new List<Vector2>();

    private List<Vector3> flooruvsz = new List<Vector3>();

    private List<Vector3> ceilingverts = new List<Vector3>();

    private List<int> ceilingtri = new List<int>();

    private List<Vector3> floorverts = new List<Vector3>();

    private List<int> floortri = new List<int>();

    private List<int> Plane = new List<int>();

    private List<int> Portal = new List<int>();

    private List<int> Render = new List<int>();

    private List<int> Collision = new List<int>();

    private RenderingData Rendering;

    [System.Serializable]
    public class RenderingData
    {
        public List<Polyhedron> Polyhedrons = new List<Polyhedron>();

        public List<PolygonData> PolygonInformation = new List<PolygonData>();

        public List<PolygonMesh> PolygonMeshes = new List<PolygonMesh>();

        public List<PlayerStarts> PlayerPosition = new List<PlayerStarts>();

        [System.Serializable]
        public class PolygonData
        {
            public int Plane;
            public int Portal;
            public int Render;
            public int Collision;
            public int CollisionNumber;
            public int PortalNumber;
            public int MeshNumber;
            public int MeshTexture;
            public int MeshTextureCollection;
        }

        [System.Serializable]
        public class PolygonMesh
        {
            public List<Vector3> Vertices = new List<Vector3>();

            public List<Vector3> Textures = new List<Vector3>();

            public List<int> Triangles = new List<int>();

            public List<Vector3> Normals = new List<Vector3>();
        }

        [System.Serializable]
        public class Polyhedron
        {
            public List<int> MeshPlanes = new List<int>();

            public List<int> MeshPortals = new List<int>();

            public List<int> MeshRenders = new List<int>();

            public List<int> MeshCollisions = new List<int>();

            public int PolyhedronNumber;
        }

        [System.Serializable]
        public class PlayerStarts
        {
            public Vector3 Position;
            public int Sector;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadLevel();

        triangles = new List<int>()
        {
            0, 1, 2, 0, 2, 3
        };

        Rendering = new RenderingData();

        StartCoroutine(StartBuild());

        StartCoroutine(SaveRendering());

        StartCoroutine(SaveAfterRendering());
    }

    public void LoadLevel()
    {
        MapFile map = new MapFile();

        level = new Level();

        try
        {
            // Change name to load a different map
            map.Load(Application.dataPath + "/Maps/" + Name + ".sceA");
            Debug.Log("Map loaded successfully!");
        }
        catch (Exception exit)
        {
            Debug.LogError("Failed to load Map: " + exit.Message);
        }
        
        try
        {
            // Change the map directory number if the map has more than one level 
            level.Load(map.Directory[LevelNumber]);
            Debug.Log("Level loaded successfully!");
        }
        catch (Exception exit)
        {
            Debug.LogError("Failed to load level: " + exit.Message);
        }
    }

    public void SaveLevel()
    {
        try
        {
            string saveData = JsonUtility.ToJson(Rendering, true);
            string path = Application.dataPath + "/Levels/" + Name + ".txt";

            File.WriteAllText(path, saveData);
            Debug.Log("Data saved successfully to " + path);
        }
        catch (Exception exit)
        {
            Debug.LogError("Failed to save data: " + exit.Message);
        }
    }

    IEnumerator SaveRendering()
    {
        for (int e = 0; e < meshes.Count; ++e)
        {
            Mesh mesh = meshes[e];

            RenderingData.PolygonMesh PolyMesh = new RenderingData.PolygonMesh();

            mesh.GetVertices(PolyMesh.Vertices);

            mesh.GetUVs(0, PolyMesh.Textures);

            mesh.GetTriangles(PolyMesh.Triangles, 0);

            mesh.GetNormals(PolyMesh.Normals);

            Rendering.PolygonMeshes.Add(PolyMesh);

            yield return null;
        }

        int p = 0;

        int r = 0;

        int c = 0;

        for (int e = 0; e < meshes.Count; ++e)
        {
            RenderingData.PolygonData PolyData = new RenderingData.PolygonData();

            PolyData.Plane = Plane[e];
            PolyData.Portal = Portal[e];
            PolyData.Render = Render[e];
            PolyData.Collision = Collision[e];
            PolyData.MeshTexture = MeshTexture[e];
            PolyData.MeshTextureCollection = MeshTextureCollection[e];

            if (Portal[e] != -1)
            {
                PolyData.PortalNumber = p;

                p++;
            }
            else
            {
                PolyData.PortalNumber = -1;
            }

            if (Render[e] != -1)
            {
                PolyData.MeshNumber = r;

                r++;
            }
            else
            {
                PolyData.MeshNumber = -1;
            }

            if (Collision[e] != -1)
            {
                PolyData.CollisionNumber = c;

                c++;
            }
            else
            {
                PolyData.CollisionNumber = -1;
            }

            Rendering.PolygonInformation.Add(PolyData);

            yield return null;
        }

        for (int h = 0; h < level.Polygons.Count; h++)
        {
            Planes.Clear();

            for (int e = 0; e < Plane.Count; e++)
            {
                if (Plane[e] == h)
                {
                    Planes.Add(e);
                }
            }

            Portals.Clear();

            for (int e = 0; e < Portal.Count; e++)
            {
                if (Plane[e] == h && Portal[e] != -1)
                {
                    Portals.Add(e);
                }
            }

            Renders.Clear();

            for (int e = 0; e < Render.Count; e++)
            {
                if (Render[e] == h)
                {
                    Renders.Add(e);
                }
            }

            Collisions.Clear();

            for (int e = 0; e < Collision.Count; e++)
            {
                if (Collision[e] == h)
                {
                    Collisions.Add(e);
                }
            }

            RenderingData.Polyhedron PolyObject = new RenderingData.Polyhedron();

            PolyObject.MeshPlanes.AddRange(Planes);

            PolyObject.MeshPortals.AddRange(Portals);

            PolyObject.MeshRenders.AddRange(Renders);

            PolyObject.MeshCollisions.AddRange(Collisions);

            PolyObject.PolyhedronNumber = h;

            Rendering.Polyhedrons.Add(PolyObject);

            yield return null;
        }

        listsFilled = true;

        yield return null;
    }

    IEnumerator SaveAfterRendering()
    {
       yield return new WaitUntil(() => listsFilled);

       SaveLevel();
    }

    IEnumerator BuildObjects()
    {
        for (int i = 0; i < level.Objects.Count; i++)
        {
            if (level.Objects[i].Type == ObjectType.Player)
            {
                RenderingData.PlayerStarts Start = new RenderingData.PlayerStarts();

                Start.Position = new Vector3((float)level.Objects[i].X / 1024 * Scale, (float)level.Polygons[level.Objects[i].PolygonIndex].FloorHeight / 1024 * Scale, (float)level.Objects[i].Y / 1024 * Scale * -1);

                Start.Sector = level.Objects[i].PolygonIndex;

                Rendering.PlayerPosition.Add(Start);
            }

            yield return null;
        }
    }

    IEnumerator StartBuild()
    {
        StartCoroutine(BuildLines());

        StartCoroutine(BuildPolygons());

        StartCoroutine(BuildObjects());

        yield return null;
    }

    IEnumerator BuildLines()
    {
        for (int i = 0; i < level.Lines.Count; ++i)
        {
            double X1 = (float)level.Endpoints[level.Lines[i].EndpointIndexes[0]].X / 1024 * Scale;
            double Z1 = (float)level.Endpoints[level.Lines[i].EndpointIndexes[0]].Y / 1024 * Scale * -1;

            double X0 = (float)level.Endpoints[level.Lines[i].EndpointIndexes[1]].X / 1024 * Scale;
            double Z0 = (float)level.Endpoints[level.Lines[i].EndpointIndexes[1]].Y / 1024 * Scale * -1;

            if (level.Lines[i].ClockwisePolygonOwner != -1)
            {
                if (level.Polygons[level.Lines[i].ClockwisePolygonOwner].CeilingHeight > level.Lines[i].LowestAdjacentCeiling)
                {
                    if (level.Polygons[level.Lines[i].ClockwisePolygonOwner].FloorHeight < level.Lines[i].LowestAdjacentCeiling)
                    {
                        double YC0 = (float)level.Polygons[level.Lines[i].ClockwisePolygonOwner].CeilingHeight / 1024 * Scale;
                        double YC1 = (float)level.Lines[i].LowestAdjacentCeiling / 1024 * Scale;

                        CW.Clear();
                        CWUV.Clear();
                        CWUVOffset.Clear();
                        CWUVOffsetZ.Clear();

                        GetVertsCW(X0, X1, YC1, YC0, Z0, Z1);

                        if (level.Lines[i].ClockwisePolygonSideIndex != -1)
                        {
                            MakeSidesCW(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary);
                        }

                        Plane.Add(level.Lines[i].ClockwisePolygonOwner);

                        Portal.Add(-1);

                        if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection == 27 || level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection == 28 ||
                            level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection == 29 || level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection == 30)
                        {
                            Render.Add(-1);
                        }
                        else
                        {
                            Render.Add(level.Lines[i].ClockwisePolygonOwner);
                        }
                            
                        MeshTexture.Add(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Bitmap);

                        MeshTextureCollection.Add(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection);

                        Collision.Add(level.Lines[i].ClockwisePolygonOwner);

                        Mesh mesh = new Mesh();

                        mesh.SetVertices(CW);

                        if (level.Lines[i].ClockwisePolygonSideIndex == -1)
                        {
                             mesh.SetUVs(0, CWUV);
                        }
                        else
                        {
                            if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.IsEmpty())
                            {
                                mesh.SetUVs(0, CWUV);
                            }
                            else
                            {
                                mesh.SetUVs(0, CWUVOffsetZ);
                            }
                        }
                            
                        mesh.SetTriangles(triangles, 0);
                        mesh.RecalculateNormals();

                        meshes.Add(mesh);
                    }
                    else
                    {
                        double YC0 = (float)level.Polygons[level.Lines[i].ClockwisePolygonOwner].CeilingHeight / 1024 * Scale;
                        double YC1 = (float)level.Polygons[level.Lines[i].ClockwisePolygonOwner].FloorHeight / 1024 * Scale;

                        CW.Clear();
                        CWUV.Clear();
                        CWUVOffset.Clear();
                        CWUVOffsetZ.Clear();

                        GetVertsCW(X0, X1, YC1, YC0, Z0, Z1);

                        if (level.Lines[i].ClockwisePolygonSideIndex != -1)
                        {
                            MakeSidesCW(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary);
                        }

                        Plane.Add(level.Lines[i].ClockwisePolygonOwner);

                        Portal.Add(-1);

                        if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection == 27 || level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection == 28 ||
                            level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection == 29 || level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection == 30)
                        {
                            Render.Add(-1);
                        }
                        else
                        {
                            Render.Add(level.Lines[i].ClockwisePolygonOwner);
                        }

                        MeshTexture.Add(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Bitmap);

                        MeshTextureCollection.Add(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection);

                        Collision.Add(level.Lines[i].ClockwisePolygonOwner);

                        Mesh mesh = new Mesh();

                        mesh.SetVertices(CW);

                        if (level.Lines[i].ClockwisePolygonSideIndex == -1)
                        {
                            mesh.SetUVs(0, CWUV);
                        }
                        else
                        {
                            if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.IsEmpty())
                            {
                                mesh.SetUVs(0, CWUV);
                            }
                            else
                            {
                                mesh.SetUVs(0, CWUVOffsetZ);
                            }
                        }

                        mesh.SetTriangles(triangles, 0);
                        mesh.RecalculateNormals();

                        meshes.Add(mesh);
                    }
                }
                if (level.Lines[i].LowestAdjacentCeiling != level.Lines[i].HighestAdjacentFloor)
                {
                    if (level.Polygons[level.Lines[i].ClockwisePolygonOwner].CeilingHeight > level.Lines[i].HighestAdjacentFloor &&
                    level.Polygons[level.Lines[i].ClockwisePolygonOwner].FloorHeight < level.Lines[i].LowestAdjacentCeiling)
                    {
                        double YC = (float)level.Lines[i].LowestAdjacentCeiling / 1024 * Scale;
                        double YF = (float)level.Lines[i].HighestAdjacentFloor / 1024 * Scale;

                        CW.Clear();
                        CWUV.Clear();
                        CWUVOffset.Clear();
                        CWUVOffsetZ.Clear();

                        GetVertsCW(X0, X1, YF, YC, Z0, Z1);

                        Plane.Add(level.Lines[i].ClockwisePolygonOwner);

                        if (level.Lines[i].CounterclockwisePolygonOwner != -1)
                        {
                            Portal.Add(level.Lines[i].CounterclockwisePolygonOwner);
                        }
                        else
                        {
                            Portal.Add(-1);
                        }

                        if (level.Lines[i].CounterclockwisePolygonOwner == -1)
                        {
                            if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection == 27 || level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection == 28 ||
                                level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection == 29 || level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection == 30)
                            {
                                Render.Add(-1);
                            }
                            else
                            {
                                Render.Add(level.Lines[i].ClockwisePolygonOwner);
                            }

                            MakeSidesCW(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary);

                            MeshTexture.Add(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Bitmap);

                            MeshTextureCollection.Add(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection);
                        }
                        else
                        {
                            if (level.Lines[i].ClockwisePolygonSideIndex != -1)
                            {
                                if (!level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Transparent.Texture.IsEmpty())
                                {
                                    if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Transparent.Texture.Collection == 27 || level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Transparent.Texture.Collection == 28 ||
                                        level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Transparent.Texture.Collection == 29 || level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Transparent.Texture.Collection == 30)
                                    {
                                        Render.Add(-1);
                                    }
                                    else
                                    {
                                        Render.Add(level.Lines[i].ClockwisePolygonOwner);
                                    }

                                    MakeSidesCW(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Transparent);

                                    MeshTexture.Add(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Transparent.Texture.Bitmap);

                                    MeshTextureCollection.Add(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Transparent.Texture.Collection);
                                }
                                else
                                {
                                    Render.Add(-1);

                                    MeshTexture.Add(-1);

                                    MeshTextureCollection.Add(-1);
                                }
                            }
                            else
                            {
                                Render.Add(-1);

                                MeshTexture.Add(-1);

                                MeshTextureCollection.Add(-1);
                            }
                        }

                        if (level.Lines[i].Solid == true)
                        {
                            Collision.Add(level.Lines[i].ClockwisePolygonOwner);
                        }
                        else
                        {
                            Collision.Add(-1);
                        }

                        Mesh mesh = new Mesh();

                        mesh.SetVertices(CW);

                        if (level.Lines[i].CounterclockwisePolygonOwner == -1)
                        {
                            mesh.SetUVs(0, CWUVOffsetZ);
                        }
                        else
                        {
                            if (level.Lines[i].ClockwisePolygonSideIndex != -1)
                            {
                                if (!level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Transparent.Texture.IsEmpty())
                                {
                                    mesh.SetUVs(0, CWUVOffsetZ);
                                }
                                else
                                {
                                    mesh.SetUVs(0, CWUV);
                                }
                            }
                            else
                            {
                                mesh.SetUVs(0, CWUV);
                            }
                        }

                        mesh.SetTriangles(triangles, 0);
                        mesh.RecalculateNormals();

                        meshes.Add(mesh);
                    }
                }

                if (level.Polygons[level.Lines[i].ClockwisePolygonOwner].FloorHeight < level.Lines[i].HighestAdjacentFloor)
                {
                    if (level.Polygons[level.Lines[i].ClockwisePolygonOwner].CeilingHeight > level.Lines[i].HighestAdjacentFloor)
                    {
                        double YF0 = (float)level.Polygons[level.Lines[i].ClockwisePolygonOwner].FloorHeight / 1024 * Scale;
                        double YF1 = (float)level.Lines[i].HighestAdjacentFloor / 1024 * Scale;

                        CW.Clear();
                        CWUV.Clear();
                        CWUVOffset.Clear();
                        CWUVOffsetZ.Clear();

                        GetVertsCW(X0, X1, YF0, YF1, Z0, Z1);

                        if (level.Lines[i].ClockwisePolygonSideIndex != -1)
                        {
                            if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Type == SideType.Low)
                            {
                                MakeSidesCW(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary);

                                if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection == 27 || level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection == 28 ||
                                    level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection == 29 || level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection == 30)
                                {
                                    Render.Add(-1);
                                }
                                else
                                {
                                    Render.Add(level.Lines[i].ClockwisePolygonOwner);
                                }

                                MeshTexture.Add(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Bitmap);

                                MeshTextureCollection.Add(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection);
                            }
                            else if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Type == SideType.Split)
                            {
                                MakeSidesCW(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Secondary);

                                if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Secondary.Texture.Collection == 27 || level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Secondary.Texture.Collection == 28 ||
                                    level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Secondary.Texture.Collection == 29 || level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Secondary.Texture.Collection == 30)
                                {
                                    Render.Add(-1);
                                }
                                else
                                {
                                    Render.Add(level.Lines[i].ClockwisePolygonOwner);
                                }

                                MeshTexture.Add(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Secondary.Texture.Bitmap);

                                MeshTextureCollection.Add(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Secondary.Texture.Collection);
                            }
                        }

                        Plane.Add(level.Lines[i].ClockwisePolygonOwner);

                        Portal.Add(-1);

                        Collision.Add(level.Lines[i].ClockwisePolygonOwner);

                        Mesh mesh = new Mesh();

                        mesh.SetVertices(CW);

                        if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Type == SideType.Low)
                        {
                            if (level.Lines[i].ClockwisePolygonSideIndex == -1)
                            {
                                mesh.SetUVs(0, CWUV);
                            }
                            else
                            {
                                if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.IsEmpty())
                                {
                                    mesh.SetUVs(0, CWUV);
                                }
                                else
                                {
                                    mesh.SetUVs(0, CWUVOffsetZ);
                                }
                            }
                        }
                        else if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Type == SideType.Split)
                        {
                            if (level.Lines[i].ClockwisePolygonSideIndex == -1)
                            {
                                mesh.SetUVs(0, CWUV);
                            }
                            else
                            {
                                if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Secondary.Texture.IsEmpty())
                                {
                                    mesh.SetUVs(0, CWUV);
                                }
                                else
                                {
                                    mesh.SetUVs(0, CWUVOffsetZ);
                                }
                            }
                        }

                        mesh.SetTriangles(triangles, 0);
                        mesh.RecalculateNormals();

                        meshes.Add(mesh);
                    }
                    else
                    {
                        double YF0 = (float)level.Polygons[level.Lines[i].ClockwisePolygonOwner].FloorHeight / 1024 * Scale;
                        double YF1 = (float)level.Polygons[level.Lines[i].ClockwisePolygonOwner].CeilingHeight / 1024 * Scale;

                        CW.Clear();
                        CWUV.Clear();
                        CWUVOffset.Clear();
                        CWUVOffsetZ.Clear();

                        GetVertsCW(X0, X1, YF0, YF1, Z0, Z1);

                        if (level.Lines[i].ClockwisePolygonSideIndex != -1)
                        {
                            if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Type == SideType.Low)
                            {
                                MakeSidesCW(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary);

                                if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection == 27 || level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection == 28 ||
                                    level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection == 29 || level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection == 30)
                                {
                                    Render.Add(-1);
                                }
                                else
                                {
                                    Render.Add(level.Lines[i].ClockwisePolygonOwner);
                                }

                                MeshTexture.Add(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Bitmap);

                                MeshTextureCollection.Add(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.Collection);
                            }
                            else if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Type == SideType.Split)
                            {
                                MakeSidesCW(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Secondary);

                                if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Secondary.Texture.Collection == 27 || level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Secondary.Texture.Collection == 28 ||
                                    level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Secondary.Texture.Collection == 29 || level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Secondary.Texture.Collection == 30)
                                {
                                    Render.Add(-1);
                                }
                                else
                                {
                                    Render.Add(level.Lines[i].ClockwisePolygonOwner);
                                }

                                MeshTexture.Add(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Secondary.Texture.Bitmap);

                                MeshTextureCollection.Add(level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Secondary.Texture.Collection);
                            }
                        }

                        Plane.Add(level.Lines[i].ClockwisePolygonOwner);

                        Portal.Add(-1);

                        Collision.Add(level.Lines[i].ClockwisePolygonOwner);

                        Mesh mesh = new Mesh();

                        mesh.SetVertices(CW);

                        if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Type == SideType.Low)
                        {
                            if (level.Lines[i].ClockwisePolygonSideIndex == -1)
                            {
                                mesh.SetUVs(0, CWUV);
                            }
                            else
                            {
                                if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Primary.Texture.IsEmpty())
                                {
                                    mesh.SetUVs(0, CWUV);
                                }
                                else
                                {
                                    mesh.SetUVs(0, CWUVOffsetZ);
                                }
                            }
                        }
                        else if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Type == SideType.Split)
                        {
                            if (level.Lines[i].ClockwisePolygonSideIndex == -1)
                            {
                                mesh.SetUVs(0, CWUV);
                            }
                            else
                            {
                                if (level.Sides[level.Lines[i].ClockwisePolygonSideIndex].Secondary.Texture.IsEmpty())
                                {
                                    mesh.SetUVs(0, CWUV);
                                }
                                else
                                {
                                    mesh.SetUVs(0, CWUVOffsetZ);
                                }
                            }
                        }

                        mesh.SetTriangles(triangles, 0);
                        mesh.RecalculateNormals();

                        meshes.Add(mesh);
                    }
                }
            }

            if (level.Lines[i].CounterclockwisePolygonOwner != -1)
            {
                if (level.Polygons[level.Lines[i].CounterclockwisePolygonOwner].CeilingHeight > level.Lines[i].LowestAdjacentCeiling)
                {
                    if (level.Polygons[level.Lines[i].CounterclockwisePolygonOwner].FloorHeight < level.Lines[i].LowestAdjacentCeiling)
                    {
                        double YC0 = (float)level.Polygons[level.Lines[i].CounterclockwisePolygonOwner].CeilingHeight / 1024 * Scale;
                        double YC1 = (float)level.Lines[i].LowestAdjacentCeiling / 1024 * Scale;

                        CCW.Clear();
                        CCWUV.Clear();
                        CCWUVOffset.Clear();
                        CCWUVOffsetZ.Clear();

                        GetVertsCCW(X0, X1, YC1, YC0, Z0, Z1);

                        if (level.Lines[i].CounterclockwisePolygonSideIndex != -1)
                        {
                            MakeSidesCCW(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary);
                        }

                        Plane.Add(level.Lines[i].CounterclockwisePolygonOwner);

                        Portal.Add(-1);

                        if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection == 27 || level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection == 28 ||
                            level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection == 29 || level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection == 30)
                        {
                            Render.Add(-1);
                        }
                        else
                        {
                            Render.Add(level.Lines[i].CounterclockwisePolygonOwner);
                        }

                        MeshTexture.Add(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Bitmap);

                        MeshTextureCollection.Add(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection);

                        Collision.Add(level.Lines[i].CounterclockwisePolygonOwner);

                        Mesh mesh = new Mesh();

                        mesh.SetVertices(CCW);

                        if (level.Lines[i].CounterclockwisePolygonSideIndex == -1)
                        {
                            mesh.SetUVs(0, CCWUV);
                        }
                        else
                        {
                            if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.IsEmpty())
                            {
                                mesh.SetUVs(0, CCWUV);
                            }
                            else
                            {
                                mesh.SetUVs(0, CCWUVOffsetZ);
                            }
                        }

                        mesh.SetTriangles(triangles, 0);
                        mesh.RecalculateNormals();

                        meshes.Add(mesh);
                    }
                    else
                    {
                        double YC0 = (float)level.Polygons[level.Lines[i].CounterclockwisePolygonOwner].CeilingHeight / 1024 * Scale;
                        double YC1 = (float)level.Polygons[level.Lines[i].CounterclockwisePolygonOwner].FloorHeight / 1024 * Scale;

                        CCW.Clear();
                        CCWUV.Clear();
                        CCWUVOffset.Clear();
                        CCWUVOffsetZ.Clear();

                        GetVertsCCW(X0, X1, YC1, YC0, Z0, Z1);

                        if (level.Lines[i].CounterclockwisePolygonSideIndex != -1)
                        {
                            MakeSidesCCW(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary);
                        }

                        Plane.Add(level.Lines[i].CounterclockwisePolygonOwner);

                        Portal.Add(-1);

                        if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection == 27 || level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection == 28 ||
                            level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection == 29 || level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection == 30)
                        {
                            Render.Add(-1);
                        }
                        else
                        {
                            Render.Add(level.Lines[i].CounterclockwisePolygonOwner);
                        }

                        MeshTexture.Add(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Bitmap);

                        MeshTextureCollection.Add(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection);

                        Collision.Add(level.Lines[i].CounterclockwisePolygonOwner);

                        Mesh mesh = new Mesh();

                        mesh.SetVertices(CCW);

                        if (level.Lines[i].CounterclockwisePolygonSideIndex == -1)
                        {
                            mesh.SetUVs(0, CCWUV);
                        }
                        else
                        {
                            if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.IsEmpty())
                            {
                                mesh.SetUVs(0, CCWUV);
                            }
                            else
                            {
                                mesh.SetUVs(0, CCWUVOffsetZ);
                            }
                        }

                        mesh.SetTriangles(triangles, 0);
                        mesh.RecalculateNormals();

                        meshes.Add(mesh);
                    }
                }

                if (level.Lines[i].LowestAdjacentCeiling != level.Lines[i].HighestAdjacentFloor)
                {
                    if (level.Polygons[level.Lines[i].CounterclockwisePolygonOwner].CeilingHeight > level.Lines[i].HighestAdjacentFloor &&
                        level.Polygons[level.Lines[i].CounterclockwisePolygonOwner].FloorHeight < level.Lines[i].LowestAdjacentCeiling)
                    {
                        double YC = (float)level.Lines[i].LowestAdjacentCeiling / 1024 * Scale;
                        double YF = (float)level.Lines[i].HighestAdjacentFloor / 1024 * Scale;

                        CCW.Clear();
                        CCWUV.Clear();
                        CCWUVOffset.Clear();
                        CCWUVOffsetZ.Clear();

                        GetVertsCCW(X0, X1, YF, YC, Z0, Z1);

                        Plane.Add(level.Lines[i].CounterclockwisePolygonOwner);

                        if (level.Lines[i].ClockwisePolygonOwner != -1)
                        {
                            Portal.Add(level.Lines[i].ClockwisePolygonOwner);
                        }
                        else
                        {
                            Portal.Add(-1);
                        }

                        if (level.Lines[i].ClockwisePolygonOwner == -1)
                        {
                            if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection == 27 || level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection == 28 ||
                                level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection == 29 || level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection == 30)
                            {
                                Render.Add(-1);
                            }
                            else
                            {
                                Render.Add(level.Lines[i].CounterclockwisePolygonOwner);
                            }

                            MakeSidesCCW(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary);

                            MeshTexture.Add(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Bitmap);

                            MeshTextureCollection.Add(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection);
                        }
                        else
                        {
                            if (level.Lines[i].CounterclockwisePolygonSideIndex != -1)
                            {
                                if (!level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Transparent.Texture.IsEmpty())
                                {
                                    if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Transparent.Texture.Collection == 27 || level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Transparent.Texture.Collection == 28 ||
                                        level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Transparent.Texture.Collection == 29 || level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Transparent.Texture.Collection == 30)
                                    {
                                        Render.Add(-1);
                                    }
                                    else
                                    {
                                        Render.Add(level.Lines[i].CounterclockwisePolygonOwner);
                                    }

                                    MakeSidesCCW(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Transparent);

                                    MeshTexture.Add(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Transparent.Texture.Bitmap);

                                    MeshTextureCollection.Add(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Transparent.Texture.Collection);
                                }
                                else
                                {
                                    Render.Add(-1);

                                    MeshTexture.Add(-1);

                                    MeshTextureCollection.Add(-1);
                                }
                            }
                            else
                            {
                                Render.Add(-1);

                                MeshTexture.Add(-1);

                                MeshTextureCollection.Add(-1);
                            }
                        }

                        if (level.Lines[i].Solid == true)
                        {
                            Collision.Add(level.Lines[i].CounterclockwisePolygonOwner);
                        }
                        else
                        {
                            Collision.Add(-1);
                        }

                        Mesh mesh = new Mesh();

                        mesh.SetVertices(CCW);

                        if (level.Lines[i].ClockwisePolygonOwner == -1)
                        {
                            mesh.SetUVs(0, CCWUVOffsetZ);
                        }
                        else
                        {
                            if (level.Lines[i].CounterclockwisePolygonSideIndex != -1)
                            {
                                if (!level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Transparent.Texture.IsEmpty())
                                {
                                    mesh.SetUVs(0, CCWUVOffsetZ);
                                }
                                else
                                {
                                    mesh.SetUVs(0, CCWUV);
                                }
                            }
                            else
                            {
                                mesh.SetUVs(0, CCWUV);
                            }
                        }

                        mesh.SetTriangles(triangles, 0);
                        mesh.RecalculateNormals();

                        meshes.Add(mesh);
                    }
                }

                if (level.Polygons[level.Lines[i].CounterclockwisePolygonOwner].FloorHeight < level.Lines[i].HighestAdjacentFloor)
                {
                    if (level.Polygons[level.Lines[i].CounterclockwisePolygonOwner].CeilingHeight > level.Lines[i].HighestAdjacentFloor)
                    {
                        double YF0 = (float)level.Polygons[level.Lines[i].CounterclockwisePolygonOwner].FloorHeight / 1024 * Scale;
                        double YF1 = (float)level.Lines[i].HighestAdjacentFloor / 1024 * Scale;

                        CCW.Clear();
                        CCWUV.Clear();
                        CCWUVOffset.Clear();
                        CCWUVOffsetZ.Clear();

                        GetVertsCCW(X0, X1, YF0, YF1, Z0, Z1);

                        if (level.Lines[i].CounterclockwisePolygonSideIndex != -1)
                        {
                            if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Type == SideType.Low)
                            {
                                MakeSidesCCW(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary);

                                if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection == 27 || level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection == 28 ||
                                    level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection == 29 || level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection == 30)
                                {
                                    Render.Add(-1);
                                }
                                else
                                {
                                    Render.Add(level.Lines[i].CounterclockwisePolygonOwner);
                                }

                                MeshTexture.Add(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Bitmap);

                                MeshTextureCollection.Add(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection);
                            }
                            else if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Type == SideType.Split)
                            {
                                MakeSidesCCW(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Secondary);

                                if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Secondary.Texture.Collection == 27 || level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Secondary.Texture.Collection == 28 ||
                                    level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Secondary.Texture.Collection == 29 || level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Secondary.Texture.Collection == 30)
                                {
                                    Render.Add(-1);
                                }
                                else
                                {
                                    Render.Add(level.Lines[i].CounterclockwisePolygonOwner);
                                }

                                MeshTexture.Add(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Secondary.Texture.Bitmap);

                                MeshTextureCollection.Add(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Secondary.Texture.Collection);
                            }
                        }

                        Plane.Add(level.Lines[i].CounterclockwisePolygonOwner);

                        Portal.Add(-1);

                        Collision.Add(level.Lines[i].CounterclockwisePolygonOwner);

                        Mesh mesh = new Mesh();

                        mesh.SetVertices(CCW);

                        if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Type == SideType.Low)
                        {
                            if (level.Lines[i].CounterclockwisePolygonSideIndex == -1)
                            {
                                mesh.SetUVs(0, CCWUV);
                            }
                            else
                            {
                                if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.IsEmpty())
                                {
                                    mesh.SetUVs(0, CCWUV);
                                }
                                else
                                {
                                    mesh.SetUVs(0, CCWUVOffsetZ);
                                }
                            }
                        }
                        else if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Type == SideType.Split)
                        {
                            if (level.Lines[i].CounterclockwisePolygonSideIndex == -1)
                            {
                                mesh.SetUVs(0, CCWUV);
                            }
                            else
                            {
                                if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Secondary.Texture.IsEmpty())
                                {
                                    mesh.SetUVs(0, CCWUV);
                                }
                                else
                                {
                                    mesh.SetUVs(0, CCWUVOffsetZ);
                                }
                            }
                        }

                        mesh.SetTriangles(triangles, 0);
                        mesh.RecalculateNormals();

                        meshes.Add(mesh);
                    }
                    else
                    {
                        double YF0 = (float)level.Polygons[level.Lines[i].CounterclockwisePolygonOwner].FloorHeight / 1024 * Scale;
                        double YF1 = (float)level.Polygons[level.Lines[i].CounterclockwisePolygonOwner].CeilingHeight / 1024 * Scale;

                        CCW.Clear();
                        CCWUV.Clear();
                        CCWUVOffset.Clear();
                        CCWUVOffsetZ.Clear();

                        GetVertsCCW(X0, X1, YF0, YF1, Z0, Z1);

                        if (level.Lines[i].CounterclockwisePolygonSideIndex != -1)
                        {
                            if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Type == SideType.Low)
                            {
                                MakeSidesCCW(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary);

                                if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection == 27 || level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection == 28 ||
                                    level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection == 29 || level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection == 30)
                                {
                                    Render.Add(-1);
                                }
                                else
                                {
                                    Render.Add(level.Lines[i].CounterclockwisePolygonOwner);
                                }

                                MeshTexture.Add(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Bitmap);

                                MeshTextureCollection.Add(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.Collection);
                            }
                            else if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Type == SideType.Split)
                            {
                                MakeSidesCCW(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Secondary);

                                if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Secondary.Texture.Collection == 27 || level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Secondary.Texture.Collection == 28 ||
                                    level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Secondary.Texture.Collection == 29 || level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Secondary.Texture.Collection == 30)
                                {
                                    Render.Add(-1);
                                }
                                else
                                {
                                    Render.Add(level.Lines[i].CounterclockwisePolygonOwner);
                                }

                                MeshTexture.Add(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Secondary.Texture.Bitmap);

                                MeshTextureCollection.Add(level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Secondary.Texture.Collection);
                            }
                        }

                        Plane.Add(level.Lines[i].CounterclockwisePolygonOwner);

                        Portal.Add(-1);

                        Collision.Add(level.Lines[i].CounterclockwisePolygonOwner);

                        Mesh mesh = new Mesh();

                        mesh.SetVertices(CCW);

                        if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Type == SideType.Low)
                        {
                            if (level.Lines[i].CounterclockwisePolygonSideIndex == -1)
                            {
                                mesh.SetUVs(0, CCWUV);
                            }
                            else
                            {
                                if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Primary.Texture.IsEmpty())
                                {
                                    mesh.SetUVs(0, CCWUV);
                                }
                                else
                                {
                                    mesh.SetUVs(0, CCWUVOffsetZ);
                                }
                            }
                        }
                        else if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Type == SideType.Split)
                        {
                            if (level.Lines[i].CounterclockwisePolygonSideIndex == -1)
                            {
                                mesh.SetUVs(0, CCWUV);
                            }
                            else
                            {
                                if (level.Sides[level.Lines[i].CounterclockwisePolygonSideIndex].Secondary.Texture.IsEmpty())
                                {
                                    mesh.SetUVs(0, CCWUV);
                                }
                                else
                                {
                                    mesh.SetUVs(0, CCWUVOffsetZ);
                                }
                            }
                        }

                        mesh.SetTriangles(triangles, 0);
                        mesh.RecalculateNormals();

                        meshes.Add(mesh);
                    }
                }
            }

            yield return null;
        }
    }

    IEnumerator BuildPolygons()
    {
        for (int i = 0; i < level.Polygons.Count; i++)
        {
            if (level.Polygons[i].FloorHeight != level.Polygons[i].CeilingHeight)
            {
                floorverts.Clear();
                flooruvs.Clear();
                flooruvsz.Clear();
                ceilingverts.Clear();
                ceilinguvs.Clear();
                ceilinguvsz.Clear();

                for (int e = 0; e < level.Polygons[i].VertexCount; ++e)
                {
                    float YF = (float)level.Polygons[i].FloorHeight / 1024 * Scale;
                    float YC = (float)level.Polygons[i].CeilingHeight / 1024 * Scale;
                    float X = (float)level.Endpoints[level.Polygons[i].EndpointIndexes[e]].X / 1024 * Scale;
                    float Z = (float)level.Endpoints[level.Polygons[i].EndpointIndexes[e]].Y / 1024 * Scale * -1;

                    float YFOX = (float)(level.Endpoints[level.Polygons[i].EndpointIndexes[e]].X + level.Polygons[i].FloorOrigin.X) / 1024 * -1;
                    float YFOY = (float)(level.Endpoints[level.Polygons[i].EndpointIndexes[e]].Y + level.Polygons[i].FloorOrigin.Y) / 1024;
                    float YCOX = (float)(level.Endpoints[level.Polygons[i].EndpointIndexes[e]].X + level.Polygons[i].CeilingOrigin.X) / 1024 * -1;
                    float YCOY = (float)(level.Endpoints[level.Polygons[i].EndpointIndexes[e]].Y + level.Polygons[i].CeilingOrigin.Y) / 1024;

                    floorverts.Add(new Vector3(X, YF, Z));
                    flooruvs.Add(new Vector2(YFOY, YFOX));
                    ceilingverts.Add(new Vector3(X, YC, Z));
                    ceilinguvs.Add(new Vector2(YCOY, YCOX));
                }

                if (floorverts.Count > 2)
                {
                    floortri.Clear();

                    for (int e = 2; e < floorverts.Count; e++)
                    {
                        int a = 0;
                        int b = e - 1;
                        int c = e;

                        floortri.Add(a);
                        floortri.Add(b);
                        floortri.Add(c);
                    }

                    Plane.Add(i);

                    Portal.Add(-1);

                    if (level.Polygons[i].FloorTexture.Collection == 27 || level.Polygons[i].FloorTexture.Collection == 28 ||
                        level.Polygons[i].FloorTexture.Collection == 29 || level.Polygons[i].FloorTexture.Collection == 30)
                    {
                        Render.Add(-1);
                    }
                    else
                    {
                        Render.Add(i);
                    }

                    if (level.Polygons[i].FloorTexture.Collection == 17)
                    {
                        for (int e = 0; e < flooruvs.Count; e++)
                        {
                            flooruvsz.Add(new Vector3(flooruvs[e].x, flooruvs[e].y, level.Polygons[i].FloorTexture.Bitmap));
                        }
                    }
                    if (level.Polygons[i].FloorTexture.Collection == 18)
                    {
                        for (int e = 0; e < flooruvs.Count; e++)
                        {
                            flooruvsz.Add(new Vector3(flooruvs[e].x, flooruvs[e].y, level.Polygons[i].FloorTexture.Bitmap + 30));
                        }
                    }
                    if (level.Polygons[i].FloorTexture.Collection == 19)
                    {
                        for (int e = 0; e < flooruvs.Count; e++)
                        {
                            flooruvsz.Add(new Vector3(flooruvs[e].x, flooruvs[e].y, level.Polygons[i].FloorTexture.Bitmap + 60));
                        }
                    }
                    if (level.Polygons[i].FloorTexture.Collection == 20)
                    {
                        for (int e = 0; e < flooruvs.Count; e++)
                        {
                            flooruvsz.Add(new Vector3(flooruvs[e].x, flooruvs[e].y, level.Polygons[i].FloorTexture.Bitmap + 90));
                        }
                    }
                    if (level.Polygons[i].FloorTexture.Collection == 21)
                    {
                        for (int e = 0; e < flooruvs.Count; e++)
                        {
                            flooruvsz.Add(new Vector3(flooruvs[e].x, flooruvs[e].y, level.Polygons[i].FloorTexture.Bitmap + 125));
                        }
                    }
                    if (level.Polygons[i].FloorTexture.Collection == 27 || level.Polygons[i].FloorTexture.Collection == 28 ||
                        level.Polygons[i].FloorTexture.Collection == 29 || level.Polygons[i].FloorTexture.Collection == 30)
                    {
                        for (int e = 0; e < flooruvs.Count; e++)
                        {
                            flooruvsz.Add(new Vector3(flooruvs[e].x, flooruvs[e].y, level.Polygons[i].FloorTexture.Bitmap));
                        }
                    }

                    MeshTexture.Add(level.Polygons[i].FloorTexture.Bitmap);

                    MeshTextureCollection.Add(level.Polygons[i].FloorTexture.Collection);

                    Collision.Add(i);

                    Mesh mesh = new Mesh();

                    mesh.SetVertices(floorverts);
                    mesh.SetUVs(0, flooruvsz);
                    mesh.SetTriangles(floortri, 0);
                    mesh.RecalculateNormals();

                    meshes.Add(mesh);
                }

                if (ceilingverts.Count > 2)
                {
                    ceilingverts.Reverse();

                    ceilinguvs.Reverse();

                    ceilingtri.Clear();

                    for (int e = 2; e < ceilingverts.Count; e++)
                    {
                        int a = 0;
                        int b = e - 1;
                        int c = e;

                        ceilingtri.Add(a);
                        ceilingtri.Add(b);
                        ceilingtri.Add(c);
                    }

                    Plane.Add(i);

                    Portal.Add(-1);

                    if (level.Polygons[i].CeilingTexture.Collection == 27 || level.Polygons[i].CeilingTexture.Collection == 28 ||
                        level.Polygons[i].CeilingTexture.Collection == 29 || level.Polygons[i].CeilingTexture.Collection == 30)
                    {
                        Render.Add(-1);
                    }
                    else
                    {
                        Render.Add(i);
                    }

                    if (level.Polygons[i].CeilingTexture.Collection == 17)
                    {
                        for (int e = 0; e < ceilinguvs.Count; e++)
                        {
                            ceilinguvsz.Add(new Vector3(ceilinguvs[e].x, ceilinguvs[e].y, level.Polygons[i].CeilingTexture.Bitmap));
                        }
                    }
                    if (level.Polygons[i].CeilingTexture.Collection == 18)
                    {
                        for (int e = 0; e < ceilinguvs.Count; e++)
                        {
                            ceilinguvsz.Add(new Vector3(ceilinguvs[e].x, ceilinguvs[e].y, level.Polygons[i].CeilingTexture.Bitmap + 30));
                        }
                    }
                    if (level.Polygons[i].CeilingTexture.Collection == 19)
                    {
                        for (int e = 0; e < ceilinguvs.Count; e++)
                        {
                            ceilinguvsz.Add(new Vector3(ceilinguvs[e].x, ceilinguvs[e].y, level.Polygons[i].CeilingTexture.Bitmap + 60));
                        }
                    }
                    if (level.Polygons[i].CeilingTexture.Collection == 20)
                    {
                        for (int e = 0; e < ceilinguvs.Count; e++)
                        {
                            ceilinguvsz.Add(new Vector3(ceilinguvs[e].x, ceilinguvs[e].y, level.Polygons[i].CeilingTexture.Bitmap + 90));
                        }
                    }
                    if (level.Polygons[i].CeilingTexture.Collection == 21)
                    {
                        for (int e = 0; e < ceilinguvs.Count; e++)
                        {
                            ceilinguvsz.Add(new Vector3(ceilinguvs[e].x, ceilinguvs[e].y, level.Polygons[i].CeilingTexture.Bitmap + 125));
                        }
                    }
                    if (level.Polygons[i].CeilingTexture.Collection == 27 || level.Polygons[i].CeilingTexture.Collection == 28 ||
                        level.Polygons[i].CeilingTexture.Collection == 29 || level.Polygons[i].CeilingTexture.Collection == 30)
                    {
                        for (int e = 0; e < ceilinguvs.Count; e++)
                        {
                            ceilinguvsz.Add(new Vector3(ceilinguvs[e].x, ceilinguvs[e].y, level.Polygons[i].CeilingTexture.Bitmap));
                        }
                    }

                    MeshTexture.Add(level.Polygons[i].CeilingTexture.Bitmap);

                    MeshTextureCollection.Add(level.Polygons[i].CeilingTexture.Collection);

                    Collision.Add(i);

                    Mesh mesh = new Mesh();

                    mesh.SetVertices(ceilingverts);
                    mesh.SetUVs(0, ceilinguvsz);
                    mesh.SetTriangles(ceilingtri, 0);
                    mesh.RecalculateNormals();

                    meshes.Add(mesh);
                }
            }

            yield return null;
        }
    }

    public void GetVertsCW(double X0, double X1, double V0, double V1, double Z0, double Z1)
    {
        CW.Add(new Vector3((float)X1, (float)V0, (float)Z1));
        CW.Add(new Vector3((float)X1, (float)V1, (float)Z1));
        CW.Add(new Vector3((float)X0, (float)V1, (float)Z0));
        CW.Add(new Vector3((float)X0, (float)V0, (float)Z0));

        LeftPlane = new Plane((CW[2] - CW[1]).normalized, CW[1]);
        TopPlane = new Plane((CW[1] - CW[0]).normalized, CW[1]);

        CWUV.Add(new Vector2(LeftPlane.GetDistanceToPoint(CW[0]) / Scale, TopPlane.GetDistanceToPoint(CW[0]) / Scale));
        CWUV.Add(new Vector2(LeftPlane.GetDistanceToPoint(CW[1]) / Scale, TopPlane.GetDistanceToPoint(CW[1]) / Scale));
        CWUV.Add(new Vector2(LeftPlane.GetDistanceToPoint(CW[2]) / Scale, TopPlane.GetDistanceToPoint(CW[2]) / Scale));
        CWUV.Add(new Vector2(LeftPlane.GetDistanceToPoint(CW[3]) / Scale, TopPlane.GetDistanceToPoint(CW[3]) / Scale));
    }

    public void MakeSidesCW(Side.TextureDefinition sideDef)
    {
        for (int e = 0; e < CWUV.Count; e++)
        {
            CWUVOffset.Add(new Vector2(CWUV[e].x + (float)sideDef.X / 1024,
            CWUV[e].y + (float)sideDef.Y / 1024 * -1));
        }

        if (sideDef.Texture.Collection == 17)
        {
            for (int e = 0; e < CWUVOffset.Count; e++)
            {
                CWUVOffsetZ.Add(new Vector3(CWUVOffset[e].x, CWUVOffset[e].y, sideDef.Texture.Bitmap));
            }
        }
        if (sideDef.Texture.Collection == 18)
        {
            for (int e = 0; e < CWUVOffset.Count; e++)
            {
                CWUVOffsetZ.Add(new Vector3(CWUVOffset[e].x, CWUVOffset[e].y, sideDef.Texture.Bitmap + 30));
            }
        }
        if (sideDef.Texture.Collection == 19)
        {
            for (int e = 0; e < CWUVOffset.Count; e++)
            {
                CWUVOffsetZ.Add(new Vector3(CWUVOffset[e].x, CWUVOffset[e].y, sideDef.Texture.Bitmap + 60));
            }
        }
        if (sideDef.Texture.Collection == 20)
        {
            for (int e = 0; e < CWUVOffset.Count; e++)
            {
                CWUVOffsetZ.Add(new Vector3(CWUVOffset[e].x, CWUVOffset[e].y, sideDef.Texture.Bitmap + 90));
            }
        }
        if (sideDef.Texture.Collection == 21)
        {
            for (int e = 0; e < CWUVOffset.Count; e++)
            {
                CWUVOffsetZ.Add(new Vector3(CWUVOffset[e].x, CWUVOffset[e].y, sideDef.Texture.Bitmap + 125));
            }
        }

        if (sideDef.Texture.Collection == 27 || sideDef.Texture.Collection == 28 ||
            sideDef.Texture.Collection == 29 || sideDef.Texture.Collection == 30)
        {
            for (int e = 0; e < CWUVOffset.Count; e++)
            {
                CWUVOffsetZ.Add(new Vector3(CWUVOffset[e].x, CWUVOffset[e].y, sideDef.Texture.Bitmap));
            }
        }
    }

    public void GetVertsCCW(double X0, double X1, double V0, double V1, double Z0, double Z1)
    {
        CCW.Add(new Vector3((float)X0, (float)V0, (float)Z0));
        CCW.Add(new Vector3((float)X0, (float)V1, (float)Z0));
        CCW.Add(new Vector3((float)X1, (float)V1, (float)Z1));
        CCW.Add(new Vector3((float)X1, (float)V0, (float)Z1));

        LeftPlane = new Plane((CCW[2] - CCW[1]).normalized, CCW[1]);
        TopPlane = new Plane((CCW[1] - CCW[0]).normalized, CCW[1]);

        CCWUV.Add(new Vector2(LeftPlane.GetDistanceToPoint(CCW[0]) / Scale, TopPlane.GetDistanceToPoint(CCW[0]) / Scale));
        CCWUV.Add(new Vector2(LeftPlane.GetDistanceToPoint(CCW[1]) / Scale, TopPlane.GetDistanceToPoint(CCW[1]) / Scale));
        CCWUV.Add(new Vector2(LeftPlane.GetDistanceToPoint(CCW[2]) / Scale, TopPlane.GetDistanceToPoint(CCW[2]) / Scale));
        CCWUV.Add(new Vector2(LeftPlane.GetDistanceToPoint(CCW[3]) / Scale, TopPlane.GetDistanceToPoint(CCW[3]) / Scale));
    }

    public void MakeSidesCCW(Side.TextureDefinition sideDef)
    {
        for (int e = 0; e < CCWUV.Count; e++)
        {
            CCWUVOffset.Add(new Vector2(CCWUV[e].x + (float)sideDef.X / 1024,
            CCWUV[e].y + (float)sideDef.Y / 1024 * -1));
        }

        if (sideDef.Texture.Collection == 17)
        {
            for (int e = 0; e < CCWUVOffset.Count; e++)
            {
                CCWUVOffsetZ.Add(new Vector3(CCWUVOffset[e].x, CCWUVOffset[e].y, sideDef.Texture.Bitmap));
            }
        }
        if (sideDef.Texture.Collection == 18)
        {
            for (int e = 0; e < CCWUVOffset.Count; e++)
            {
                CCWUVOffsetZ.Add(new Vector3(CCWUVOffset[e].x, CCWUVOffset[e].y, sideDef.Texture.Bitmap + 30));
            }
        }
        if (sideDef.Texture.Collection == 19)
        {
            for (int e = 0; e < CCWUVOffset.Count; e++)
            {
                CCWUVOffsetZ.Add(new Vector3(CCWUVOffset[e].x, CCWUVOffset[e].y, sideDef.Texture.Bitmap + 60));
            }
        }
        if (sideDef.Texture.Collection == 20)
        {
            for (int e = 0; e < CCWUVOffset.Count; e++)
            {
                CCWUVOffsetZ.Add(new Vector3(CCWUVOffset[e].x, CCWUVOffset[e].y, sideDef.Texture.Bitmap + 90));
            }
        }
        if (sideDef.Texture.Collection == 21)
        {
            for (int e = 0; e < CCWUVOffset.Count; e++)
            {
                CCWUVOffsetZ.Add(new Vector3(CCWUVOffset[e].x, CCWUVOffset[e].y, sideDef.Texture.Bitmap + 125));
            }
        }

        if (sideDef.Texture.Collection == 27 || sideDef.Texture.Collection == 28 ||
            sideDef.Texture.Collection == 29 || sideDef.Texture.Collection == 30)
        {
            for (int e = 0; e < CCWUVOffset.Count; e++)
            {
                CCWUVOffsetZ.Add(new Vector3(CCWUVOffset[e].x, CCWUVOffset[e].y, sideDef.Texture.Bitmap));
            }
        }
    }
}
