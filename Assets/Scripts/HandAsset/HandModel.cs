using Assets.Script;
using Tames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;
using Multi;
namespace HandAsset
{
    public class HandModel
    {
        public int handIndex;
        public ControllerData data;
        public int gripDirection;
        public int gripAxis;
        public int tipAxis;

        private float[,] initialAngle;
        public float[] rotationMax;
        public GameObject wrist;
        public GameObject[,] joint;
        public GameObject palmObject;
        public GameObject[] tip;
        public Vector3 palm;
        public Vector3 gripCenter, lastGripCenter;
        public Vector3 tipVector;
        public InputDevice input;
        //    public UnityEngine.XR.Interaction.Toolkit.XRController controller;
        public Vector3[] tipPosition, lastTipPosition;
        public float fingerAngle = 0;
        public float indexAngle = 0;
        public const int Pinky_ID = 1;
        public const int Ring_ID = 2;
        public const int Middle_ID = 4;
        public const int Index_ID = 8;
        public const int Thumb_ID = 16;
        public const int Pinky = 0;
        public const int Ring = 1;
        public const int Middle = 2;
        public const int Index = 3;
        public const int Thumb = 4;

        public const float GripThreshold = 0.6f;
        public HandModel(GameObject control, GameObject model, int index)
        {
            data = new ControllerData(control);
            //     this.control.gameObject = control;
            //           gameObject = control;
            wrist = model;
            handIndex = index;
            Init();
        }
        private void Init()
        {
            //       controller = gameObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.XRController>();
            tipPosition = new Vector3[5];
            tipAxis = 0;
            lastTipPosition = new Vector3[5];
            joint = new GameObject[5, 3];
            tip = new GameObject[5];
            initialAngle = new float[5, 3];
            rotationMax = new float[] { 70, 55, 40 };
            gripCenter = Vector3.zero;
            lastGripCenter = Vector3.zero;
            gripDirection = 1;
            gripAxis = 2;
        }
        public void GetFingers(string fingerHeader)
        {
            if (wrist == null) return;
            float d;
            palmObject = Identifier.DescendentStartsWith(wrist, "palm");
            for (int f = 0; f < 5; f++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (j == 0)
                        joint[f, j] = Identifier.DescendentStartsWith(wrist, fingerHeader + (f + 1));
                    else
                        joint[f, j] = joint[f, j - 1] != null ? joint[f, j - 1].transform.GetChild(0).gameObject : null;
                    initialAngle[f, j] = joint[f, j] != null ? joint[f, j].transform.localEulerAngles[gripAxis] : 0;
                }
                tip[f] = joint[f, 2] != null ? joint[f, 2].transform.GetChild(0).gameObject : null;
            }
            for (int f = 0; f < 5; f++)
                lastTipPosition[f] = tipPosition[f] = tip[f] != null ? tip[f].transform.position : Vector3.positiveInfinity;
            if (palmObject == null)
            {
                palmObject = new GameObject();
                palmObject.transform.parent = wrist.transform;
                float max = float.NegativeInfinity;
                int index = -1;
                for (int i = 0; i < 5; i++)
                    if (joint[i, 0] != null)
                        if ((d = Vector3.Distance(joint[i, 0].transform.position, wrist.transform.position)) > max)
                        {
                            max = d;
                            index = i;
                        }
                palmObject.transform.position = (joint[index, 0].transform.position + wrist.transform.position) / 2;
            }
        }

        public void Update(Person person = null)
        {
            if (person != null)
            {
                data.Import(person, handIndex);
                wrist.transform.position = person.position[handIndex];
                wrist.transform.localEulerAngles = person.localEuler[handIndex];
                lastTipPosition[Index] = tipPosition[Index];
                tipPosition[Index] = person.index[handIndex];
                lastTipPosition[Middle] = tipPosition[Middle];
                tipPosition[Middle] = person.middle[handIndex];

                Grip(Index_ID, data.trigger.Value);
                Grip(Pinky_ID + Ring_ID + Middle_ID, data.grip.Value);
                Grip(Thumb_ID, 0);
                AfterGrip(false);
            }
            else
            { 
                if (data.controller == null)
                    return;
                data.Update();
                wrist.transform.position = data.transform.position;
                wrist.transform.LookAt(data.transform.position + data.transform.up, data.transform.right * (-gripDirection));

                //  Debug.Log(data.grip.Value);
                Grip(Index_ID, data.trigger.Value);
                Grip(Pinky_ID + Ring_ID + Middle_ID, data.grip.Value);
                Grip(Thumb_ID, 0);
                AfterGrip(true);
            }
            //        if (reverseFactor < 0)
            //          Debug.Log("HM " + lastGripCenter.ToString("0.00000") + gripCenter.ToString("0.00000"));

        }
        public void AfterGrip(bool calcFingers)
        {
            if (calcFingers)
                for (int f = 0; f < 5; f++)
                {
                    lastTipPosition[f] = tipPosition[f];
                    tipPosition[f] = tip[f] != null ? tip[f].transform.position : Vector3.positiveInfinity;
                }
            palm = palmObject.transform.position;
            lastGripCenter = gripCenter;
            gripCenter = (tipPosition[Middle] - palm) * 0.6f + palm;
            tipVector = tipAxis == 0 ? tip[Middle].transform.right : (tipAxis == 1 ? tip[Middle].transform.up : tip[Middle].transform.forward);

        }
        public virtual void CreateHand()
        {


        }
        public void Grip(int fingers, float grip)
        {
            Vector3 lea;
            grip *= gripDirection;
            for (int i = 0; i < 4; i++)
                if ((fingers & (1 << i)) > 0)
                    for (int j = 0; j < 3; j++)
                        if (joint[i, j] != null)
                        {
                            lea = joint[i, j].transform.localEulerAngles;
                            joint[i, j].transform.localEulerAngles = new Vector3
                                (
                                gripAxis == 0 ? initialAngle[i, j] + rotationMax[j] * grip : lea.x,
                                gripAxis == 1 ? initialAngle[i, j] + rotationMax[j] * grip : lea.y,
                                gripAxis == 2 ? initialAngle[i, j] + rotationMax[j] * grip : lea.z
                                );
                            ///                 Debug.Log(grip + " " + joint[i, j].transform.localEulerAngles.ToString("0.000") + " " + lea.ToString("0.000"));
                        }
        }


        public void AddToMessage(RiptideNetworking.Message m)
        {
            m.AddVector3(wrist.transform.position);
            m.AddVector3(wrist.transform.localEulerAngles);
            m.AddVector3(gripCenter);
            m.AddVector3(tipPosition[Index]);
            m.AddVector3(tipPosition[Middle]);
            m.AddFloat(data.grip.Value);
            m.AddFloat(data.trigger.Value);
            m.AddBool(data.A.Status);
            m.AddBool(data.B.Status);
        }

    }
}