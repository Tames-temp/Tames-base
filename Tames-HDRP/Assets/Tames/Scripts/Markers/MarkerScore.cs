using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Markers
{

    public class MarkerScore : MonoBehaviour
    {
        public bool isBasket = false;
        public int score = 1;
        public int passScore = 10;
        public int count = 1;
        public GameObject onlyAfter;
        public GameObject basket;
        public GameObject activate;
        public GameObject show;
        public InputSetting control;
        public GameObject activateAfter;
        public GameObject showAfter;
    }
}
