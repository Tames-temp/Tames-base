using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Markers
{

    public enum EditorUpdate
    {
        Auto, Fixed, Local, Mover
    }

    public class MarkerArea : MonoBehaviour
    {
        //  public bool thisIsArea;
        public bool applyToSelf = false;
        public GameObject appliesTo;
        public InteractionGeometry geometry;
        public string input;
        public EditorUpdate update;
        public InteractionMode mode;
        public bool autoPosition = false;
        public static List<MarkerArea> allAreas = new List<MarkerArea>();
        //    public GameObject area = null;

        // Start is called before the first frame update
        public string[] ToLines()
        {
            return new string[]
            {
                ":area",
                MarkerSettings.ObjectToLine(gameObject),
               MarkerSettings.ObjectToLine(appliesTo),
                geometry.ToString(),
                input,
                update.ToString(),
                mode.ToString(),
                autoPosition?"1":"0",
               applyToSelf?"1":"0"
            };
        }
        public static int FromLines(string[] line, int index, int version)
        {
            GameObject go = MarkerSettings.LineToObject(line[index]);
            MarkerArea ma;
            if (go != null)
                switch (version)
                {
                    case 1:
                        if ((ma = go.GetComponent<MarkerArea>()) == null) ma = go.AddComponent<MarkerArea>();
                        ma.appliesTo = MarkerSettings.LineToObject(line[index + 1]);
                        ma.geometry = Geo(line[index + 2]);
                        ma.input = line[index + 3];
                        ma.update = Up(line[index + 4]);
                        ma.mode = Mod(line[index + 5]);
                        ma.autoPosition = line[index + 6] == "1";
                        ma.applyToSelf = line[index + 7] == "1";
                        return index + 7;
                }
            return index;
        }


        private static InteractionGeometry Geo(string s)
        {
            return s switch
            {
                "Box" => InteractionGeometry.Box,
                "Sphere" => InteractionGeometry.Sphere,
                "Cylinder" => InteractionGeometry.Cylinder,
                "Remote" => InteractionGeometry.Remote,
                "Distance" => InteractionGeometry.Distance,
                _ => InteractionGeometry.Plane
            };
        }
        private static EditorUpdate Up(string s)
        {
            return s switch
            {
                "Auto" => EditorUpdate.Auto,
                "Fixed" => EditorUpdate.Fixed,
                "Mover" => EditorUpdate.Mover,
                _ => EditorUpdate.Local
            };
        }
        private static InteractionMode Mod(string s)
        {
            return s switch
            {
                "Outside" => InteractionMode.Outside,
                "Inside" => InteractionMode.Inside,
                "Negative" => InteractionMode.Negative,
                "Positive" => InteractionMode.Positive,
                "Switch1" => InteractionMode.Switch1,
                "Switch2" => InteractionMode.Switch2,
                "Switch3" => InteractionMode.Switch3,
                _ => InteractionMode.Grip,
            };
        }

        public InteractionUpdate GetUpdate()
        {
            switch (update)
            {
                case EditorUpdate.Auto:
                    if (mode == InteractionMode.Grip) return InteractionUpdate.Mover;
                    else if ((mode == InteractionMode.Switch1) || (mode == InteractionMode.Switch2) || (mode == InteractionMode.Switch3)) return InteractionUpdate.Parent;
                    else return InteractionUpdate.Fixed;
                case EditorUpdate.Fixed: return InteractionUpdate.Fixed;
                case EditorUpdate.Local: return InteractionUpdate.Parent;
                default: return InteractionUpdate.Mover;
            }
        }

        private static void Populate(GameObject parent, List<MarkerArea> areas)
        {
            int cc = parent.transform.childCount;
            for (int i = 0; i < cc; i++)
            {
                GameObject go = parent.transform.GetChild(i).gameObject;
                MarkerArea area = go.GetComponent<MarkerArea>();
                if (area != null)
                {
                    //       Debug.Log("areas: " + area.mode + " owner: " + area.appliesTo.name);
                    areas.Add(area);
                    //         Debug.Log("all areas " + area.gameObject.name + " > " + (area.appliesTo == null ? "null" : area.appliesTo.name));
                }
                Populate(go, areas);
            }
        }
        public static void PopulateAll(GameObject[] root)
        {
            allAreas.Clear();
            for (int i = 0; i < root.Length; i++)
                Populate(root[i], allAreas);
        }
        public static List<GameObject> FindAreas(GameObject g)
        {
            List<GameObject> r = new();
            for (int i = 0; i < allAreas.Count; i++)
            {
                if (allAreas[i].appliesTo == g)
                    r.Add(allAreas[i].gameObject);
                else if ((allAreas[i].appliesTo == null) && (allAreas[i].gameObject.transform.parent.gameObject == g))
                    r.Add(g);
                else if ((allAreas[i].applyToSelf) && (allAreas[i].gameObject == g))
                    r.Add(g);
                if (allAreas[i].name == "rotar") Debug.Log("l area "+g.name + (g==allAreas[i].gameObject));
            }
            return r;
        }
        public static List<GameObject> FindAreasForCustom(GameObject g)
        {
            List<GameObject> r = new();
            for (int i = 0; i < allAreas.Count; i++)
            {
                if (allAreas[i].appliesTo == g)
                    r.Add(allAreas[i].gameObject);
                else if ((allAreas[i].appliesTo == null) && (allAreas[i].gameObject == g))
                    r.Add(g);
            }
            if (g.name == "_speed") Debug.Log("found " + r.Count);
            return r;
        }
    }
}