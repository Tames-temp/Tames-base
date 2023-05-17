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
            progress.continuity = ContinuityMode.Cycle;
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
            progress.continuity = ContinuityMode.Cycle;
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
            progress.continuity = ContinuityMode.Cycle;
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
            progress.continuity = ContinuityMode.Cycle;
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
         public float[] range = new float[] { 0, 1 };
        public TameCustomValue()
        {
            tameType = TameKeys.Custom;
            progress = new TameProgress(this);

            basis = TrackBasis.Time;
        }
        public override void AssignParent(TameEffect[] all, int index)
        {
            TameEffect ps = GetParent();
             all[index] = ps;
        }
        override public void Update(float p)
        {
            if (progress != null) progress.SetProgress(p);
            //        if (name == "barrier sign") Debug.Log("by number");
        }

        override public void Update(TameProgress p)
        {
            SetByParent(p);
        }
        public override void UpdateManually()
        {
            base.UpdateManually();
        }
        /// <summary>
        /// updates the material by time, overriding <see cref="TameElement.Update"/>
        /// </summary>
        override public void Update()
        {

            if (directProgress >= 0)
                Update(directProgress);
            else
                SetByTime();
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
  
   
    
}
