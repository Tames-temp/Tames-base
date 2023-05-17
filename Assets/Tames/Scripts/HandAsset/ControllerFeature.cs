using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HandAsset
{
    public class ControllerFeature
    {
        public int type = BOOL;
        public const int BOOL = 0;
        public const int FLOAT = 1;
        public const int VECTOR = 2;
        private bool now = false, past = false;
        private float[] value = new float[] { 0, 0 }, last = new float[] { 0, 0 };
        public bool Pressed
        {
            get
            {
                return type switch
                {
                    BOOL => (now != past) && (now),
                    FLOAT => (Mathf.Abs(value[0]) >= threshold) && (Mathf.Abs(last[0]) < threshold),
                    VECTOR => ((Mathf.Abs(value[0]) >= threshold) || (Mathf.Abs(value[1]) >= threshold)) && ((Mathf.Abs(last[0]) < threshold) && (Mathf.Abs(last[1]) < threshold)),
                    _ => false,
                };
            }
        }
        public bool Released
        {
            get
            {
                return type switch
                {
                    BOOL => (now != past) && (!now),
                    FLOAT => (Mathf.Abs(value[0]) < threshold) && (Mathf.Abs(last[0]) >= threshold),
                    VECTOR => ((Mathf.Abs(value[0]) < threshold) || (Mathf.Abs(value[1]) < threshold)) && ((Mathf.Abs(last[0]) >= threshold) && (Mathf.Abs(last[1]) >= threshold)),
                    _ => false,
                };
            }
        }
        public float Value { get { return value[0]; } }
        public bool Status { get { return now; } }
        public Vector2 Vector { get { return new Vector2(value[0], value[1]); } }

        public float holdDuration = 0;
        public float threshold = 0.8f;
        public Vector2 VectorDelta { get { return new Vector2(value[0] - last[0], value[1] - last[1]); } }
        public float FloatDelta { get { return value[0] - last[0]; } }
        public float InFloat { set { Update(value); } }
        public bool InBool { set { Update(value); } }
        public Vector2 InVector { set { Update(value); } }
        public ControllerFeature(int type) { this.type = type; }
        public ControllerFeature(int type, float threshold) { this.type = type; this.threshold = threshold; }

        public void Update(bool b)
        {
            past = now;
            now = b;
            last[0] = last[1] = value[0];
            value[0] = value[1] = now ? 1 : 0;
            if (now) holdDuration += Tames.TameElement.deltaTime;
        }
        public void Update(float f)
        {
            last[0] = last[1] = value[0];
            value[0] = value[1] = f;
            past = now;
            now = Mathf.Abs(value[0]) >= threshold;
            if (now) holdDuration += Tames.TameElement.deltaTime;
        }
        public void Update(Vector2 v)
        {
            past = now;
            last[0] = value[0];
            last[1] = value[1];
            value[0] = v.x;
            value[1] = v.y;
            now = (Mathf.Abs(v.x) >= threshold) && (Mathf.Abs(v.y) >= threshold);
            if (now) holdDuration += Tames.TameElement.deltaTime;
        }
    }
}
