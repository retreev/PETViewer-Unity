using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Util;
using Texture = PangLib.PET.DataModels.Texture;

namespace Helper
{
    public static class MaterialHelper
    {
        private static readonly Shader OpaqueShader = Shader.Find("OpaqueTexture");
        private static readonly Shader MaskedShader = Shader.Find("MaskedTexture");

        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int Mask = Shader.PropertyToID("_Mask");

        public static Material[] CreateMaterials(IEnumerable<Texture> petTextures, string textureSearchPath)
        {
            Material[] materials = petTextures
                .Select(texture =>
                {
                    string textureFileName = texture.FileName;
                    string texturePath = FileHelper.GetTexture(textureSearchPath, textureFileName);

                    // TODO there might be an attribute in the PET file telling which textures have masks
                    // find mask for each texture or create an empty one if it doesn't exist
                    string maskPath = GetMaskPath(texturePath);

                    Material material = File.Exists(maskPath)
                        ? CreateMaskedMaterial(texturePath, maskPath)
                        : CreateOpaqueMaterial(texturePath);

                    material.name = textureFileName;

                    return material;
                })
                .ToArray();

            return materials;
        }

        private static Material CreateOpaqueMaterial(string texturePath)
        {
            Material material = new Material(OpaqueShader);
            material.SetTexture(MainTex, CreateTexture(texturePath));

            return material;
        }

        private static Material CreateMaskedMaterial(string texturePath, string maskPath)
        {
            Material material = new Material(MaskedShader);
            material.SetTexture(MainTex, CreateTexture(texturePath));
            material.SetTexture(Mask, CreateTexture(maskPath));

            return material;
        }

        private static Texture2D CreateTexture(string path)
        {
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(fileData);
            return texture;
        }

        private static string GetMaskPath(string texturePath)
        {
            return texturePath.Replace(".jpg", "_mask.jpg");
        }
    }
}
