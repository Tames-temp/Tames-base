﻿using System.Collections.Generic;
using UnityEngine;
using Multi;

namespace Tames
{
    /// <summary>
    /// the class containing the necessary properties for an interactive light. This class is only concerned with the light component not the game objects. 
    /// </summary>
    public class TameLight : TameElement
    {
        /// <summary>
        /// the Light component, automatically set in <see cref="TameManager"/> class. The component should be assigned to a game object that is a child or descendant of the root object "interactives" in the scene.
        /// </summary>
        public Light light;
      //  public List<TameArea> areas;
        public bool isSwitch = false;
        public Markers.MarkerChanger[] changers = null;
        public TameLight()
        {
            tameType = TameKeys.Light;
        }
       
        /// <summary>
        /// ovverrides <see cref="TameElement.AssignParent"/>
        /// </summary>
        /// <param name="all"></param>
        /// <param name="index"></param>
        public override void AssignParent(TameEffect[] all, int index)
        {
            TameEffect ps = GetParent();
            //        if (name.Equals("blink-blue"))
            //        Debug.Log("custom: blink " + index+ " "+all.Length);
            // for (int i = 0; i < 3; i++)
            all[index] = ps;
        }
        /// <summary>
        /// applies updates after progress is set by other update methods: <see cref="Update()"/> and <see cref="Update(TameProgress)"/>. This uses information read from the manifest file and stored in the manifest headers of the <see cref="manifest"/> field.
        /// </summary>
        private void ApplyUpdate()
        {
            float[] f;
            ManifestLight m = (ManifestLight)manifest;
            //   Debug.Log("coll "+(m == null ? "null" : "not"));
            if (progress != null)
                if (m != null)
                {
                    //           Debug.Log("color updating " + m.properties.Count);
                    foreach (TameChanger tc in m.properties)
                    {
                        f = tc.On(progress.slerpProgress);
                        switch (tc.property)
                        {
                            case MaterialProperty.Glow:
                            case MaterialProperty.Color:
                                light.color = TameColor.ToColor(f);
                                //       if (name == "cooler") Debug.Log("colj: " + name + " " + progress.progress + light.color.ToString());
                                break;
                            case MaterialProperty.Bright: light.intensity = f[0]; break;
                            case MaterialProperty.Focus:
                                light.spotAngle = f[0];
                                if (name == "corlight") Debug.Log(tc.steps[0].value[0] + " " + light.spotAngle); break;
                        }
                    }
                }
        }
        /// <summary>
        /// updates the light element as child of another <see cref="TameElement"/>, overriding <see cref="TameElement.Update(TameProgress)"/>
        /// </summary>
        /// <param name="p"></param>
        override public void Update(float p)
        {
            SetProgress(p);
            ApplyUpdate();
        }
        override public void Update(TameProgress p)
        {
            SetByParent(p);
            ApplyUpdate();
        }
        public override void UpdateManually()
        {
            base.UpdateManually();
            ApplyUpdate();
        }
        /// <summary>
        /// updates the light element by time, overriding <see cref="TameElement.Update"/>
        /// </summary>
        override public void Update()
        {
            SetByTime();
            ApplyUpdate();
        }
    }

}