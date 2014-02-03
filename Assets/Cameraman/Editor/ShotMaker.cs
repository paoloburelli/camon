using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ShotMaker
{
    [MenuItem("Assets/Create/Cameraman/Shot")]
    public static void Crate()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (path == "")
            path = "Assets";

        Shot asset = ScriptableObject.CreateInstance<Shot>();
        AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(path + "/Shot.asset"));
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}