using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Markers
{
    public class MarkerEnvironment : MonoBehaviour
    {
        public enum EditorChangerType
        {
            Tint, Exposure
        }
        public EditorChangerType property;
        public enum EditorChangeStep
        {
            Stepped, Gradual, Switch
        }
        public EditorChangeStep mode;
        public float switchValue;
        public string steps;

        public EnvironmentProperty GetProperty()
        {
            switch (property)
            {
                case EditorChangerType.Tint: return EnvironmentProperty.Tint;
                case EditorChangerType.Exposure: return EnvironmentProperty.Exposure;
              
                default: return EnvironmentProperty.Tint; //EditorChangerType.Emissive_V
            }
        }
        public ToggleType GetToggle()
        {
            switch (mode)
            {
                case EditorChangeStep.Gradual: return ToggleType.Gradient;
                case EditorChangeStep.Switch: return ToggleType.Switch;
                default: return ToggleType.Stepped;
            }
        }

    }
}
