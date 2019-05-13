using System;
using System.Collections.Generic;
using System.Linq;
using PangLib.PET;
using PangLib.PET.DataModels;
using UnityEngine;

namespace Helper
{
    public static class MeshHelper
    {
        public static Mesh CreateMesh(PETFile pet)
        {
            Mesh mesh = new Mesh
            {
                vertices = CreateVertices(pet),
                boneWeights = CreateBoneWeights(pet),
                bindposes = CreateBindPoses(pet)
            };

            Dictionary<int, Vector2[]> uvsById = CreateUvsById(pet);
            for (int i = 0; i < uvsById.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        mesh.uv = uvsById[i];
                        break;
                    case 1:
                        mesh.uv2 = uvsById[i];
                        break;
                    default:
                        throw new NotImplementedException("Currently only two UV mappings are supported");
                }
            }

            Dictionary<int, List<int>> trianglesByTextureIndex = CreateTrianglesByTextureIndex(pet);
            mesh.subMeshCount = trianglesByTextureIndex.Count;

            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                mesh.SetTriangles(trianglesByTextureIndex[i].ToArray(), i);
            }

            return mesh;
        }

        // array size is vertex count        
        private static Vector3[] CreateVertices(PETFile pet)
        {
            // create unique vertices with the index used in the polygons
            Vector3[] uniqueVertices = pet.Vertices
                .Select(vertex => new Vector3(vertex.X, vertex.Y, vertex.Z))
                .ToArray();

            // every unique vertex can appear multiple times in a model (at points where polygons touch each other)
            Vector3[] vertices = pet.Polygons
                .SelectMany(polygon => polygon.PolygonIndices)
                .Select(polygonIndex => uniqueVertices[polygonIndex.Index])
                .ToArray();

            return vertices;
        }

        // array size is vertex count
        private static BoneWeight[] CreateBoneWeights(PETFile pet)
        {
            int uniqueVertexCount = pet.Vertices.Count;

            // create unique vertices with the index used in the polygons
            BoneWeight[] uniqueBoneWeights = new BoneWeight[uniqueVertexCount];
            for (int i = 0; i < uniqueVertexCount; i++)
            {
                List<BoneInformation> boneInformationList = pet.Vertices[i].BoneInformation;

                // TODO meh
                for (int index = 0; index < boneInformationList.Count; index++)
                {
                    BoneInformation boneInformation = boneInformationList[index];
                    switch (index)
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
                        default:
                            throw new NotSupportedException(
                                "Unity only supports 4 different weights for a single bone");
                    }
                }
            }

            BoneWeight[] boneWeights = pet.Polygons
                .SelectMany(polygon => polygon.PolygonIndices)
                .Select(polygonIndex => uniqueBoneWeights[polygonIndex.Index])
                .ToArray();

            return boneWeights;
        }

        // array size is bone count
        private static Matrix4x4[] CreateBindPoses(PETFile pet)
        {
            Matrix4x4[] bindPoses = pet.Bones
                .Select(bone => bone.Matrix)
                .Select(m => new Matrix4x4(
                    new Vector4(m[0], m[1], m[2], 0),
                    new Vector4(m[3], m[4], m[5], 0),
                    new Vector4(m[6], m[7], m[8], 0),
                    new Vector4(m[9], m[10], m[11], 1))).ToArray();

            return bindPoses;
        }

        private static Dictionary<int, Vector2[]> CreateUvsById(PETFile pet)
        {
            Dictionary<int, Vector2[]> uvs = Enumerable.Range(0, pet.Polygons[0].PolygonIndices[0].UVMappings.Count)
                .ToDictionary(uvIndex => uvIndex, uvIndex => pet.Polygons
                    .SelectMany(polygon => polygon.PolygonIndices)
                    .Select(polygonIndex => polygonIndex.UVMappings[uvIndex])
                    .Select(uvMapping => new Vector2(uvMapping.U, 1 - uvMapping.V)) // V in PETFiles is flipped
                    .ToArray());

            return uvs;
        }

        // same size as Materials array
        private static Dictionary<int, List<int>> CreateTrianglesByTextureIndex(PETFile pet)
        {
            Dictionary<int, List<int>> trianglesByTextureIndex = Enumerable.Range(0, pet.Textures.Count)
                .ToDictionary(i => i, _ => new List<int>());

            for (int i = 0; i < pet.Polygons.Count; i++)
            {
                Polygon polygon = pet.Polygons[i];
                for (int j = 0; j < 3; j++)
                {
                    int targetIndex = i * 3 + j;
                    int textureIndex = (int) polygon.TextureIndex;

                    trianglesByTextureIndex[textureIndex].Add(targetIndex);
                }
            }

            return trianglesByTextureIndex;
        }
    }
}
