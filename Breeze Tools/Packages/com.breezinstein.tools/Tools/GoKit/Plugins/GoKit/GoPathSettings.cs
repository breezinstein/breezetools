﻿using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "New GoPath", menuName = "GoKit/Create New GoPath Asset")]
public class GoPathSettings : ScriptableObject
{
    public List<Vector3> Nodes;

#if UNITY_EDITOR
    // From: https://wiki.unity3d.com/index.php?title=CreateScriptableObjectAsset
    public static void CreateAsset(string path, List<Vector3> nodes)
    {
        GoPathSettings asset = ScriptableObject.CreateInstance<GoPathSettings>();
        asset.Nodes = nodes;

        AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(path));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
#endif
}


