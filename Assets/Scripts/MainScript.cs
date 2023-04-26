using HandAsset;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Multi;
using System;
using Tames;
using Records;

namespace Assets.Script
{
    public class MainScript : MonoBehaviour
    {
    //    public GameObject canvas;
    //    public GameObject panel;
        public bool VRMode;
        public static Light Sun;
        public static bool multiPlayer = false;
        public static GameObject HeadObject;
        public static Person[] people;
        public static ITameEffect[] ies;
        public static List<TameElement> tes;
        //  List<Person> people;
        public static Person localPerson;

        public static int WheelDirection;
        public static int MouseButton;
        // objects
        public static Camera mainCamera;
      //  public Text text;
        public static Vector2 screenSize;
        // mirror
        // door
        // timing
        float averageFPS;
        int counter;
        Quaternion rotationDefault;
        Vector3 positionDefault;
        HandModel[] hand;
        Vector3 handAng = Vector3.zero;
        TameManager manager;
        string audioFolder;
        private bool savingTexture = false;
        private TameArea closestGrip;
        private TameObject grippedObject = null;
        private float gripSpeed = 1;
        public static string fingerHeader = "upfinger";
        public static bool replayMode = false;
         private Camera rendCam;
        void Start()
        {
            Utils.SetPipelineLogics();
            Utils.SetPOCO();
         //   panel.SetActive(false);
            PrepareLoadScene();
         //   text.text = messages[0];
            people = Person.people;
            for (int i = 0; i < people.Length; i++)
                people[i] = null;
            audioFolder = "Audio/";
            Slerp sl = new Slerp(0.4f);
            string ss = "";
            for (float f = 0; f <= 1; f += 0.05f)
            {
                float fo = sl.On(f);
                ss += "\t" + fo;
            }
        //    Debug.Log(ss);
            screenSize = new Vector2(Screen.width, Screen.height);
            mainCamera = Camera.main;
            TameCamera.cameraTransform = mainCamera.transform.parent.parent;
            TameManager.settings = TameCamera.cameraTransform.gameObject.GetComponent<Markers.MarkerSettings>();
            if (TameManager.settings != null)
                replayMode = TameManager.settings.replay;
            Camera[] allCam = Camera.allCameras;
            foreach (Camera c in allCam)
                if (c != mainCamera)
                    rendCam = c;
            counter = -1;
            averageFPS = 0;
            GameObject[] root = SceneManager.GetActiveScene().GetRootGameObjects();
            Material handmat = Identifier.HandMats(root);
            hand = Identifier.Inputs(root, fingerHeader);
            //     Debug.Log("c: " + hand[1].data.controller.name);

            manager = new TameManager();
            ManifestKeys.LoadCSV("aliases");
            manager.LoadManifest(Identifier.LoadLines(Tames.TameManager.ManifestPath));
            tes = manager.tes;
            TameEffect.AllEffects = new TameEffect[manager.tes.Count];
            ies = ITameEffect.AllEffects = new ITameEffect[manager.tes.Count];
            ITameEffect.Initialize();
            //  text.text = "";

            Person.localPerson = localPerson = new Person(0)
            {
                isLocal = true,
                head = mainCamera.gameObject,
                hand = hand
            };
            //    localPerson.head.SetActive(false);
            localPerson.hand[0].wrist.SetActive(VRMode);
            localPerson.hand[1].wrist.SetActive(VRMode);
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
                if (args[i].StartsWith("HostIP:"))
                {
                    NetworkManager.commandIP = args[i].Substring(7);
                    break;
                }
        }

        void PrepareLoadScene()
        {

        }
        void GetFPS()
        {
            if (counter >= 0)
            {
                counter++;
                averageFPS += 1 / TameElement.deltaTime;
                if (counter == 30)
                {
                    counter = 0;
             //       text.text = (averageFPS / 30).ToString("0");
                    averageFPS = 0;
                }
            }
            else
                counter++;
        }
        void Update()
        {
            if (!replayMode)
            {
                int k = 0;
                if (!multiPlayer)
                {
                    if (hand[0].data.controller != null)
                        if (hand[0].data.trigger.Status) k++;
                    if (hand[1].data.controller != null)
                        if (hand[1].data.trigger.Status) k++;
                    if (k == 2)
                    {
                        multiPlayer = true;
                        NetworkManager.Singleton.Connect();
                    }
                }


                UpdateSolo();
                if (rendCam != null)
                    rendCam.transform.rotation = mainCamera.transform.rotation;
            }
            else
                UpdateReplay();
        }
        public int lastFrameIndex = -1;
        void UpdateBoth()
        {
            TameElement.PassTime();
            if (VRMode)
            {
                localPerson.Update();
                localPerson.EncodeLocal();
            }
            else
            {
                localPerson.UpdateHeadOnly();
            }
            FrameShot f = CheckInput();
            if (multiPlayer)
                Player.SendFrame(Player.index, f);
            //    AggregateInput(f);
            int n = TameElement.GetAllParents(TameEffect.AllEffects, tes);
            //  Debug.Log("custom n " + n);
            for (int i = 0; i < n; i++)
            {
                TameEffect.AllEffects[i].Apply();
                //          ies[i].Set(TameEffect.AllEffects[i]);
            }
            for (int i = 0; i < manager.altering.Count; i++)
                manager.altering[i].Update();
        }

        void UpdateReplay()
        {
            float t = TameElement.ActiveTime;
            float time = t + Time.deltaTime;
            int next = lastFrameIndex + 1;
            for (int i = lastFrameIndex + 1; i < TameFullRecord.allRecords.frame.Count; i++)
            {
                next = i;
                if (TameFullRecord.allRecords.frame[i].time > time)
                    break;
            }
            for (int i = lastFrameIndex; i < next; i++)
            {
                if (TameFullRecord.allRecords.frame[i].passed)
                    TameFullRecord.allRecords.frame[i].Unpress();
                for (int j = 0; j < people.Length; j++)
                    if ((people[j] == null) && (TameFullRecord.allRecords.persons[j] != null))
                    {
                        // connect person as record
                    }
                    else if ((people[j] != null) && (TameFullRecord.allRecords.persons[j] == null))
                    {
                        // discon person
                    }
                    else if ((people[j] != null) && (TameFullRecord.allRecords.persons[j] != null))
                    {
                        // update person based on time
                    }
                TameInputControl.keyMap = TameFullRecord.allRecords.frame[i].keyMap;
                //        delta = 

                UpdateSolo();

            }
            // check which frames are active

            // check peoples connection

            // 

        }
        void UpdateSolo()
        {

            TameElement.PassTime();
            if (VRMode)
            {
                localPerson.Update();
                localPerson.EncodeLocal();
            }
            else
            {
                localPerson.UpdateHeadOnly();
            }
            CheckInput();
            int n = TameElement.GetAllParents(TameEffect.AllEffects, tes);
            //  Debug.Log("custom n " + n);
            for (int i = 0; i < n; i++)
            {
           //     if (TameEffect.AllEffects[i].child.name == "barrier sign") Debug.Log("UP: child ");
                TameEffect.AllEffects[i].Apply();
                ies[i].Set(TameEffect.AllEffects[i]);
            }
            for (int i = 0; i < manager.altering.Count; i++)
                manager.altering[i].Update();
        }
        // Update is called once per frame


        private int msgIndex = 0;
        private FrameShot CheckInput(int index = -1)
        {
            FrameShot f = TameInputControl.CheckKeys(index);
            TameInputControl.keyMap.Aggregate(Player.frames, f);
            if (Keyboard.current.escapeKey.isPressed)
                Application.Quit();

            if (VRMode) return null;
            TameCamera.UpdateCamera();

            CheckGripAndSwitch();
            return null;
        }
        bool GripInputActive()
        {
            if (Keyboard.current != null)
                if (Keyboard.current.spaceKey.isPressed || Keyboard.current.qKey.isPressed || Keyboard.current.eKey.isPressed)
                    return true;
            if (Gamepad.current != null)
                if (Gamepad.current.xButton.isPressed || Gamepad.current.bButton.isPressed)
                    return true;
            return false;
        }
        int GripMoveDirection()
        {
            if (MouseButton != 0)
                return MouseButton;
            if (Gamepad.current != null)
            {
                if (Gamepad.current.xButton.isPressed) return -1;
                if (Gamepad.current.bButton.isPressed) return 1;
            }
            if (Keyboard.current != null)
            {
                if (Keyboard.current.qKey.isPressed) return -1;
                if (Keyboard.current.eKey.isPressed) return 1;
            }
            return 0;
        }
        bool SwitchInput()
        {
            if (Keyboard.current != null)
                if (Keyboard.current.backquoteKey.wasPressedThisFrame) return true;
            if (Gamepad.current != null)
                if (Gamepad.current.startButton.wasPressedThisFrame) return true;
            return false;
        }
        void CheckGripAndSwitch()
        {
            TameArea sa;
            float gmd;
            if (grippedObject != null)
            {
                if (GripInputActive())
                {
                    if ((gmd = GripMoveDirection()) != 0)
                    {
                        grippedObject.Grip(TameElement.deltaTime * gmd * gripSpeed);
                        localPerson.action = Person.ActionUpdateGrip;
                    }
                }
                else
                {
                    grippedObject = null;
                    localPerson.action = 0;
                }
            }
            else
            {
                if (SwitchInput())
                {
                    sa = TameArea.ClosestSwitch(tes, TameCamera.cameraTransform, 2.1f, out TameObject to);
                    //if (sa != null)                        sa.Switch(true);
                    if (sa != null)
                    {
                        localPerson.nextArea = sa;
                        localPerson.action = Person.ActionSwitch;
                    }
                    Debug.Log("switch: " + (sa == null ? "null" : sa.element.name));
                }
                else if (localPerson.action!=Person.ActionUpdateSwitch)
                {
                    if (GripInputActive())
                    {
                        closestGrip = TameArea.ClosestGrip(tes, TameCamera.cameraTransform, 2.1f, out TameObject to);
                        if (closestGrip != null)
                        {
                            Debug.Log("grip: " + closestGrip.element.name);
                            grippedObject = to;
                            localPerson.nextArea = closestGrip;
                            localPerson.action = Person.ActionGrip;
                        }
                    }
                }
            }
        }
        void PrepareSave()
        {
            savingTexture = true;
            RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
            mainCamera.targetTexture = rt;
        }
        void SaveTexture()
        {
            savingTexture = false;
            RenderTexture.active = mainCamera.targetTexture;
            Texture2D t = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            t.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            t.Apply();
            Destroy(t);
            byte[] b = t.EncodeToPNG();
            System.IO.File.WriteAllBytes("C:\\Work\\screenshot.png", b);
            mainCamera.targetTexture = null;
        }

    }
}