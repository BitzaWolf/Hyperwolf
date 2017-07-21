/**
 * Level Start controls the level starting UI. It gets activated by the Game Manager
 * and actually kicks off a level of the game.
 */

using UnityEngine;
using UnityEngine.UI;

public class LevelStart : MonoBehaviour
{
    public Image fadePanel, pnl_Text;
    public Text txt_name, txt_ready, txt_go;

    private bool secondPanelMovement = false;

    void Start()
    {
        fadePanel.GetComponent<EasingTransparency>().OnFinish += onFadeFinish;
        pnl_Text.GetComponent<EasingTranslate>().OnFinish += onPanelFinished;
        txt_ready.GetComponent<EasingTranslate>().OnFinish += onReadyFinished;
        txt_go.GetComponent<EasingTranslate>().OnFinish += onGoFinished;
    }

    /**
     * Resets the intro UI state assuming we came from level loading,
     * so the pink panel is set to full opacity and the Ready Wolf! text
     * is hidden.
     */
    public void show()
    {
        fadePanel.GetComponent<Easing>().Play();

        txt_name.text = GameManager.i().levelMetadata.levelName;
        txt_name.GetComponent<Easing>().Reset();

        secondPanelMovement = false;
        gameObject.SetActive(true);
    }

    private void onFadeFinish()
    {
        pnl_Text.GetComponent<Easing>().Play();
        txt_name.GetComponent<Easing>().Play();
    }

    private void onPanelFinished()
    {
        if (secondPanelMovement)
        {
            gameObject.SetActive(false);
            EasingTranslate e = pnl_Text.GetComponent<EasingTranslate>();
            e.UndoTranslation();
            e.UndoTranslation(); // undo twice because we call the animation twice
            e = txt_ready.GetComponent<EasingTranslate>();
            e.UndoTranslation();
            e = txt_go.GetComponent<EasingTranslate>();
            e.UndoTranslation();
        }
        else
        {
            txt_ready.GetComponent<Easing>().Play();
            secondPanelMovement = true;
        }
    }

    private void onReadyFinished()
    {
        txt_go.GetComponent<EasingTranslate>().Play();
    }

    private void onGoFinished()
    {
        pnl_Text.GetComponent<Easing>().Play();
        GameManager.i().triggerGameStart();
    }
}
