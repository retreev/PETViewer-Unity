using System.IO;
using Exception;
using Helper;
using UnityEngine;

public class TestViewer : MonoBehaviour
{
    private const string PetPath = "local_ignored_pets";
    private const string TextureSearchPath = "local_ignored_pets";

    private void Start()
    {
        Vector3 spacing = new Vector3(3, 0, 0);
        ViewAllPets(spacing);

//        ViewSinglePet("local_ignored_pets/item/ase/item0_01.pet", new Vector3(-5, 1, 0), Vector3.zero);
//        ViewSinglePet("local_ignored_pets/item/ase/item0_34.pet", new Vector3(-2, 1, 0), new Vector3(0, 0, -90));
//        ViewSinglePet("local_ignored_pets/item/ase/item0_18.pet", new Vector3(1, 1, 0), Vector3.zero);
//        ViewSinglePet("local_ignored_pets/item/ase/item1_132_jp.pet", new Vector3(5, 1, 0), new Vector3(0, 0, 90));

//        ViewSinglePet("local_ignored_pets/item/ase/item1_pma.pet", new Vector3(0, 1, 0), Vector3.zero); // odd one, v1.3
//        ViewSinglePet("local_ignored_pets/item/item/ase/item1_380.pet", new Vector3(-5, 1, 0), Vector3.zero); // v1.3
        // v1.3 files: 1_330, 1_331, 335, 336, 337
//        ViewSinglePet("local_ignored_pets/item/ase/item0_34.pet", new Vector3(-2, 1, 0), new Vector3(0, 0, -90));
//        ViewSinglePet("local_ignored_pets/item/ase/item0_18.pet", new Vector3(1, 1, 0), Vector3.zero);
//        ViewSinglePet("local_ignored_pets/item/ase/item1_132_jp.pet", new Vector3(5, 1, 0), new Vector3(0, 0, 90));


//        ViewSinglePet("local_ignored_pets/29qb/qb.pet", new Vector3(0, 1, 0), Vector3.zero);
    }

    private void ViewAllPets(Vector3 spacing)
    {
        string[] files = Directory.GetFiles(PetPath, "*.pet", SearchOption.AllDirectories);

        Vector3 lastPos = new Vector3(0, 1, 0);
        for (int i = 0; i < files.Length; i++)
        {
            string file = files[i];
            try
            {
                Debug.Log("Creating object from file: " + file);
                ViewSinglePet(file, lastPos, Vector3.zero);
                lastPos += spacing;
            }
            catch (TextureNotFoundException e)
            {
                // files with missing textures will be created but not visible
                Debug.LogWarning(e.Message);
            }
            catch (System.Exception e)
            {
                // files that can't be parsed won't be created at all
                Debug.LogError(e.ToString());
            }
        }
    }

    private void ViewSinglePet(string filePath, Vector3 position, Vector3 eulers)
    {
        GameObject pangObject = GameObjectHelper.CreateGameObjectFromPet(filePath, TextureSearchPath);
        // TODO position and scale adjusted, because switching to a SkinnedMeshRenderer increased all model sizes
        pangObject.transform.position = position * 25;
        pangObject.transform.localScale = new Vector3(.1f, .1f, .1f);
        pangObject.transform.Rotate(eulers);
    }
}
