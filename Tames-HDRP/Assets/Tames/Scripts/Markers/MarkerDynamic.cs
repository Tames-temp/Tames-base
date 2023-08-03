using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Markers
{
    public enum DynamicTypes
    {
        SliderOrPath,
        Orbit,
        WideOrbit,
        FullOrbit,
        FreeRotator,
        AcuteFreeRotator,
        WalkSlide,
        WalkOrbit
    }
    public enum MoverType
    {
        Progress, Mover, Head,
    }
    public class MarkerDynamic : MonoBehaviour
    {
        public DynamicTypes type = DynamicTypes.SliderOrPath;
        public bool up = false;
        public MoverType mover = MoverType.Progress;
        private static string[] names = new string[] { "_start", "_end", "_mid", "_axis", "_pivot", "_fstart", "_fend", "_faxis", "_fpivot" };
        public void Remove()
        {
            GameObject g;
            for (int i = 0; i < names.Length; i++)
                if ((g = Exists(names[i])) != null)
                    DestroyImmediate(g);
            DestroyImmediate(this);
        }
        public void ChangeDynamic()
        {
            if (transform.parent == null) return;
            uint present;
            GameObject g;
            bool upable = false;
            switch (type)
            {
                case DynamicTypes.SliderOrPath: present = 0b000000011; break;
                case DynamicTypes.Orbit: present = 0b000011011; upable = true; break;
                case DynamicTypes.WideOrbit: present = 0b000011111; upable = true; break;
                case DynamicTypes.FullOrbit: present = 0b000011001; upable = true; break;
                case DynamicTypes.FreeRotator: present = 0b000010001; break;
                case DynamicTypes.AcuteFreeRotator: present = 0b000010011; break;
                case DynamicTypes.WalkSlide: present = 0b001100011; break;
                case DynamicTypes.WalkOrbit: present = 0b110000011; break;
                default: present = 0b000000000; break;
            }
            for (int i = 0; i < 9; i++)
                if ((present & (1 << i)) != 0)
                {
                    if (Exists(names[i]) == null)
                    {
                        g = new GameObject(names[i]);
                        g.transform.parent = transform.parent;
                        Position(g, i);
                    }
                }
                else if ((g = Exists(names[i])) != null)
                    DestroyImmediate(g);
            if (upable)
            {
                if (up && (Exists("_up") == null))
                {
                    g = new GameObject("_up");
                    g.transform.parent = transform.parent;
                    Position(g, -1);
                }
                else if (!up)
                    if ((g = Exists("_up")) != null)
                        DestroyImmediate(g);
            }
            else if ((g = Exists("_up")) != null)
                DestroyImmediate(g);
        }
        private GameObject Exists(string s)
        {
            Transform t = gameObject.transform.parent;
            int cc = t.childCount;
            for (int i = 0; i < cc; i++)
                if (t.GetChild(i).name.StartsWith(s))
                    return t.GetChild(i).gameObject;
            return null;
        }
        private void Position(GameObject g, int index)
        {
            switch (index)
            {
                case -1: g.transform.position = transform.position + transform.up; break;
                case 0:
                case 5: g.transform.position = transform.position + Vector3.right; break;
                case 1:
                case 6: g.transform.position = transform.position + Vector3.forward; break;
                case 2: g.transform.position = -transform.forward - transform.right; break;
                case 3:
                case 7: g.transform.position = transform.position + transform.up; break;
                case 4:
                case 8: g.transform.position = transform.position; break;
            }
        }
    }
}
