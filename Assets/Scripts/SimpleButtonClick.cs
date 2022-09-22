using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Valve.VR.InteractionSystem.Sample
{
    public class SimpleButtonClick : UIElement
    {
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void OnButtonClick()
        {
            base.OnButtonClick();
        }

        protected override void OnHandHoverBegin(Hand hand)
        {
            base.OnHandHoverBegin(hand);
            gameObject.GetComponent<Image>().color = Color.red;
        }

        protected override void OnHandHoverEnd(Hand hand)
        {
            base.OnHandHoverEnd(hand);
            gameObject.GetComponent<Image>().color = Color.white;
        }
    }
}