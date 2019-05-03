using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(FractalCoral))]
public class VoronoiFractalCoralEditor : Editor
{
    public override void OnInspectorGUI()
    {
        FractalCoral myFractalCoral = (FractalCoral)target;

        if (GUILayout.Button("Generate Coral"))
        {
            myFractalCoral.GenerateCoral();
        }



        DrawDefaultInspector();






        // base.OnInspectorGUI();
    }
}
