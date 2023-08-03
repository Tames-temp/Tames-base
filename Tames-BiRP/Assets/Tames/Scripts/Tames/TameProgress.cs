using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Tames
{
    public class TameTrigger
    {
        public const short Negative = -1;
        public const short None = 0;
        public const short Positive = 1;
        public bool mono = true;
        public int[] sign = new int[] { None, None, None };
        public float[] value = new float[] { 0f, 1f };
        public int Direction(float x)
        {
            for (int i = 0; i < value.Length; i++)
                if (x <= value[i])
                    return sign[i];
            return sign[value.Length];
        }
        public static TameTrigger TriggerFromText(string s)
        {
            if (s == "") return null;
            List<int> sign = new();
            List<float> value = new();
            int p;
            float f;
            string num = "";
            bool expectNumber = false;
            if ("+- ".IndexOf(s[0]) < 0) sign.Add(0);
            for (int i = 0; i < s.Length; i++)
                if ((p = "- +".IndexOf(s[i])) >= 0)
                {
                    if (expectNumber) return null;
                    else
                    {
                        expectNumber = true;
                        if (num != "")
                        {
                            if (Utils.SafeParse(num, out f))
                                value.Add(f);
                            else
                                return null;
                        }
                        num = "";
                        sign.Add(p - 1);
                    }
                }
                else
                {
                    num += s[i];
                    expectNumber = false;
                }
            if (sign.Count == value.Count) sign.Add(0);
            bool integer = false;
            float[] vi = value.ToArray();
            for (int i = 0; i < value.Count; i++)
            {
                vi[i] /= 100;
                if (value[i] > 1.0001f)
                    integer = true;
            }
            return new TameTrigger()
            {
                sign = sign.ToArray(),
                value = integer ? vi : value.ToArray()
            };
        }
    }
    public class TameDurationManager
    {
        public TameElement parent = null;
        public float offset = 0;
        public float factor = 0;
        /// <summary>
        /// the speed of changing progress (per second). Changing the speed would change the duration (= 1 / speed)
        /// </summary>
        public float Speed { get { return speed; } set { speed = value; if (speed != 0) duration = 1 / speed; else duration = 0; } }
        private float speed = -1;
        /// <summary>
        /// the duration of completing 0 to 1 of the progress, per second. Changing the duration would change the speed (= 1 / duration)
        /// </summary>
        public float Duration { get { return duration; } set { duration = value; if (duration != 0) speed = 1 / duration; else speed = 0; } }
        private float duration = -1;
        public void Refresh()
        {
            if (parent != null)
            {
                Speed = offset < 0 ? 0 : offset + parent.progress.progress * factor;
                //       if (parent.name == "_speed")                    Debug.Log("speed: " + speed+ " < "+offset+" "+factor+" "+parent.progress.progress);
            }
        }
        public TameDurationManager Clone()
        {
            return new TameDurationManager()
            {
                parent = parent,
                offset = offset,
                factor = factor,
                Speed = speed,
                Duration = duration
            };
        }
        public static TameDurationManager CreateByDuration(float dur)
        {
            return new TameDurationManager()
            {
                Duration = dur
            };
        }
    }
    /// <summary>
    /// this class is used to process all mathematical but non-physical changes in objects. The progress value is used to calculate the physical properties in other classes.
    /// </summary>
    public class TameProgress
    {
        /// <summary>
        /// tick is used to know whether a progress is updated in the current frame or not. When updating tick is valued equal to the current project Tick in the TameElement class. If a progress tick is equal to the project Tick, it means it is already updated and so is not updated again. This doesn't have any effect currently but is included in case future changes would make a recursive update possible. 
        /// </summary>
        public int tick = -1;

        /// <summary>
        /// whether the progress is updated simultanously with its parent, or independently after triggered. It only returns With if <see cref="trigger"/> in null.
        /// <summary>
        /// the type of cycle when the total progress is not between 0 and 1
        /// </summary>
        public ContinuityMode continuity = ContinuityMode.Stop;
        /// <summary>
        /// not currently in use.
        /// </summary>
        public float stopValue = 1;
        /// <summary>
        /// whether this progress' children are updated based on the 0..1 value or total of this progress
        /// </summary>
        public PassTypes passToChildren = PassTypes.Total;
        /// <summary>
        /// determines when the progress starts moving after parent reaches a certain progress value. There are four ways that the trigger works:
        /// trigger[0] < 0: the progress is active when its parent passed progress (or time passed) &lt= trigger[1] 
        /// trigger[1] < 0: the progress is active when its parent passed progress (or time passed) &gt= trigger[0] 
        /// trigger[0] &lt tigger[1]: this progress would change normally when the parent progress is between the triggers and stops when not. 
        /// trigger[0] &gt trigger[1]: this progress stops when the parent progress is between the triggers, changes normally when the latter is over trigger[0] and reverses when the latter is under progress[1]. 
        /// </summary>
        public TameTrigger trigger = null;
        public TameTrigger activeTrigger = null;
        /// <summary>
        /// the progress change direction (-1: downward, 0: still, 1: upward). If the parent is another TameElement, this value is determined automatically depending on the trigger. However, if the parent is a position or time, this is determined by interaction with the interactors.
        /// </summary>
        private int changingDirection = 0;
        /// <summary>
        /// indicates if the progress would change normally (+1 or upward), reverse (-1 or downward), remains unchanged (0). This value will be multiplied by the progress's own direction that is controlled by <see cref="trigger"/>s. This value is passed to the progress based on the elements interaction with the interactors.
        /// </summary>
        public int interactDirection = 0;
        public TameDurationManager manager = new TameDurationManager();
        /// <summary>
        ///  the last progress value before the update, between 0 and 1
        /// </summary>
        public float lastProgress = 0;
        /// <summary>
        ///  the current progress value (after the last update), between 0 and 1
        /// </summary>
        public float progress = 0;
        public float lastSub = 0;
        public float subProgress = 0;
        /// <summary>
        /// the last total progress value before the last update
        /// </summary>
        public float lastTotal = 0;
        /// <summary>
        /// the current total progress value after the last update, usually accummulates and is not limited to 0 and 1. This is used to control cycles  
        /// </summary>
        public float totalProgress = 0;
        /// <summary>
        /// returns the value of the would-be-progress if it is the result of the ping-pong of the total value. This is most useful for ever sliding objects and fading lights 
        /// </summary>
        /// <param name="value">total value (or total progress)</param>
        /// <returns>the value of the would-be-progress</returns>
        private float time = 0;
        public LerpManager lerp = null;
        public TameElement element = null;
        public bool active = true;

        public float A = 0;
        public float lastSpeed = 0;
        public int stepCount = 0;
        public float[] steps = null;
        public bool isMultiAlter = false;
        public int fromAlter = 0, toAlter = 1;
        public int initiated = 0;
        private int direction = 0;
        public bool isOn = true;
        public float frameWaitCount = 0;
        public TameProgress(TameElement element)
        {
            this.element = element;
        }
        public TameProgress(TameProgress p)
        {
            continuity = p.continuity;
            //   stop = p.stop;
            manager = p.manager.Clone();
            //       Speed = p.speed;
            trigger = p.trigger;
            passToChildren = p.passToChildren;
            lerp = p.lerp;
        }
        public void GetSteps(string s)
        {
            List<float> st = new List<float>();
            float f, max = 0;
            if (s == "") stepCount = 0;
            else
            {
                string[] sp = s.Split(new char[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (sp.Length == 1)
                    try { stepCount = int.Parse(sp[0]); return; } catch { }
                else
                {
                    for (int i = 0; i < sp.Length; i++)
                        try
                        {
                            f = float.Parse(sp[i]);
                            if (f >= 0 && f <= 100) st.Add(f);
                            else continue;
                            //       Debug.Log("step: " + element.name + " #" + st.Count + " " + f);
                            max = Mathf.Max(max, f);
                        }
                        catch { }
                    if (st.Count < 2) return;
                    else max = max > 1 ? 100 : 1;
                    steps = st.ToArray();
                    stepCount = steps.Length;
                    for (int i = 0; i < steps.Length; i++)
                        steps[i] /= max;
                    fromAlter = 0;
                }
            }
        }
        private float Reverse(float value)
        {
            if ((value <= 1f) && (value >= 0f))
                return value;
            else
            {
                float v = value > 1 ? value : -value;
                return 1f - Mathf.Abs(1f - (v % 2f));
            }
        }
        /// <summary>
        /// returns the value of the would-be-progress if it is the result of the cycling of the total value from start. This is most useful in full rotation cycles, blinking lights, and sliding materials
        /// </summary>
        /// <param name="value">total value (or total progress)</param>
        /// <returns>the value of the would-be-progress</returns>
        private float Cycle(float value)
        {
            if ((value <= 1f) && (value >= 0f))
                return value;
            else if (value > 1f)
                return value % 1f;
            else
                return 1 - (-value) % 1;
        }
        /// <summary>
        /// not currently in use
        /// </summary>
        public void Stop()
        {
            changingDirection = 0;
        }
        /// <summary>
        /// not currently in use
        /// </summary>
        /// <param name="dir"></param>
        public void Start(int dir)
        {
            changingDirection = dir;
        }
        public void Retain(float deltaTime)
        {
            progress = lastProgress;
            totalProgress = lastTotal;
            //  time += deltaTime;
        }

        /// <summary>
        /// sets the current progress based on its parent. This method is where the trigger, speed, duration, changingDirection and follow fields take effect 
        /// </summary>
        /// <param name="parentProg">the array including the parents last and current progress values</param>
        /// <param name="parentTotal">the array including the parents last and current total progress values</param>
        /// <param name="pass">how the parent passes the progress to children</param>
        /// <param name="deltaTime">the delta time of the frame</param>       
        public void SetByParent(float[] parentProg, float[] parentTotal, PassTypes pass, float deltaTime)
        {
            //   float pt;
            // float speed = duration < 0 ? (parentTotal[1] - parentTotal[0]) / deltaTime : this.speed;
            float parentSpeed;
            float dp, dif;
            float pt;
            manager.Refresh();
            //   float[] passed = pass == PassTypes.Progress ? parentProg : parentTotal;
            if (trigger == null)
            {
                if (manager.Speed == 0)
                    SetProgress(pass == PassTypes.Total ? parentTotal[1] : parentProg[1], false);
                else
                {
                    if (pass == PassTypes.Progress)
                    {
                        dif = parentProg[1] - parentProg[0];
                        if (Mathf.Abs(dif) < 0.0001) dif = 0;
                        parentSpeed = dif / deltaTime;
                        if (parentSpeed == 0) { Retain(deltaTime); return; }
                        //       dp = (speed < 0 ? 1 : speed / Mathf.Abs(parentSpeed)) * (parentProg[1] - parentProg[0]);
                        dp = manager.Speed < 0 ? parentProg[1] - parentProg[0] : (manager.Speed / parentSpeed) * (parentProg[1] - parentProg[0]);
                    }
                    else
                    {
                        dif = parentTotal[1] - parentTotal[0];
                        if (Mathf.Abs(dif) < 0.0001) dif = 0;
                        if (element.name == "window.001") Debug.Log(":: " + dif + " " + progress + " " + tick + " " + TameElement.Tick);
                        parentSpeed = dif / deltaTime;
                        if (parentSpeed == 0) { Retain(deltaTime); return; }
                        dp = manager.Speed < 0 ? parentTotal[1] - parentTotal[0] : (manager.Speed / parentSpeed) * (parentTotal[1] - parentTotal[0]);
                    }
                    SetProgress(totalProgress + dp * interactDirection, false);
                }
            }
            else
            {
                time += deltaTime;
                pt = parentProg[1];
                changingDirection = trigger.Direction(pt) * interactDirection;
                dp = totalProgress + changingDirection * deltaTime * (manager.Speed < 0 ? 1 : manager.Speed);
                //   if (element.name == "door1") Debug.Log("door: " + changingDirection + " " + pt + " " + element.parents[0].parent.name) ;

                //   if (element.name == "cooler") Debug.Log("enfo prog " + progress + " " + trigger.mono + " " + changingDirection);
                SetProgress(dp, false);
            }
        }
        public void SetByTime(float deltaTime)
        {
            //   if (element.name == "rotator") Debug.Log("prog " + manager.parent.progress.progress);

            // float[] pp = new float[] { time, time + deltaTime * interactDirection };
            // time += deltaTime * interactDirection;
            //   if (element.name == "item1" )                Debug.Log(" : " + initiated);
            if (isMultiAlter) SetByAlter(deltaTime);
            else
            {
                float[] pp = new float[] { time, time + deltaTime };
                time += deltaTime;
                SetByParent(pp, pp, PassTypes.Total, deltaTime);
            }
        }
        private float _p, _lp, _sp, _t, _lt;
        private void Push()
        {
            _p = progress;
            _lp = lastProgress;
            _sp = subProgress;
            _t = totalProgress;
            _lt = lastTotal;
            //       _time = time;
        }
        private void Pull()
        {
            progress = _p;
            lastProgress = _lp;
            subProgress = _sp;
            totalProgress = _t;
            lastTotal = _lt;
            //         time = _time;
            //         tick--;
        }
        public float FakeByOffset(float offset)
        {
            Push();
            float v = totalProgress + offset;
            Set(v);
            v = progress;
            Pull();
            return v;
        }
        /// <summary>
        /// sets the progress based on the passage of time. The progress is determined by time, changingDirection and speed of the progress
        /// </summary>
        /// <param name="deltaTime">the frame's delta time</param>

        public void SetProgress(float value)
        {
            SetProgress(value, true);
        }
        /// <summary>
        /// converts the total progress to limited progress based on the value and cycle type 
        /// </summary>
        /// <param name="value">the intended total progress</param>
        public void SetProgress(float value, bool refresh)
        {
            //   if (element.name == "Quad") Debug.Log(":: " + progress);
            if (!active) return;
            if (refresh) manager.Refresh();
            if (tick < TameElement.Tick)
            {
                Set(value);
                tick++;
            }
            //    if (element.name == "traain")                Debug.Log("train: " + (progress-lastProgress));
        }
        private float FromSteps(float value)
        {
            float v = value;
            int i;
            if (stepCount > 0)
                if (steps == null)
                    v = (int)(value * stepCount) / (float)stepCount;
                else
                {
                    i = (int)(value * stepCount);
                    if (i < 0) i = 0;
                    if (i >= stepCount) i = stepCount - 1;
                    v = steps[i];
                }
            return v;
        }
        private void Set(float value)
        {
         //   if (element.name == "lights") Debug.Log(progress);
            int i;
            float v = value;
            lastSub = subProgress;
            switch (continuity)
            {
                case ContinuityMode.Stop:
                    v = FromSteps(v);
                    if (v < 0) v = 0;
                    if (v > 1f) v = 1f;
                    lastProgress = lastTotal = progress;
                    progress = totalProgress = v;
                    break;
                case ContinuityMode.Reverse:
                    v = Reverse(value);
                    v = FromSteps(v);
                    lastTotal = totalProgress;
                    lastProgress = progress;
                    progress = v;
                    totalProgress = value;
                    break;
                case ContinuityMode.Cycle:
                    v = Cycle(value);
                    v = FromSteps(v);
                    lastTotal = totalProgress;
                    lastProgress = progress;
                    progress = v;
                    totalProgress = value;
                    break;
            }
            if (lerp == null)
                subProgress = progress;
            else
                subProgress = lerp.On(progress);

        }
        private float NormalAlter(float v)
        {
            return v;
        }
        private void SetByAlter(float dt)
        {
     //       if (element.name == "item1" && initiated != 0)                Debug.Log(" : " + initiated);

            if (initiated != 0)
                direction = initiated;
            if (direction == 0) return;

            if (Mathf.Abs(dt) < 0.0001) dt = 0;
            if (dt == 0) { Retain(dt); return; }
            float dp = manager.Speed < 0 ? dt : manager.Speed * dt;

            float v = progress + (!isOn ? dp * direction : 0);
            float min, max;
            int next;
            lastSub = subProgress;
            bool cycled = false;
            switch (continuity)
            {
                case ContinuityMode.Stop:
                case ContinuityMode.Reverse:
                    if (isOn)
                    {
                        if (initiated > 0)
                        {
                            toAlter = fromAlter == stepCount - 1 ? fromAlter : fromAlter + 1;
                            if (toAlter == fromAlter) v = 0;
                            else { v = dp; isOn = false; }
                        }
                        else if (initiated < 0)
                        {
                            toAlter = fromAlter;
                            fromAlter = fromAlter == 0 ? 0 : fromAlter + 1;
                            if (fromAlter == 0) v = 0;
                            else { v = 1 - dp; isOn = false; }
                        }
                    }
                    else
                    {
                        if (v <= 0)
                        {
                            isOn = true;
                            v = 0;
                        }
                        else if (v >= 1)
                        {
                            isOn = true;
                            v = 0;
                            fromAlter = toAlter;
                        }
                    }
                    break;
                case ContinuityMode.Cycle:
                    if (isOn)
                    {
                        if (initiated > 0)
                        {
                            if (steps == null)
                            {
                                toAlter = fromAlter == stepCount - 1 ? 1 : fromAlter + 1;
                                fromAlter = fromAlter == stepCount - 1 ? 0 : fromAlter;
                                v = dp; isOn = false;
                            }
                            else
                            {
                                toAlter = fromAlter == stepCount - 1 ? 0 : fromAlter + 1;
                                v = dp; isOn = false;
                            }
                        }
                        else if (initiated < 0)
                        {
                            if (steps == null)
                            {
                                toAlter = fromAlter == 0 ? stepCount - 1 : fromAlter;
                                fromAlter = fromAlter == 0 ? stepCount - 2 : fromAlter - 1;
                                v = 1 - dp; isOn = false;
                            }
                            else
                            {
                                toAlter = fromAlter;
                                fromAlter = fromAlter == 0 ? stepCount - 1 : fromAlter - 1;
                                v = 1 - dp; isOn = false;
                            }
                        }
                    }
                    else
                    {
                        if (v <= 0)
                        {
                            isOn = true;
                            v = 0;
                        }
                        else if (v >= 1)
                        {
                            isOn = true;
                            v = 0;
                            fromAlter = toAlter;
                        }
                    }
                    break;
            }
            lastProgress = lastTotal = progress;
            progress = totalProgress = v;
            if (steps == null)
            {
                subProgress = (fromAlter + v) / (steps.Length - 1);
            }
            else
            {
                min = steps[fromAlter];
                max = steps[toAlter];
                subProgress = min + v * (max - min);
            }
        }

        public void SetAt(float p)
        {
            int i = (int)(p * stepCount);
            if (i < 0) i = 0;
            if (i >= stepCount) i = stepCount - 1;
            lastProgress = totalProgress = progress = lastProgress = 0;
            if (steps != null)
            {
                fromAlter = (int)(p * (stepCount - 1));
                fromAlter = fromAlter > stepCount - 2 ? stepCount - 2 : fromAlter;
                subProgress = steps[fromAlter];
            }
            else
            {
                fromAlter = i;
                subProgress = (float)i / stepCount;
            }
            isOn = true;
        }

        public TameProgress Clone(TameElement te)
        {
            TameProgress tp = new TameProgress(te)
            {
                progress = progress,
                totalProgress = totalProgress,
                lastProgress = lastProgress,
                lastTotal = lastTotal,
                lastSub = lastSub,
                subProgress = subProgress,
                lerp = lerp == null ? null : lerp.Clone(),
                trigger = trigger,
                manager = manager.Clone(),
                active = active,
                continuity = continuity,
                frameWaitCount = frameWaitCount,
                stepCount = stepCount,
                steps = steps,
            };
            return tp;
        }
    }
}