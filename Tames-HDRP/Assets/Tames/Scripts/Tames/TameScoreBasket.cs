using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Markers;
namespace Tames
{
    public class ScoreBase
    {
        public MarkerScore marker;
        public TameElement element = null;
        public TameElement activateAfter=null, showAfter=null;
        public GameObject show = null;
        public bool fulfilled = false;
        public bool active = false;
        public void Fulfill(bool f)
        {
            if (element != null) element.progress.active = f;
            if (show != null) show.SetActive(f);
        }
        public void FindElements(List<TameGameObject> tgos)
        {
            TameGameObject tg = TameGameObject.Find(marker.activate, tgos);
            if (tg != null)
                if (tg.isElement)
                    element = tg.tameParent;
            tg = TameGameObject.Find(marker.showAfter, tgos);
            if (tg != null)
                if (tg.isElement)
                    showAfter = tg.tameParent;
            tg = TameGameObject.Find(marker.activateAfter, tgos);
            if (tg != null)
                if (tg.isElement)
                    activateAfter = tg.tameParent;
            active = activateAfter == null;
            marker.gameObject.SetActive(showAfter == null);
            Debug.Log(marker.gameObject.name + " > " + (showAfter == null));
        }

    }
    public class TameScore : ScoreBase
    {
        public TameScoreBasket parent;
        public InputSetting control;
        public TameScore after;
        public float lastPassed = -1;
        public int count = 0;
        public float interval = 10;
        public int lastAfterCount = 0;

        public TameScore(MarkerScore ms)
        {
            marker = ms;
            marker.control.AssignControl(InputSetting.ControlTypes.Mono);
            show = ms.show;
            control = marker.control;
        }

        public bool Update()
        {
            bool check, visible = true, passed = false;
     //      if(fulfilled) Debug.Log("fulfilled " + marker.name + " " + fulfilled);
            if (fulfilled) return false;
            if (activateAfter != null)
                active = activateAfter.progress.progress > 0.99f;
            if (showAfter != null)
                marker.gameObject.SetActive(visible = showAfter.progress.progress > 0.99f);
            if (active)
            {
                if ((lastPassed < 0) || (TameElement.ActiveTime - lastPassed >= interval))
                {
                    check = after == null;
                    if (!check)
                        if (after.count > lastAfterCount) check = true;
                    if (check)
                        if (control.CheckMono(marker.gameObject))
                        {
                            lastPassed = TameElement.ActiveTime;
                            count++;
                            fulfilled = count == marker.count;
                            lastAfterCount = after != null ? after.count : 0;
                            Debug.Log("updating score " + marker.name + " " + fulfilled);
                            passed = true;
                        }
                 }
                Fulfill(passed);
            }
            return passed;
        }
    }

    public class TameScoreBasket : ScoreBase
    {

        public List<TameScore> scores = new();
        public float totalScore = 0;
        public TameScoreBasket(MarkerScore ms)
        {
            marker = ms;
            show = ms.show;
        }
        public void Update()
        {
            if (fulfilled) return;
            if (activateAfter != null)
                active = activateAfter.progress.progress > 0.99f;
            foreach (TameScore ts in scores)
            {
                if (ts.Update())
                {
                    totalScore += ts.marker.score;
                    Debug.Log("updating from " + totalScore);
                }
            }
            if (totalScore >= marker.passScore)
            {
                fulfilled = true;
                Fulfill(true);
            }
        }
    }

}