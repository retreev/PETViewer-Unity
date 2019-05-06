using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using PangLib.PET;
using PangLib.PET.DataModels;
using UnityEngine;
using Texture = PangLib.PET.DataModels.Texture;

public class PETViewer : MonoBehaviour
{
    private void Start()
    {
//        DirectoryInfo dir = new DirectoryInfo("Assets/item/ase");
//        FileInfo[] info = dir.GetFiles("*.pet");

//        Vector3 lastPos = Vector3.zero;
//        foreach (FileInfo file in info)
//        {
//            try
//            {
//                CreateGameObject(file.FullName, lastPos);
//                lastPos += new Vector3(10, 0, 0);
//            }
//            catch (Exception e)
//            {
//                Debug.Log(e.Message);
//            }
//        }

        CreateGameObject("Assets/item/ase/item0_01.pet", Vector3.zero);
    }

    private void CreateGameObject(string fileName, Vector3 position)
    {
        Debug.Log(fileName);

        PETFile file = new PETFile(fileName);

        GameObject go = new GameObject {name = Regex.Replace(fileName, @"$.*\\", "")};

        go.transform.position = position;

        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateMesh(file);

        MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
        List<Material> materials = new List<Material>();
        foreach (Texture texture in file.Textures)
        {
            materials.Add(CreateMaterial(texture.FileName));
        }

        meshRenderer.materials = materials.ToArray();
//        meshRenderer.materials = new[] {materials[0]};
    }

    private Mesh CreateMesh(PETFile file)
    {
        Mesh mesh = new Mesh();

        int vertexCount = file.Vertices.Count;
        int polygonCount = file.Polygons.Count;

        // create unique vertices with the index used in the polygons
        Vector3[] uniqueVertices = new Vector3[vertexCount];
        for (int i = 0; i < vertexCount; i++)
        {
            Vertex fileVertex = file.Vertices[i];
            uniqueVertices[i] = new Vector3(fileVertex.X, fileVertex.Y, fileVertex.Z);
        }

        Vector3[] vertices = new Vector3[polygonCount * 3];
        Vector2[] uv = new Vector2[polygonCount * 3];
        int[] triangles = new int[polygonCount * 3];

        // polygon order is not important
        for (int i = 0; i < polygonCount; i++)
        {
            Polygon polygon = file.Polygons[i];
            for (int j = 0; j < 3; j++)
            {
                PolygonIndex polygonIndex = polygon.PolygonIndices[j];

                int vertexIndex = (int) polygonIndex.Index;
                int targetIndex = i * 3 + j;

                vertices[targetIndex] = uniqueVertices[vertexIndex];
                uv[targetIndex] = new Vector2(polygonIndex.U, 1 - polygonIndex.V);
                triangles[targetIndex] = targetIndex;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        return mesh;
    }

    private Material CreateMaterial(string textureName)
    {
        byte[] fileData = File.ReadAllBytes(GetPath("item/map_source/" + textureName));
        Texture2D texture = new Texture2D(1, 1); // size will be changed after loading image
        texture.LoadImage(fileData);
        //Find the Standard Shader
        Material material = new Material(Shader.Find("Standard"));
        //Set Texture on the material
        material.SetTexture("_MainTex", texture);

        return material;
    }

    private string GetPath(string fileName)
    {
        return Application.dataPath + "/" + fileName;
    }
}
