using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class AssetHandler
{
    [OnOpenAsset()]
    public static bool OpenEditor(int instanceId, int line)
    {
        TileArea obj = EditorUtility.InstanceIDToObject(instanceId) as TileArea;
        if(obj != null)
        {
            TileArea_Window.Open(obj);
            return true;
        }
        return false;
    }
}

[CustomEditor(typeof(TileArea))]
public class TileArea_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        if(GUILayout.Button("Open Editor")){
            TileArea_Window.Open((TileArea)target);
        }
    }
}
