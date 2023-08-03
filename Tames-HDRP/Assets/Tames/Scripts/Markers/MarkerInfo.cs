using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
namespace Markers
{
    public enum Vertical
    {
        Top, Bottom, Stretch
    }
    public enum Horizontal

    {
        Left, Right, Stretch
    }
    public enum InfoPosition
    {
        OnObject, WithObject, Top, Bottom, TopLeft, BottomRight, TopRight, BottomLeft, Left, Right
    }
    public enum ImagePosition
    {
        Top, Bottom, TopLeft, BottomRight, TopRight, BottomLeft, Left, Right
    }
    public enum TextPosition
    {
        Left, Mid, Right, Justified
    }
    public enum InfoOrder
    {
        ReplacePrevious, ReplaceAll, AddVertical, AddHorizontal
    }
    [System.Serializable]
    public class InfoItem
    {
        public GameObject element;
        public Texture image;
        [SerializeField]
        [TextAreaAttribute(5, 10)]
        private string text;
        public float textPortion = 0.7f;
        public InfoOrder replace = InfoOrder.ReplacePrevious;
        public string Text { get { return text; } }
    }
    public class MarkerInfo : MonoBehaviour
    {
        private static InfoPosition lastPosition = InfoPosition.OnObject;
        private static Vertical lastVT = Vertical.Top;
        private static Horizontal lastHT = Horizontal.Stretch;
        private static Color lastBack = Color.white, lastFore = Color.black;
        private static float lastThick = 0.02f, lastWidth = 0.4f, lastHeight = 0.4f;
        private static int lastLine = 10;

        public Vertical vertical { get { return GetVertical(); } }
        public Horizontal horizontal { get { return GetHorizontal(); } }
        public ImagePosition imagePosition;
        public TextPosition textPosition;
        public int lineCount = lastLine;
        public Color color = lastBack;
        public Texture background = null;
        public Color textColor = lastFore;
        public Color textHighlight = lastFore;
        public TMP_FontAsset font;
        public InfoItem[] items;
        //   public Color frame = lastFrame;
        public float margin = lastThick;
        public float width = lastWidth;
        public float height = lastHeight;
        public InfoPosition position = lastPosition;
        public InputSetting.Axis X = InputSetting.Axis.X;
        public InputSetting.Axis Y = InputSetting.Axis.Y;
        public bool rotateObject = true;
        public InputSetting control;
        public GameObject[] areas;
        public GameObject[] references;
        public GameObject link;
        private Texture[] output;
        private bool changed = false;
        private InfoUI.InfoControl ic = null;
        /*
         * width and height,  
         */
        Vertical GetVertical()
        {
            return imagePosition switch
            {
                ImagePosition.Top => Vertical.Top,
                ImagePosition.TopLeft => Vertical.Top,
                ImagePosition.TopRight => Vertical.Top,
                ImagePosition.Bottom => Vertical.Bottom,
                ImagePosition.BottomRight => Vertical.Bottom,
                ImagePosition.BottomLeft => Vertical.Bottom,
                _ => Vertical.Stretch,
            };
        }
        Horizontal GetHorizontal()
        {
            return imagePosition switch
            {
                ImagePosition.Left => Horizontal.Left,
                ImagePosition.TopLeft => Horizontal.Left,
                ImagePosition.TopRight => Horizontal.Right,
                ImagePosition.BottomLeft => Horizontal.Left,
                ImagePosition.Right => Horizontal.Right,
                ImagePosition.BottomRight => Horizontal.Right,
                _ => Horizontal.Stretch,
            };
        }
        public void SetIC(InfoUI.InfoControl ic)
        {
            this.ic = ic;
        }
        public void ChangedThisFrame(bool shouldChange)
        {
            if (UnityEditor.EditorApplication.isPlaying || UnityEditor.EditorApplication.isPaused)
                changed = shouldChange;
            else changed = false;
        }
        private void LateUpdate()
        {
            if (changed)
            {
                changed = false;
                if (ic != null) ic.UpdateMarker();
            }
        }
        private object DrawX(object[] args)
        {
            return null;
        }
        public void CaptureLast()
        {
            lastBack = color;
            lastFore = textColor;
            //   lastFrame = frame;
            lastThick = margin;
            lastWidth = width;
            lastHeight = height;
            lastVT = vertical;
            lastPosition = position;
        }

    }
}

