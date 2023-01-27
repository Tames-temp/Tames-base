using System.Collections.Generic;
using UnityEngine;

namespace Tames
{
    /// <summary>
    /// the class containing the necessary properties for an interactive light. This class is only concerned with the light component not the game objects. 
    /// </summary>
    public class TameLight : TameElement
    {
        /// <summary>
        /// the Light component, automatically set in <see cref="TameManifest"/> class. The component should be assigned to a game object that is a child or descendant of the root object "interactives" in the scene.
        /// </summary>
        public Light light;
        public TameLight()
        {
            tameType = TameKeys.Light;
        }
        /// <summary>
        /// overrides the <see cref="TameElement.GetParent"/>.
        /// </summary>
        /// <returns>the update parent of the light, hence only the first element of the returned array is assigned</returns>
        override public TameEffect GetParent()
        {

            TameEffect r = null;
            //      if (name.Equals("illum"))
            //          Debug.Log("mix: assign " + basis[0] + " " + updateParents.Count);
            if (basis == TrackBasis.Time)
                r = TameEffect.Time();
            else if (parents.Count > 0)
                r = parents[0];
            if (r != null) r.child = this;
          //  Debug.Log("inlight: " + basis);
            return r;
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
            TameLightManifest m = (TameLightManifest)manifest;
            //   Debug.Log("coll "+(m == null ? "null" : "not"));
            if (progress != null)
                if (m != null)
                {
                    //           Debug.Log("color updating " + m.properties.Count);
                    foreach (TameChanger tc in m.properties)
                    {
                        f = tc.On(progress.progress);
                        switch (tc.property)
                        {
                            case MaterialProperty.Glow:
                            case MaterialProperty.Color:
                                light.color = TameColor.ToColor(f);
                             //   Debug.Log("colj: " +name+ " "+progress.progress + light.color.ToString());
                                break;
                            case MaterialProperty.Bright: light.intensity = f[0]; break;
                            case MaterialProperty.Focus: light.spotAngle = f[0]; break;
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