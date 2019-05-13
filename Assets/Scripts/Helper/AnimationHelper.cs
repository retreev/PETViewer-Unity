using System.Collections.Generic;
using PangLib.PET;
using PangLib.PET.DataModels;
using UnityEngine;
using Animation = PangLib.PET.DataModels.Animation;

namespace Helper
{
    public class AnimationHelper
    {
        private AnimationClip _clip;

        private PETFile _pet;

        // TODO handle multiple animations?
        public AnimationHelper(PETFile pet)
        {
            _clip = new AnimationClip
            {
                legacy = true,
                wrapMode = WrapMode.Loop
            };

            _pet = pet;
        }

        public AnimationClip Convert()
        {
            foreach (Animation anim in _pet.Animations)
            {
                // TODO AnimationFlags

                string boneName = _pet.Bones[anim.BoneID].Name;

                AddPositionDataToClip(anim.PositionData, boneName);
                AddRotationDataToClip(anim.RotationData, boneName);
                AddScalingDataToClip(anim.ScalingData, boneName);
            }

            return _clip;
        }

        private void AddPositionDataToClip(IReadOnlyList<PositionData> positionData, string boneName)
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

            _clip.SetCurve(boneName, typeof(Transform), "localPosition.x", new AnimationCurve(posX));
            _clip.SetCurve(boneName, typeof(Transform), "localPosition.y", new AnimationCurve(posY));
            _clip.SetCurve(boneName, typeof(Transform), "localPosition.z", new AnimationCurve(posZ));
        }

        private void AddRotationDataToClip(IReadOnlyList<RotationData> rotationData, string boneName)
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

            _clip.SetCurve(boneName, typeof(Transform), "localRotation.x", new AnimationCurve(rotX));
            _clip.SetCurve(boneName, typeof(Transform), "localRotation.y", new AnimationCurve(rotY));
            _clip.SetCurve(boneName, typeof(Transform), "localRotation.z", new AnimationCurve(rotZ));
            _clip.SetCurve(boneName, typeof(Transform), "localRotation.w", new AnimationCurve(rotW));
        }

        private void AddScalingDataToClip(IReadOnlyList<ScalingData> scalingData, string boneName)
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

            _clip.SetCurve(boneName, typeof(Transform), "localScale.x", new AnimationCurve(scaleX));
            _clip.SetCurve(boneName, typeof(Transform), "localScale.y", new AnimationCurve(scaleY));
            _clip.SetCurve(boneName, typeof(Transform), "localScale.z", new AnimationCurve(scaleZ));
        }
    }
}
