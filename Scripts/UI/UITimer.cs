using UnityEngine;
using UnityEngine.UI;
using System;

public class UITimer : MonoBehaviour
{
    public Text timer;

	void Update ()
    {
        TimeSpan time = TimeSpan.FromSeconds(GameManager.i().levelTimer);

        timer.text = string.Format("{0:D1}:{1:D2}:{2:D2}", time.Minutes, time.Seconds, time.Milliseconds);
	}
}
