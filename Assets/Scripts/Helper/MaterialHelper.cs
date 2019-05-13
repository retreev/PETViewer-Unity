using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Util;
using Texture = PangLib.PET.DataModels.Texture;

namespace Helper
{
    public static class MaterialHelper
    {
        public static Material[] CreateMaterials(List<Texture> petTextures,
            string textureSearchPath)
        {
            int textureCount = petTextures.Count;

            Material[] materials = new Material[textureCount];
            for (int i = 0; i < textureCount; i++)
            {
                Material material;

                string textureFileName = petTextures[i].FileName;
                string texturePath = FileHelper.GetTexture(textureSearchPath, textureFileName);
                Texture2D mainTexture = CreateTexture(texturePath);

                // TODO there might be an attribute in the PET file telling which textures have masks
                // find mask for each texture or create an empty one if it doesn't exist
                string maskPath = GetMaskPath(texturePath);
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

        private static Texture2D CreateTexture(string path)
        {
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(0, 0); // size will be automatically adjusted after loading image
            texture.LoadImage(fileData);
            return texture;
        }

        private static Texture2D CreateAlphaTexture(string path)
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
