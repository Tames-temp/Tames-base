using Multi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace Tames
{
    /// <summary>
    /// A basic element representing time. 
    /// </summary>
    public class TameTime : TameElement
    {
        /// <summary>
        /// the basic time element
        /// </summary>
        public static TameTime RootTame = new TameTime();
        public TameTime()
        {
            tameType = TameKeys.Time;
            progress = new TameProgress(this);
        }
        public override void Update(TameProgress p)
        {
            progress.totalProgress = Time.time;
            progress.progress = Time.time % 1;
            progress.passToChildren = PassTypes.Total;
            //     progress.stop = StopTypes.Never;
            progress.cycle = CycleTypes.Cycle;
        }
    }

    public class TameHead : TameElement
    {
        public static TameHead Heads = new TameHead();
        public Person[] people;
        public TameHead()
        {
            tameType = TameKeys.Head;
            progress = new TameProgress(this);
        }
        public override void Update(TameProgress p)
        {
            progress.totalProgress = Time.time;
            progress.progress = Time.time % 1;
            progress.passToChildren = PassTypes.Total;
            //   progress.stop = StopTypes.Never;
            progress.cycle = CycleTypes.Cycle;
        }
    }
    public class TameHand : TameElement
    {
        public static TameHand Hands = new TameHand();
        public Person[] people;
        public TameHand()
        {
            tameType = TameKeys.Head;
            progress = new TameProgress(this);
            progress.passToChildren = PassTypes.Total;
            //    progress.stop = StopTypes.Never;
            progress.cycle = CycleTypes.Cycle;
        }
        public override void Update(TameProgress p)
        {
            progress.totalProgress = Time.time;
            progress.progress = Time.time % 1;
        }
    }
    public class TameCalendar : TameElement
    {
        public TameCalendar()
        {
            tameType = TameKeys.Calendar;
            progress = new TameProgress(this);
            progress.passToChildren = PassTypes.Total;
            //   progress.stop = StopTypes.Never;
            progress.cycle = CycleTypes.Cycle;
        }
        public override void Update(TameProgress p)
        {
            SetSunPosition();
        }
        public void IncreaseHour(int x)
        {
            progress.SetProgress(progress.totalProgress + x / 24f);
        }
        public void IncreaseMonth(int x)
        {
            progress.SetProgress(progress.totalProgress + x / 12f);
        }
        public void SetSunPosition()
        {

        }
    }
    /// <summary>
    /// this class is used to create custom parameters that are linked to the input devices
    /// </summary>
    public class TameCustomValue : TameElement
    {
        public List<TameInputControl> control = new List<TameInputControl>();
        public float[] range = new float[] { 0, 1 };
        public TameCustomValue()
        {
            tameType = TameKeys.Custom;
            progress = new TameProgress(this);

            basis = TrackBasis.Time;
        }
        public override void AssignParent(TameEffect[] all, int index)
        {
            //     Debug.Log("custom parent " + name);
            all[index] = TameEffect.Time();
            all[index].child = this;
        }
         public override void Update()
        {
            //     Debug.Log("custom updating");
            int d = 0;
            foreach (TameInputControl c in control)
                if ((d = c.Hold()) != 0)
                    break;
            if (d != 0)
                progress.SetProgress(progress.totalProgress + d * deltaTime * (progress.manager.Speed == -1 ? 1 : progress.manager.Speed));
            else progress.Retain(deltaTime);
        }
        public static void FromMarker(Markers.MarkerCustom mc, List<TameElement> tes)
        {
            ManifestCustom man = new ManifestCustom();
         //   man.Read(mc.manifestLines.Split(";"), -1);
            TameCustomValue tcv = new TameCustomValue();
            tcv.manifest = man;
            tcv.markerProgress = mc.gameObject.GetComponent<Markers.MarkerProgress>();  
            tcv.name = mc.name;
           
        }
    }
    public enum InputTypes
    {
        VRController = 1,
        GamePad = 2,
        KeyboardMouse = 3
    }
    public enum InputDirections
    {
        MouseButton = 1,
        MouseWheel = 2,
        Key = 0
    }
    public enum InputHoldType
    {
        Key = 1,

        VRScrollLeft = 10,
        VRScrollRight = 11,
        VRTrigger = 12,

        GPSRX = 21,
        GPSRY = 22,
        GPDX = 23,
        GPDY = 24,
        GPTrigger = 25,
        GPShoulder = 26,
    }
   
    
}
