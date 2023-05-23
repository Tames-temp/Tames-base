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
using System.IO;

public class CoreTame : MonoBehaviour
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
    public static Markers.ExportOption exportOption;
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
    //  string audioFolder;
    private bool savingTexture = false;
    private TameArea closestGrip;
    private TameObject grippedObject = null;
    private float gripSpeed = 1;
    public static string fingerHeader = "finger";
    public static bool replayMode = false;
    public static GameObject torch;
    private GameObject camObject;
    public RenderTexture renderTexture;
    void Start()
    {
        Utils.SetPipelineLogics();
        Utils.SetPOCO();
        //   panel.SetActive(false);
        mainCamera = Camera.main;
        camObject = new GameObject("camera object");
        renderTexture = new RenderTexture(Screen.width * 3, Screen.height * 3, 24);
     //   rendCam.targetTexture = renderTexure;
     //   rendCam.enabled = false;
        Transform t = mainCamera.transform;
        while (t != null)
        {
            TameCamera.cameraTransform = t;
            t = t.parent;
        }
        TameCamera.camera = mainCamera;
        screenSize = new Vector2(Screen.width, Screen.height);
        PrepareLoadScene();
        TameCamera.ZKey = TameInputControl.FindKey("z", false);
        TameCamera.XKey = TameInputControl.FindKey("x", false);
        TameCamera.CKey = TameInputControl.FindKey("c", false);
     //   Debug.Log(TameCamera.ZKey + " " + TameCamera.XKey + " " + TameCamera.CKey);
        TameInputControl.FindKey("n-");
        TameInputControl.FindKey("n+");

        //   text.text = messages[0];
        people = Person.people;
        for (int i = 0; i < people.Length; i++)
            people[i] = null;
        // audioFolder = "Audio/";

          counter = -1;
        averageFPS = 0;
        ManifestKeys.SetKeywords();
        manager = new TameManager();
        manager.Initialize();
        if (TameManager.settings != null)
            torch = TameManager.settings.torch;
        tes = manager.tes;
        if (TameManager.settings != null)
            replayMode = TameManager.settings.replay;

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
        //    Debug.Log("cam " + TameCamera.cameraTransform.position.ToString());
    }

    void PrepareLoadScene()
    {
        Identifier.rig = TameCamera.cameraTransform.gameObject;
        XRController xrcl = null, xrcr = null;
        XRController[] all = Identifier.rig.GetComponentsInChildren<XRController>();
        if (all.Length > 0)
            for (int i = 0; i < all.Length; i++)
                if (all[i].controllerNode == XRNode.LeftHand) xrcl = all[i];
                else if (all[i].controllerNode == XRNode.RightHand) xrcr = all[i];
        GameObject go = (GameObject)Resources.Load("Tames models\\leftHand");
        Identifier.left = GameObject.Instantiate(go);
        go = (GameObject)Resources.Load("Tames models\\rightHand");
        Identifier.right = GameObject.Instantiate(go);
        if (xrcl == null)
        {
            xrcl = Identifier.left.AddComponent<XRController>();
            xrcl.controllerNode = XRNode.LeftHand;
        }
        if (xrcr == null)
        {
            xrcr = Identifier.right.AddComponent<XRController>();
            xrcr.controllerNode = XRNode.RightHand;
        }
        go = (GameObject)Resources.Load("Tames models\\external head");
        HeadObject = Identifier.head = GameObject.Instantiate(go);
        hand = Identifier.Inputs(xrcl, xrcr, fingerHeader);
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
            
        }
        else
            UpdateReplay();
    }
    public static int lastFrameIndex = -1;
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
        Person p;
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
            {
                p = TameFullRecord.allRecords.persons[j];
                if ((people[j] == null) && (p != null))
                {
                    // connect person as record
                }
                else if ((people[j] != null) && (p == null))
                {
                    // discon person
                }
                else if ((people[j] != null) && (p != null))
                {
                    // update person based on time
                }
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
        if (!TameElement.isPaused)
        {
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
            //   Debug.Log("custom n " + n);
            for (int i = 0; i < n; i++)
            {
                //     if (TameEffect.AllEffects[i].child.name == "barrier sign") Debug.Log("UP: child ");
                TameEffect.AllEffects[i].Apply();
                ies[i].Set(TameEffect.AllEffects[i]);
            }
            for (int i = 0; i < manager.altering.Count; i++)
                manager.altering[i].Update();
            for (int i = 0; i < manager.alteringMaterial.Count; i++)
                manager.alteringMaterial[i].Update();
            TameCamera.UpdateCamera();
        }
    }
    // Update is called once per frame
    IEnumerator SavePNG()
    {
        // We should only read the screen buffer after rendering is complete
        yield return new WaitForEndOfFrame();

        RenderTexture.active = renderTexture;
        // Create a texture the size of the screen, RGB24 format
        int width = renderTexture.width;
        int height = renderTexture.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Read screen contents into the texture
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        // Encode texture into PNG
        byte[] bytes = tex.EncodeToPNG();
        Destroy(tex);

        // For testing purposes, also write to a file in the project folder
        File.WriteAllBytes("E:\\" + DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss") + ".png", bytes);
        TameElement.isPaused = false;   

    }
    private FrameShot CheckInput(int index = -1)
    {
        TameKeyMap km = TameInputControl.CheckKeys(index);
        FrameShot fa = TameInputControl.keyMap.Aggregate(Player.frames, km.Capture());
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            TameElement.isPaused = true;
            mainCamera.targetTexture = renderTexture;
            mainCamera.Render();
            StartCoroutine(SavePNG());
            mainCamera.targetTexture = null;
        }
        TameFullRecord.allRecords.Capture(TameElement.ActiveTime, index < 0 ? null : km);
        if (Keyboard.current.escapeKey.isPressed)
        {
             Application.Quit();
        }
        string path;
        if (Keyboard.current.ctrlKey.isPressed && Keyboard.current.sKey.wasPressedThisFrame)
        {
            DateTime now = DateTime.Now;
            bool saved = false;
            if (exportOption != null)
                if (exportOption.folder != "")
                {
                    path = exportOption.folder;
                    if ("/\\".IndexOf(path[^1]) < 0) path += "\\";
                    saved = TameFullRecord.allRecords.Save(path + now.ToString("yyyy.MM.dd HH.mm.ss") + ".tfr");
                }
            if (!saved)
            {
                path = UnityEditor.EditorUtility.OpenFolderPanel("Select a directory", "Assets", "");
                if ("/\\".IndexOf(path[^1]) < 0) path += "\\";
                if (path == "") saved = true;
                else
                    TameFullRecord.allRecords.Save(path + now.ToString("yyyy.MM.dd HH.mm.ss") + ".tfr");
            }

        }
        if (VRMode) return null;
        //    Debug.Log("bef " + TameCamera.cameraTransform.position.ToString());
        //   Debug.Log("aft " + TameCamera.cameraTransform.position.ToString());

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
            else if (localPerson.action != Person.ActionUpdateSwitch)
            {
                if (GripInputActive())
                {
                    closestGrip = TameArea.ClosestGrip(tes, TameCamera.cameraTransform, 2.1f, 70, out TameObject to);
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
