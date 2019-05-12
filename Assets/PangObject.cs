using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PangLib.PET;
using PangLib.PET.DataModels;
using UnityEngine;
using Animation = UnityEngine.Animation;
using Texture = PangLib.PET.DataModels.Texture;

public class PangObject
{
    public GameObject GameObject { get; }

    private readonly PETFile _pet;

    private readonly string _textureSearchPath;

    private Dictionary<int, string> boneNameById = new Dictionary<int, string>();

    public PangObject(string filePath, string textureSearchPath)
    {
        _textureSearchPath = textureSearchPath;

        _pet = new PETFile(filePath);
        GameObject = new GameObject();
        GameObject.name = Path.GetFileName(filePath);

//        MeshFilter meshFilter = GameObject.AddComponent<MeshFilter>();
//        meshFilter.mesh = CreateMesh();

        SkinnedMeshRenderer renderer = GameObject.AddComponent<SkinnedMeshRenderer>();
        renderer.materials = CreateMaterials();
        renderer.sharedMesh = CreateMesh();

        (Transform[] bones, Matrix4x4[] bindPoses) = CreateBones();
        renderer.bones = bones;
        renderer.sharedMesh.bindposes = bindPoses;

        Animation animation = GameObject.AddComponent<Animation>();
        animation.AddClip(CreateFirstAnim(), "test");
        animation.Play("test");
    }

    private AnimationClip CreateFirstAnim()
    {
        AnimationClip clip = new AnimationClip();
        clip.legacy = true;
        clip.wrapMode = WrapMode.Loop;

        List<PangLib.PET.DataModels.Animation> petAnim = _pet.Animations;
        foreach (PangLib.PET.DataModels.Animation anim in petAnim)
        {
            // TODO create curves for position, scale, ...
            int rotFrames = anim.RotationData.Count;
            Keyframe[] rotX = new Keyframe[rotFrames];
            Keyframe[] rotY = new Keyframe[rotFrames];
            Keyframe[] rotZ = new Keyframe[rotFrames];
            Keyframe[] rotW = new Keyframe[rotFrames];
            for (int i = 0; i < rotFrames; i++)
            {
                RotationData rd = anim.RotationData[i];
                rotX[i] = new Keyframe(rd.Time, rd.X);
                rotY[i] = new Keyframe(rd.Time, rd.Y);
                rotZ[i] = new Keyframe(rd.Time, rd.Z);
                rotW[i] = new Keyframe(rd.Time, rd.W);
            }

            clip.SetCurve(boneNameById[anim.BoneID], typeof(Transform), "localRotation.x", new AnimationCurve(rotX));
            clip.SetCurve(boneNameById[anim.BoneID], typeof(Transform), "localRotation.y", new AnimationCurve(rotY));
            clip.SetCurve(boneNameById[anim.BoneID], typeof(Transform), "localRotation.z", new AnimationCurve(rotZ));
            clip.SetCurve(boneNameById[anim.BoneID], typeof(Transform), "localRotation.w", new AnimationCurve(rotW));
        }

        return clip;
    }

    private Material[] CreateMaterials()
    {
        List<Texture> petTextures = _pet.Textures;
        int textureCount = petTextures.Count;

        Material[] materials = new Material[textureCount];
        for (int i = 0; i < textureCount; i++)
        {
            Material material;

            string textureFileName = petTextures[i].FileName;
            string texturePath = FileHelper.GetTexture(_textureSearchPath, textureFileName);
            Texture2D mainTexture = CreateTexture(texturePath);

            // TODO there might be an attribute in the PET file telling which textures have masks
            // find mask for each texture or create an empty one if it doesn't exist
            string maskPath = texturePath.Replace(".jpg", "_mask.jpg");
            if (File.Exists(maskPath))
            {
                material = new Material(Shader.Find("MaskedTexture"));
                material.SetTexture("_Mask", CreateAlphaTexture(maskPath));
            }
            else
            {
                material = new Material(Shader.Find("OpaqueTexture"));
            }

            material.SetTexture("_MainTex", mainTexture);
            material.name = textureFileName;

            materials[i] = material;
        }

        return materials;
    }

    private Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();

        int uniqueVertexCount = _pet.Vertices.Count;

        // create unique vertices with the index used in the polygons
        Vector3[] uniqueVertices = new Vector3[uniqueVertexCount];
        BoneWeight[] uniqueBoneWeights = new BoneWeight[uniqueVertexCount];
        for (int i = 0; i < uniqueVertexCount; i++)
        {
            Vertex fileVertex = _pet.Vertices[i];
            uniqueVertices[i] = new Vector3(fileVertex.X, fileVertex.Y, fileVertex.Z);

            // TODO meh
            for (int bi = 0; bi < fileVertex.BoneInformation.Count; bi++)
            {
                BoneInformation boneInformation = fileVertex.BoneInformation[bi];
                switch (bi)
                {
                    case 0:
                        uniqueBoneWeights[i].boneIndex0 = boneInformation.BoneID;
                        uniqueBoneWeights[i].weight0 = boneInformation.Weight;
                        break;
                    case 1:
                        uniqueBoneWeights[i].boneIndex1 = boneInformation.BoneID;
                        uniqueBoneWeights[i].weight1 = boneInformation.Weight;
                        break;
                    case 2:
                        uniqueBoneWeights[i].boneIndex2 = boneInformation.BoneID;
                        uniqueBoneWeights[i].weight2 = boneInformation.Weight;
                        break;
                    case 3:
                        uniqueBoneWeights[i].boneIndex3 = boneInformation.BoneID;
                        uniqueBoneWeights[i].weight3 = boneInformation.Weight;
                        break;
                }
            }
        }

        int polygonCount = _pet.Polygons.Count;
        int vertexCount = polygonCount * 3;

        Vector3[] vertices = new Vector3[vertexCount];
        Dictionary<int, Vector2[]> uvByUvMappingId = new Dictionary<int, Vector2[]>();
        BoneWeight[] boneWeights = new BoneWeight[vertexCount];

        Dictionary<int, List<int>> trianglesByTextureIndex = Enumerable.Range(0, _pet.Textures.Count)
            .ToDictionary(i => i, _ => new List<int>());

        bool uvDictInit = false;
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

                List<UVMapping> polygonIndexUvMappings = polygonIndex.UVMappings;
                if (!uvDictInit)
                {
                    uvByUvMappingId = Enumerable.Range(0, polygonIndexUvMappings.Count)
                        .ToDictionary(k => k, _ => new Vector2[vertexCount]);
                    uvDictInit = true;
                }

                for (int k = 0; k < polygonIndexUvMappings.Count; k++)
                {
                    UVMapping polygonIndexUvMapping = polygonIndexUvMappings[k];
                    uvByUvMappingId[k][targetIndex] =
                        new Vector2(polygonIndexUvMapping.U, 1 - polygonIndexUvMapping.V); // V in PETFiles is flipped
                }

                boneWeights[targetIndex] = uniqueBoneWeights[uniqueVertexIndex];

                // group triangles by textureIndex to create a subMesh for each texture
                trianglesByTextureIndex[textureIndex].Add(targetIndex);
            }
        }

        mesh.vertices = vertices;

        for (int i = 0; i < uvByUvMappingId.Count; i++)
        {
            switch (i)
            {
                case 0:
                    mesh.uv = uvByUvMappingId[i];
                    break;
                case 1:
                    mesh.uv2 = uvByUvMappingId[i];
                    break;
                default:
                    throw new NotImplementedException("Currently only two UV mappings are supported");
            }
        }

        mesh.boneWeights = boneWeights;

        mesh.subMeshCount = trianglesByTextureIndex.Keys.Count;

        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            mesh.SetTriangles(trianglesByTextureIndex[i].ToArray(), i);
        }

        return mesh;
    }

    private Tuple<Transform[], Matrix4x4[]> CreateBones()
    {
        int boneCount = _pet.Bones.Count;
        Transform[] bones = new Transform[boneCount];
        Matrix4x4[] bindPoses = new Matrix4x4[boneCount];

        for (int i = 0; i < boneCount; i++)
        {
            Bone petBone = _pet.Bones[i];

            boneNameById[i] = petBone.Name;
            Transform bone = new GameObject(petBone.Name).transform;
            if (petBone.Parent == 255)
            {
                // 255 is the root bone
                bone.parent = GameObject.transform;
            }
            else
            {
                bone.parent = bones[petBone.Parent].transform;
            }

            float[] m = petBone.Matrix;
            Matrix4x4 bindPose = new Matrix4x4(
                new Vector4(m[0], m[1], m[2], 0),
                new Vector4(m[3], m[4], m[5], 0),
                new Vector4(m[6], m[7], m[8], 0),
                new Vector4(m[9], m[10], m[11], 1));

            bones[i] = bone;
            bindPoses[i] = bindPose;
        }

        return Tuple.Create(bones, bindPoses);
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
