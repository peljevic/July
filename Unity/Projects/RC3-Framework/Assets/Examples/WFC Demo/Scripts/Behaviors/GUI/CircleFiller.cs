using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleFiller : MonoBehaviour
{
    public Image circle;
    public float percentage;
    public RectTransform position;
   

    private void Awake()
    {
        
    }

    public float FillingPercent
    {
        get {return percentage; }
        set { percentage = value; }
    }

    void CirclePercentage()
    {
       circle.fillAmount = percentage*360;
    }
}
