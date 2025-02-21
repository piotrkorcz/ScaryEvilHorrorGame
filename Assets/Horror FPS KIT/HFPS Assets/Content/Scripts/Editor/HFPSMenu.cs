using System.IO;
using UnityEngine;
using UnityEditor;

static class HFPSMenu  {

    static string SaveFolder = Application.dataPath + "/Data/SaveGame/";

    [MenuItem("Tools/" + "HFPS KIT" + "/SaveGame/Set LoadGame")]
    static void LoadGame_True()
    {
        int display = EditorUtility.DisplayDialogComplex("Set LoadGame State", "Load SavedGame on Start?", "Yes", "No", "Cancel");
        if(display == 0)
        {
            PlayerPrefs.SetInt("LoadGame", 1);
            Debug.Log("LoadGame was set to \"True\"");
        }
        else if(display == 1)
        {
            PlayerPrefs.SetInt("LoadGame", 0);
            Debug.Log("LoadGame was set to \"False\"");
        }
        else if (display == 2)
        {
            return;
        }
    }

    [MenuItem("Tools/" + "HFPS KIT" + "/SaveGame/" + "Delete SavedGame")]
    static void DeleteSavedGame()
    {
        if (Directory.Exists(SaveFolder))
        {
            string[] files = Directory.GetFiles(SaveFolder, "Save?.sav");
            if (files.Length > 0)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    File.Delete(files[i]);
                    EditorUtility.DisplayDialog("SaveGame Deleted", "Deleting SavedGame is completed.", "Okay");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Directory empty", "Directory is already empty.", "Okay");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Directory not found", "Failed to find Directory  " + "\"/Data/SaveGame/\"", "Okay");
        }
    }
}
