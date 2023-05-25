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
        public static List<TameInputControl> GetControl(ManifestHeader header, int start)
        {
            List<TameInputControl> r = new List<TameInputControl>();
            for (int i = start; i < header.items.Count; i++)
            {
                TameInputControl tci = TameInputControl.ByStringDuo(header.items[i]);
                if (tci != null) r.Add(tci);
            }
            return r;
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
             TameCustomValue tcv = new TameCustomValue();
            tcv.markerProgress = mc.gameObject.GetComponent<Markers.MarkerProgress>();  
            tcv.name = mc.name;
           
        }
    }
  
   
    
}
