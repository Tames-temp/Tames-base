using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Walking
{
    public class MoveGesture
    {
        public GameObject person;
        public Transform[] mono;
        public Transform[,] sym;
        public Vector3[] rotMono;
        public Vector3[,] rotSym;

        public const int Hip = 0;
        public const int Waist = 1;
        public const int Chest = 2;
        public const int Neck = 3;
        public const int Head = 4;
        public const int Shoulder = 0;
        public const int Arm = 1;
        public const int Elbow = 2;
        public const int Wrist = 3;
        public const int Thigh = 4;
        public const int Sheen = 5;
        public const int Foot = 6;
        public const int Toes = 7;

        private float tiptoeHeight;
        public MoveGesture(GameObject model)
        {
            person = model;
            mono = new Transform[5];
            rotMono = new Vector3[5];
            Transform a = person.transform.Find("Armature");
            mono[Hip] = a.transform.Find("hip");
            mono[Waist] = mono[Hip].Find("waist");
            mono[Chest] = mono[Waist].Find("chest");
            mono[Neck] = mono[Chest].Find("neck");
            mono[Head] = mono[Neck].Find("head");
            for (int i = 0; i < 5; i++) rotMono[i] = mono[i].localEulerAngles;
            sym = new Transform[2, 8];
            sym[0, Shoulder] = mono[Chest].Find("shoulderL");
            sym[1, Shoulder] = mono[Chest].Find("shoulderR");
            sym[0, Arm] = sym[0, Shoulder].Find("armL");
            sym[1, Arm] = sym[1, Shoulder].Find("armR");
            sym[0, Elbow] = sym[0, Arm].Find("elbowL");
            sym[1, Elbow] = sym[1, Arm].Find("elbowR");
            sym[0, Wrist] = sym[0, Elbow].Find("wristL");
            sym[1, Wrist] = sym[1, Elbow].Find("wristR");
            sym[0, Thigh] = mono[Hip].Find("thighL");
            sym[1, Thigh] = mono[Hip].Find("thighR");
            sym[0, Sheen] = sym[0, Thigh].Find("sheenL");
            sym[1, Sheen] = sym[1, Thigh].Find("sheenR");
            sym[0, Foot] = sym[0, Sheen].Find("footL");
            sym[1, Foot] = sym[1, Sheen].Find("footR");
            sym[0, Toes] = sym[0, Foot].Find("toesL");
            sym[1, Toes] = sym[1, Foot].Find("toesR");
            rotSym = new Vector3[2, 8];
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 8; j++)
                    rotSym[i, j] = sym[i, j].localEulerAngles;
        }
        public void LiftLeg(int index, float angle, float kneeStiff, float footStiff)
        {
            sym[index, Thigh].transform.localRotation = Quaternion.Euler(rotSym[index, Thigh] + Vector3.right * angle);
            sym[index, Sheen].transform.localRotation = Quaternion.Euler(rotSym[index, Sheen] + Vector3.right * 80 * kneeStiff);
            sym[index, Foot].transform.localRotation = Quaternion.Euler(rotSym[index, Thigh] + Vector3.right * 30 * footStiff);
        }
        public void Tiptoe(int index, float amount)
        {

        }
        public void Walk(int index, float stepLength, float prog)
        {
            int n = (int)(prog / 2);


        }
    }
}
