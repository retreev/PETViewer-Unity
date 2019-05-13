using System;
using System.IO;
using PangLib.PET;
using UnityEngine;

namespace Helper
{
    public static class GameObjectHelper
    {
        public static GameObject CreateGameObjectFromPet(string petFilePath, string textureSearchPath)
        {
            PETFile pet = new PETFile(petFilePath);

            GameObject gameObject = new GameObject
            {
                name = Path.GetFileName(petFilePath) ??
                       throw new InvalidOperationException("Please specify a PET file which should be initialized")
            };

            AddRenderer(pet, gameObject, textureSearchPath);
            AddAnimation(pet, gameObject);

            return gameObject;
        }

        private static void AddRenderer(PETFile pet, GameObject gameObject, string textureSearchPath)
        {
            SkinnedMeshRenderer renderer = gameObject.AddComponent<SkinnedMeshRenderer>();
            renderer.materials = MaterialHelper.CreateMaterials(pet.Textures, textureSearchPath);
            renderer.sharedMesh = MeshHelper.CreateMesh(pet);
            renderer.bones = BoneHelper.CreateBones(pet.Bones, gameObject);
        }

        private static void AddAnimation(PETFile pet, GameObject gameObject)
        {
            Animation animation = gameObject.AddComponent<Animation>();
            animation.AddClip(AnimationHelper.CreateAnimationClip(pet), "test");
            animation.Play("test");
        }
    }
}
