using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



[CustomEditor(typeof(Markers.MarkerArea)), CanEditMultipleObjects]
public class AreaInEditor : Editor
{
    //    SerializedProperty thisIsArea;
    SerializedProperty geometry;
    SerializedProperty range;
    SerializedProperty update;
    SerializedProperty mode;
    SerializedProperty appliesTo;
    SerializedProperty applyToSelf;
    SerializedProperty autoPosition;
    SerializedProperty control;

    void OnEnable()
    {
        //      thisIsArea = serializedObject.FindProperty("thisIsArea");
        geometry = serializedObject.FindProperty("geometry");
        range = serializedObject.FindProperty("range");
        update = serializedObject.FindProperty("update");
        mode = serializedObject.FindProperty("mode");
        applyToSelf = serializedObject.FindProperty("applyToSelf");
        appliesTo = serializedObject.FindProperty("appliesTo");
        autoPosition = serializedObject.FindProperty("autoPosition");
        control = serializedObject.FindProperty("control");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        //      EditorGUILayout.PropertyField(thisIsArea);
        EditorGUILayout.PropertyField(geometry);
        EditorGUILayout.PropertyField(range);
        EditorGUILayout.PropertyField(update);
        EditorGUILayout.PropertyField(mode);
        EditorGUILayout.PropertyField(applyToSelf);
        EditorGUILayout.PropertyField(appliesTo);
        EditorGUILayout.PropertyField(autoPosition);
        EditorGUILayout.PropertyField(control);

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
[CustomEditor(typeof(Markers.MarkerDynamic))]
public class DynamicInEditor : Editor
{
    //    SerializedProperty thisIsArea;
    SerializedProperty type;
    SerializedProperty up;
    SerializedProperty mover;
    void OnEnable()
    {
        type = serializedObject.FindProperty("type");
        up = serializedObject.FindProperty("up");
        mover = serializedObject.FindProperty("mover");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(type);
        EditorGUILayout.PropertyField(up);
        EditorGUILayout.PropertyField(mover);
        serializedObject.ApplyModifiedProperties();
        Markers.MarkerDynamic dyn = (Markers.MarkerDynamic)target;
        dyn.ChangeDynamic();
        if (GUILayout.Button("Remove"))
        {
            dyn.Remove();
        }
    }
}
[CustomEditor(typeof(Markers.MarkerChanger)), CanEditMultipleObjects]
public class MarkerChangerEditor : Editor
{
    SerializedProperty byElement;
    SerializedProperty property;
    SerializedProperty mode;
    SerializedProperty switchValue;
    SerializedProperty steps;
    SerializedProperty factor;
    SerializedProperty colorSteps;
    SerializedProperty flicker;

    void OnEnable()
    {
        byElement = serializedObject.FindProperty("byElement");
        property = serializedObject.FindProperty("property");
        mode = serializedObject.FindProperty("mode");
        switchValue = serializedObject.FindProperty("switchValue");
        steps = serializedObject.FindProperty("steps");
        factor = serializedObject.FindProperty("factor");
        colorSteps = serializedObject.FindProperty("colorSteps");
        flicker = serializedObject.FindProperty("flicker");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(byElement);
        EditorGUILayout.PropertyField(property);
        EditorGUILayout.PropertyField(mode);
        EditorGUILayout.PropertyField(switchValue);
        EditorGUILayout.PropertyField(steps);
        EditorGUILayout.PropertyField(factor);
        EditorGUILayout.PropertyField(colorSteps);
        EditorGUILayout.PropertyField(flicker);

        serializedObject.ApplyModifiedProperties();
        Markers.MarkerChanger changer = (Markers.MarkerChanger)target;
        changer.ChangedThisFrame(true);
    }
}

[CustomEditor(typeof(Markers.MarkerSettings))]
class MarkerSettingsEditor : Editor
{
    SerializedProperty torch;
    SerializedProperty eyeHeights;
    SerializedProperty customManifests;
    SerializedProperty materialEmission;
    SerializedProperty replay;
    void OnEnable()
    {
        torch = serializedObject.FindProperty("torch");
        replay = serializedObject.FindProperty("replay");
        eyeHeights = serializedObject.FindProperty("eyeHeights");
        customManifests = serializedObject.FindProperty("customManifests");
        materialEmission = serializedObject.FindProperty("materialEmission");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(torch);
        EditorGUILayout.PropertyField(replay);
        EditorGUILayout.PropertyField(eyeHeights);
        EditorGUILayout.PropertyField(customManifests);
        EditorGUILayout.PropertyField(materialEmission);
        Markers.MarkerSettings settings = (Markers.MarkerSettings)target;
        if (GUILayout.Button("Save intensity"))
        {
            settings.FreezeIntensity();
        }
        if (GUILayout.Button("Reset intensity"))
        {
            settings.ResetIntensity();
        }
        if (GUILayout.Button("Save"))
        {
            settings.Save();
        }
        if (GUILayout.Button("Load"))
        {
            settings.Load();
        }
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
    SerializedProperty steps;
    SerializedProperty preset;
    SerializedProperty setTo;
    SerializedProperty duration;
    SerializedProperty lerpXY;
    SerializedProperty trigger;
    SerializedProperty parent;
  //  SerializedProperty byMaterial;
//    SerializedProperty manualControl;
    SerializedProperty update;
   // SerializedProperty showBy;
    SerializedProperty active;
 //   SerializedProperty activationControl;
 //   SerializedProperty visibilityControl;
  //  SerializedProperty activateBy;
  Markers.MarkerProgress progress;

    void OnEnable()
    {
        continuity = serializedObject.FindProperty("continuity");
        steps = serializedObject.FindProperty("steps");
        preset = serializedObject.FindProperty("preset");
        setTo = serializedObject.FindProperty("setTo");
        duration = serializedObject.FindProperty("duration");
        lerpXY = serializedObject.FindProperty("lerpXY");
        trigger = serializedObject.FindProperty("trigger");
        parent = serializedObject.FindProperty("parent");
          update = serializedObject.FindProperty("update");
        active = serializedObject.FindProperty("active");
   }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(continuity);
        EditorGUILayout.PropertyField(steps);
        EditorGUILayout.PropertyField(preset);
        EditorGUILayout.PropertyField(setTo);
        EditorGUILayout.PropertyField(duration);
        EditorGUILayout.PropertyField(lerpXY);
        EditorGUILayout.PropertyField(trigger);
        EditorGUILayout.PropertyField(parent);
        EditorGUILayout.PropertyField(update);
        EditorGUILayout.PropertyField(active);
  
        serializedObject.ApplyModifiedProperties();
        progress = (Markers.MarkerProgress)target;
        progress.ChangedThisFrame(true);
    }
}
[CustomEditor(typeof(Markers.MarkerFlicker)), CanEditMultipleObjects]
public class MarkerFlickerEditor : Editor
{
    SerializedProperty property;
    SerializedProperty byMaterial;
    SerializedProperty byLight;
    SerializedProperty minFlicker;
    SerializedProperty maxFlicker;
    SerializedProperty flickerCount;
    SerializedProperty steadyPortion;

    void OnEnable()
    {
        property = serializedObject.FindProperty("property");
        byMaterial = serializedObject.FindProperty("byMaterial");
        byLight = serializedObject.FindProperty("byLight");
        minFlicker = serializedObject.FindProperty("minFlicker");
        maxFlicker = serializedObject.FindProperty("maxFlicker");
        flickerCount = serializedObject.FindProperty("flickerCount");
        steadyPortion = serializedObject.FindProperty("steadyPortion");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(property);
        EditorGUILayout.PropertyField(byMaterial);
        EditorGUILayout.PropertyField(byLight);
        EditorGUILayout.PropertyField(minFlicker);
        EditorGUILayout.PropertyField(maxFlicker);
        EditorGUILayout.PropertyField(flickerCount);
        EditorGUILayout.PropertyField(steadyPortion);
        serializedObject.ApplyModifiedProperties();
    }
}
[CustomEditor(typeof(Markers.ExportOption))]
public class ExportOptionEditor : Editor
{
    SerializedProperty folder;
    SerializedProperty time;
    SerializedProperty onlyIfChanged;
    SerializedProperty personIndex;
    SerializedProperty headPosition;
    SerializedProperty lookDirection;
    SerializedProperty handPosition;
    SerializedProperty handRotation;
    SerializedProperty bothHands;
    SerializedProperty actionKeys;
    SerializedProperty actionMouse;
    SerializedProperty actionGamePad;
    SerializedProperty actionVRController;

    void OnEnable()
    {
        folder = serializedObject.FindProperty("folder");
        time = serializedObject.FindProperty("time");
        onlyIfChanged = serializedObject.FindProperty("onlyIfChanged");
        personIndex = serializedObject.FindProperty("personIndex");
        headPosition = serializedObject.FindProperty("headPosition");
        lookDirection = serializedObject.FindProperty("lookDirection");
        handPosition = serializedObject.FindProperty("handPosition");
        handRotation = serializedObject.FindProperty("handRotation");
        bothHands = serializedObject.FindProperty("bothHands");
        actionKeys = serializedObject.FindProperty("actionKeys");
        actionMouse = serializedObject.FindProperty("actionMouse");
        actionGamePad = serializedObject.FindProperty("actionGamePad");
        actionVRController = serializedObject.FindProperty("actionVRController");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(folder);
        EditorGUILayout.PropertyField(time);
        EditorGUILayout.PropertyField(onlyIfChanged);
        EditorGUILayout.PropertyField(personIndex);
        EditorGUILayout.PropertyField(headPosition);
        EditorGUILayout.PropertyField(lookDirection);
        EditorGUILayout.PropertyField(handPosition);
        EditorGUILayout.PropertyField(handRotation);
        EditorGUILayout.PropertyField(bothHands);
        EditorGUILayout.PropertyField(actionKeys);
        EditorGUILayout.PropertyField(actionMouse);
        EditorGUILayout.PropertyField(actionGamePad);
        EditorGUILayout.PropertyField(actionVRController);
        Markers.ExportOption option = (Markers.ExportOption)target;
        if (GUILayout.Button("Export To CSV"))
        {
            option.Export();
        }
        serializedObject.ApplyModifiedProperties();
    }
}
[CustomEditor(typeof(Markers.MarkerInfo)), CanEditMultipleObjects]
public class MarkerInfoEditor : Editor
{
    SerializedProperty imagePosition;
    SerializedProperty textPosition;
 ///   SerializedProperty lineCount;
    SerializedProperty color;
    SerializedProperty background;
    SerializedProperty textColor;
    SerializedProperty textHighlight;
    SerializedProperty font;
    SerializedProperty items;
    SerializedProperty margin;
    SerializedProperty width;
    SerializedProperty height;
    SerializedProperty position;
    SerializedProperty X;
    SerializedProperty Y;
    SerializedProperty rotateObject;
    SerializedProperty control;
    SerializedProperty areas;
    SerializedProperty references;
    SerializedProperty link;


    void OnEnable()
    {
        imagePosition = serializedObject.FindProperty("imagePosition");
        textPosition = serializedObject.FindProperty("textPosition");
     //   lineCount = serializedObject.FindProperty("lineCount");
        color = serializedObject.FindProperty("color");
        background = serializedObject.FindProperty("background");
        textColor = serializedObject.FindProperty("textColor");
        textHighlight = serializedObject.FindProperty("textHighlight");
        font = serializedObject.FindProperty("font");
        items = serializedObject.FindProperty("items");
        margin = serializedObject.FindProperty("margin");
        width = serializedObject.FindProperty("width");
        height = serializedObject.FindProperty("height");
        position = serializedObject.FindProperty("position");
        X = serializedObject.FindProperty("X");
        Y = serializedObject.FindProperty("Y");
        rotateObject = serializedObject.FindProperty("rotateObject");
        control = serializedObject.FindProperty("control");
        areas = serializedObject.FindProperty("areas");
        references = serializedObject.FindProperty("references");
        link = serializedObject.FindProperty("link");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(imagePosition);
        EditorGUILayout.PropertyField(textPosition);
   //     EditorGUILayout.PropertyField(lineCount);
        EditorGUILayout.PropertyField(color);
        EditorGUILayout.PropertyField(background);
        EditorGUILayout.PropertyField(textColor);
        EditorGUILayout.PropertyField(textHighlight);
      //  EditorGUILayout.PropertyField(font);
        EditorGUILayout.PropertyField(items);
        EditorGUILayout.PropertyField(margin);
        EditorGUILayout.PropertyField(width);
        EditorGUILayout.PropertyField(height);
        EditorGUILayout.PropertyField(position);
        EditorGUILayout.PropertyField(X);
        EditorGUILayout.PropertyField(Y);
        EditorGUILayout.PropertyField(rotateObject);
        EditorGUILayout.PropertyField(control);
         EditorGUILayout.PropertyField(areas);
        EditorGUILayout.PropertyField(references);
        EditorGUILayout.PropertyField(link);
        serializedObject.ApplyModifiedProperties();
        Markers.MarkerInfo ms = (Markers.MarkerInfo)target;
        ms.ChangedThisFrame(true);
    }
}
[CustomEditor(typeof(Markers.MarkerScore)), CanEditMultipleObjects]
public class MarkerScoreditor : Editor
{
    SerializedProperty isBasket;
    SerializedProperty score;
    SerializedProperty count;
    SerializedProperty passScore;
    SerializedProperty basket;
    SerializedProperty interval;
    SerializedProperty onlyAfter;
    SerializedProperty activate;
    SerializedProperty show;
    SerializedProperty control;
    SerializedProperty showAfter;
    SerializedProperty activateAfter;

    void OnEnable()
    {
        isBasket = serializedObject.FindProperty("isBasket");
        score = serializedObject.FindProperty("score");
        count = serializedObject.FindProperty("count");
        passScore = serializedObject.FindProperty("passScore");
        basket = serializedObject.FindProperty("basket");
        interval = serializedObject.FindProperty("interval");
        onlyAfter = serializedObject.FindProperty("onlyAfter");
        activate = serializedObject.FindProperty("activate");
        show = serializedObject.FindProperty("show");
        control = serializedObject.FindProperty("control");
        activateAfter = serializedObject.FindProperty("activateAfter");
        showAfter = serializedObject.FindProperty("showAfter");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Markers.MarkerScore ms = (Markers.MarkerScore)target;
        EditorGUILayout.PropertyField(isBasket);
        if (ms.isBasket)
        {
            EditorGUILayout.PropertyField(passScore);
        }
        else
        {
            EditorGUILayout.PropertyField(score);
            EditorGUILayout.PropertyField(count);
            EditorGUILayout.PropertyField(interval);
            EditorGUILayout.PropertyField(basket);
            EditorGUILayout.PropertyField(onlyAfter);
            EditorGUILayout.PropertyField(control);
            EditorGUILayout.PropertyField(showAfter);
        }
        EditorGUILayout.PropertyField(activateAfter);
        EditorGUILayout.PropertyField(activate);
        EditorGUILayout.PropertyField(show);
        serializedObject.ApplyModifiedProperties();
    }
}