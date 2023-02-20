using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



[CustomEditor(typeof(Markers.MarkerArea))]
public class MarkerGeometry : Editor
{
//    SerializedProperty thisIsArea;
    SerializedProperty geometry;
    SerializedProperty input;
    SerializedProperty update;
    SerializedProperty mode;
    SerializedProperty appliesTo;

    void OnEnable()
    {
  //      thisIsArea = serializedObject.FindProperty("thisIsArea");
        geometry = serializedObject.FindProperty("geometry");
        input = serializedObject.FindProperty("input");
        update = serializedObject.FindProperty("update");
        mode = serializedObject.FindProperty("mode");
        appliesTo = serializedObject.FindProperty("appliesTo");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
  //      EditorGUILayout.PropertyField(thisIsArea);
        EditorGUILayout.PropertyField(geometry);
        EditorGUILayout.PropertyField(input);
        EditorGUILayout.PropertyField(update);
        EditorGUILayout.PropertyField(mode);
        EditorGUILayout.PropertyField(appliesTo);

        serializedObject.ApplyModifiedProperties();
    }
}
[CustomEditor(typeof(Markers.MarkerChanger)), CanEditMultipleObjects]
public class MarkerChangerEditor : Editor
{
    SerializedProperty property;
    SerializedProperty mode;
    SerializedProperty switchValue;
    SerializedProperty steps;

    void OnEnable()
    {
        property = serializedObject.FindProperty("property");
        mode = serializedObject.FindProperty("mode");
        switchValue = serializedObject.FindProperty("switchValue");
        steps = serializedObject.FindProperty("steps");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(property);
        EditorGUILayout.PropertyField(mode);
        EditorGUILayout.PropertyField(switchValue);
        EditorGUILayout.PropertyField(steps);

        serializedObject.ApplyModifiedProperties();
    }
}
[CustomEditor(typeof(Markers.MarkerProgress)), CanEditMultipleObjects]
public class MarkerProgressEditor : Editor
{
    SerializedProperty cycleType;
    SerializedProperty initialStatus;
    SerializedProperty setAt;
    SerializedProperty duration;
    SerializedProperty speedFactor;
    SerializedProperty speedOffset;
    SerializedProperty byElement;
    SerializedProperty byName;
    SerializedProperty trigger;


    void OnEnable()
    {
        cycleType = serializedObject.FindProperty("cycleType");
        initialStatus = serializedObject.FindProperty("initialStatus");
        setAt = serializedObject.FindProperty("setAt");
        duration = serializedObject.FindProperty("duration");
        speedFactor = serializedObject.FindProperty("speedFactor");
        speedOffset = serializedObject.FindProperty("speedOffset");
        byElement = serializedObject.FindProperty("byElement");
        byName = serializedObject.FindProperty("byName");
        trigger = serializedObject.FindProperty("trigger");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(cycleType);
        EditorGUILayout.PropertyField(initialStatus);
        EditorGUILayout.PropertyField(setAt);
        EditorGUILayout.PropertyField(duration);
        EditorGUILayout.PropertyField(speedFactor);
        EditorGUILayout.PropertyField(speedOffset);
        EditorGUILayout.PropertyField(byElement);
        EditorGUILayout.PropertyField(byName);
        EditorGUILayout.PropertyField(trigger);

        serializedObject.ApplyModifiedProperties();
    }
}
