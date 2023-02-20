using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Markers
{
    public enum EditorGeometry
    {
        Box, Sphere, Cylinder, Plane, Remote
    }
    public enum EditorUpdate
    {
        Auto, Fixed, Local, Mover
    }
    public enum EditorMode
    {
        InsideOnly, OutsideOnly, InsidePositive, OutsidePositive, Grip, Switch_1, Switch_2, Switch_3
    }
    public class MarkerArea : MonoBehaviour
    {
        //  public bool thisIsArea;
        public GameObject appliesTo;
        public EditorGeometry geometry;
        public string input;
        public EditorUpdate update;
        public EditorMode mode;
        public static List<MarkerArea> allAreas = new List<MarkerArea>();
        //    public GameObject area = null;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        public InteractionGeometry GetGeometry()
        {
            switch (geometry)
            {
                case EditorGeometry.Box: return InteractionGeometry.Box;
                case EditorGeometry.Sphere: return InteractionGeometry.Sphere;
                case EditorGeometry.Cylinder: return InteractionGeometry.Cylinder;
                case EditorGeometry.Remote: return InteractionGeometry.Remote;
                default: return InteractionGeometry.Plane;
            }
        }
        public InteractionMode GetMode()
        {
            switch (mode)
            {
                case EditorMode.Grip: return InteractionMode.Grip;
                case EditorMode.Switch_1: return InteractionMode.Switch1;
                case EditorMode.Switch_2: return InteractionMode.Switch2;
                case EditorMode.Switch_3: return InteractionMode.Switch3;
                case EditorMode.InsideOnly: return InteractionMode.Inside;
                case EditorMode.OutsideOnly: return InteractionMode.Outside;
                case EditorMode.InsidePositive: return InteractionMode.InOut;
                case EditorMode.OutsidePositive: return InteractionMode.OutIn;
                default: return InteractionMode.Grip;
            }
        }
        public InteractionUpdate GetUpdate()
        {
            switch (update)
            {
                case EditorUpdate.Auto:
                    if (mode == EditorMode.Grip) return InteractionUpdate.Mover;
                    else if ((mode == EditorMode.Switch_1) || (mode == EditorMode.Switch_2) || (mode == EditorMode.Switch_3)) return InteractionUpdate.Parent;
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
                    Debug.Log("all areas " + area.gameObject.name + " > " + (area.appliesTo == null ? "null" : area.appliesTo.name));
                }
                Populate(go, areas);
            }
        }
        public static void PopulateAll(GameObject root)
        {
            allAreas.Clear();
            Populate(root, allAreas);
        }
        public static void PopulateAll(List<Tames.TameGameObject> tgos)
        {
            allAreas.Clear();
            MarkerArea area;
            foreach (Tames.TameGameObject tgo in tgos)
                if ((area = tgo.gameObject.GetComponent<MarkerArea>()) != null)
                {
                    allAreas.Add(area);
                    Debug.Log("all areas " +area.gameObject.name+" > "+ (area.appliesTo==null?"null":area.appliesTo.name));
                }
        }
        public static List<GameObject> FindAreas(GameObject g)
        {
            List<GameObject> r = new List<GameObject>();
            bool b = g.name == "door3";
            for (int i = 0; i < allAreas.Count; i++)
            {
                if (allAreas[i].appliesTo !=null)
                    Debug.Log("Applies to "+allAreas[i].appliesTo.name);
                else
                    Debug.Log("Applies not " +allAreas[i].gameObject.name);

                if (allAreas[i].appliesTo == g)
                    r.Add(allAreas[i].gameObject);
                else if (allAreas[i].gameObject.transform.parent.gameObject == g)
                    r.Add(g);
            }
            return r;

        }
    }
}