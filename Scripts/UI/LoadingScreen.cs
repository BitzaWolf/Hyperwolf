using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    public EasingTransparency fadePanel;

    private void Start()
    {
        fadePanel.OnFinish += OnFadeFinish;
    }

    public void fadeIn()
    {
        fadePanel.playBackwards = true;
        fadePanel.Play();
        gameObject.SetActive(true);
    }

    public void fadeOut()
    {
        fadePanel.playBackwards = false;
        fadePanel.Play();
        gameObject.SetActive(true);
    }

    private void OnFadeFinish()
    {
        //gameObject.SetActive(false);
    }
}
