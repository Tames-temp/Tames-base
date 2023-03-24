using System.Collections.Generic;
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
        /// overrides the <see cref="TameElement.GetParent"/>.
        /// </summary>
        /// <returns>the update parent of the light, hence only the first element of the returned array is assigned</returns>
        private TameEffect GetEffect(Person headOwner, Person handOwner, TameAreaTrack tat)
        {
            TameEffect r = null;
            //  if (name == "door1") Debug.Log("enfo z:" + basis + " "+parents.Count);
            if (TrackBasis.Time == basis)
                r = TameEffect.Time();
            else if (parents.Count > 0) 
                r = parents[0];
            return r;
        }
        override public TameEffect GetParent()
        {
            TameEffect r = null;
            int closest;
            float d;
            float dis;
            float min = float.PositiveInfinity;
            bool[] set = new bool[] { false, false, false };
            TameArea ti;
            Person pe;
            //   Debug.Log("before error 1");

            //    Debug.Log("before error 2");
            if (isSwitch)
            {
                //      Debug.Log("before error 2.1 "+name +" "+areas.Count);
                if (areas[0].geometry == InteractionGeometry.Remote)
                {
                    //         Debug.Log("before error 2.15 "+ areas[0].key);
                    if (TameInputControl.checkedKeys[areas[0].key].wasPressedThisFrame)
                    {
                        areas[0].Switch(true);
                        r = TameEffect.Time();
                        r.child = this;
                        changingDirection = areas[0].switchDirection;
                    }
                }
                else
                {
                    //        Debug.Log("before error 2.2");
                    int sd = TameArea.CheckSwitch(areas);
                    r = TameEffect.Time();
                    if (sd != TameArea.NotSwitched)
                        if (changingDirection != sd)
                        {
                            Debug.Log("direction " + changingDirection + " > " + sd);
                            changingDirection = sd;
                        }
                    r.child = this;
                }
                //          Debug.Log("before error 2.3");
                return r;
            }
            //      Debug.Log("before error 3");
            Person headOwner = null, handOwner = null;
            Vector3 closestPosition = Vector3.positiveInfinity;
            //    Debug.Log("name = " + areas.Count);
            TameAreaTrack tat = areas.Count > 0 ? TameArea.TrackWithAreas(areas, owner.transform.position) : TameArea.Track(owner.transform.position);
            byte tp = 0;
            if (tat.person >= 0)
            {
                handOwner = tat.person == Person.LocalDefault ? Person.localPerson : Person.people[tat.person];
                //     tp = TrackBasis.Hand;
            }
            if (tat.head >= 0)
            {
                headOwner = tat.head == Person.LocalDefault ? Person.localPerson : Person.people[tat.head];
                //    tp = TrackBasis.Head;
            }

            changingDirection = tat.direction;
            if (changingDirection != 0)
            {
                r = GetEffect(headOwner, handOwner, tat);
                if (r != null)
                {
                    r.direction = tat.direction;
                    r.child = this;
                }
            }
            //      Debug.Log("before error 4");
            return r;
        }
        override public void CleanAreas()
        {
            isSwitch = false;
            int retain = 1;
          
                foreach (TameArea ti in areas)
                    if (ti.geometry == InteractionGeometry.Remote)
                    {
                        retain = 1;
                        isSwitch = true;
                        break;
                    }
                    else if (TameArea.IsSwitch(ti.mode))
                    {
                        retain = 2;
                        isSwitch = true;
                        break;
                    }

            if ( isSwitch)
            {
                for (int i = areas.Count - 1; i >= 0; i--)
                    if ((areas[i].geometry != InteractionGeometry.Remote) || ((retain == 2) && (!TameArea.IsSwitch(areas[i].mode))))
                        areas.RemoveAt(i);
                parents.Clear();
                     //       Debug.Log(name + " " + handle.DoesSlide + " " + handle.RotationType);
            }
        }
        public TameEffect GetParentOld()
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
            ManifestLight m = (ManifestLight)manifest;
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