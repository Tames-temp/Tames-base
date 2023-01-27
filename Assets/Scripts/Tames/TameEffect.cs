using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Tames
{    public class TameEffect
    {
        public static TameEffect[] AllEffects;
        /// <summary>
        /// determines what effect this would have on the child's change (0: Update, 1: Slide, 2: Rotate). While Slide and Rotate are different actions and do not share effect, Update shares with both of them. When adding an effect to parents lists (<see cref="TameElement.parents"/>, <see cref="TameElement.slideParents"/> and <see cref="TameElement.rotateParents"/>), previously added sharing but different effects will be removed (e.g. if an Update effect is added, all Rotate and Slide effects will be removed but when a Slide effect is added, only Update effects will be removed and Rotate effect will be retained). 
        /// </summary>
       // public int effect;
        /// <summary>
        /// this is the <see cref="ManifestKeys"> version of effect
        /// </summary>
     //   public ManifestKeys Effect { get { return effect == 0 ? ManifestKeys.Update : (effect == 1 ? ManifestKeys.Slide : ManifestKeys.Rotate); } set { effect = value == ManifestKeys.Update ? 0 : (value == ManifestKeys.Slide ? 1 : 2); } }
        /// <summary>
        /// the source of tracking (tame element, object, hand, head or time
        /// </summary>
        public byte type;
        /// <summary>
        /// if the tracking source is head or hand, this will contain the index of that person
        /// </summary>
        public int personIndex;
        /// <summary>
        /// if the tracking source is hand, this will contain the hand index (0: left, 1: right)  
        /// </summary>
        public int handIndex;
        /// <summary>
        /// the direction of changing progress value (-1: downward, 0:still, 1:upward)
        /// </summary>
        public int direction = 1;
        /// <summary>
        /// if the tracking source is hand, head or object, this would contain the world position of their corresponding objet 
        /// </summary>
        public Vector3 position;
        /// <summary>
        /// if the tracking source is object, this would contain a reference to the TameGameObject containing that object
        /// </summary>
        public TameGameObject gameObject;
        /// <summary>
        /// if the tracking source is a tameElement, this refers to it 
        /// </summary>
        public TameElement parent;
        /// <summary>
        /// the child which this would affect
        /// </summary>
        public TameElement child;
        /// <summary>
        /// the index of interactor affecting the update
        /// </summary>
        public int areaIndex;
        public TameEffect() { }
        /// <summary>
        /// creates a TameEffect when the source is a TameElement
        /// </summary>
        /// <param name="e">the effect</param>
        /// <param name="te">the source of the effect</param>
        public TameEffect(int e, TameElement te)
        {
            parent = te;
            type = TrackBasis.Tame;
       //     effect = e == ManifestKeys.Update ? 0 : (e == ManifestKeys.Slide ? 1 : 2);
        }
        /// <summary>
        /// creates a TameEffect when the source is a GameObject
        /// </summary>
        /// <param name="e">the effect</param>
        /// <param name="g">the source object containing the game object</param>
        public TameEffect(int e, TameGameObject g)
        {
            gameObject = g;
            type = TrackBasis.Object;
         //   effect = e == ManifestKeys.Update ? 0 : (e == ManifestKeys.Slide ? 1 : 2);
        }
        private static TameEffect TimeUpdate = new TameEffect() {type = TrackBasis.Time };
      //  private static TameEffect TimeSlide = new TameEffect() { effect = 1, type = TrackBasis.Time };
       // private static TameEffect TimeRotate = new TameEffect() { effect = 2, type = TrackBasis.Time };
        /// <summary>
        /// returns a TameEffect based on passing of time
        /// </summary>
        /// <param name="effect">the effect</param>
        /// <returns></returns>
        public static TameEffect Time()
        {
            return new TameEffect() {  type = TrackBasis.Time };
        }
        /// <summary>
        /// creates a TameEffect if the source is hand or head
        /// </summary>
        /// <param name="effect">the effect</param>
        /// <param name="p">the position of the source at the frame</param>
        /// <returns></returns>
        public static TameEffect Position(Vector3 p)
        {
            TameEffect tp = new TameEffect();
            //tp.effect = effect;
            tp.type = TrackBasis.Object;
            tp.position = p;
            return tp;
        }
        public void Apply()
        {
            TameElement.Apply(this);
        }

    }
}
