using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public Text lbl_levelTitle;
    public Text lbl_CollectablesCounter;

    private void Update()
    {
        LevelMetadata meta = GameManager.i().levelMetadata;
        lbl_CollectablesCounter.text = string.Format("{0}/{1}", GameManager.i().collectablesGot, meta.totalCollectables);
    }

    public void Show()
    {
        lbl_levelTitle.text = GameManager.i().levelMetadata.levelName;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void OnResumeClicked()
    {
        GameManager.i().unPauseGame();
    }

    public void OnMainMenuClicked()
    {
        GameManager.i().loadingScreen.fadeIn();
        GameManager.i().loadingScreen.fadePanel.OnFinish += OnFadeInFinish;
    }

    public void OnQuitClicked()
    {
        GameManager.i().quitGame();
    }

    private void OnFadeInFinish()
    {
        Hide();
        GameManager.i().loadingScreen.fadePanel.OnFinish -= OnFadeInFinish;
        GameManager.i().loadLevel("Title");
    }
}
