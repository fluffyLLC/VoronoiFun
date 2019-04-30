using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEditor;



[CustomEditor(typeof(Voronoi))]
public class VoronoiEditor : Editor
{
    

    public override void OnInspectorGUI()
    {
        Voronoi myVoronoiScript = (Voronoi)target;

        if (GUILayout.Button("New Mesh"))
        {
            myVoronoiScript.NewMesh();
        }

        if (GUILayout.Button("Save Mesh")) {
            myVoronoiScript.SaveMesh();
        }

        DrawDefaultInspector();

        

        


       // base.OnInspectorGUI();
    }

}
