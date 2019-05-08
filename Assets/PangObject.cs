using System.Collections.Generic;
using System.IO;
using PangLib.PET;
using PangLib.PET.DataModels;
using UnityEngine;
using Texture = PangLib.PET.DataModels.Texture;

public class PangObject
{
    public GameObject GameObject { get; }

    private readonly PETFile _pet;
    private readonly string _textureSearchPath;
    // by textureId, describes the position of the texture in the texture atlas
    private Rect[] _atlasUvRects;

    public PangObject(string filePath, string textureSearchPath)
    {
        _textureSearchPath = textureSearchPath;

        _pet = new PETFile(filePath);
        GameObject = new GameObject();
        GameObject.name = Path.GetFileName(filePath);

        MeshRenderer meshRenderer = GameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = CreateMaterial();

        MeshFilter meshFilter = GameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateMesh();
    }

    private Material CreateMaterial()
    {
        List<Texture> petTextures = _pet.Textures;
        int textureCount = petTextures.Count;

        Texture2D[] mainTextures = new Texture2D[textureCount];
        Texture2D[] maskTextures = new Texture2D[textureCount];
        for (int i = 0; i < textureCount; i++)
        {
            Texture texture = petTextures[i];

            string texturePath = FileHelper.GetTexture(_textureSearchPath, texture.FileName);
            Texture2D mainTexture = CreateTexture(texturePath);
            mainTextures[i] = mainTexture;

            // TODO there might be an attribute in the PET file telling which textures have masks
            // find mask for each texture or create an empty one if it doesn't exist
            string maskPath = texturePath.Replace(".jpg", "_mask.jpg");
            if (File.Exists(maskPath))
            {
                maskTextures[i] = CreateAlphaTexture(maskPath);
            }
            else
            {
                maskTextures[i] = CreateEmptyAlphaTextureFrom(mainTexture);
            }
        }

        return CreateMaterialWithMasks(mainTextures, maskTextures);
    }

    private Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();

        int vertexCount = _pet.Vertices.Count;
        int polygonCount = _pet.Polygons.Count;

        // create unique vertices with the index used in the polygons
        Vector3[] uniqueVertices = new Vector3[vertexCount];
        for (int i = 0; i < vertexCount; i++)
        {
            Vertex fileVertex = _pet.Vertices[i];
            uniqueVertices[i] = new Vector3(fileVertex.X, fileVertex.Y, fileVertex.Z);
        }

        Vector3[] vertices = new Vector3[polygonCount * 3];
        Vector2[] uv = new Vector2[polygonCount * 3];
        int[] triangles = new int[polygonCount * 3];

        for (int i = 0; i < polygonCount; i++)
        {
            Polygon polygon = _pet.Polygons[i];
            for (int j = 0; j < 3; j++)
            {
                PolygonIndex polygonIndex = polygon.PolygonIndices[j];

                int vertexIndex = (int) polygonIndex.Index;
                int targetIndex = i * 3 + j;

                vertices[targetIndex] = uniqueVertices[vertexIndex];
                uv[targetIndex] = CalcUvInTextureAtlas(polygonIndex, polygon);
                triangles[targetIndex] = targetIndex;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        return mesh;
    }

    private Material CreateMaterialWithMasks(Texture2D[] mainTextures, Texture2D[] maskTextures)
    {
        // combine all textures into a single atlas and create a matching atlas for the masks
        Texture2D textureAtlas = new Texture2D(0, 0);
        _atlasUvRects = textureAtlas.PackTextures(mainTextures, 0);

        Texture2D maskAtlas = new Texture2D(textureAtlas.width, textureAtlas.height);
        maskAtlas.PackTextures(maskTextures, 0);

        Material material = new Material(Shader.Find("MaskedTexture"));
        material.SetTexture("_MainTex", textureAtlas);
        material.SetTexture("_Mask", maskAtlas);

        return material;
    }

    private Vector2 CalcUvInTextureAtlas(PolygonIndex polygonIndex, Polygon polygon)
    {
        // V in PETFiles is flipped
        Vector2 polygonUv = new Vector2(polygonIndex.U, 1 - polygonIndex.V);

        Rect atlasUv = _atlasUvRects[polygon.TextureIndex];

        return polygonUv * new Vector2(atlasUv.width, atlasUv.height) + new Vector2(atlasUv.x, atlasUv.y);
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

    private Texture2D CreateEmptyAlphaTextureFrom(Texture2D mainTexture)
    {
        Texture2D emptyAlphaTexture = new Texture2D(mainTexture.width, mainTexture.height);
        
        // white = no transparency
        Color[] fillColorArray =  emptyAlphaTexture.GetPixels();
        for(int i = 0; i < fillColorArray.Length; i++)
        {
            fillColorArray[i] = Color.white;
        }
        emptyAlphaTexture.SetPixels(fillColorArray);
        
        return emptyAlphaTexture;
    }
}
