using Assets.Script;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Tames
{
    public class TameCamera
    {
        public static GameObject gameObject = null;
        private static GameObject camera;
        public static Transform cameraTransform;
        private static int turningDirection;
        private static int tiltingDirection;
        private static int movingDirection;
        private static float walkingSpeed = 1f;
        private static float walkingMode = 1f;
        private static float rotationSpeed = 100f; // degree/s
        private static float tiltingSpeed = 70f; // degree/s
        private static string objectName = "...";
        private static bool moveByObject = false;
        private static bool turnByObject = false;
        private static float currentTilt = 0;
        public static Vector3 eyeHeight = 1.6f * Vector3.up;
        public static Walking.WalkFace currentFace = null;

        public static int currentObject = -1;
        private static List<string> names = new List<string>();
        public static GameObject[] gameObjects;
        public static List<byte> feature = new List<byte>();
        public static void ToggleCamera()
        {
            if (currentObject == -1) currentObject = gameObjects.Length != 0 ? 0 : -1;
            else if (currentObject == gameObjects.Length - 1) currentObject = -1;
            else currentObject++;
        }
        public static int ReadCamera2(ManifestHeader mh, string[] lines, int index)
        {
            int i = index;
            int fcount = 0;
            bool[] fs = new bool[] { false, false };
            if (mh.items.Count > 0) objectName = mh.items[0];
            if (lines.Length >= index)
            {
                ManifestHeader h = ManifestHeader.Read(lines[index + 1]);
                if (h.subKey == ManifestKeys.Camera)
                {
                    i++;
                    for (int j = 0; j < h.items.Count; j++)
                    {
                        switch (h.items[j].ToLower())
                        {
                            //       case "tilt": fs[0] = true; break;
                            case "move": fs[0] = true; break;
                            case "turn": fs[1] = true; break;
                        }
                    }
                    for (int j = 0; j < 2; j++)
                        fcount += fs[j] ? 1 : 0;
                    if (fcount == 0)
                    { moveByObject = false; turnByObject = false; }
                    else
                    {
                        //  tilt = fs[0];
                        moveByObject = fs[0];
                        turnByObject = fs[1];
                    }
                }
            }
            return i;
        }
        public static int ReadCamera3(ManifestHeader mh, string[] lines, int index)
        {
            int fcount = 0;
            moveByObject = false;
            turnByObject = false;
            string what;
            if (mh.items.Count > 1)
            {
                what = mh.items[0].ToLower();
                objectName = mh.items[1];
                switch (what)
                {
                    //       case "tilt": fs[0] = true; break;
                    case "both":
                    case "all": moveByObject = turnByObject = true; break;
                    case "move": moveByObject = true; break;
                    case "turn": turnByObject = true; break;
                }
            }
            return index;
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
        public static void AssignCamera2(List<TameGameObject> tgos)
        {
            TameFinder finder = new TameFinder();
            List<string> split = Utils.Split(objectName, ",");
            finder.header = new ManifestHeader() { items = split };
            finder.PopulateObjects(tgos);
            if (finder.objectList.Count > 0)
                gameObject = finder.objectList[0].gameObject;
            //    camera = new GameObject();
            if (gameObject == null)
            {
                moveByObject = false;
                turnByObject = false;
            }

        }
        public static void AssignCamera(List<TameGameObject> tgos)
        {
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
                    float x = TameInputControl.keyMap.gpMap.stick[0].x;
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
                {
                    if (TameInputControl.keyMap.gpMap.pressed[3]) tiltingDirection = 1;
                    if (TameInputControl.keyMap.gpMap.pressed[2]) tiltingDirection = -1;
                }
                currentTilt += tiltingDirection * tiltingSpeed * TameElement.deltaTime;
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
            Vector3 p = cameraTransform.position;
            float x;
            float y;

            if (Keyboard.current != null)
                if (Keyboard.current.zKey.wasPressedThisFrame)
                    InputBasis.ToggleNext();
            if (Gamepad.current != null)
                if (Gamepad.current.selectButton.wasPressedThisFrame)
                    InputBasis.ToggleNext();

            if (Keyboard.current != null)
                if (Keyboard.current.xKey.wasPressedThisFrame)
                    ToggleCamera();

            if (Mouse.current != null)
            {
                float ms = Mouse.current.scroll.y.ReadValue();
                MainScript.WheelDirection = ms > 0 ? 1 : (ms < 0 ? -1 : 0);
                MainScript.MouseButton = 0;
                if (Mouse.current.leftButton.isPressed) MainScript.MouseButton = -1;
                if (Mouse.current.rightButton.isPressed) MainScript.MouseButton = 1;
            }

            SetMovingDirection();
            SetTiltAngle();
            SetTurningDirection();

            Vector3 fwd;
            Vector3 flat;
            Vector3 moving = Vector3.zero;
            if (currentObject < 0)
            {
                fwd = cameraTransform.forward;
                flat = new Vector3(fwd.x, 0, fwd.z);
                if (InputBasis.turn != InputBasis.VR)
                {
                    flat = Utils.Rotate(flat, Vector3.zero, Vector3.up, rotationSpeed * TameElement.deltaTime * turningDirection);
                    moving = flat.normalized * movingDirection * walkingSpeed * walkingMode * TameElement.deltaTime;
                    flat.y = flat.magnitude * Mathf.Tan(currentTilt * Mathf.Deg2Rad);
                    cameraTransform.forward = flat.normalized;
                }
                else
                    moving = flat.normalized * movingDirection * walkingSpeed * TameElement.deltaTime;
            }
            else
            {
                if ((feature[currentObject] & 2) == 0)
                {
                    if (InputBasis.turn != InputBasis.VR)
                        cameraTransform.rotation = gameObject.transform.rotation;
                }
                else if (InputBasis.turn != InputBasis.VR)
                {
                    fwd = cameraTransform.forward;
                    flat = new Vector3(fwd.x, 0, fwd.z);
                    flat = Utils.Rotate(flat, Vector3.zero, Vector3.up, rotationSpeed * TameElement.deltaTime * turningDirection);
                    flat.y = flat.magnitude * Mathf.Tan(currentTilt * Mathf.Deg2Rad);
                    cameraTransform.forward = flat.normalized;
                }

                if ((feature[currentObject] & 1) == 0)
                    cameraTransform.position = gameObject.transform.position;
                else
                {
                    fwd = cameraTransform.forward;
                    flat = new Vector3(fwd.x, 0, fwd.z);
                    moving = flat.normalized * movingDirection * walkingSpeed*walkingMode * TameElement.deltaTime;
                }
            }
            if (TameManager.walkManager == null)
                cameraTransform.position += moving;
            else
            {
                fwd = Vector3.zero;
                //       Debug.Log("walk " + fwd.ToString("0.00") + (currentFace==null?"null":currentFace.parent.name));
                if ((currentFace != null) && (!moveByObject))
                {
                    fwd = currentFace.Pushing(cameraTransform.position, TameElement.deltaTime);
                }
                //        flat = p;
                p += moving + fwd;
                //  fwd = TameManifest.walkManager.foot;
                currentFace = TameManager.walkManager.Move(p - eyeHeight);
                //    if(currentFace!=null)                    Debug.Log("walk " + currentFace.parent.name);
                cameraTransform.position = TameManager.walkManager.foot + eyeHeight;
            }

        }
    }
}