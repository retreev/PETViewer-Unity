using System.IO;
using Helper;
using PangLib.PET;
using UnityEngine;

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

//        MeshFilter filter = GameObject.AddComponent<MeshFilter>();
//        filter.mesh = MeshHelper.CreatePlainMesh(_pet);
//        MeshRenderer renderer = GameObject.AddComponent<MeshRenderer>();
//        renderer.materials = MaterialHelper.CreateMaterials(_pet.Textures, _textureSearchPath);

        SkinnedMeshRenderer renderer = GameObject.AddComponent<SkinnedMeshRenderer>();
        renderer.materials = MaterialHelper.CreateMaterials(_pet.Textures, _textureSearchPath);
        renderer.sharedMesh = MeshHelper.CreateMesh(_pet);
        renderer.bones = BoneHelper.CreateBones(_pet, GameObject);

        Animation animation = GameObject.AddComponent<Animation>();
        animation.AddClip(new AnimationHelper(_pet).Convert(), "test");
        animation.Play("test");
    }
}
