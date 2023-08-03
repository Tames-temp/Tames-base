
using Multi;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tames
{
    public class TameCamera
    {
        public static GameObject gameObject = null;
        public static Transform cameraTransform;
        private static Transform onWalk = null;
        public static Camera camera;
        private static int turningDirection;
        private static int tiltingDirection;
        private static int movingDirection;
        private static float walkingSpeed = 1f;
        private static float walkingMode = 1f;
        private static float rotationSpeed = 100f; // degree/s
        private static float tiltingSpeed = 70f; // degree/s
        private static bool moveByObject = false;
        private static float currentTilt = 0;
        public static Vector3 eyeHeight = 1.6f * Vector3.up;
        public static Walking.WalkFace currentFace = null;

        public static int CKey, XKey, ZKey;
        public static int currentObject = -1;
        private static List<string> names = new List<string>();
        public static GameObject[] gameObjects;
        public static List<byte> feature = new List<byte>();
        public static void ToggleCamera()
        {
            if (currentObject == -1) currentObject = gameObjects.Length != 0 ? 0 : -1;
            else if (currentObject == gameObjects.Length - 1) currentObject = -1;
            else currentObject++;
            Debug.Log("RC tog " + currentObject);
        }
        public static void ReadCamera(List<TameGameObject> tgos)
        {
            Markers.MarkerCarrier mc;
            List<GameObject> objects = new List<GameObject>();
            foreach (TameGameObject tgo in tgos)
                if ((mc = tgo.gameObject.GetComponent<Markers.MarkerCarrier>()) != null)
                {
                    objects.Add(tgo.gameObject);
                    feature.Add((byte)((mc.rotation ? 2 : 0) + (mc.position ? 1 : 0)));
                }
            gameObjects = objects.ToArray();
            //    Debug.Log("RC: " + gameObjects.Length);
        }
        public static int ReadCamera(ManifestHeader mh, string[] lines, int index)
        {
            string what;
            byte b = 0;
            string name = "";
            if (mh.items.Count > 1)
            {
                what = mh.items[0].ToLower();
                name = mh.items[1];
                for (int i = 2; i < mh.items.Count; i++)
                    name += " " + mh.items[1].ToLower();
                names.Add(name);
                switch (what)
                {
                    //       case "tilt": fs[0] = true; break;
                    case "both":
                    case "all": b = 3; break;
                    case "move": b = 1; break;
                    case "turn": b = 2; break;
                }
                feature.Add(b);
            }
            return index;
        }

        public static void AssignCamera(List<TameGameObject> tgos)
        {
            return;
            TameFinder finder = new TameFinder();
            GameObject gameObject = null;
            List<GameObject> objects = new List<GameObject>();
            List<byte> what = new List<byte>();
            for (int i = 0; i < names.Count; i++)
            {
                finder.header = new ManifestHeader() { items = new List<string>() { names[i] } };
                finder.PopulateObjects(tgos);
                if (finder.objectList.Count > 0)
                    gameObject = finder.objectList[0].gameObject;
                finder.objectList.Clear();
                //    camera = new GameObject();
                if (gameObject != null)
                {
                    objects.Add(gameObject);
                    what.Add(feature[i]);
                }
            }
            feature = what;
            gameObjects = objects.ToArray();
        }
        public static int ReadEye(ManifestHeader mh, int index)
        {

            return index;
        }
        private static bool CheckDistanceAndAngle(GameObject owner, Vector3 position, Vector3 forward, float activeDistance, float activeAngle, Markers.InputSetting.Axis axis)
        {
            Vector3 u = owner.transform.position - position;
            if (activeAngle > 0 || activeDistance > 0)
                if (axis != Markers.InputSetting.Axis.None)
                    if (Vector3.Angle(u, forward) >= 90) return false;
            if (activeDistance > 0)
            {
                if (Vector3.Distance(owner.transform.position, position) > activeDistance) return false;
                if (activeAngle > 0)
                {
                    switch (axis)
                    {
                        case Markers.InputSetting.Axis.X: return activeAngle > Vector3.Angle(-owner.transform.right, forward);
                        case Markers.InputSetting.Axis.Y: return activeAngle > Vector3.Angle(-owner.transform.up, forward);
                        case Markers.InputSetting.Axis.Z: return activeAngle > Vector3.Angle(-owner.transform.forward, forward);
                        case Markers.InputSetting.Axis.NegX: return activeAngle > Vector3.Angle(owner.transform.right, forward);
                        case Markers.InputSetting.Axis.NegY: return activeAngle > Vector3.Angle(owner.transform.up, forward);
                        case Markers.InputSetting.Axis.NegZ: return activeAngle > Vector3.Angle(owner.transform.forward, forward);
                        default: return activeAngle > Vector3.Angle(owner.transform.position - position, forward);
                    }
                }
                else return true;
            }
            else if (activeAngle > 0)
                switch (axis)
                {
                    case Markers.InputSetting.Axis.X: return activeAngle > Vector3.Angle(-owner.transform.right, forward);
                    case Markers.InputSetting.Axis.Y: return activeAngle > Vector3.Angle(-owner.transform.up, forward);
                    case Markers.InputSetting.Axis.Z: return activeAngle > Vector3.Angle(-owner.transform.forward, forward);
                    case Markers.InputSetting.Axis.NegX: return activeAngle > Vector3.Angle(owner.transform.right, forward);
                    case Markers.InputSetting.Axis.NegY: return activeAngle > Vector3.Angle(owner.transform.up, forward);
                    case Markers.InputSetting.Axis.NegZ: return activeAngle > Vector3.Angle(owner.transform.forward, forward);
                    default: return activeAngle > Vector3.Angle(owner.transform.position - position, forward);
                }
            else return true;
        }
        public static bool CheckDistanceAndAngle(GameObject owner, float activeDistance, float activeAngle, Markers.InputSetting.Axis axis)
        {
            return CheckDistanceAndAngle(owner, cameraTransform.position, cameraTransform.forward, activeDistance, activeAngle, axis);
        }
        public static bool CheckDistanceAndAngle(GameObject owner, Person person, float activeDistance, float activeAngle, Markers.InputSetting.Axis axis)
        {
            return CheckDistanceAndAngle(owner, person.headPosition, person.headForward, activeDistance, activeAngle, axis);
        }
        public static void SetFirstFace(Walking.WalkFace f)
        {
            currentFace = f;
            if (f != null)
            {
                onWalk = new GameObject().transform;
                onWalk.parent = currentFace.control.owner.transform;
                onWalk.position = TameManager.walkManager.foot + eyeHeight;
                onWalk.rotation = cameraTransform.rotation;
                onWalk.gameObject.SetActive(false);
            }
            else onWalk = cameraTransform;
        }
        private static void SetMovingDirection()
        {
            movingDirection = 0;
            if (InputBasis.move == InputBasis.Button)
            {
                if (TameInputControl.keyMap.forward) movingDirection = 1;
                if (TameInputControl.keyMap.back) movingDirection = -1;
                if (TameInputControl.keyMap.shift) walkingMode = 2f; else walkingMode = 1f;

                if ((movingDirection == 0) && (Gamepad.current != null))
                {
                    float y = TameInputControl.keyMap.gpMap.stick[0].y;
                    movingDirection = y < -0.5f ? -1 : (y > 0.5f ? 1 : 0);
                }

                if (TameInputControl.keyMap.vrMap.A[0]) movingDirection = 1;
                if (TameInputControl.keyMap.vrMap.B[0]) movingDirection = -1;
            }
        }
        private static void SetTurningDirection()
        {
            turningDirection = 0;
            if (InputBasis.turn == InputBasis.Button)
            {

                if (TameInputControl.keyMap.left) turningDirection = -1;
                if (TameInputControl.keyMap.right) turningDirection = 1;

                if ((turningDirection == 0) && (Gamepad.current != null))
                {
                    float x = TameInputControl.keyMap.gpMap.stick[1].x;
                    turningDirection = x < -0.5f ? -1 : (x > 0.5f ? 1 : 0);
                }
            }
        }
        private static void SetTiltAngle()
        {
            float y;
            tiltingDirection = 0;
            if (InputBasis.tilt == InputBasis.Button)
            {
                if (TameInputControl.keyMap.up) tiltingDirection = 1;
                if (TameInputControl.keyMap.down) tiltingDirection = -1;
                if (tiltingDirection == 0)
                    if (!(TameInputControl.keyMap.gpMap.hold[4] || TameInputControl.keyMap.gpMap.hold[5]))
                    {
                        y = TameInputControl.keyMap.gpMap.stick[1].y;
                        tiltingDirection = y < -0.5f ? -1 : (y > 0.5f ? 1 : 0);
                    }
                currentTilt += tiltingDirection * tiltingSpeed * TameElement.originalDelta;
                if (currentTilt > 80) currentTilt = 80;
                if (currentTilt < -80) currentTilt = -80;
            }
            else if (InputBasis.tilt == InputBasis.Mouse)
            {
                y = TameInputControl.keyMap.mouse.y;
                if (Mathf.Abs(y) < 0.2f) y = 0; else y = Mathf.Sign(y) * (Mathf.Abs(y) - 0.2f) / 0.8f;
                currentTilt = y * 80;
            }
        }
        public static void UpdateCamera()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.numpadMinusKey.wasPressedThisFrame)
                    TameElement.ScaleFactor--;
                if (Keyboard.current.numpadPlusKey.wasPressedThisFrame)
                    TameElement.ScaleFactor++;

            }
            Vector3 p = onWalk.position;
            float x;
            float y;
            if (TameInputControl.keyMap.pressed[XKey])
                InputBasis.ToggleNext();
            if (TameInputControl.keyMap.gpMap.pressed[8] && TameInputControl.keyMap.gpMap.hold[4] && TameInputControl.keyMap.gpMap.hold[5])
                InputBasis.ToggleNext();

            if (TameInputControl.keyMap.pressed[CKey])
                ToggleCamera();
            if (TameInputControl.keyMap.gpMap.pressed[8] && TameInputControl.keyMap.gpMap.hold[4] && (!TameInputControl.keyMap.gpMap.hold[5]))
                InputBasis.ToggleNext();
            if (TameInputControl.keyMap.info)
            {
                InfoUI.InfoControl.InfoVisibility = !InfoUI.InfoControl.InfoVisibility;
                if (!InfoUI.InfoControl.InfoVisibility)
                    foreach (InfoUI.InfoControl info in TameManager.info)
                        info.Visible = false;

            }
            //  Debug.Log(TameManager.info.Count);
            if (InfoUI.InfoControl.InfoVisibility)
                foreach (InfoUI.InfoControl info in TameManager.info)
                {
                    info.Visible = info.InView();
                    info.Update();
                }
            if (Mouse.current != null)
            {
                float ms = Mouse.current.scroll.y.ReadValue();
                CoreTame.WheelDirection = ms > 0 ? 1 : (ms < 0 ? -1 : 0);
                CoreTame.MouseButton = 0;
                if (Mouse.current.leftButton.isPressed) CoreTame.MouseButton = -1;
                if (Mouse.current.rightButton.isPressed) CoreTame.MouseButton = 1;
            }

            SetMovingDirection();
            SetTiltAngle();
            SetTurningDirection();

            Vector3 fwd;
            Vector3 flat;
            Vector3 moving = Vector3.zero;
            if (currentObject < 0)
            {
                moveByObject = false;
                fwd = onWalk.forward;
                flat = new Vector3(fwd.x, 0, fwd.z);
                if (InputBasis.turn != InputBasis.VR)
                {
                    flat = Utils.Rotate(flat, Vector3.zero, Vector3.up, rotationSpeed * TameElement.originalDelta * turningDirection);
                    moving = flat.normalized * movingDirection * walkingSpeed * walkingMode * TameElement.originalDelta;
                    flat.y = flat.magnitude * Mathf.Tan(currentTilt * Mathf.Deg2Rad);
                    onWalk.forward = flat.normalized;
                }
                else
                    moving = flat.normalized * movingDirection * walkingSpeed * TameElement.originalDelta;
            }
            else
            {
                if ((feature[currentObject] & 2) > 0)
                {
                    if (InputBasis.turn != InputBasis.VR)
                        cameraTransform.rotation = gameObjects[currentObject].transform.rotation;
                }
                else if (InputBasis.turn != InputBasis.VR)
                {

                    fwd = cameraTransform.forward;
                    flat = new Vector3(fwd.x, 0, fwd.z);
                    flat = Utils.Rotate(flat, Vector3.zero, Vector3.up, rotationSpeed * TameElement.originalDelta * turningDirection);
                    flat.y = flat.magnitude * Mathf.Tan(currentTilt * Mathf.Deg2Rad);
                    cameraTransform.forward = flat.normalized;
                }

                if ((feature[currentObject] & 1) > 0)
                {
                    cameraTransform.position = gameObjects[currentObject].transform.position;
                    moveByObject = true;
                }
                else
                {
                    moveByObject = false;
                    fwd = cameraTransform.forward;
                    flat = new Vector3(fwd.x, 0, fwd.z);
                    moving = flat.normalized * movingDirection * walkingSpeed * walkingMode * TameElement.originalDelta;
                }
            }
            Walking.WalkFace lastFace = currentFace;
            if (!moveByObject)
            {
                if (TameManager.walkManager == null)
                    onWalk.position += moving;
                else
                {
                    TameManager.walkManager.UpdateActive();
                    fwd = currentFace != null ? currentFace.Pushing(p, TameElement.deltaTime) : Vector3.zero;
                    p += moving + fwd;
                    currentFace = TameManager.walkManager.Move(p - 0.95f * eyeHeight);
                    if (currentFace == null) currentFace = lastFace;
                    if (currentFace != lastFace)
                    {
                        Quaternion q = onWalk.rotation;
                        onWalk.parent = currentFace.control.owner.transform;
                        onWalk.position = TameManager.walkManager.foot + eyeHeight;
                        onWalk.rotation = q;
                    }
                    else
                        onWalk.position = TameManager.walkManager.foot + eyeHeight;
                    TameManager.walkManager.RecordLastStatus();
                }
                cameraTransform.SetPositionAndRotation(onWalk.position, onWalk.rotation);
            }
            if (CoreTame.torch != null)
            {
                CoreTame.torch.transform.SetPositionAndRotation(cameraTransform.position, cameraTransform.rotation);
                if (TameInputControl.keyMap.pressed[ZKey] || (TameInputControl.keyMap.gpMap.pressed[8] && TameInputControl.keyMap.gpMap.hold[5] && (!TameInputControl.keyMap.gpMap.hold[4])))
                    CoreTame.torch.gameObject.SetActive(!CoreTame.torch.gameObject.activeSelf);
            }
        }
        public static void TeleportTo(Transform tran)
        {
            Vector3 fwd;
            if (InputBasis.move == InputBasis.VR) return;
            currentObject = -1;
            moveByObject = false;
            if (InputBasis.turn != InputBasis.VR)
            {
                fwd = tran.forward;
                Vector3 flat = new Vector3(fwd.x, 0, fwd.z);
                flat.Normalize();
                if (flat.magnitude == 0) flat = Vector3.forward;
                onWalk.rotation = Quaternion.LookRotation(flat, Vector3.up);
            }

            Walking.WalkFace lastFace = currentFace;
            if (TameManager.walkManager == null)
                onWalk.position = tran.position;
            else
            {
                Vector3 p = tran.position;
                //         TameManager.walkManager.UpdateActive();
                currentFace = TameManager.walkManager.MoveTo(p);
                if (currentFace == null) currentFace = lastFace;
                if (currentFace != lastFace)
                {
                    Quaternion q = onWalk.rotation;
                    onWalk.parent = currentFace.control.owner.transform;
                    onWalk.position = TameManager.walkManager.foot + eyeHeight;
                    onWalk.rotation = q;
                    Debug.Log("tele " + onWalk.position.ToString() + tran.position.ToString());
                }
                else
                    onWalk.position = TameManager.walkManager.foot + eyeHeight;
                TameManager.walkManager.RecordLastStatus();
            }
            cameraTransform.SetPositionAndRotation(onWalk.position, onWalk.rotation);
        }
    }
}