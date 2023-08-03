using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
namespace Markers
{
    public class ExportModel : MonoBehaviour
    {
        public GameObject[] objects;
        public bool withChildren = true;

        public void Save()
        {
#if UNITY_EDITOR
            string path = UnityEditor.EditorUtility.SaveFilePanel("Select file ", "Assets", "", "");
            if (!string.IsNullOrEmpty(path))
            {

            }
#endif
        }
        public void Load()
        {
#if UNITY_EDITOR         
            string path = UnityEditor.EditorUtility.OpenFilePanel("Select file ", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {

            }
#endif     
        }
    }
}
