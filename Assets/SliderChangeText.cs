using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderChangeText : MonoBehaviour
{
    public TMP_Text TMPText;

    // Start is called before the first frame update
    void Start()
    {
        TMPText = GetComponent<TextMeshProUGUI>();
        TMPText.text = "0°";
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnSliderChange(float value)
    {
        TMPText.text = System.Math.Round(value, 3).ToString() + "°";
    }
}
