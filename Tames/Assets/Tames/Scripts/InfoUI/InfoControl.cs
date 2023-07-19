using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markers;
using Tames;
using UnityEngine;
namespace InfoUI
{
    public class InfoReference
    {
        public GameObject gameObject;
        public RefType refType;
        public TameScoreBasket basket;
        public TameScore score;
        public TameElement element;
        public TameAlternative alternative;
        public List<InfoReference> references;
        public enum RefType
        {
            Basket, Score, Element, Alternative, Object
        }
        public enum RefProperty
        {
            Name, Value, Time, Total, None
        }
        public static string[] Labels = new string[] { "name", "max" };
        public bool Identify()
        {
            List<TameGameObject> tgos = TameManager.tgos;
            TameGameObject tgo = null;
            for (int i = 0; i < tgos.Count; i++)
                if (tgos[i].gameObject == gameObject)
                {
                    tgo = tgos[i];
                    break;
                }
            //     Debug.Log("id?: " + gameObject.name + "" + tgos.Count);
            if (tgo == null) return false;
            //      Debug.Log("id: " + tgo.gameObject.name);
            if (tgo.isElement)
            {
                refType = RefType.Element;
                element = tgo.tameParent;
                //   Debug.Log("id: element " + element.name);
                return true;
            }

            MarkerAlterObject mao = gameObject.GetComponent<MarkerAlterObject>();
            if (mao != null)
            {
                foreach (TameAlternative ta in TameManager.altering)
                    if (ta.marker.gameObject == gameObject)
                    {
                        refType = RefType.Alternative;
                        alternative = ta;
                        return true;
                    }
            }

            foreach (TameScoreBasket tsb in TameManager.basket)
                if (tsb.marker.gameObject == gameObject)
                {
                    refType = RefType.Basket;
                    basket = tsb;
                    return true;
                }
                else
                    foreach (TameScore ts in tsb.scores)
                        if (ts.marker.gameObject == gameObject)
                        {
                            refType = RefType.Score;
                            score = ts;
                            return true;
                        }

            refType = RefType.Object;
            return true;
        }
        public string Get(RefProperty rp)
        {
            switch (rp)
            {
                case RefProperty.Time: return TameElement.ActiveTime.ToString("0");
                case RefProperty.Name: return GetName();
                case RefProperty.Value: return GetValue();
                case RefProperty.Total: return GetTotal();
                default: return "";
            }
        }
        private string GetName()
        {
            switch (refType)
            {
                case RefType.Alternative: return alternative.alternatives[alternative.current].gameObject[0].name;
                case RefType.Basket: return basket.marker.gameObject.name;
                case RefType.Score: return score.marker.name;
                case RefType.Element: return element.name;
                default: return gameObject.name;
            }
        }
        private string GetValue()
        {
            switch (refType)
            {
                case RefType.Alternative: return "" + alternative.current;
                case RefType.Basket: return "" + basket.totalScore;
                case RefType.Score: return "" + score.marker.score * score.count;
                case RefType.Element: return "" + MathF.Round(100 * element.progress.subProgress);
                default: return "";
            }
        }
        private string GetTotal()
        {
            switch (refType)
            {
                case RefType.Alternative: return "" + alternative.alternatives.Count;
                case RefType.Basket: return "" + basket.marker.passScore;
                case RefType.Score: return "" + score.marker.score * score.marker.count;
                case RefType.Element: return "" + 100;
                default: return "";
            }
        }
        public string MaxLength()
        {
            string s = GetTotal();
            string r = "";
            for (int i = 0; i < s.Length; i++)
                r += "8";
            return r;
        }

    }
    public class InfoControl
    {
        public enum FaceCamera { None, RestrictY, Free }
        public static bool InfoVisibility = true;
        public MarkerInfo marker;
        public InfoFrame[] frames;
        Material material;
        public InputSetting control;
        public List<Tames.TameArea> areas;
        public int current = 0;
        bool firstUpdate = true;
        public List<InfoReference> references = new List<InfoReference>();
   //     public FaceCamera faceCamera = FaceCamera.None;
        float lastUpdate = 0;
        public Material lineMaterial;
        public const float RefUpdateInterval = 0.3f;
        public InfoControl(MarkerInfo m)
        {
            m.SetIC(this);
              marker = m;
            if (marker.link != null)
            {
                lineMaterial = new Material(Shader.Find("Unlit/Color"));
                lineMaterial.SetColor("_Color", marker.textHighlight);
            }
            InfoReference ir;
            for (int i = 0; i < marker.references.Length; i++)
                if (marker.references[i] != null)
                {
                    //      Debug.Log("id + " + marker.references[i].name);
                    ir = new InfoReference() { gameObject = marker.references[i] };
                    if (ir.Identify()) references.Add(ir);
                    else references.Add(null);
                }
            control = m.control;
            control.AssignControl(InputSetting.ControlTypes.DualPress);
            areas = new(); 
            List<InfoFrame> infos = new List<InfoFrame>();
            for (int i = 0; i < marker.items.Length; i++)
                infos.Add(new InfoFrame() { marker = marker, material = material, index = infos.Count, parent = this, item = marker.items[i] });
            frames = infos.ToArray();
            for (int i = 0; i < frames.Length; i++)
                frames[i].Initialize();
            if (marker.items.Length > 0) current = 0;
            MarkerArea ma;
            Tames.TameArea ta;
            for (int i = 0; i < marker.areas.Length; i++)
                if (marker.areas[i] != null)
                {
                    ta = null;
                    if ((ma = marker.areas[i].GetComponent<MarkerArea>()) != null)
                    {
                        ma.update = EditorUpdate.Fixed;
                        ma.mode = InteractionMode.Inside;
                        ma.autoPosition = false;
                        switch (ma.geometry)
                        {
                            case InteractionGeometry.Box:
                            case InteractionGeometry.Cylinder:
                            case InteractionGeometry.Sphere:
                                ta = Tames.TameArea.ImportArea(ma.gameObject, new Tames.TameElement() { owner = marker.gameObject, tameType = TameKeys.Custom });
                                break;
                        }
                        if (ta != null)
                            areas.Add(ta);
                    }
                }

        }
        public bool Inside(Vector3 p)
        {
            if (areas.Count == 0) return true;
            Tames.TameAreaTrack tat = Tames.TameArea.TrackWithAreas(areas, p);
            return tat.direction == 1;
        }
        public bool InView()
        {
            if (!Inside(TameCamera.camera.transform.position)) return false;
            return TameCamera.CheckDistanceAndAngle(marker.gameObject, control.maxDistance, control.maxAngle, control.axis);
        }
        private bool visible = false, visibilityChanged = false;
        public bool Visible
        {
            get { return visible; }
            set { if (value != visible) visibilityChanged = true; visible = value; }
        }
        private bool indexChanged = false;

        private void Change(int dir)
        {
            indexChanged = false;
            Debug.Log(marker.name + " " + dir);
            if (frames.Length > 0)
            {
                if (dir < 0)
                {
                    if (!frames[current].GoPrev())
                    {
                        current = current < 0 ? frames.Length - 1 : (current + frames.Length - 1) % frames.Length;
                        frames[current].Enter(dir);
                        indexChanged |= true;
                    }
                }
                else if (!frames[current].GoNext())
                {
                    current = current < 0 ? 0 : (current + 1) % frames.Length;
                    frames[current].Enter(dir);
                    indexChanged = true;
                }
            }
        }
        public void Update()
        {
            if (visible)
            {
                int d = control.CheckDualPressed(marker.gameObject);
                if (d != 0) Change(d);
                int replace = 0;
                if (frames[current].type == InfoFrame.ItemType.Object)
                    frames[current].SetInstancePosition();
                if (marker.position == InfoPosition.OnObject)
                {
                    Vector3 p = Camera.main.WorldToScreenPoint(marker.transform.position);
                    for (int i = 0; i < frames.Length; i++)
                        frames[i].MoveTo(p.x, p.y, frames[0].outer.xMin, frames[0].outer.yMin);
                }
                if (TameElement.ActiveTime - lastUpdate > RefUpdateInterval)
                {
                    frames[current].UpdateReferences();
                    lastUpdate = TameElement.ActiveTime;
                }
                if (marker.link != null) frames[current].UpdateLine();
                if (current < frames.Length)
                    for (int i = frames.Length - 1; i >= 0; i--)
                    {
                        if (i > current)
                            frames[i].owner.SetActive(false);
                        else switch (replace)
                            {
                                case 0:
                                    frames[i].Show(true, i < current);
                                    replace = frames[i].GetReplace();
                                    break;
                                case 1:
                                    frames[i].Show(false);
                                    replace = frames[i].GetReplace();
                                    break;
                                case 2:
                                    frames[i].Show(false);
                                    break;
                            }
                        //       if (1 == current && 1 == i) Debug.Log(marker.name + " current = " + i + " " + replace + " " + frames[i].owner.activeSelf);

                    }
            }
            else
                for (int i = 0; i < frames.Length; i++) frames[i].owner.SetActive(false);
        }
        public void UpdateMarker()
        {
            foreach (InfoFrame frame in frames)
            {
                frame.UpdateColors();
            }
        }
    }
}
