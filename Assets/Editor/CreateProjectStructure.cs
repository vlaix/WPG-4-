using UnityEngine;
using UnityEditor;

public class CreateProjectStructure
{
    [MenuItem("Tools/Create 3D Project Structure")]
    public static void CreateFolders()
    {
        string[] folders =
        {
            "Assets/1. Scenes",
           
            "Assets/2. Scripts",
            
            "Assets/3. Art",
    
            "Assets/4. Rendering",
            
            "Assets/5. Audio",
          
            "Assets/6. Prefabs",

            "Assets/7. ScriptableObjects",

            "Assets/8. Inputs",

            "Assets/9. Resources",

            "Assets/10. Plugins"
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
