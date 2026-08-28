using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.UI.MTP
{
    public class SliderValue : MonoBehaviour
    {
        public Slider mainSlider;
        public TextMeshProUGUI valueText;

        public void GetValue(float tempValue)
        {
            valueText.text = tempValue.ToString("F1") + "x";
        }
    }
}