using PangLib.PET;
using PangLib.PET.DataModels;
using UnityEngine;

namespace Helper
{
    public static class BoneHelper
    {
        // size is bone count
        public static Transform[] CreateBones(PETFile pet, GameObject parentGameObject)
        {
            int boneCount = pet.Bones.Count;
            Transform[] bones = new Transform[boneCount];

            for (int i = 0; i < boneCount; i++)
            {
                Bone petBone = pet.Bones[i];

                Transform bone = new GameObject(petBone.Name).transform;
                if (petBone.Parent == 255)
                {
                    // 255 is the root bone
                    bone.parent = parentGameObject.transform;
                }
                else
                {
                    bone.parent = bones[petBone.Parent].transform;
                }

                bones[i] = bone;
            }

            return bones;
        }
    }
}
