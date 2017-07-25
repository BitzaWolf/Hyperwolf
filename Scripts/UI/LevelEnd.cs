/**
 * Level end controls the UI at the end of a level. It gets activated by the Game Manager
 * and runs until one of its UI buttons are clicked.
 */
using UnityEngine;
using UnityEngine.UI;

public class LevelEnd : MonoBehaviour
{
    public Text txt_name, txt_time, txt_collectables, txt_deaths;

    /**
     * Triggers the Level End UI panel to be shown.
     */
    public void show()
    {
        GameManager gm = GameManager.i();
        System.TimeSpan span = System.TimeSpan.FromSeconds(gm.levelTimer);
        txt_time.text = string.Format("{0:D2}:{1:D2}:{2:D3}", span.Minutes, span.Seconds, span.Milliseconds);

        txt_name.text = gm.levelMetadata.levelName;
        txt_collectables.text = string.Format("{0} / {1}", gm.collectablesGot, gm.levelMetadata.totalCollectables);

        txt_deaths.text = "" + gm.deaths;

        gameObject.SetActive(true);
    }

    /**
     * Triggers the pink overlay panel to start being shown.
     */
    public void hide()
    {
        GameManager gm = GameManager.i();
        gm.loadingScreen.fadePanel.OnFinish += OnFadeinFinished;
        gm.loadingScreen.fadeIn();
    }

    public void OnNextLevelClicked()
    {
        hide();
    }

    public void OnMainMenuClicked()
    {
        GameManager.i().nextLevel = "Title";
        hide();
    }

    private void OnFadeinFinished()
    {
        GameManager gm = GameManager.i();
        gm.loadingScreen.fadePanel.OnFinish -= OnFadeinFinished;
        gm.loadNextLevel();
        gameObject.SetActive(false);
    }
}
