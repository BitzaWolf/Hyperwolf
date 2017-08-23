/**
 * Manages the Main Menu UI. 
 */
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public Animator buttonContainer;
    public Animator titleContainer;
    [Tooltip("How many seconds to wait inbetween triggering the title and then the button panel.")]
    public float waitTime;

    public delegate void FlyOutFinsihedEvent();
    public event FlyOutFinsihedEvent OnAnimationOutFinish;

    private enum State
    {
        ANIMATING_IN,
        ANIMATING_OUT,
        IDLE
    }

    private GameManager gm;
    private State state = State.IDLE;
    private bool animatingIn;
    private bool initialized = false;
    private float waitTimer = 0;

    private void Start()
    {
        gm = GameManager.i();
        OnAnimationOutFinish += onFlyOutFinish;
    }

    /**
     * Displays the main menu, animating in the title and buttons.
     */
    public void show()
    {
        gameObject.SetActive(true);
        waitTimer = 0;
        state = State.ANIMATING_IN;

        if (!initialized)
        {
            titleContainer.gameObject.SetActive(true);
        }
        else
        {
            titleContainer.SetTrigger("Fly Out");
        }
    }
    
    /**
     * Hides the main menu, animating out the title and buttons.
     */
    public void hide()
    {
        waitTimer = 0;
        state = State.ANIMATING_OUT;
        buttonContainer.SetTrigger("Fly Out");
    }

    private void Update()
    {
        switch(state)
        {
            case State.ANIMATING_IN: updateAnimateIn(); break;
            case State.ANIMATING_OUT: updateAnimateOut(); break;
            case State.IDLE: break; // Do nothing
        }
    }

    private void updateAnimateIn()
    {
        waitTimer += Time.deltaTime;
        if (waitTimer >= waitTime)
        {
            if (!initialized)
            {
                buttonContainer.gameObject.SetActive(true);
                initialized = true;
            }
            else
            {
                buttonContainer.SetTrigger("Fly Out");
            }
            state = State.IDLE;
        }
    }

    private void updateAnimateOut()
    {
        waitTimer += Time.deltaTime;
        if (waitTimer >= waitTime)
        {
            titleContainer.SetTrigger("Fly Out");
            state = State.IDLE;
            OnAnimationOutFinish();
        }
    }

    public void onStartClicked()
    {
        hide();
    }

    public void onOptionsClicked()
    {
        // TODO show options menu. (Hey... What's -in- the options menu??)
    }

    public void onExitClicked()
    {
        Application.Quit();
    }

    private void onFlyOutFinish()
    {
        gm.loadingScreen.fadePanel.OnFinish += OnFadeinFinish;
        gm.loadingScreen.fadeIn();
        gm.playerWolf.setToLevelEndState();
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
