using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Markers;
using TMPro;
using UnityEngine.UI;

namespace InfoUI
{
    public class InfoFrame
    {
        private class WordRef
        {
            public string text = "";
            //      public InfoReference.RefType type = InfoReference.RefType.Element;
            public InfoReference.RefProperty prop = InfoReference.RefProperty.None;
            public int style = 0;
            public int depth = -1;
            public int bullet = 0;
            public bool spaceBefore = true;
            public bool IsHighlight { get { return (style & Highlight) > 0; } set { style &= (value ? BIH : BIH - Highlight); } }
            public bool IsItalic { get { return (style & Italic) > 0; } set { style &= (value ? BIH : BIH - Italic); } }
            public bool IsBold { get { return (style & Bold) > 0; } set { style &= (value ? BIH : BIH - Bold); } }
        }
        public InfoControl parent;
        public Rect outer, inner, main;
        Rect[] context, lines;
        public MarkerInfo marker;
        public Material material;
        public int index;
        public float lineHeight = -1;
        Canvas canvas = null;
        public GameObject owner;
        RectTransform rectTransform;
        float margin, textHeight, factor = 1;
        Vector3 objectBound;
        LineRenderer lineRenderer = null;
        public enum ItemType { TextOnly, Image, Object }
        public ItemType type = ItemType.TextOnly;
        Vector2[] panelSize = new Vector2[10];
        Vector2[] panelPos = new Vector2[10];
        GameObject instance;
        List<RectTransform> children = new();
        //List<Vector2> Ys = new List<Vector2>();
        List<Vector3> childCoord = new List<Vector3>();
        List<int> childTypes = new List<int>();
        List<WordRef> words = new List<WordRef>();
        TextMeshProUGUI[] text = null;
        const int Bold = 1;
        const int Italic = 2;
        const int Highlight = 4;
        const int NewLine = 16;
        const int Bullet = 32;
        const int Dash = 64;
        const int Number = 128;
        const int Alpha = 256;
        const int Align = 512;
        const int BIH = Bold + Italic + Highlight;
        const int ListStyles = Bullet + Dash + Number;
        static string StyleSymbols = "|\\!";
        static int[] Styles = new int[] { Bold, Italic, Highlight };
        const int TypePanel = -1;
        const int TypeImage = -2;
        int validWordCount = 0;
        List<IndexCounter> sections = new List<IndexCounter>();
        public InfoItem item;
        public int linesCount = 0;
        public void Reset()
        {
            words.Clear();
            childCoord.Clear();
            childTypes.Clear();
            if (text != null)
                for (int i = 0; i < text.Length; i++)
                    if (text[i] != null)
                        GameObject.Destroy(text[i].gameObject);

            foreach (RectTransform rect in children)
                if (rect != null)
                    GameObject.Destroy(rect.gameObject);
            children.Clear();
            if (owner != null)
                GameObject.Destroy(owner);
            if (instance != null) GameObject.Destroy(instance);
            lineHeight = -1;
        }
        public void Initialize(float height)
        {
            if (height < 0)
            {
                if (marker.position == InfoPosition.WithObject)
                {
                    main = outer = new Rect(0, 0, marker.width, marker.height);
                    inner = new Rect(marker.margin, marker.margin, marker.width - marker.margin * 2, marker.height - marker.margin * 2);
                    margin = marker.margin;
                }
                else
                {
                    main = outer = new Rect(0, 0, Screen.width, Screen.height);
                    margin = Screen.height * marker.margin;
                    MoveRects();
                    inner = new Rect(margin + outer.xMin, margin + outer.yMin, outer.width - margin * 2, outer.height - margin * 2);
                }
                context = ItemRect();
                lines = TextRectangles();
                //    lineHeight = lines[0].height;
                Debug.Log("base: " + marker.name + " " + outer.height);
            }
            else
            {
                float h;
                if (marker.position == InfoPosition.WithObject)
                {
                    margin = marker.margin;
                    h = item.lineCount * height + margin * 2;
                    main = outer = new Rect(0, 0, marker.width, h);
                    inner = new Rect(marker.margin, marker.margin, marker.width - marker.margin * 2, h - marker.margin * 2);
                }
                else
                {
                    margin = Screen.height * marker.margin;
                    h = item.lineCount * height + margin * 2;
                    main = outer = new Rect(0, 0, Screen.width, Screen.height);
                    MoveRects(h);
                    inner = new Rect(margin + outer.xMin, margin + outer.yMin, outer.width - margin * 2, outer.height - margin * 2);
                }
                lineHeight = height;
                context = ItemRect();
                lines = TextRectangles();
                Debug.Log("next: " + marker.name + " " + outer.height);

            }
            CreateCanvas();
            Panelize();
            if (marker.position == InfoPosition.WithObject)
                AdjustWithObject();
            else
                AdjustOnScreen();
            ReadText(marker.items[index].Text);
            CreateText();
            if (marker.link != null)
            {
                lineRenderer = canvas.gameObject.AddComponent<LineRenderer>();
                lineRenderer.material = parent.lineMaterial;
            }
        }


        void AddChild(RectTransform child, int type, Vector3 coord)
        {
            children.Add(child);
            childCoord.Add(coord);
            childTypes.Add(type);
        }
        static bool MultiStyle(string s, out int highlight, out int bold, out int italic)
        {
            highlight = bold = italic = 0;
            for (int i = 0; i < s.Length; i++)
                if (StyleSymbols.IndexOf(s[i]) < 0)
                    return false;
            int[] ss = new int[] { 0, 0, 0 };
            for (int i = 0; i < s.Length; i++)
            {
                int p = StyleSymbols.IndexOf(s[i]);
                ss[p]++;
            }
            bold = ss[0] == 0 ? 0 : (ss[0] == 1 ? 1 : -1);
            italic = ss[1] == 0 ? 0 : (ss[1] == 1 ? 1 : -1);
            highlight = ss[2] == 0 ? 0 : (ss[2] == 1 ? 1 : -1);
            return true;
        }
        static string[] SplitBySpace(string s)
        {
            bool inSpace = true;
            string w = "";
            for (int i = 0; i < s.Length; i++)
                if (s[i] == ' ')
                {
                    if (inSpace)
                        w += "\t";
                    else
                    {
                        w += ' ';
                        inSpace = true;
                    }
                }
                else
                {
                    w += s[i];
                    inSpace = false;
                }
            string[] r = w.Split(' ');
            for (int i = 0; i < r.Length; i++)
                r[i] = r[i].Replace('\t', ' ');
            return r;
        }
        void ReadText(string text)
        {
            string[] la = text.Split('\n', StringSplitOptions.None);
            //   List<string> words = new List<string>();
            int lastStyle = 0, currentStyle = 0;
            int hi, bi, ii;
            if (text[0] == '\n')
            {
                words.Add(new WordRef() { style = NewLine, });
                //           linesCount++;
            }
            //     linesCount += la.Length;
            int sec = -1;
            bool first = true;
            for (int l = 0; l < la.Length; l++)
            {
                first = true;
                string[] ta = SplitBySpace(la[l]);
                for (int i = 0; i < ta.Length; i++)
                {
                    if (ta[i] == "##")
                    {
                        if (sec < 0)
                        {
                            if (words.Count > 0)
                            {
                                sections.Add(IndexCounter.ByCount(0, words.Count));
                                sec++;
                                Debug.Log(marker.name + ": section " + sec + ": " + sections[sec].start);
                            }
                            sections.Add(IndexCounter.ByCount(words.Count, words.Count));
                            sec++;
                            Debug.Log(marker.name + ": section " + sec + ": " + sections[sec].start);

                        }
                        else
                        {
                            sections[sec].end = words.Count;
                            sections.Add(IndexCounter.ByCount(words.Count, words.Count));
                            sec++;
                            Debug.Log(marker.name + ": section " + sec + ": " + sections[sec].start);
                        }
                    }
                    else if (MultiStyle(ta[i], out hi, out bi, out ii))
                    {
                        if (hi == 1) lastStyle |= Highlight; else if (hi == -1) lastStyle &= Bold + Italic;
                        if (bi == 1) lastStyle |= Bold; else if (bi == -1) lastStyle &= Highlight + Italic;
                        if (ii == 1) lastStyle |= Italic; else if (ii == -1) lastStyle &= Bold + Highlight;
                    }
                    else
                    {
                        currentStyle = lastStyle;
                        string net = GetStylePart(ta[i], out hi, out bi, out ii);
                        if (hi == 1) currentStyle |= Highlight; else if (hi == -1) currentStyle &= Bold + Italic;
                        if (bi == 1) currentStyle |= Bold; else if (bi == -1) currentStyle &= Highlight + Italic;
                        if (ii == 1) currentStyle |= Italic; else if (ii == -1) currentStyle &= Bold + Highlight;
                        //    Debug.Log("Style: " + net + " " + currentStyle + " "+hi+" "+bi+" "+ii+" "+lastStyle);
                        if (i == 0)
                            if (Bulleted(ta[i], out int indent, out int type))
                            {
                                words.Add(new WordRef() { bullet = type, depth = indent - 1, style = currentStyle });
                                continue;
                            }

                        List<string> brack = ExtractBrackets(net);
                        for (int j = 0; j < brack.Count; j++)
                            if (brack[j].StartsWith("{"))
                            {
                                net = GetReference(brack[j], out int id, out InfoReference.RefProperty prop);
                                if (prop != InfoReference.RefProperty.None)
                                    words.Add(new WordRef() { style = currentStyle, text = "", depth = id, prop = prop, spaceBefore = j == 0 });
                                else
                                    words.Add(new WordRef() { style = currentStyle, text = brack[j], spaceBefore = j == 0 });
                            }
                            else
                                words.Add(new WordRef() { style = currentStyle, text = brack[j] });
                    }

                }
                words.Add(new WordRef() { style = NewLine });
            }
            if (sec >= 0)
            {
                if (words.Count > sections[sec].end) sections[sec].end = words.Count;
                Debug.Log(marker.name + " section " + sec + ": " + sections[sec].end + "< " + words.Count);
                for (int i = 0; i < sections.Count; i++)
                    for (int j = sections[i].start; j < sections[i].end; j++)
                        Debug.Log(marker.name + " [" + i + "]: " + words[j].text);
            }
        }
        static bool Bulleted(string s, out int indent, out int type)
        {
            indent = 0;
            type = 0;
            for (int i = 0; i < s.Length; i++)
                if (s[i] == '>')
                    indent++;
                else
                {
                    type = (s[i]) switch
                    {
                        '*' => Bullet,
                        '-' => Dash,
                        '#' => Number,
                        '@' => Alpha,
                        _ => 0
                    };
                    break;
                }
            return indent > 0 && type > 0;
        }
        static List<string> ExtractBrackets(string s)
        {
            int a = s.IndexOf('{');
            int b = s.IndexOf('}');
            if (b > a && a >= 0)
            {
                List<string> r = new List<string>();
                if (a > 0) r.Add(s.Substring(0, a));
                r.Add(s.Substring(a, b - a + 1));
                if (b < s.Length - 1) r.Add(s.Substring(b + 1));
                return r;
            }
            else
                return new List<string>() { s };
        }
        string GetReference(string text, out int index, out InfoReference.RefProperty prop)
        {
            prop = InfoReference.RefProperty.None;
            index = -1;
            if (text.Length <= 2)
                return text;
            else if ((text[0] == '{') && (text[^1] == '}'))
            {
                string s = text.Substring(1, text.Length - 2);
                //    Debug.Log(s);
                if ("time".IndexOf(s) == 0)
                {
                    prop = InfoReference.RefProperty.Time;
                    return "";
                }
                else
                {
                    try
                    {
                        int p = int.Parse(s);
                        if ((p >= 0) || (p < parent.references.Count))
                        {
                            if (parent.references[p] == null) return text;
                            index = p;
                            //        Debug.Log("id: " + index + " > " + parent.references[p].element.name);
                            prop = InfoReference.RefProperty.Value;
                            return "";
                        }
                        else
                            return text;
                    }
                    catch
                    {
                        int p, k = s.IndexOf(':');
                        if ((k > 0) && (k < s.Length - 1))
                        {
                            try
                            {
                                p = int.Parse(s.Substring(0, k));
                                Debug.Log("name = " + p);
                                if ((p >= 0) || (p < parent.references.Count))
                                {
                                    Debug.Log("name = " + s.Substring(k + 1));
                                    if (InfoReference.Labels[0].IndexOf(s.Substring(k + 1).ToLower()) == 0)
                                    {
                                        Debug.Log("name = ");
                                        if (parent.references[p] == null) return text;
                                        prop = InfoReference.RefProperty.Name;
                                        index = p;
                                        return "";
                                    }
                                    if (InfoReference.Labels[1].IndexOf(s.Substring(k + 1).ToLower()) == 0)
                                    {
                                        Debug.Log("name = ");
                                        if (parent.references[p] == null) return text;
                                        prop = InfoReference.RefProperty.Total;
                                        index = p;
                                        return "";
                                    }
                                    return text;
                                }
                                else
                                    return text;
                            }
                            catch { return text; }
                        }
                        else
                            return text;
                    }
                }
            }
            else
            {
                prop = InfoReference.RefProperty.None;
                index = -1;
                return text;
            }

        }

        static string GetStylePart(string s, out int highlight, out int bold, out int italic)
        {
            string sp = "";
            string r = "";
            for (int i = 0; i < s.Length; i++)
                if (StyleSymbols.IndexOf(s[i]) >= 0)
                    sp += s[i];
                else { r = s.Substring(i); break; }
            MultiStyle(sp, out highlight, out bold, out italic);
            return r;
        }
        static string[] Split(string text)
        {
            List<string> words = new List<string>();
            bool dash = false;
            string w = "";
            for (int i = 0; i < text.Length; i++)
                if (text[i] == ' ')
                {
                    if (w != "")
                    {
                        words.Add(w);
                        w = "";
                    }
                }
                else if (text[i] == '-')
                {
                    dash = true;
                    w += text[i];
                }
                else if (text[i] == '\n')
                {
                    words.Add("\n");
                    w = "";
                    dash = false;
                }
                else if (dash)
                {
                    words.Add(w);
                    w = text[i] + "";
                    dash = false;
                }
                else w += text[i];
            if (w != "") words.Add(w);
            return words.ToArray();
        }
        void MoveRects()
        {
            Vector2 s = new Vector2(Screen.width, Screen.height);
            Vector2 m = new Vector2(marker.width, marker.height);
            m = Vector2.Scale(m, s);
            Vector2 d = s - m;
            //    Debug.Log(marker.position);
            switch (marker.position)
            {
                case InfoPosition.Top: outer = new Rect(d.x / 2, 0, m.x, m.y); break;
                case InfoPosition.TopLeft: outer = new Rect(0, 0, m.x, m.y); break;
                case InfoPosition.TopRight: outer = new Rect(d.x, 0, m.x, m.y); break;
                case InfoPosition.Bottom: outer = new Rect(d.x / 2, d.y, m.x, m.y); break;
                case InfoPosition.BottomLeft: outer = new Rect(0, d.y, m.x, m.y); break;
                case InfoPosition.BottomRight: outer = new Rect(d.x, d.y, m.x, m.y); break;
                case InfoPosition.Left: outer = new Rect(0, d.y / 2, m.x, m.y); break;
                case InfoPosition.Right: outer = new Rect(d.x, d.y / 2, m.x, m.y); break;
                case InfoPosition.OnObject: outer = new Rect(0, 0, m.x, m.y); break;
            }
            bool repPrev = false;
            if (index != 0)
            {
                InfoFrame prev = parent.frames[index - 1];
                switch (item.replace)
                {
                    case InfoOrder.ReplacePrevious:
                        repPrev = index > 0;
                        break;
                    case InfoOrder.ReplaceAll:
                        break;
                    case InfoOrder.AddVertical:
                        switch (marker.position)
                        {
                            case InfoPosition.Top:
                            case InfoPosition.TopLeft:
                            case InfoPosition.TopRight:
                            case InfoPosition.OnObject: outer.y = prev.outer.yMax + margin; break;
                            case InfoPosition.Bottom:
                            case InfoPosition.BottomLeft:
                            case InfoPosition.BottomRight: outer.y = prev.outer.yMin - margin - outer.height; break;
                            default: repPrev = index > 0; break;
                        }
                        break;
                    case InfoOrder.AddHorizontal:
                        switch (marker.position)
                        {
                            case InfoPosition.Left:
                            case InfoPosition.TopLeft:
                            case InfoPosition.BottomLeft:
                            case InfoPosition.OnObject: outer.x = prev.outer.xMax + margin; break;
                            case InfoPosition.Right:
                            case InfoPosition.TopRight:
                            case InfoPosition.BottomRight: outer.x = prev.outer.xMin - margin - outer.width; break;
                            default: repPrev = index > 0; break;
                        }
                        break;
                }

                if (repPrev)
                {
                    outer.y = prev.outer.yMin;
                    outer.x = prev.outer.xMin;
                }
            }
        }
        void MoveRects(float h)
        {
            Vector2 s = new Vector2(Screen.width, Screen.height);
            Vector2 m = new Vector2(marker.width * Screen.width, h);
            //    m = Vector2.Scale(m, s);
            Vector2 d = s - m;
            //    Debug.Log(marker.position);
            switch (marker.position)
            {
                case InfoPosition.Top: outer = new Rect(d.x / 2, 0, m.x, m.y); break;
                case InfoPosition.TopLeft: outer = new Rect(0, 0, m.x, m.y); break;
                case InfoPosition.TopRight: outer = new Rect(d.x, 0, m.x, m.y); break;
                case InfoPosition.Bottom: outer = new Rect(d.x / 2, d.y, m.x, m.y); break;
                case InfoPosition.BottomLeft: outer = new Rect(0, d.y, m.x, m.y); break;
                case InfoPosition.BottomRight: outer = new Rect(d.x, d.y, m.x, m.y); break;
                case InfoPosition.Left: outer = new Rect(0, d.y / 2, m.x, m.y); break;
                case InfoPosition.Right: outer = new Rect(d.x, d.y / 2, m.x, m.y); break;
                case InfoPosition.OnObject: outer = new Rect(0, 0, m.x, m.y); break;
            }
            bool repPrev = false;
            if (index != 0)
            {
                InfoFrame prev = parent.frames[index - 1];
                switch (item.replace)
                {
                    case InfoOrder.ReplacePrevious:
                        repPrev = index > 0;
                        break;
                    case InfoOrder.ReplaceAll:
                        break;
                    case InfoOrder.AddVertical:
                        switch (marker.position)
                        {
                            case InfoPosition.Top:
                            case InfoPosition.TopLeft:
                            case InfoPosition.TopRight:
                            case InfoPosition.OnObject: outer.y = prev.outer.yMax + margin; break;
                            case InfoPosition.Bottom:
                            case InfoPosition.BottomLeft:
                            case InfoPosition.BottomRight: outer.y = prev.outer.yMin - margin - outer.height; break;
                            default: repPrev = index > 0; break;
                        }
                        break;
                    case InfoOrder.AddHorizontal:
                        switch (marker.position)
                        {
                            case InfoPosition.Left:
                            case InfoPosition.TopLeft:
                            case InfoPosition.BottomLeft:
                            case InfoPosition.OnObject: outer.x = prev.outer.xMax + margin; break;
                            case InfoPosition.Right:
                            case InfoPosition.TopRight:
                            case InfoPosition.BottomRight: outer.x = prev.outer.xMin - margin - outer.width; break;
                            default: repPrev = index > 0; break;
                        }
                        break;
                }

                if (repPrev)
                {
                    outer.y = prev.outer.yMin;
                    outer.x = prev.outer.xMin;
                }
            }
        }

        public int GetReplace()
        {
            return item.replace switch
            {
                InfoOrder.ReplacePrevious => 1,
                InfoOrder.ReplaceAll => 2,
                _ => 0,
            };
        }
        public void MoveTo(float x, float y, float Ox, float Oy)
        {
            for (int i = 0; i < children.Count; i++)
                children[i].anchoredPosition = new Vector2(x + childCoord[i].x - Ox, outer.height - childCoord[i].z - (childCoord[i].y - Oy) + y);
        }
        public void MoveTo(float x, float y)
        {
            for (int i = 0; i < children.Count; i++)
                children[i].anchoredPosition = new Vector2(x + childCoord[i].x - outer.xMin, main.height - childCoord[i].z - childCoord[i].y + y);
        }
        Vector3 GetRatio(GameObject g)
        {
            if (marker.items[index].element != null) type = ItemType.Object;
            else type = ItemType.Image;
            try
            {
                Vector3[] allBound = new Vector3[2];
                MeshFilter[] mfs = g.GetComponentsInChildren<MeshFilter>();
                foreach (MeshFilter mf in mfs)
                {
                    Mesh mesh = mf.sharedMesh;
                    if (mesh != null)
                    {
                        Vector3[] v = mesh.vertices;
                        for (int i = 0; i < v.Length; i++)
                            v[i] = mf.transform.TransformPoint(v[i]);
                        Vector3[] bound = Utils.MinMax(v);
                        allBound[0] = Vector3.Min(bound[0], allBound[0]);
                        allBound[1] = Vector3.Max(bound[1], allBound[1]);
                    }
                }
                Vector3 extent = allBound[1] - allBound[0];
                return extent;
            }
            catch { return Vector3.one; }
        }
        Rect[] ItemRect()
        {
            float tr = marker.items[index].textPortion;
            float ir = 1 - tr;
            float frameRatio = inner.width / inner.height;
            float expected = frameRatio * ir;
            float itemRatio = 1;
            if ((marker.items[index].image == null) && (marker.items[index].element == null))
                return new Rect[] { inner };
            else
            {
                if (marker.items[index].element != null)
                {
                    instance = GameObject.Instantiate(marker.items[index].element);
                    instance.transform.parent = null;
                    instance.transform.localScale = Vector3.one;
                    objectBound = GetRatio(instance);
                    if (objectBound.y != 0)
                        itemRatio = MathF.Max(objectBound.x, objectBound.z) / objectBound.y;
                    if (itemRatio > 5) itemRatio = 5;
                }
                else
                {
                    type = ItemType.Image;
                    itemRatio = marker.items[index].image.width / (float)marker.items[index].image.height;
                }
            }
            //  Debug.Log(itemRatio + " " + type);
            int added = 1;
            float w, h;
            float x = inner.xMin, y = inner.yMin;
            if ((marker.vertical == Vertical.Stretch) && (marker.horizontal == Horizontal.Stretch))
                return new Rect[] { inner, inner };
            else if (marker.vertical == Vertical.Stretch)
            {
                if (expected < itemRatio)
                {
                    w = inner.width * ir;
                    h = w / itemRatio;
                    y += (inner.height - h) / 2;
                    if (marker.horizontal == Horizontal.Right)
                        x += inner.width * tr;
                }
                else
                {
                    h = inner.height;
                    w = h * itemRatio;
                    if (marker.horizontal == Horizontal.Right)
                        x += inner.width - w;
                }
            }
            else if (marker.horizontal == Horizontal.Stretch)
            {
                if (expected < itemRatio)
                {
                    w = inner.width;
                    h = w / itemRatio;
                    if (marker.vertical == Vertical.Bottom)
                        y += inner.height - h;
                }
                else
                {
                    h = inner.height / 2;
                    w = h * itemRatio;
                    x += (inner.width - w) / 2;
                    if (marker.vertical == Vertical.Bottom)
                        y += inner.height - h;
                }
            }
            else
            {
                added = 2;
                if (frameRatio < itemRatio)
                {
                    w = inner.width * ir;
                    h = w / itemRatio;
                }
                else
                {
                    h = inner.height * ir;
                    w = h * itemRatio;
                }
                if ((marker.vertical == Vertical.Top) && (marker.horizontal == Horizontal.Right)) x += inner.width - w;
                else if ((marker.vertical == Vertical.Bottom) && (marker.horizontal == Horizontal.Left)) y += inner.width - h;
                else if ((marker.vertical == Vertical.Bottom) && (marker.horizontal == Horizontal.Right))
                {
                    x += inner.width - w; y += inner.width - h;
                }
            }



            Rect[] r = new Rect[added + 1];
            r[0] = new Rect(x, y, w, h);
            if (added == 1)
            {
                x = marker.horizontal == Horizontal.Left ? r[0].xMax + margin : inner.xMin;
                y = marker.vertical == Vertical.Top ? r[0].yMax : inner.yMin;
                w = marker.horizontal == Horizontal.Stretch ? inner.width - margin : inner.width - w - margin;
                h = marker.vertical == Vertical.Stretch ? inner.height : inner.height - h;
                r[1] = new Rect(x, y, w, h);
            }
            else
            {
                if (marker.horizontal == Horizontal.Left)
                {
                    if (marker.vertical == Vertical.Top)
                    {
                        r[1] = new Rect(r[0].xMax + margin, inner.yMin, inner.width - w - margin, h);
                        r[2] = new Rect(inner.xMin, r[0].yMax, inner.width, inner.height - h);
                    }
                    else
                    {
                        r[1] = new Rect(inner.xMin, inner.yMin, inner.width, inner.height - h);
                        r[2] = new Rect(r[0].xMax + margin, r[0].yMin, inner.width - w - margin, h);
                    }
                }
                else
                {
                    if (marker.vertical == Vertical.Top)
                    {
                        r[1] = new Rect(inner.xMin, inner.yMin, inner.width - w - margin, h);
                        r[2] = new Rect(inner.xMin, r[0].yMax, inner.width, inner.height - h);
                    }
                    else
                    {
                        r[1] = new Rect(inner.xMin, inner.yMin, inner.width, inner.height - h);
                        r[2] = new Rect(inner.xMin, r[0].yMin, inner.width - w - margin, h);
                    }
                }
            }
            return r;
        }
        void PanelizeBorderWithObject()
        {
            GameObject go;
            RectTransform rect;
            Image image;
            Rect C = context[0];
            Vector2 p;
            float w = outer.width, h = outer.height;
            // top left
            panelSize[0] = new(margin, margin);
            panelSize[1] = new(w - 2 * margin, margin);
            panelSize[2] = new(margin, margin);
            panelSize[3] = new(margin, h - 2 * margin);
            panelSize[4] = new(margin, margin);
            panelSize[5] = new(w - 2 * margin, margin);
            panelSize[6] = new(margin, margin);
            panelSize[7] = new(margin, h - 2 * margin);

            panelPos[0] = new(0, 0);
            panelPos[1] = new(margin, 0);
            panelPos[2] = new(w - margin, 0);
            panelPos[3] = new(0, margin);
            panelPos[4] = new(0, h - margin);
            panelPos[5] = new(margin, h - margin);
            panelPos[6] = new(w - margin, h - margin);
            panelPos[7] = new(w - margin, margin);

            int m = 0;
            Rect[] iRect = new Rect[8];
            if (marker.background != null)
            {
                w = marker.background.width;
                h = marker.background.height;
                m = (int)(margin / main.height * marker.background.height);
                iRect[4] = new(0, 0, m, m);
                iRect[5] = new(m, 0, w - m * 2, m);
                iRect[6] = new(w - m, 0, m, m);
                iRect[3] = new(0, m, m, h - 2 * m);
                iRect[0] = new(0, h - m, m, m);
                iRect[1] = new(m, h - m, w - 2 * m, m);
                iRect[2] = new(w - m, h - m, m, m);
                iRect[7] = new(w - m, m, m, h - 2 * m);
                //      Debug.Log("margin is " + m + " " + margin);
            }
            Vector3 cc;
            for (int i = 0; i < 8; i++)
            {
                panelPos[i] += outer.min;
                cc = new(panelPos[i].x, panelPos[i].y, panelSize[i].y);
                panelPos[i].y = main.height - panelPos[i].y - panelSize[i].y;
                go = new GameObject("border " + i);
                image = go.AddComponent<Image>();
                rect = go.GetComponent<RectTransform>();
                rect.parent = rectTransform;
                rect.sizeDelta = panelSize[i];
                rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
                rect.anchoredPosition = panelPos[i];
                if (marker.background == null)
                    image.color = marker.color;
                else
                    image.sprite = Sprite.Create((Texture2D)marker.background, iRect[i], Vector2.zero);
                AddChild(rect, TypePanel, cc);
            }
        }
        void PanelizeBorder()
        {
            if (marker.position == InfoPosition.WithObject)
            {
                PanelizeBorderWithObject();
                return;
            }
            GameObject go;
            RectTransform rect;
            Image image;
            Rect C = context[0];
            Vector2 p;
            int w = (int)outer.width, h = (int)outer.height;
            // top left
            panelSize[0] = new(margin, margin);
            panelSize[1] = new(w - 2 * margin, margin);
            panelSize[2] = new(margin, margin);
            panelSize[3] = new(margin, h - 2 * margin);
            panelSize[4] = new(margin, margin);
            panelSize[5] = new(w - 2 * margin, margin);
            panelSize[6] = new(margin, margin);
            panelSize[7] = new(margin, h - 2 * margin);

            panelPos[0] = new(0, 0);
            panelPos[1] = new(margin, 0);
            panelPos[2] = new(w - margin, 0);
            panelPos[3] = new(0, margin);
            panelPos[4] = new(0, h - margin);
            panelPos[5] = new(margin, h - margin);
            panelPos[6] = new(w - margin, h - margin);
            panelPos[7] = new(w - margin, margin);

            int m = 0;
            Rect[] iRect = new Rect[8];
            if (marker.background != null)
            {
                w = marker.background.width;
                h = marker.background.height;
                m = (int)(margin / main.height * marker.background.height);
                iRect[4] = new(0, 0, m, m);
                iRect[5] = new(m, 0, w - m * 2, m);
                iRect[6] = new(w - m, 0, m, m);
                iRect[3] = new(0, m, m, h - 2 * m);
                iRect[0] = new(0, h - m, m, m);
                iRect[1] = new(m, h - m, w - 2 * m, m);
                iRect[2] = new(w - m, h - m, m, m);
                iRect[7] = new(w - m, m, m, h - 2 * m);
                //      Debug.Log("margin is " + m + " " + margin);
            }
            Vector3 cc;
            for (int i = 0; i < 8; i++)
            {
                panelPos[i] += outer.min;
                cc = new(panelPos[i].x, panelPos[i].y, panelSize[i].y);
                panelPos[i].y = main.height - panelPos[i].y - panelSize[i].y;
                go = new GameObject("border " + i);
                image = go.AddComponent<Image>();
                rect = go.GetComponent<RectTransform>();
                rect.parent = rectTransform;
                rect.sizeDelta = panelSize[i];
                rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
                rect.anchoredPosition = panelPos[i];
                if (marker.background == null)
                    image.color = marker.color;
                else
                    image.sprite = Sprite.Create((Texture2D)marker.background, iRect[i], Vector2.zero);
                AddChild(rect, TypePanel, cc);
            }
        }
        void PanelizeArea()
        {
            int j;
            int m = 0, h = 0, w = 0, wa, ha, cx, cy;
            GameObject go;
            RectTransform rect;
            Image image;
            if (marker.background != null)
            {
                w = marker.background.width;
                h = marker.background.height;
                m = (int)(margin / main.height * marker.background.height);
            }
            Vector3 cc;
            for (int i = 1; i < context.Length; i++)
            {
                j = i + 7;
                panelPos[j] = context[i].min;
                panelSize[j] = context[i].size;
                cc = new(panelPos[j].x, panelPos[j].y, panelSize[j].y);
                panelPos[j].y = main.height - panelPos[j].y - panelSize[j].y;
                go = new GameObject("area " + i);
                image = go.AddComponent<Image>();
                rect = go.GetComponent<RectTransform>();
                rect.parent = rectTransform;
                rect.sizeDelta = panelSize[j];
                rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
                rect.anchoredPosition = panelPos[j];
                AddChild(rect, TypePanel, cc);
                if (marker.background == null)
                    image.color = marker.color;
                else
                {
                    wa = (int)(context[i].width / inner.width * (marker.background.width - 2 * m));
                    ha = (int)(context[i].height / inner.height * (marker.background.height - 2 * m));
                    cx = m + (int)((context[i].xMin - inner.xMin) / inner.width * w);
                    cy = m + (int)((context[i].yMin - inner.yMin) / inner.height * h);
                    image.sprite = Sprite.Create((Texture2D)marker.background, new(cx, cy, wa, ha), Vector2.zero);
                }
            }
        }
        void Panelize()
        {
            GameObject go;
            RectTransform rect;
            RawImage image;
            Image back;
            if (type == ItemType.Object)
            {
                PanelizeBorder();
                PanelizeArea();
            }
            else
            {
                if (marker.background != null)
                    PanelizeBorder();
                go = new GameObject("area");
                back = go.AddComponent<Image>();
                if (marker.background == null)
                    back.color = marker.color;
                else
                {
                    int w = marker.background.width;
                    int h = marker.background.height;
                    int m = (int)(margin / main.height * marker.background.height);
                    back.sprite = Sprite.Create((Texture2D)marker.background, new Rect(m, m, w - 2 * m, h - 2 * m), Vector2.zero);
                }
                rect = go.GetComponent<RectTransform>();
                rect.parent = rectTransform;
                Rect R = marker.background != null ? inner : outer;
                rect.sizeDelta = R.size;
                rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
                rect.anchoredPosition = new Vector2(R.xMin, main.height - R.yMin - R.height);
                AddChild(rect, TypePanel, new(R.xMin, R.yMin, R.height));
                if (marker.items[index].image != null)
                {
                    go = new GameObject("image");
                    image = go.AddComponent<RawImage>();
                    image.texture = marker.items[index].image;
                    rect = go.GetComponent<RectTransform>();
                    rect.parent = rectTransform;
                    rect.sizeDelta = context[0].size;
                    rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
                    rect.anchoredPosition = new Vector2(context[0].x, main.height - context[0].yMin - context[0].height);
                    AddChild(rect, TypeImage, new(context[0].xMin, context[0].yMin, context[0].height));
                }
            }
        }
        Rect[] TextRectangles()
        {
            float offsetY, offsetX, endX;
            Rect[] line;
            int cTop, cBottom;
            float y;
            int count = item.lineCount;
            float height = lineHeight < 0 ? inner.height / count : lineHeight;
            if (lineHeight < 0) lineHeight = height;
            Rect T;
            //      if (marker.name == "savoye") Debug.Log("savo " + context.Length);
            if (context.Length < 3)
            {
                T = context.Length == 1 ? context[0] : context[1];
                if (inner.height > T.height)
                    count = (int)(T.height / height);
                //  if (marker.name == "savoye") Debug.Log("savo " + count + " " + T.height + " " + inner.height);
                //  height = T.height / count;
                offsetY = height / 4f;
                offsetX = (context.Length == 2 && marker.horizontal == Horizontal.Left) ? height / 4 : 0;
                endX = (context.Length == 2 && marker.horizontal == Horizontal.Right) ? height / 4 : 0;
                textHeight = height - offsetY * 2;
                line = new Rect[count];
                for (int i = 0; i < count; i++)
                    line[i] = new Rect(T.xMin + offsetX, T.yMin + height * i + offsetY, T.width - endX, textHeight);
            }
            else
            {
                //      count = (int)(inner.height / height);
                //        height = inner.height / count;
                cTop = (int)(context[1].height / height);
                cBottom = (int)(context[2].height / height);
                offsetY = height / 6f;
                y = context[2].y - cTop * height + offsetY;
                textHeight = height - offsetY * 2;
                line = new Rect[cBottom + cTop];
                count = cBottom + cTop;
                int j;
                for (int i = 0; i < count; i++)
                {
                    if ((i < cTop && marker.vertical == Vertical.Top) || (i >= cTop && marker.vertical == Vertical.Bottom))
                        offsetX = marker.horizontal == Horizontal.Left ? height / 4 : 0;
                    else offsetX = 0;
                    if ((i < cTop && marker.vertical == Vertical.Top) || (i >= cTop && marker.vertical == Vertical.Bottom))
                        endX = marker.horizontal == Horizontal.Right ? height / 4 : 0;
                    else endX = 0;
                    j = i < cTop ? 1 : 2;
                    line[i] = new Rect(context[j].x + offsetX, y + height * i + offsetY, context[j].width - endX, textHeight);
                }
            }
            return line;
        }

        void CreateCanvas()
        {
            owner = new GameObject("canvas");
            canvas = owner.AddComponent<Canvas>();
            if ((rectTransform = owner.GetComponent<RectTransform>()) == null) rectTransform = owner.AddComponent<RectTransform>();
            rectTransform.parent = marker.transform;
            if (marker.position != InfoPosition.WithObject) owner.layer = 5; // ui
            owner.SetActive(false);
            //      Debug.Log("world " + main.size.ToString() + outer.size.ToString());
        }
        void SetTransform()
        {
            Vector3 right, up;
            bool shiftX = false, shiftY = false;
            switch (marker.X)
            {
                case InputSetting.Axis.Facing:
                case InputSetting.Axis.X: right = marker.transform.right; break;
                case InputSetting.Axis.NegX: right = -marker.transform.right; shiftX = false; break;
                case InputSetting.Axis.Y: right = marker.transform.up; break;
                case InputSetting.Axis.NegY: right = -marker.transform.up; shiftX = false; break;
                case InputSetting.Axis.Z: right = marker.transform.up; break;
                default: right = -marker.transform.up; shiftX = false; break;
            }
            switch (marker.Y)
            {
                case InputSetting.Axis.Facing:
                case InputSetting.Axis.X: up = marker.transform.right; break;
                case InputSetting.Axis.NegX: up = -marker.transform.right; shiftY = false; break;
                case InputSetting.Axis.Y: up = marker.transform.up; break;
                case InputSetting.Axis.NegY: up = -marker.transform.up; shiftY = false; break;
                case InputSetting.Axis.Z: up = marker.transform.forward; break;
                default: up = -marker.transform.forward; shiftY = false; break;
            }
            Vector3 fwd = Vector3.Cross(right, up);
            Vector3 pos = Vector3.zero;
            if (shiftX) pos.x -= outer.width;
            if (shiftY) pos.y -= outer.height;
            Quaternion rot = Quaternion.LookRotation(fwd, up);
            owner.transform.rotation = rot;
            owner.transform.position = marker.transform.position - pos.x * right - pos.y * up;
        }
        void AdjustWithObject()
        {
            rectTransform.anchoredPosition = Vector2.zero;
            canvas.renderMode = RenderMode.WorldSpace;
            rectTransform.sizeDelta = outer.size;
            //     rectTransform.pivot=
            canvas.worldCamera = Camera.main;
            owner.transform.parent = marker.transform;
            SetTransform();

            if (type == ItemType.Object)
            {
                float max = new Vector3(objectBound.x, objectBound.z).magnitude;
                Vector3 displacement = marker.items[index].element.transform.position - objectBound;
                Vector3 center = new Vector3(panelPos[2].x - panelSize[2].x / 2 + panelPos[0].x + panelSize[0].x / 2, panelPos[0].y - panelSize[0].y / 2 + panelPos[3].y - panelSize[3].y / 2);
                center /= 2;
                float size = context[0].width;
                instance.transform.localScale *= size / max;
                displacement *= size / max;
                instance.transform.parent = owner.transform;
                instance.transform.localPosition = center - displacement;
                if (marker.rotateObject)
                    Tames.TameObject.CreateOrbiter(instance, center, Vector3.up, Vector3.forward, 5);
            }
        }
        void AdjustOnScreen()
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            factor = Screen.height;
            if (marker.position == InfoPosition.OnObject)
            {
                //         canvas.renderMode = RenderMode.ScreenSpaceCamera;
                //       owner.transform.parent = marker.transform;
                //        owner.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                //       rectTransform.localPosition = Vector3.zero;
            }
            if (type == ItemType.Object)
            {
                float max = new Vector3(objectBound.x, objectBound.z).magnitude;
                Vector3 displacement = marker.items[index].element.transform.position - objectBound;
                Vector3 center = new Vector3(panelPos[2].x - panelSize[2].x / 2 + panelPos[0].x + panelSize[0].x / 2, panelPos[0].y - panelSize[0].y / 2 + panelPos[3].y - panelSize[3].y / 2);
                center /= 2;
                float size = context[0].width;
                instance.transform.localScale *= size / max;
                displacement *= size / max;
                instance.transform.parent = owner.transform;
                instance.transform.localPosition = center - displacement;
                if (marker.rotateObject)
                    Tames.TameObject.CreateOrbiter(instance, center, Vector3.up, Vector3.forward, 5);
            }
        }
        static FontStyles GetStyle(int s)
        {
            FontStyles r = (s & Bold) != 0 ? FontStyles.Bold : FontStyles.Normal;
            r |= ((s & Italic) != 0 ? FontStyles.Italic : 0);
            return r;
        }
        string AddSpace(string s, int total)
        {
            string r = "";
            if (s.Length < total)
                for (int i = s.Length; i < total; i++)
                    r += "8";
            return r + s;
        }
        string BulletText(int n, int type)
        {
            return type switch
            {
                Dash => "-",
                Bullet => "●",
                Alpha => " abcdefghijklmnopqrstuvwxyz"[n] + ".",
                _ => n + "."
            };
        }

        int AddLine(int y, int start, int indent, string indentString, out TextMeshProUGUI created, float width, out bool manualBreak)
        {
            manualBreak = false;
            int i = start, r = start;
            string s;
            GameObject go;
            RectTransform rect;
            created = null;

            bool existing = text[start] != null;
            float offset = (indent + 1) * lines[y].height;
            float maxWidth = lines[y].width;
            float spaceCount = text[start] == null ? 0 : 1;
            float space = lines[y].height / 5;
            float[] widths = new float[words.Count];

            while (i < words.Count)
            {
                if (words[i].style == NewLine) { r = i; manualBreak = true; break; }
                s = words[i].text;

                if (words[i].prop == InfoReference.RefProperty.Name)
                    s = parent.references[words[i].depth].MaxName();
                else if (words[i].prop != InfoReference.RefProperty.None)
                    s = words[i].depth < 0 ? "888" : "" + parent.references[words[i].depth].MaxLength();
                if (s == "") { widths[i] = 0; ; continue; }
                else
                {
                    if (text[i] == null)
                    {
                        go = new GameObject("word " + i);
                        text[i] = go.AddComponent<TextMeshProUGUI>();
                        text[i].fontSize = textHeight;
                        text[i].color = (words[i].style & Highlight) == 0 ? marker.textColor : marker.textHighlight;
                        text[i].fontStyle = GetStyle(words[i].style);
                        text[i].text = s;
                    }
                    Vector2 pvs = text[i].GetPreferredValues();
                    spaceCount++;
                    //    Debug.Log(s + " O: " + offset + " X: " + pvs.x + " W: " + width + " M: " + maxWidth + " S: " + (space * (spaceCount - 1)));
                    if (width + pvs.x + offset + space * (spaceCount - 1) > maxWidth)
                    { r = i; created = i == start ? null : text[i]; break; }
                    else
                    {
                        width += (existing && i == start) ? 0 : pvs.x;
                        widths[i] = pvs.x;
                    }

                    if (words[i].prop != InfoReference.RefProperty.None)
                    {
                        if (words[i].prop == InfoReference.RefProperty.Time)
                        {
                            text[i].text = (int)Tames.TameElement.ActiveTime + "";
                            text[i].alignment = TextAlignmentOptions.Right;
                        }
                        else if (parent.references[words[i].depth] != null)
                        {
                            text[i].text = parent.references[words[i].depth].Get(words[i].prop);
                            text[i].alignment = words[i].prop != InfoReference.RefProperty.Name ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;
                        }
                    }
                }
                i++;
            }
            float x, xi;
            if (r > start)
            {
                x = lines[y].xMin;
                switch (marker.textPosition)
                {
                    case Markers.TextPosition.Left: break;
                    case Markers.TextPosition.Right: x = lines[y].xMin + lines[y].width - width - spaceCount * space - offset; break;
                    case Markers.TextPosition.Mid: x = lines[y].xMin + (lines[y].width - width - spaceCount * space - offset) / 2; break;
                    default: space = (lines[y].width - width - offset) / (spaceCount <= 1 ? 1 : spaceCount); break;
                }
                xi = x + indent * lines[y].height;
                x += offset;
                for (i = start; i < r; i++)
                    if (text[i] != null)
                    {
                        rect = text[i].GetComponent<RectTransform>();
                        rect.SetParent(rectTransform, false);
                        rect.pivot = rect.anchorMin = rect.anchorMax = Vector2.zero;
                        rect.sizeDelta = new Vector2(widths[i], lines[y].height);
                        rect.anchoredPosition = new Vector2(x, main.height - lines[y].yMin - lines[y].height);
                        rect.anchoredPosition3D = new Vector3(rect.anchoredPosition.x, rect.anchoredPosition.y, -0.01f);
                        AddChild(rect, i, new(x, lines[y].yMin, lines[y].height));
                        x += widths[i] + space;
                        //     if (marker.name == "bargraph") Debug.Log("bar: " + s);
                    }
                i = start - 1;
                if (indentString != "")
                {
                    go = new GameObject("word " + i);
                    text[i] = go.AddComponent<TextMeshProUGUI>();
                    text[i].fontSize = textHeight;
                    text[i].color = (words[i].style & Highlight) == 0 ? marker.textColor : marker.textHighlight;
                    text[i].fontStyle = GetStyle(words[i].style);
                    text[i].text = indentString;
                    rect = text[i].GetComponent<RectTransform>();
                    rect.SetParent(rectTransform, false);
                    rect.pivot = rect.anchorMin = rect.anchorMax = Vector2.zero;
                    rect.sizeDelta = new Vector2(width, lines[y].height);
                    rect.anchoredPosition = new Vector2(xi, main.height - lines[y].yMin - lines[y].height);
                    rect.anchoredPosition3D = new Vector3(rect.anchoredPosition.x, rect.anchoredPosition.y, -0.01f);
                    AddChild(rect, i, new(xi, lines[y].yMin, lines[y].height));
                }
            }


            return r;
        }
        void CreateText()
        {
            text = new TextMeshProUGUI[words.Count];
            int y = 0;
            if (lines.Length == 0) return;
            int[] order = new int[4];
            int lastIndent = -1, indent = -1;
            for (int i = 0; i < order.Length; i++)
                order[i] = 0;
            int next, w = 0;
            float lastTextWidth = 0;
            string indentString = "";
            while (w < words.Count)
            {
                next = AddLine(y, w, indent, indentString, out TextMeshProUGUI tm, lastTextWidth, out bool manualBreak);
                if (tm != null)
                    lastTextWidth = tm.GetPreferredValues().x;
                if (manualBreak) lastTextWidth = 0;
                y++;
                w = next + (manualBreak ? 1 : 0);
                if (y >= lines.Length || w >= words.Count)
                    break;
                if (words[w].bullet != 0)
                {
                    indent = words[w].depth;
                    if (lastIndent == indent)
                        order[indent]++;
                    else
                    {
                        for (int j = indent + 1; j < order.Length; j++)
                            order[j] = 1;

                        if (lastIndent > indent)
                            order[indent]++;
                        else
                            order[indent] = 1;
                        lastIndent = indent;
                    }
                    indentString = BulletText(order[indent], words[w].bullet);
                    w++;
                }
                else
                {
                    indentString = "";
                    if (manualBreak) lastIndent = indent = -1;
                }
            }
            for (int i = 0; i < sections.Count; i++)
                for (int j = sections[i].start; j < sections[i].end; j++)
                    ShowText(text[j], i == 0);
            //   Debug.Log("valid count = " + validWordCount + " > " + marker.gameObject.name);
        }



        public void SetInstancePosition()
        {
            Tames.TameCamera.camera.ScreenToWorldPoint(new Vector3());
        }
        public void UpdateReferences()
        {
            //    Debug.Log("update ref " + text[0].text);
            for (int i = 0; i < words.Count; i++)
                if (text[i] != null)
                {
                    if (words[i].prop == InfoReference.RefProperty.Time)
                        text[i].text = (int)Tames.TameElement.ActiveTime + "";
                    else if (words[i].prop == InfoReference.RefProperty.Value)
                    {
                        if (parent.references[words[i].depth] != null)
                        {
                            //       Debug.Log("ref bef " + words[i].index + "," + (text[i] == null) + " " + words[i].style);
                            text[i].text = parent.references[words[i].depth].Get(InfoReference.RefProperty.Value);
                            //          Debug.Log("ref aft " + words[i].index);
                        }
                    }
                    else if (words[i].prop == InfoReference.RefProperty.Name)
                        text[i].text = parent.references[words[i].depth].Get(InfoReference.RefProperty.Name);

                }
        }
        private static float[] XS = new float[] { -1, -1, -1, 0, 1, 1, 1, 0 };
        private static float[] YS = new float[] { -1, 0, 1, 1, 1, 0, -1, -1 };
        public Vector3 ClosestToTarget(Vector3 p)
        {
            Vector3 q = Vector3.zero;
            Vector3[] a = new Vector3[8];
            float d, min;
            if (marker.position == InfoPosition.WithObject)
            {
                for (int i = 0; i < 4; i++)
                    a[i] = rectTransform.position + main.size.x * (i < 2 ? -0.5f : 0.5f) * rectTransform.right + main.size.y * (i % 2 == 0 ? -0.5f : 0.5f) * rectTransform.up;
                q = a[0];
                min = Vector3.Distance(p, a[0]);
                for (int i = 1; i < 4; i++)
                    if ((d = Vector3.Distance(p, a[i])) < min) { min = d; q = a[i]; }
                q = Camera.main.transform.position + (q - Camera.main.transform.position).normalized;
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    a[i] = rectTransform.position + main.size.x * XS[i] * 0.5f * rectTransform.right + main.size.y * YS[i] * 0.5f * rectTransform.up;
                    a[i] = Camera.main.ScreenToWorldPoint(new Vector3(a[i].x, a[i].y, 1));
                }
                q = a[0];
                min = Vector3.Distance(p, a[0]);
                for (int i = 1; i < 8; i++)
                    if ((d = Vector3.Distance(p, a[i])) < min) { min = d; q = a[i]; }
            }

            return q;
        }
        public void UpdateLine()
        {
            Vector3 q = marker.link.transform.position;
            Vector3 p = ClosestToTarget(q);
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(new Vector3[] { p, q });
            float sw = (Screen.height / 400f) * 0.005f;
            Vector3 qs = Camera.main.WorldToScreenPoint(q);
            qs = Camera.main.transform.position + (qs - Camera.main.transform.position).normalized;
            lineRenderer.startWidth = lineRenderer.endWidth = sw;
        }
        /*
         * 
         * 
         */
        public void UpdateColors()
        {
            if (marker.background == null)
                for (int i = 0; i < childTypes.Count; i++)
                    if (childTypes[i] == -1)
                    {
                        Image im = children[i].gameObject.GetComponent<Image>();
                        if (im != null) im.color = marker.color;
                    }
                    else if (childTypes[i] >= 0)
                        text[childTypes[i]].color = words[childTypes[i]].IsHighlight ? marker.textHighlight : marker.textColor;

        }
        public int currentSection = 0;
        public bool GoPrev()
        {
            if (currentSection <= 0 || sections.Count == 0)
                return false;
            currentSection--;
            ShowSections();
            return true;
        }
        public bool GoNext()
        {
            if (currentSection >= sections.Count - 1 || sections.Count == 0)
                return false;
            currentSection++;
            Debug.Log(marker.name + " " + index + ": sec " + currentSection + " of " + sections.Count);
            ShowSections();
            return true;

        }
        public void Enter(int dir)
        {
            if (dir < 0)
                currentSection = sections.Count - 1;
            else
                currentSection = 0;
            ShowSections();
        }
        void ShowSections()
        {
            if (sections.Count != 0)
                for (int i = 0; i < sections.Count; i++)
                    for (int j = sections[i].start; j < sections[i].end; j++)
                        ShowText(text[j], i <= currentSection);

        }
        void ShowText(TextMeshProUGUI t, bool vis)
        {
            if (t != null)
            {
                t.gameObject.SetActive(vis);
                t.enabled = vis;
            }
        }
        public void Show(bool vis, bool passed = true)
        {
            owner.SetActive(vis);
            if (vis && passed) Enter(-1);
        }


    }
}