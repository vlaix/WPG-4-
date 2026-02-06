using UnityEngine;
using UnityEditor;

public class CreateProjectStructure
{
    [MenuItem("Tools/Create 3D Project Structure")]
    public static void CreateFolders()
    {
        string[] folders =
        {
            "Assets/0. Core",
            "Assets/0. Core/Scenes",
            "Assets/0. Core/Prefabs",
            "Assets/0. Core/Scripts",
            "Assets/0. Core/ScriptableObjects",

            "Assets/1. Art",
            "Assets/1. Art/Models",
            "Assets/1. Art/Materials",
            "Assets/1. Art/Textures",

            "Assets/2. Audio",
            "Assets/2. Audio/Music",
            "Assets/2. Audio/SFX",

            "Assets/3. Environment",
            "Assets/3. Environment/Props",

            "Assets/4. Characters",
            "Assets/4. Characters/Player",
            "Assets/4. Characters/Enemies",

            "Assets/5. UI",
            "Assets/6. Systems",
            "Assets/7. Rendering",
            "Assets/8. Resources",
            "Assets/9. Plugins"
        };

        foreach (string folder in folders)
        {
            CreateFolder(folder);
        }

        AssetDatabase.Refresh();
        Debug.Log("STRUCTURE CREATED!");
    }

    static void CreateFolder(string fullPath)
    {
        if (AssetDatabase.IsValidFolder(fullPath))
            return;

        string parent = System.IO.Path.GetDirectoryName(fullPath);
        string newFolderName = System.IO.Path.GetFileName(fullPath);

        if (!AssetDatabase.IsValidFolder(parent))
        {
            CreateFolder(parent);
        }

        AssetDatabase.CreateFolder(parent, newFolderName);
    }
}
