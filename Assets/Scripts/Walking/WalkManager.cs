using System.Collections.Generic;
using UnityEngine;

namespace Walking
{
    public class WalkManager
    {

        /// <summary>
        /// list of faces as walking paths
        /// </summary>
        public List<WalkFace> faces;
        /// <summary>
        /// the last position of the <see cref="Multi.Person"/>'s head according to the walking constraints.
        /// </summary>
        public Vector3 foot;
        public Vector3 push = Vector3.zero;
        /// <summary>
        /// the height difference between the current position of the <see cref="foot"/> and what it should be. This value is used to smoothly transit the height when chaning the walking surface abruptly (e.e. on stairs)
        /// </summary>
        private float heightDifference = 0;
        /// <summary>
        /// maximum height that can be climbed or descended in one step (default value = 0.3 meters). When moving between <see cref="faces"/>, if the landing points on them differ more than this value in height, the movement would not be possible. 
        /// </summary>
        public float maxStepHeight = 0.3f;
        /// <summary>
        /// this constructor is called from <see cref="Tames.TameManifest.LoadManifest"/>. The mannifest line for this type is simple the keyword "walk" (<see cref="TameKeys.Walk"/>) and the comma delimited names of gameobjects whose faces are considered walking surface. The naming follows the logic of <see cref="Tames.TameFinder.Relations"/>. If there are multiple walk manifests in the manifest file, their objects are added to the list.
        /// </summary>
        /// <param name="tgos">the list of all game objects that are defined with keyword walk (see <see cref="TameKeys"/>)</param>
        /// <param name="onlyUpward">if only it should select faces in the objects' meshes whose normal's angle with the Y axis is less than 90 degrees. </param>
        public WalkManager(List<Tames.TameGameObject> tgos, bool onlyUpward)
        {
            faces = new List<WalkFace>();
            foreach (Tames.TameGameObject go in tgos)
                faces.AddRange(WalkFace.GetFaces(go.gameObject, onlyUpward));
        }
        /// <summary>
        /// finds the possibilty of moving to a new point for <see cref="foot"/> and a new eye level. This method is called from <see cref="Move(Vector3, float, float)"/>
        /// </summary>
        /// <param name="targetPoint">the expected point (in world space) to move based on a person's moving direction</param>
        /// <param name="onFace">output in world sapce for the landing of target point on the first <see cref="faces"/> below. Only assigned if walking is possible </param>
        /// <returns>returns true if there is a face under the target point, and the landing point on that face does not differ more than <see cref="maxStepHeight"/> from the current landing point</returns>
        public WalkFace Move(Vector3 targetPoint, out Vector3 onFace)
        {
            float dy;
            float currentDY = -heightDifference;
            float min = float.PositiveInfinity;
            WalkFace wf = null;
            foreach (WalkFace face in faces)
            {
                if (face.On(targetPoint, out dy))
                {
                    if (Mathf.Abs(dy - currentDY) < maxStepHeight)
                        if (dy < min)
                        {
                            wf = face;
                            min = dy;
                        }
                }
            }
            if (min != float.PositiveInfinity)
            {
                onFace = targetPoint - min * Vector3.up;
                return wf;
            }
            else
            {
                onFace = Vector3.zero;
                return null;
            }
        }
        /// <summary>
        /// finds and moves to the next possible point based on a target point.
        /// </summary>
        /// <param name="target">the target point in world space</param>
        /// <param name="speed">maximum speed of height change per second</param>
        /// <param name="dT">the frame delta time</param>
        public void _Move(Vector3 target, float speed, float dT)
        {
            float dy;
            float left = 0;
            if (Move(target, out Vector3 onFace) != null)
            {
                dy = foot.y - onFace.y;
                if (Mathf.Abs(dy) <= dT * speed)
                {
                    left = Mathf.Abs(dy) / speed;
                    heightDifference += 0;
                    foot = onFace;
                }
                else
                {
                    heightDifference += Mathf.Sign(dy) * (Mathf.Abs(dy) - dT * speed);
                    foot = onFace + dT * speed * Vector3.up;
                }
                foot.x = onFace.x;
                foot.z = onFace.z;
            }
            dy = left * speed;
            dy = heightDifference < 0 ? dy : -dy;
            foot += dy * Vector3.up;
            heightDifference += dy;
        }
        public WalkFace Move(Vector3 target)
        {
            float dy;
            WalkFace face;
            if ((face = Move(target, out Vector3 onFace)) != null)
            {
                dy = foot.y - onFace.y;
        //        Debug.Log("WLK: " + onFace.ToString("0.00") + " > " + dy + " ");
                //       if (Mathf.Abs(dy) < heightDifference)
                foot = onFace;
             }
            return face;
        }
        /// <summary>
        /// sets the initial position based on a given point. If the point cannot land on a face, the face with the closest (by <see cref="WalkFace.center"/> to that point is selected instead and the <see cref="foot"/> is set to the <see cref="eyeHeight"/> above the center.
        /// </summary>
        /// <param name="p">the given position in world space</param>
        public WalkFace InitiatePosition(Vector3 p)
        {
            heightDifference = 0;
            WalkFace r = null;
            float dy;
            float min = float.PositiveInfinity;
            foreach (WalkFace face in faces)
                if (face.On(p, out dy))
                    if (dy > 0)
                        if (dy < min)
                        {
                            foot = p - dy * Vector3.up;
                            min = dy;
                            r = face;
                        }
            if (min == float.PositiveInfinity)
                if (faces.Count > 0)
                {
                    Vector3 q, center = faces[0].parent.transform.TransformPoint(faces[0].center);
                    r = faces[0];
                    float d;
                    min = Vector3.Distance(center, p);
                    for (int i = 1; i < faces.Count; i++)
                    {
                        q = faces[i].parent.transform.TransformPoint(faces[i].center);
                        if ((d = Vector3.Distance(p, q)) < min)
                        {
                            min = d;
                            center = q;
                            r=faces[i];
                        }
                    }
                    foot = center;
                }
            return r;
        }
    }
}