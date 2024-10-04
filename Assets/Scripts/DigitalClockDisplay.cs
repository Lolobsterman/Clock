using System;
using TMPro;
using UnityEngine;

public class DigitalClockDisplay : MonoBehaviour
{
    public TextMeshProUGUI digitalClockText;

    public void UpdateClock(DateTime currentTime)
    {
        string timeString = currentTime.ToString("HH:mm:ss");
        digitalClockText.text = timeString;
    }
}
