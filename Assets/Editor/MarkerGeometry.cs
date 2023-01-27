using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Tames.MarkerArea))]
public class MarkerGeometry : Editor
{
    public enum Geometry { Box, Sphere, Cylinder};
    public Geometry geometry = new Geometry();

    // The function that makes the custom editor work
    public override void OnInspectorGUI()
    {

        // Display the enum popup in the inspector
        geometry = (Geometry)EditorGUILayout.EnumPopup("Geometry", geometry);

        // Create a space to separate this enum popup from the other variables 
        EditorGUILayout.Space();

        // Check the value of the enum and display variables based on it

        // Save all changes made on the Inspector
        serializedObject.ApplyModifiedProperties();
    }
}
