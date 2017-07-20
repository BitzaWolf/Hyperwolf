/**
 * Level Start controls the level starting UI. It gets activated by the Game Manager
 * and actually kicks off a level of the game.
 */

using UnityEngine;
using UnityEngine.UI;

public class LevelStart : MonoBehaviour
{
    // This is how many Unity Units to offset UI elements by so they're hidden.
    private const float HIDDEN_OFFSET = 150f;

    enum State
    {
        SHOWING,    // Showing is the level after loading and it removes the pink overlay panel.
        COUNT_DOWN, // Countdown is a quick countdown. Ready, go!
        HIDING      // Hiding is after the countdown. Quickly remove the intro UI.
    };
    public Image fadePanel, pnl_Text;
    public Text txt_name, txt_ready, txt_go;
    public float fadeLength = 0.5f;
    
    private float timer = 0;
    private State curState = State.HIDING;

    void Start()
    {
        pnl_Text.GetComponent<EasingTranslate>().OnFinish += onPanelFinished;
        txt_ready.GetComponent<EasingTranslate>().OnFinish += onReadyFinished;
        txt_go.GetComponent<EasingTranslate>().OnFinish += onGoFinished;
    }

    void Update ()
    {
        switch (curState)
        {
            case State.SHOWING: updateShowing(); break;
            case State.COUNT_DOWN: updateCountdown(); break;
            case State.HIDING: updateHiding(); break;
        }
	}

    private void updateShowing()
    {
        timer += Time.deltaTime;
        if (timer > fadeLength)
            timer = fadeLength;

        // fade out the pink overlay
        float alpha = timer / fadeLength;
        alpha = 1 - alpha * alpha;
        Color c = fadePanel.color;
        c.a = alpha;
        fadePanel.color = c;

        // if the pink is done, switch to a level countdown.
        if (timer >= fadeLength)
            transitionState(State.COUNT_DOWN);
    }

    private void updateCountdown()
    {

    }

    private void updateHiding()
    {

    }

    private void transitionState(State nextSate)
    {
        switch (nextSate)
        {
            case State.COUNT_DOWN:
                fadePanel.enabled = false;
                pnl_Text.GetComponent<EasingTranslate>().Play();
                txt_name.GetComponent<EasingTransparency>().Play();
                timer = 0;
                break;
            case State.HIDING:
                GameManager.i().triggerGameStart();
                pnl_Text.GetComponent<EasingTranslate>().Play();
                timer = 0;
                break;
        }
        curState = nextSate;
    }

    /**
     * Resets the intro UI state assuming we came from level loading,
     * so the pink panel is set to full opacity and the Ready Wolf! text
     * is hidden.
     */
    public void show()
    {
        fadePanel.enabled = true;
        Color c = fadePanel.color;
        c.a = 1;
        fadePanel.color = c;

        c = txt_name.color;
        c.a = 1;
        txt_name.color = c;
        txt_name.text = GameManager.i().levelMetadata.levelName;
        txt_name.GetComponent<EasingTransparency>().Reset();
        timer = 0;
        
        curState = State.SHOWING;
        gameObject.SetActive(true);
    }

    private void onPanelFinished()
    {
        if (curState == State.COUNT_DOWN)
            txt_ready.GetComponent<EasingTranslate>().Play();
        else if (curState == State.HIDING)
        {
            gameObject.SetActive(false);
            EasingTranslate e = pnl_Text.GetComponent<EasingTranslate>();
            e.UndoTranslation();
            e.UndoTranslation(); // undo twice because we call the animation twice
        }
    }

    private void onReadyFinished()
    {
        txt_go.GetComponent<EasingTranslate>().Play();
    }

    private void onGoFinished()
    {
        transitionState(State.HIDING);
    }
}
