/**
 * Level Start controls the level starting UI. It gets activated by the Game Manager
 * and actually kicks off a level of the game.
 */

using UnityEngine;
using UnityEngine.UI;

public class LevelStart : MonoBehaviour
{
    public Animator anim_container, anim_ready, anim_go;
    [Tooltip("The level title.")]
    public Text txt_title;
    public float
        panelInTime,
        readyInTime,
        readyOutTime,
        goInTime,
        goOutTime;

    private enum State
    {
        READY,
        PANEL_IN,
        READY_IN,
        READY_OUT,
        GO_IN,
        GO_OUT,
        ALL_OUT
    }

    private State currentState = State.READY;
    private float timer;

    void Update()
    {
        if (currentState == State.READY)
            return;

        timer += Time.deltaTime;

        switch (currentState)
        {
            case State.PANEL_IN:
                if (timer >= panelInTime)
                {
                    anim_ready.SetTrigger("Fly");
                    currentState = State.READY_IN;
                    timer = 0;
                }
                break;

            case State.READY_IN:
                if (timer >= readyInTime)
                {
                    anim_ready.SetTrigger("Fly");
                    currentState = State.READY_OUT;
                    timer = 0;
                }
                break;

            case State.READY_OUT:
                if (timer >= readyOutTime)
                {
                    anim_go.SetTrigger("Fly");
                    currentState = State.GO_IN;
                    timer = 0;
                }
                break;

            case State.GO_IN:
                if (timer >= goInTime)
                {
                    anim_go.SetTrigger("Fly");
                    anim_container.SetTrigger("Fly");
                    currentState = State.GO_OUT;
                    timer = 0;
                    GameManager.i().triggerGameStart();
                }
                break;

            case State.GO_OUT:
                if (timer >= goOutTime)
                {
                    anim_container.SetTrigger("Fly");
                    anim_ready.SetTrigger("Fly");
                    anim_go.SetTrigger("Fly");
                    currentState = State.READY;
                    timer = 0;
                    gameObject.SetActive(false);
                }
                break;
        }
    }

    /**
     * Resets the intro UI state assuming we came from level loading,
     * so the pink panel is set to full opacity and the Ready Wolf! text
     * is hidden.
     */
    public void show()
    {
        GameManager.i().loadingScreen.fadePanel.OnFinish += onPinkFadeFinish; // already fading out, from GM transition state

        txt_title.text = GameManager.i().levelMetadata.levelName;
        txt_title.GetComponent<Easing>().Reset();
        
        gameObject.SetActive(true);
    }

    private void onPinkFadeFinish()
    {
        GameManager.i().loadingScreen.fadePanel.OnFinish -= onPinkFadeFinish;
        txt_title.GetComponent<Easing>().Play();
        anim_container.SetTrigger("Fly");
        currentState = State.PANEL_IN;
    }
}
