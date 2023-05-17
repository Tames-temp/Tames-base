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
            string path = UnityEditor.EditorUtility.SaveFilePanel("Select file ", "Assets", "", "");
            if (!string.IsNullOrEmpty(path))
            {

            }
        }
        public void Load()
        {
            string path = UnityEditor.EditorUtility.OpenFilePanel("Select file ", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {

            }
        }
    }
}
