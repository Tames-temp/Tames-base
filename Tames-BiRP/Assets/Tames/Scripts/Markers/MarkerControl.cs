using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Markers
{
    public enum ControlTarget
    {
        Progress, Activation, Visibility, Alter
    }
    public class MarkerControl : MonoBehaviour
    {
        public ControlTarget type;
        public bool initial = true;
        public GameObject parent;
        public string trigger;
        public InputSetting control;
    }
    /* if control not set
     * * if parent not set
     * * * trigger will apply by main parent
     * * else
     * * * trigger apply for the set parent
     * *
     * * if set, trigger applies on control 
     */

}
