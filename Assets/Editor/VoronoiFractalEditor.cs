using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FractalGroundCover))]
public class VoronoiFractalEditor : Editor
{
    public override void OnInspectorGUI()
    {
        FractalGroundCover myFractalGroundCover = (FractalGroundCover)target;

        if (GUILayout.Button("Generate Coral"))
        {
            myFractalGroundCover.GenerateCoral();
        }

        

        DrawDefaultInspector();






        // base.OnInspectorGUI();
    }
}
