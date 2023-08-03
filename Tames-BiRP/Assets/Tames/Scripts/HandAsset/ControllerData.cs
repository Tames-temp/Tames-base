using Multi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;
namespace HandAsset
{
    public class ControllerData
    {

        public UnityEngine.XR.Interaction.Toolkit.XRController controller = null;
        private InputDevice input;
        public GameObject gameObject;
        public Transform transform { get { return gameObject.transform; } }
        public ControllerFeature trigger;
        public ControllerFeature grip;
        public ControllerFeature A;
        public ControllerFeature B;
        public ControllerFeature stick;
        public ControllerFeature thumb;
        private Vector3 position = Vector3.zero;
        private Vector3 lastPosition = Vector3.zero;
        public Vector3 LastPosition { get { return lastPosition; } }
        public Vector3 DeltaPosition { get { return position - lastPosition; } }
        private Quaternion rotation = Quaternion.identity;
        private Quaternion lastRotation = Quaternion.identity;
        public Quaternion Rotation { get { return rotation; } }
        public Quaternion LastRotation { get { return lastRotation; } }
        public Quaternion DeltaRotation { get { return rotation * Quaternion.Inverse(lastRotation); } }



        public ControllerData(GameObject control)
        {
            gameObject = control;
            if (control != null)
                controller = control.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRController>();
            //Debug.Log("CD: controller " + (controller.name));
            trigger = new ControllerFeature(ControllerFeature.FLOAT, 0.9f);
            grip = new ControllerFeature(ControllerFeature.FLOAT, 0.9f);
            A = new ControllerFeature(ControllerFeature.BOOL);
            B = new ControllerFeature(ControllerFeature.BOOL);
            stick = new ControllerFeature(ControllerFeature.VECTOR, 0.1f);
            thumb = new ControllerFeature(ControllerFeature.FLOAT, 0.1f);
        }
        public void Update()
        {
            if (controller == null) return;
            lastPosition = position;
            position = gameObject.transform.position;
            lastRotation = rotation;
            rotation = gameObject.transform.rotation;
            input = controller.inputDevice;
            // Debug.Log("CD: input " + (input.name));

            bool b;
            float f;
            Vector2 v;
            input.TryGetFeatureValue(CommonUsages.grip, out f); grip.Update(f);
            //   Debug.Log("data update: "+f);
            input.TryGetFeatureValue(CommonUsages.trigger, out f); trigger.Update(f);
            input.TryGetFeatureValue(CommonUsages.primaryButton, out b); A.Update(b);
            input.TryGetFeatureValue(CommonUsages.secondaryButton, out b); B.Update(b);
            input.TryGetFeatureValue(CommonUsages.primary2DAxis, out v); stick.Update(v);
            //   Debug.Log("CD " + v.ToString("0.000"));
            //         input.TryGetFeatureValue(CommonUsages.primaryTouch, out f); thumb.Update(f);
        }
        public void Import(Person pd, int i)
        {
            // if (controller == null) return;
            lastPosition = position;
            position = pd.position[i];
            lastRotation = rotation;
            rotation = Quaternion.Euler(pd.localEuler[i]);
            grip.Update(pd.grip[i]);
            trigger.Update(pd.trigger[i]);
            A.Update(pd.A[i]);
            B.Update(pd.B[i]);
            stick.Update(pd.stick[i]);
        }
    }
}
