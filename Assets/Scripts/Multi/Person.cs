using UnityEngine;
using RiptideNetworking;
using HandAsset;

namespace Multi
{
    /// <summary>
    /// this class contains all the activities of a person, including their id, and transforms and status of head and hands
    /// </summary>
    public class Person
    {
        public static Person[] people = new Person[10];
        public const int LocalDefault = 16;
        public static Person localPerson;
        public bool initiated = false;
        public bool isLocal = false;
        public GameObject head;
        public HandModel[] hand = new HandModel[2];

        public ushort id;
        public Vector3 headPosition;
        public Vector3 headLocalEuler;
        public Vector3[] position;
        public Vector3[] localEuler;
        public Vector3[] index;
        public Vector3[] middle;
        public bool[] A, B;
        public float[] grip, trigger;
        public Vector2[] stick;

        Vector3[] gripVector;
        private int[] gripIndex;
        private int gripForward;
        private int gripSign;
        private int gripUp;
        public Vector3 switchPosition = Vector3.negativeInfinity;
        private int tick;
        public Person(ushort id)
        {
            this.id = id;
            position = new Vector3[2];
            localEuler = new Vector3[2];
            index = new Vector3[2];
            middle = new Vector3[2];
            A = new bool[2];
            B = new bool[2];
            grip = new float[2];
            trigger = new float[2];
            stick = new Vector2[2];
        }
        public void EncodeInput()
        {
            headPosition = head.transform.position;
            headLocalEuler = head.transform.localEulerAngles;
            for (int i = 0; i < 2; i++)
            {
                position[i] = hand[i].wrist.transform.position;
                localEuler[i] = hand[i].wrist.transform.localEulerAngles;
                index[i] = hand[i].tipPosition[HandModel.Index];
                middle[i] = hand[i].tipPosition[HandModel.Middle];
                A[i] = hand[i].data.A.Status;
                B[i] = hand[i].data.B.Status;
                grip[i] = hand[i].data.grip.Value;
                trigger[i] = hand[i].data.trigger.Value;
                stick[i] = hand[i].data.stick.Vector;            
            }
        }
        public void AddToMessage(Message m)
        {
            m.AddUShort(id);
            m.AddVector3(headPosition);
            m.AddVector3(headLocalEuler);
            for (int i = 0; i < 2; i++)
            {
                m.AddVector3(position[i]);
                m.AddVector3(localEuler[i]);
                m.AddVector3(index[i]);
                m.AddVector3(middle[i]);
                m.AddBool(A[i]);
                m.AddBool(B[i]);
                m.AddFloat(grip[i]);
                m.AddFloat(trigger[i]);
                m.AddVector2(stick[i]);
            }
        }
        public static void Skip(Message m)
        {
            m.GetVector3();
            m.GetVector3();
            for (int i = 0; i < 2; i++)
            {
                m.GetVector3();
                m.GetVector3();
                m.GetVector3();
                m.GetVector3();
                m.GetBool();
                m.GetBool();
                m.GetFloat();
                m.GetFloat();
                m.GetVector2();
            }
        }
        public void ReadMessage(Message m)
        {

            id = m.GetUShort();
            ContinueReadMessage(m);
        }
        public void ContinueReadMessage(Message m)
        {
            headPosition = m.GetVector3();
            headLocalEuler = m.GetVector3();
            for (int i = 0; i < 2; i++)
            {
                position[i] = m.GetVector3();
                localEuler[i] = m.GetVector3();
                index[i] = m.GetVector3();
                middle[i] = m.GetVector3();
                A[i] = m.GetBool();
                B[i] = m.GetBool();
                grip[i] = m.GetFloat();
                trigger[i] = m.GetFloat();
                stick[i] = m.GetVector2();
            }
            if (!isLocal)
            {
                head.transform.position = headPosition;
                head.transform.localEulerAngles = headLocalEuler;
                for (int i = 0; i < 2; i++)
                    hand[i].Update(this);
            }
        }
        /// <summary>
        /// creates the hand model based on the bones in the prefab
        /// </summary>
        /// <param name="fingerHeader"></param>
        public void CreateModel(string fingerHeader)
        {
            head = GameObject.Instantiate(Assets.Script.MainScript.HeadObject);
            head.SetActive(true);
            GameObject g0 = GameObject.Instantiate(Assets.Script.MainScript.localPerson.hand[0].wrist);
            GameObject g1 = GameObject.Instantiate(Assets.Script.MainScript.localPerson.hand[1].wrist);
            hand[0] = new HandModel(null, g0, 0);
            hand[0].GetFingers(fingerHeader);
            hand[1] = new HandModel(null, g1, 1);
            hand[1].GetFingers(fingerHeader);
            hand[1].gripDirection = -1;
        }
        /// <summary>
        /// updates the status of head and hands
        /// </summary>
        public void Update()
        {
            hand[0].Update();
            hand[1].Update();
            UpdateHeadOnly();
        }
        /// <summary>
        ///  updates only the head position
        /// </summary>
        public void UpdateHeadOnly()
        {
            headPosition = head.gameObject.transform.position;
            headLocalEuler = head.gameObject.transform.localEulerAngles;
        }
        /// <summary>
        /// finds the proper grip vectors
        /// </summary>
        /// <param name="t"></param>
        private void GripVectors(Transform t)
        {
           gripVector = new Vector3[]
            {
                gripIndex[0] == 0 ? t.right : (gripIndex[0] == 1 ? t.up : t.forward),
                gripIndex[1] == 0 ? t.right : (gripIndex[1] == 1 ? t.up : t.forward),
                gripIndex[2] == 0 ? t.right : (gripIndex[2] == 1 ? t.up : t.forward)
            };
        }
        /// <summary>
        /// finds the forward and up vector indexes for the hand that fits the grip geometry
        /// </summary>
        /// <param name="area"></param>
        /// <param name="cam"></param>
        private void FU(Tames.TameArea area, Transform cam)
        {
            Transform t = area.relative.transform;
            if (t.localScale.x > t.localScale.y)
                gripIndex = t.localScale.x > t.localScale.z ? new int[] { 0, 1, 2 } : new int[] { 2, 0, 1 };
            else
                gripIndex = t.localScale.y > t.localScale.z ? new int[] { 1, 0, 2 } : new int[] { 2, 0, 1 };
             GripVectors(t);
            int f = 1;
            float d, min = Vector3.Angle(cam.forward, gripVector[1]);
            if (min > (d = Vector3.Angle(cam.forward, -gripVector[1]))) { min = d; f = -1; }
            if (min > (d = Vector3.Angle(cam.forward, gripVector[2]))) { min = d; f = 2; }
            if (min > (d = Vector3.Angle(cam.forward, -gripVector[2]))) { min = d; f = -2; }
            gripForward = f < 0 ? -f : f;
            gripSign = f < 0 ? -1 : 1;
            gripUp = 3 - Mathf.Abs(f);
        }
        /// <summary>
        /// starts gripping by locating the left hand grip around the grip area
        /// </summary>
        /// <param name="area"></param>
        /// <param name="cam"></param>
        public void Grip(Tames.TameArea area, Transform cam)
        {
            hand[0].wrist.SetActive(true);
            hand[0].wrist.transform.parent = null;
            FU(area, cam);
            hand[0].wrist.transform.LookAt(hand[0].wrist.transform.position - gripVector[gripForward] * gripSign, gripVector[gripUp]);
            hand[0].data.grip.Update(1);
            hand[0].Grip(15, 1);
            hand[0].AfterGrip(true);

            Vector3 v = hand[0].wrist.transform.forward * 0.07f + hand[0].wrist.transform.up * 0.02f;
            hand[0].wrist.transform.position = area.relative.transform.position + v;
            hand[0].AfterGrip(true);
    //       Debug.Log("grip: " + area.relative.transform.position.ToString("0.00") + " , " + hand[0].wrist.transform.position.ToString("0.00"));
        }
        /// <summary>
        ///  updates the hand position and rotation based on the changed transform of the grip area
        /// </summary>
        /// <param name="area"></param>
        public void UpdateGrip(Tames.TameArea area)
        {
            GripVectors(area.relative.transform);
            hand[0].wrist.transform.LookAt(hand[0].wrist.transform.position - gripVector[gripForward] * gripSign, gripVector[gripUp]);

            Vector3 v = hand[0].wrist.transform.forward * 0.07f + hand[0].wrist.transform.up * 0.02f;
            hand[0].wrist.transform.position = area.relative.transform.position + v;
            hand[0].AfterGrip(true);
   //         Debug.Log("grip: " + area.relative.transform.position.ToString("0.00") + " , " + hand[0].wrist.transform.position.ToString("0.00"));
        }
        /// <summary>
        /// detaches the hand from the grip area
        /// </summary>
        public void Ungrip()
        {
            hand[0].wrist.SetActive(false);
            hand[0].wrist.transform.parent = null;
            //      hand[0].wrist.transform.LookAt(hand[0].wrist.transform.position + vs[f < 0 ? -f : f] * Mathf.Sign(f), vs[3 - Mathf.Abs(f)]);
            hand[0].data.grip.Update(0);
            hand[0].Grip(15, 0);
            hand[0].AfterGrip(true);
            hand[0].wrist.transform.position = head.transform.position - head.transform.up * 0.7f - head.transform.right * 0.3f;
        }
       
    }
}