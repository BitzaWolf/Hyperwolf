/**
 * Manages the Main Menu UI. 
 */
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    GameManager gm;
    private void Start()
    {
        gm = GameManager.i();
    }

    public void show()
    {
        animateIn();
        gameObject.SetActive(true);
    }

    private void animateIn()
    {
        // TODO animate in menu items
    }

    private void animateOut()
    {
        // TODO animate out menu items
    }

    public void onStartClicked()
    {
        animateOut();
        // TODO after animate out is done...
        gm.loadingScreen.fadePanel.OnFinish += OnFadeinFinish;
        gm.loadingScreen.fadeIn();
        gm.playerWolf.phaseOut();
    }

    public void onOptionsClicked()
    {
        animateOut();
        // TODO show options menu. (Hey... What's -in- the options menu??)
    }

    public void onExitClicked()
    {
        Application.Quit();
    }

    /**
     * After play is selected, time to start loading the first level!
     */
    private void OnFadeinFinish()
    {
        gm.loadingScreen.fadePanel.OnFinish -= OnFadeinFinish;
        gm.loadLevel("Level_001");
        gameObject.SetActive(false);
    }
}
