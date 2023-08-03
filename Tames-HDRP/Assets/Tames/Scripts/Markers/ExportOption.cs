using UnityEngine;
using UnityEditor;
using Records;
namespace Markers
{
    public class ExportOption : MonoBehaviour
    {
        public string folder;
        public bool time = true;
        public bool onlyIfChanged = true;
        public int[] personIndex = null;
        public bool headPosition = true;
        public bool lookDirection = true;
        public bool handPosition = true;
        public bool handRotation = true;
        public bool bothHands = false;
        public bool actionKeys = false;
        public bool actionMouse = false;
        public bool actionGamePad = false;
        public bool actionVRController = false;
        public void Export()
        {
            bool success = false;
#if UNITY_EDITOR
            string path = EditorUtility.OpenFilePanel("Select file", "Assets", "");
            if (path != "")
            {
                TameFullRecord.allRecords = new TameFullRecord(null);
                success = TameFullRecord.allRecords.Load(path);
            }

            if (success)
            {
                path = System.IO.Path.GetDirectoryName(path);
                if ("/\\".IndexOf(path[^1]) < 0) path += "\\";
                TameFullRecord.allRecords.ExportToCSV(path, this);
            }
#endif
        }
        }
    }