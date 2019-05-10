using System.Collections.Generic;
using System.IO;
using System.Linq;
using PangLib.PET;
using PangLib.PET.DataModels;
using UnityEngine;
using Texture = PangLib.PET.DataModels.Texture;

public class PangObject
{
    public GameObject GameObject { get; }

    private readonly PETFile _pet;

    private readonly string _textureSearchPath;

    public PangObject(string filePath, string textureSearchPath)
    {
        _textureSearchPath = textureSearchPath;

        _pet = new PETFile(filePath);
        GameObject = new GameObject();
        GameObject.name = Path.GetFileName(filePath);

        MeshFilter meshFilter = GameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateMesh();

        MeshRenderer meshRenderer = GameObject.AddComponent<MeshRenderer>();
        meshRenderer.materials = CreateMaterials();
    }

    private Material[] CreateMaterials()
    {
        List<Texture> petTextures = _pet.Textures;
        int textureCount = petTextures.Count;

        Material[] materials = new Material[textureCount];
        for (int i = 0; i < textureCount; i++)
        {
            Material material;

            string texturePath = FileHelper.GetTexture(_textureSearchPath, petTextures[i].FileName);
            Texture2D mainTexture = CreateTexture(texturePath);

            // TODO there might be an attribute in the PET file telling which textures have masks
            // find mask for each texture or create an empty one if it doesn't exist
            string maskPath = texturePath.Replace(".jpg", "_mask.jpg");
            if (File.Exists(maskPath))
            {
                material = new Material(Shader.Find("MaskedTexture"));
                material.SetTexture("_MainTex", mainTexture);
                material.SetTexture("_Mask", CreateAlphaTexture(maskPath));
            }
            else
            {
                material = new Material(Shader.Find("OpaqueTexture"));
                material.SetTexture("_MainTex", mainTexture);
            }

            materials[i] = material;
        }

        return materials;
    }

    private Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();

        int vertexCount = _pet.Vertices.Count;

        // create unique vertices with the index used in the polygons
        Vector3[] uniqueVertices = new Vector3[vertexCount];
        for (int i = 0; i < vertexCount; i++)
        {
            Vertex fileVertex = _pet.Vertices[i];
            uniqueVertices[i] = new Vector3(fileVertex.X, fileVertex.Y, fileVertex.Z);
        }

        int polygonCount = _pet.Polygons.Count;

        Vector3[] vertices = new Vector3[polygonCount * 3];
        Vector2[] uv = new Vector2[polygonCount * 3];

        Dictionary<int, List<int>> trianglesByTextureIndex = Enumerable.Range(0, _pet.Textures.Count)
            .ToDictionary(i => i, _ => new List<int>());

        for (int i = 0; i < polygonCount; i++)
        {
            Polygon polygon = _pet.Polygons[i];
            for (int j = 0; j < 3; j++)
            {
                PolygonIndex polygonIndex = polygon.PolygonIndices[j];

                int targetIndex = i * 3 + j;
                int uniqueVertexIndex = (int) polygonIndex.Index;
                int textureIndex = (int) polygon.TextureIndex;

                vertices[targetIndex] = uniqueVertices[uniqueVertexIndex];
                uv[targetIndex] = new Vector2(polygonIndex.U, 1 - polygonIndex.V); // V in PETFiles is flipped

                // group triangles by textureIndex to create a subMesh for each texture
                trianglesByTextureIndex[textureIndex].Add(targetIndex);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;

        mesh.subMeshCount = trianglesByTextureIndex.Keys.Count;

        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            mesh.SetTriangles(trianglesByTextureIndex[i].ToArray(), i);
        }

        return mesh;
    }

    private Texture2D CreateTexture(string path)
    {
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(0, 0); // size will be automatically adjusted after loading image
        texture.LoadImage(fileData);
        return texture;
    }

    private Texture2D CreateAlphaTexture(string path)
    {
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(0, 0);
        texture.LoadImage(fileData);
        return texture;
    }
}
