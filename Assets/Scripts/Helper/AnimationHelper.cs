using System.Collections.Generic;
using PangLib.PET;
using PangLib.PET.DataModels;
using UnityEngine;
using Animation = PangLib.PET.DataModels.Animation;

namespace Helper
{
    public static class AnimationHelper
    {
        public static AnimationClip CreateAnimationClip(PETFile pet)
        {
            AnimationClip clip = new AnimationClip
            {
                legacy = true,
                wrapMode = WrapMode.Loop
            };

            foreach (Animation anim in pet.Animations)
            {
                // TODO AnimationFlags

                string boneName = pet.Bones[anim.BoneID].Name;

                AddPositionDataToClip(anim.PositionData, boneName, clip);
                AddRotationDataToClip(anim.RotationData, boneName, clip);
                AddScalingDataToClip(anim.ScalingData, boneName, clip);
            }

            return clip;
        }

        private static void AddPositionDataToClip(IReadOnlyList<PositionData> positionData, string boneName,
            AnimationClip clip)
        {
            int frameCount = positionData.Count;

            Keyframe[] posX = new Keyframe[frameCount];
            Keyframe[] posY = new Keyframe[frameCount];
            Keyframe[] posZ = new Keyframe[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                PositionData data = positionData[i];
                posX[i] = new Keyframe(data.Time, data.X);
                posY[i] = new Keyframe(data.Time, data.Y);
                posZ[i] = new Keyframe(data.Time, data.Z);
            }

            clip.SetCurve(boneName, typeof(Transform), "localPosition.x", new AnimationCurve(posX));
            clip.SetCurve(boneName, typeof(Transform), "localPosition.y", new AnimationCurve(posY));
            clip.SetCurve(boneName, typeof(Transform), "localPosition.z", new AnimationCurve(posZ));
        }

        private static void AddRotationDataToClip(IReadOnlyList<RotationData> rotationData, string boneName,
            AnimationClip clip)
        {
            int frameCount = rotationData.Count;

            Keyframe[] rotX = new Keyframe[frameCount];
            Keyframe[] rotY = new Keyframe[frameCount];
            Keyframe[] rotZ = new Keyframe[frameCount];
            Keyframe[] rotW = new Keyframe[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                RotationData data = rotationData[i];
                rotX[i] = new Keyframe(data.Time, data.X);
                rotY[i] = new Keyframe(data.Time, data.Y);
                rotZ[i] = new Keyframe(data.Time, data.Z);
                rotW[i] = new Keyframe(data.Time, data.W);
            }

            clip.SetCurve(boneName, typeof(Transform), "localRotation.x", new AnimationCurve(rotX));
            clip.SetCurve(boneName, typeof(Transform), "localRotation.y", new AnimationCurve(rotY));
            clip.SetCurve(boneName, typeof(Transform), "localRotation.z", new AnimationCurve(rotZ));
            clip.SetCurve(boneName, typeof(Transform), "localRotation.w", new AnimationCurve(rotW));
        }

        private static void AddScalingDataToClip(IReadOnlyList<ScalingData> scalingData, string boneName,
            AnimationClip clip)
        {
            int frameCount = scalingData.Count;

            Keyframe[] scaleX = new Keyframe[frameCount];
            Keyframe[] scaleY = new Keyframe[frameCount];
            Keyframe[] scaleZ = new Keyframe[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                ScalingData data = scalingData[i];
                scaleX[i] = new Keyframe(data.Time, data.X);
                scaleY[i] = new Keyframe(data.Time, data.Y);
                scaleZ[i] = new Keyframe(data.Time, data.Z);
            }

            clip.SetCurve(boneName, typeof(Transform), "localScale.x", new AnimationCurve(scaleX));
            clip.SetCurve(boneName, typeof(Transform), "localScale.y", new AnimationCurve(scaleY));
            clip.SetCurve(boneName, typeof(Transform), "localScale.z", new AnimationCurve(scaleZ));
        }
    }
}
