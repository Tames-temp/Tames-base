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
            return new TameTrigger()
            {
                sign = sign.ToArray(),
                value = value.ToArray(),
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
        public float Speed { get { return speed; } set { speed = value; duration = 1 / speed; } }
        private float speed = -1;
        /// <summary>
        /// the duration of completing 0 to 1 of the progress, per second. Changing the duration would change the speed (= 1 / duration)
        /// </summary>
        public float Duration { get { return duration; } set { duration = value; speed = 1 / duration; } }
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
        public float lastSlerp = 0;
        public float slerpProgress = 0;
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
        public Slerp slerp = null;
        public TameElement element = null;
        public bool active = true;

        public float A = 0;
        public float lastSpeed = 0;
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
            slerp = p.slerp;
        }
        private float Reverse(float value)
        {
            if ((value <= 1) && (value >= 0))
                return value;
            else
            {
                float v = value > 1 ? value : -value;
                return 1 - Mathf.Abs(1 - (v % 2));
            }
        }
        /// <summary>
        /// returns the value of the would-be-progress if it is the result of the cycling of the total value from start. This is most useful in full rotation cycles, blinking lights, and sliding materials
        /// </summary>
        /// <param name="value">total value (or total progress)</param>
        /// <returns>the value of the would-be-progress</returns>
        private float Cycle(float value)
        {
            if ((value <= 1) && (value >= 0))
                return value;
            else if (value > 1)
                return value % 1;
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
                    //     dp = (speed < 0 ? 1 : speed / Mathf.Abs(parentSpeed)) * (parentTotal[1] - parentTotal[0]);
                    dp = manager.Speed < 0 ? parentTotal[1] - parentTotal[0] : (manager.Speed / parentSpeed) * (parentTotal[1] - parentTotal[0]);
                }
                SetProgress(totalProgress + dp * interactDirection, false);
                if (element.name == "areacheck 0") Debug.Log("switch: > " + progress + " " + dp +" "+manager.Speed+" "+ parentProg[1]+ " "+element.parents[0].parent.name);
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

            float[] pp = new float[] { time, time + deltaTime };
            time += deltaTime;

            SetByParent(pp, pp, PassTypes.Total, deltaTime);
        }
        private float _p, _lp, _sp, _t, _lt;
        private void Push()
        {
            _p = progress;
            _lp = lastProgress;
            _sp = slerpProgress;
            _t = totalProgress;
            _lt = lastTotal;
            //       _time = time;
        }
        private void Pull()
        {
            progress = _p;
            lastProgress = _lp;
            slerpProgress = _sp;
            totalProgress = _t;
            lastTotal = _lt;
            //         time = _time;
            //         tick--;
        }
        public float FakeByOffset(float offset)
        {
            Push();
            float v = totalProgress + offset;
            SetProgLin(v);
            v = progress;
            Pull();
            return v;
        }
        /// <summary>
        /// sets the progress based on the passage of time. The progress is determined by time, changingDirection and speed of the progress
        /// </summary>
        /// <param name="deltaTime">the frame's delta time</param>
        public void SetProgressByTime(float deltaTime)
        {
            float pt;
            manager.Refresh();
            float duration = manager.Duration < 0 ? 1 : manager.Duration;
            if (trigger == null)
            {
                if (continuity != ContinuityMode.Stop)
                {
                    time += deltaTime * interactDirection;
                    SetProgress(time / duration, false);
                    if (manager.parent != null) Debug.Log(time / duration + " " + interactDirection);
                }
                else
                {
                    pt = deltaTime * interactDirection / duration;
                    if (progress + pt < 0)
                        SetProgress(0, false);
                    else if (progress + pt > 1)
                        SetProgress(1, false);
                    else
                        SetProgress(pt + progress, false);
                }
            }
            else
            {
                changingDirection = 0;
                time += deltaTime;
                pt = time / duration;
                if (trigger.mono)
                {
                    changingDirection = trigger.Direction(pt) * interactDirection;
                }
                else
                {
                    switch (continuity)
                    {
                        case ContinuityMode.Stop:
                            if (pt < 0) pt = 0;
                            if (pt > 1) pt = 1;
                            break;
                        case ContinuityMode.Reverse:
                            pt = Reverse(pt);
                            break;
                        case ContinuityMode.Cycle:
                            pt = Cycle(pt);
                            break;
                    }
                    changingDirection = trigger.Direction(pt) * interactDirection;

                }
                pt = totalProgress + changingDirection * deltaTime * manager.Speed;
                SetProgress(pt, false);
            }
        }
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
                if (A > 0)
                    SetProgAcce(value);
                else
                    SetProgLin(value);
                tick++;
            }
            //    if (element.name == "traain")                Debug.Log("train: " + (progress-lastProgress));
        }
        private void SetProgLin(float value)
        {
            float v;
            lastSlerp = slerpProgress;
            switch (continuity)
            {
                case ContinuityMode.Stop:
                    if (value < 0) value = 0;
                    if (value > 1) value = 1;
                    lastProgress = lastTotal = progress;
                    progress = totalProgress = value;
                    break;
                case ContinuityMode.Reverse:
                    v = Reverse(value);
                    lastTotal = totalProgress;
                    lastProgress = progress;
                    progress = v;
                    totalProgress = value;
                    break;
                case ContinuityMode.Cycle:
                    v = Cycle(value);
                    lastTotal = totalProgress;
                    lastProgress = progress;
                    progress = v;
                    totalProgress = value;
                    break;
            }
            if (slerp == null)
                slerpProgress = progress;
            else
                slerpProgress = slerp.On(progress);
        }

        public void SetProgAcce(float value)
        {
            float v;
            lastSlerp = slerpProgress;
            float Ve, Va, Pe, Pa, Ae, dt = TameElement.deltaTime;
            switch (continuity)
            {
                case ContinuityMode.Stop:
                    if (value < 0) value = 0;
                    if (value > 1) value = 1;
                    Pe = value;
                    Pa = Va = Ve = (Pe - progress) / dt;
                    Ae = (Ve - lastSpeed) / dt;
                    if (Mathf.Abs(Ae) > A)
                    {
                        Va = A * dt * Mathf.Sign(Ae) + lastSpeed;
                        Pa = Va * dt + progress;
                    }
                    lastProgress = lastTotal = progress;
                    progress = totalProgress = Pa;
                    lastSpeed = Va;
                    break;
                case ContinuityMode.Reverse:
                    v = Reverse(value);
                    lastTotal = totalProgress;
                    lastProgress = progress;
                    progress = v;
                    totalProgress = value;
                    break;
                case ContinuityMode.Cycle:
                    v = Cycle(value);
                    lastTotal = totalProgress;
                    lastProgress = progress;
                    progress = v;
                    totalProgress = value;
                    break;
            }
            if (slerp == null)
                slerpProgress = progress;
            else
                slerpProgress = slerp.On(progress);
        }

        public void Initialize(float p)
        {
            lastProgress = totalProgress = progress = lastTotal = p;
        }

        public TameProgress Clone(TameElement te)
        {
            TameProgress tp = new TameProgress(te)
            {
                progress = progress,
                totalProgress = totalProgress,
                lastProgress = lastProgress,
                lastTotal = lastTotal,
                lastSlerp = lastSlerp,
                slerpProgress = slerpProgress,
                slerp = slerp == null ? null : slerp.Clone(),
                trigger = trigger,
                manager = manager.Clone(),
                active = active,
                continuity = continuity,
            };
            return tp;
        }
    }
}