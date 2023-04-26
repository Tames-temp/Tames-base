using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Markers
{

    public class MarkerLink : MonoBehaviour
    {
        public enum LinkTypes
        {
            Custom, Random, Parent
        }
        public enum CloneTypes
        {
            CloneMover, CloneEverything, LinkMover
        }
        public CloneTypes type = CloneTypes.CloneMover;
        public string childrenNames = "";
        public GameObject childrenOf = null;
        public GameObject parent = null;
        public LinkTypes offsetBase = LinkTypes.Custom;
        public float offset = 0;// -1 is random
        public LinkTypes speedBase = LinkTypes.Custom;
        public float factor = 1;//-1 random
    }

   
}
