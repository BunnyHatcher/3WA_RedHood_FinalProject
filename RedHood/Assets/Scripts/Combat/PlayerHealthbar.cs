using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthbar : MonoBehaviour
{
    public Slider slider;
    public Image fill;
    //public Gradient gradient;

    public void SetMaxHealth(float health)
    {
        slider.maxValue = health;
        slider.value = health;

        //gradient.Evaluate(1f);
    }

    public void SetHealth(float health)
    {

        slider.value = health;

        //fill.color = gradient.Evaluate(slider.normalizedValue);

    }

}
