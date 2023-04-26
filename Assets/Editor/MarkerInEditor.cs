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
[CustomEditor(typeof(Markers.MarkerOrigin))]
public class OriginInEditor : Editor
{
    //    SerializedProperty thisIsArea;
    SerializedProperty origin;
     void OnEnable()
    {
         origin = serializedObject.FindProperty("origin");
     }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
         EditorGUILayout.PropertyField(origin);
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
[CustomEditor(typeof(Markers.MarkerEnvironment)), CanEditMultipleObjects]
public class MarkerEnvironmentEditor : Editor
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
[CustomEditor(typeof(Markers.MarkerLink)), CanEditMultipleObjects]
public class MarkerLinkEditor : Editor
{
    SerializedProperty type;
    SerializedProperty childrenNames;
    SerializedProperty childrenOf;
    SerializedProperty parent;
    SerializedProperty offsetBase;
    SerializedProperty speedBase;
    SerializedProperty offset;
    SerializedProperty factor;

    void OnEnable()
    {
        type = serializedObject.FindProperty("type");
        childrenNames = serializedObject.FindProperty("childrenNames");
        childrenOf = serializedObject.FindProperty("childrenOf");
        parent = serializedObject.FindProperty("parent");
        offsetBase = serializedObject.FindProperty("offsetBase");
        offset = serializedObject.FindProperty("offset");
        speedBase = serializedObject.FindProperty("speedBase");
        factor = serializedObject.FindProperty("factor");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(type);
        EditorGUILayout.PropertyField(childrenNames);
        EditorGUILayout.PropertyField(childrenOf);
        EditorGUILayout.PropertyField(parent);
        EditorGUILayout.PropertyField(offsetBase);
         EditorGUILayout.PropertyField(offset);
        EditorGUILayout.PropertyField(speedBase);
        EditorGUILayout.PropertyField(factor);

        serializedObject.ApplyModifiedProperties();
    }
}
[CustomEditor(typeof(Markers.MarkerScale)), CanEditMultipleObjects]
public class MarkerScaleEditor : Editor
{
    SerializedProperty byObject;
    SerializedProperty byName;
    SerializedProperty childrenOf;
    SerializedProperty axis;
    SerializedProperty from;
    SerializedProperty to;
    SerializedProperty affectedUV;
   


    void OnEnable()
    {
        byObject = serializedObject.FindProperty("byObject");
        byName = serializedObject.FindProperty("byName");
        childrenOf = serializedObject.FindProperty("childrenOf");
        axis = serializedObject.FindProperty("axis");
        from = serializedObject.FindProperty("from");
        to = serializedObject.FindProperty("to");
        affectedUV = serializedObject.FindProperty("affectedUV");
     
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(byObject);
        EditorGUILayout.PropertyField(byName);
        EditorGUILayout.PropertyField(childrenOf);
        EditorGUILayout.PropertyField(axis);
        EditorGUILayout.PropertyField(from);
        EditorGUILayout.PropertyField(to);
        EditorGUILayout.PropertyField(affectedUV);
      

        serializedObject.ApplyModifiedProperties();
    }
}
[CustomEditor(typeof(Markers.MarkerProgress)), CanEditMultipleObjects]
public class MarkerProgressEditor : Editor
{
    SerializedProperty continuity;
    SerializedProperty initialStatus;
    SerializedProperty setAt;
    SerializedProperty duration;
    SerializedProperty slerp;
    SerializedProperty trigger;
    SerializedProperty byElement;
    SerializedProperty byMaterial;
    SerializedProperty manual;
    SerializedProperty update;
    SerializedProperty showBy;
    SerializedProperty active;
    SerializedProperty activateBy;


    void OnEnable()
    {
        continuity = serializedObject.FindProperty("continuity");
        initialStatus = serializedObject.FindProperty("initialStatus");
        setAt = serializedObject.FindProperty("setAt");
        duration = serializedObject.FindProperty("duration");
        slerp = serializedObject.FindProperty("slerp");
        trigger = serializedObject.FindProperty("trigger");
        byElement = serializedObject.FindProperty("byElement");
        byMaterial = serializedObject.FindProperty("byMaterial");
        manual = serializedObject.FindProperty("manual");
        update = serializedObject.FindProperty("update");
        showBy = serializedObject.FindProperty("showBy");
        active = serializedObject.FindProperty("active");
        activateBy = serializedObject.FindProperty("activateBy");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(continuity);
        EditorGUILayout.PropertyField(initialStatus);
        EditorGUILayout.PropertyField(setAt);
        EditorGUILayout.PropertyField(duration);
        EditorGUILayout.PropertyField(slerp);
        EditorGUILayout.PropertyField(trigger);
        EditorGUILayout.PropertyField(byElement);
        EditorGUILayout.PropertyField(byMaterial);
        EditorGUILayout.PropertyField(manual);
        EditorGUILayout.PropertyField(update);
        EditorGUILayout.PropertyField(showBy);
        EditorGUILayout.PropertyField(active);
        EditorGUILayout.PropertyField(activateBy);

        serializedObject.ApplyModifiedProperties();
    }
}
