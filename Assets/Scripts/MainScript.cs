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

namespace Assets.Script
{
    public class MainScript : MonoBehaviour
    {
        //      public GameObject canvas;
        //      public GameObject panel;
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
 //       public Text text;
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
        TameManager manifest;
        string audioFolder;
        private bool savingTexture = false;
        private TameArea closestGrip;
        private TameObject grippedObject = null;
        private float gripSpeed = 1;
        public static string fingerHeader = "upfinger";

        //    private Camera rendCam;
        void Start()
        {
            Utils.SetPipelineLogics();
            //       panel.SetActive(false);
            PrepareLoadScene();
            //     text.text=messages[0];
            people = Person.people;
            for (int i = 0; i < people.Length; i++)
                people[i] = null;
            audioFolder = "Audio/";

            screenSize = new Vector2(Screen.width, Screen.height);
            mainCamera = Camera.main;
            TameCamera.cameraTransform = mainCamera.transform.parent.parent;
            //     Camera[] allCam=   Camera.allCameras;
            //        foreach(Camera c in allCam)
            //           if(c!=mainCamera)
            //               rendCam = c;
            counter = -1;
            averageFPS = 0;
            GameObject[] root = SceneManager.GetActiveScene().GetRootGameObjects();
            Material handmat = Identifier.HandMats(root);
            hand = Identifier.Inputs(root, fingerHeader);
            //     Debug.Log("c: " + hand[1].data.controller.name);

            manifest = new TameManager();
            ManifestKeys.LoadCSV("aliases");
            manifest.LoadManifest(Identifier.LoadLines(Tames.TameManager.ManifestPath));
            tes = manifest.tes;
            TameEffect.AllEffects = new TameEffect[manifest.tes.Count];
            ies = ITameEffect.AllEffects = new ITameEffect[manifest.tes.Count];
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
            //        text.text = (averageFPS / 30).ToString("0");
                    averageFPS = 0;
                }
            }
            else
                counter++;
        }
        void Update()
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
            if (multiPlayer)
                UpdateMulti();
            else
                UpdateSolo();
            //       if (rendCam != null)
            //           rendCam.transform.rotation=mainCamera.transform.rotation;
        }
        void UpdateSolo()
        {

            TameElement.PassTime();
            if (VRMode)
            {
                localPerson.Update();
                localPerson.EncodeInput();
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
                TameEffect.AllEffects[i].Apply();
                ies[i].Set(TameEffect.AllEffects[i]);
            }
        }
        // Update is called once per frame
        void UpdateMulti()
        {
            TameElement.PassTime();
            GetFPS();
            CheckInput();
            if (Player.Id < 255)
            {
                if (VRMode)
                {
                    hand[0].Update(null);
                    hand[1].Update(null);
                }
                localPerson.EncodeInput();
                Player.SendPersonUpdate();
                //       Debug.Log("MS: update sent");
                for (int i = 0; i < people.Length; i++)
                    if (people[i] != null)
                        if (people[i].initiated)
                            if (people[i] != localPerson)
                            {
                                // background.Update();
                                people[i].hand[0].Update(people[i]);
                                people[i].hand[1].Update(people[i]);
                            }
                if (Player.Id == Player.bossId)
                {
                    int n = TameElement.GetAllParents(TameEffect.AllEffects, tes);
                    for (int i = 0; i < n; i++)
                    {
                        TameEffect.AllEffects[i].Apply();
                        ies[i].Set(TameEffect.AllEffects[i]);
                    }
                    Player.UpdateInteractives();
                }
                else
                {
                    for (int i = 0; i < ITameEffect.EffectCount; i++)
                        ies[i].Apply(tes);
                }
            }
        }

        private int msgIndex = 0;
        private void CheckInput()
        {
            if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                //         msgIndex++;
                //         text.text = messages[msgIndex];
            }
            //      if (Keyboard.current.digit4Key.wasPressedThisFrame)
            //         panel.SetActive(true);
            TameInputControl.CheckKeys();

            if (Keyboard.current.escapeKey.isPressed)
                Application.Quit();

            if (VRMode) return;
            TameCamera.UpdateCamera();

            CheckGripAndSwitch();
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
                        localPerson.UpdateGrip(closestGrip);
                    }
                }
                else
                {
                    grippedObject = null;
                    localPerson.Ungrip();
                }
            }
            else
            {
                if (SwitchInput())
                {
                    sa = TameArea.ClosestSwitch(tes, TameCamera.cameraTransform, 2.1f, out TameObject to);
                    if (sa != null)
                        sa.Switch(true);
                    Debug.Log("switch: " + (sa == null ? "null" : sa.element.name));
                }
                else
                {
                    if (GripInputActive())
                    {
                        closestGrip = TameArea.ClosestGrip(tes, TameCamera.cameraTransform, 2.1f, out TameObject to);
                        if (closestGrip != null)
                        {
                            Debug.Log("grip: " + closestGrip.element.name);
                            grippedObject = to;
                            localPerson.Grip(closestGrip, TameCamera.cameraTransform);
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