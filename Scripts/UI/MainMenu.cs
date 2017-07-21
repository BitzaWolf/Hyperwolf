/**
 * Manages the Main Menu UI. 
 */
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject pinkFadein;

    private void Start()
    {
        pinkFadein.GetComponent<EasingTransparency>().OnFinish += OnFadeFinish;
    }

    public void onStartClicked()
    {
        pinkFadein.GetComponent<EasingTransparency>().Play();
    }

    private void OnFadeFinish()
    {
        GameManager gm = GameManager.i();
        gm.loadLevel("Level_001");
    }

    public void onOptionsClicked()
    {

    }

    public void onExitClicked()
    {
        Application.Quit();
    }
}
