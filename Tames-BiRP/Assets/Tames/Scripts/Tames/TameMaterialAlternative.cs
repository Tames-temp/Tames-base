using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Markers;

namespace Tames
{
    public class TameMaterialAlternative
    {
        /// <summary>
        /// The alternative inventory
        /// </summary>
        public Material[] alternatives;
        /// <summary>
        /// The changed material. The properties of this material are copied from the current alternative.  
        /// </summary>
        public Material target;
        /// <summary>
        /// The index of the current alternative.
        /// </summary>
        public int current = -1;
        /// <summary>
        /// The index of the initial alternative. This is set by <see cref="MarkerAlterMaterial.initial"/>.
        /// </summary>
        public int initial = -1;
        public InputSetting control;
        /// <summary>
        /// Change the current alternative to the next one in the queue.
        /// </summary>
        public MarkerAlterMaterial marker = null;
        public void GoNext()
        {
            if (current >= 0)
            {
                if (alternatives.Length > 0)
                    current = (current + 1) % alternatives.Length;
            }
            else if (alternatives.Length > 0) current = 0;
            Progress();
        }
        /// <summary>
        /// Change the current alternative to the previous one in the queue.
        /// </summary>
        public void GoPrevious()
        {
            if (current >= 0)
            {
                if (alternatives.Length > 0)
                    current = (current + alternatives.Length - 1) % alternatives.Length;
            }
            else if (alternatives.Length > 0) current = 0;
            Progress();
        }
        /// <summary>
        /// Sets the <see cref="initial"/> alternative.
        /// </summary>
        /// <param name="i"></param>
        public void SetInitial(int i)
        {
            if (alternatives.Length > 0)
                current = i;
            Progress();
        }
        /// <summary>
        /// Updates the material (by copying the current alternative's property)
        /// </summary>
        public void Progress()
        {
            if (current >= 0)
                target.CopyPropertiesFromMaterial(alternatives[current]);
        }
        /// <summary>
        /// Finds change direction (if any) based on inputs (<see cref="back"/> and <see cref="forth"/>) and then calls <see cref="Progress"/>. 
        /// </summary>
        public void Update()
        {
            int d = control.CheckDualPressed(marker.gameObject);
            if (d != 0) Debug.Log("checking " + d);
            if (d < 0) GoPrevious();
            else if (d > 0) GoNext();
        }
        /// <summary>
        /// Sets the inputs for a given input set"/>
        /// </summary>
        /// <param name="keys">a string representing the inputs</param>
        /// <param name="backOrFoth">whether the input set is <see cref="back"/>(true) or <see cref="forth"/> (false).</param>
        public void SetKeys(InputSetting keys)
        {
            control = keys;
            control.AssignControl(InputSetting.ControlTypes.DualPress);
            Debug.Log("keys: " + control.back.Count+ " "+control.key+ " "+control.back[0].hold);
       
        }

        /// <summary>
        /// Finds and creates all alternatives in the scene.
        /// </summary>
        /// <param name="tgos">The list of <see cref="TameGameObject"/>s extracted in <see cref="TameManager"/></param>
        /// <returns></returns>
        public static List<TameMaterialAlternative> GetAlternatives(List<TameGameObject> tgos)
        {
            List<TameMaterialAlternative> tmas = new List<TameMaterialAlternative>();
            TameMaterialAlternative tma;
            MarkerAlterMaterial mam;
            for (int i = 0; i < tgos.Count; i++)
                if ((mam = tgos[i].gameObject.GetComponent<MarkerAlterMaterial>()) != null)
                {
                    tma = new() { marker = mam };
                    tma.SetKeys(mam.control);
                    tma.alternatives = mam.alternatives;
                    tma.target = mam.applyTo;
                    if (mam.initial == null)
                        tma.initial = tma.alternatives.Length > 0 ? 0 : -1;
                    else
                        for (int j = 0; j < tma.alternatives.Length; j++)
                            if (tma.alternatives[j] == mam.initial)
                                tma.initial = j;

                    for (int j = 0; j < tma.alternatives.Length; j++)
                        if (tma.alternatives[j] == mam.applyTo)
                        {
                            MaterialReference mr = MaterialReference.AddToReference(tma.alternatives[j]);
                            tma.target = mr.clone;
                        }
                    tma.SetInitial(tma.initial);
                    tmas.Add(tma);
                }
            return tmas;
        }
    }
}
