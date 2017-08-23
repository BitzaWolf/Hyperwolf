/**
 * Level end controls the UI at the end of a level. It gets activated by the Game Manager
 * and runs until one of its UI buttons are clicked.
 */
using UnityEngine;
using UnityEngine.UI;

public class LevelEnd : MonoBehaviour
{
    public Animator
            anim_time,
            anim_collectables,
            anim_deaths,
            anim_nextLevel,
            anim_mainMenu;

    [Tooltip("How many seconds to wait between flying in each UI component.")]
    public float flyInDelay = 0.1f;

    [Tooltip("How many seconds to wait between flying each UI component out.")]
    public float flyOutDelay = 0.05f;

    // Grab these so we can display the game-state values.
    public Text txt_name, txt_time, txt_collectables, txt_deaths;

    private enum State
    {
        IDLE,
        FLYING_IN,
        FLYING_OUT
    }

    private State currentState;
    private Animator[] animators;
    private int currentAnimClip = 0;
    private float timer;
    private bool initialized = false;

    void Update()
    {
        if (currentState == State.IDLE)
            return;

        timer += Time.deltaTime;
        float delay = (currentState == State.FLYING_IN) ? flyInDelay : flyOutDelay;
        
        if (timer >= delay)
        {
            ++currentAnimClip;
            if (currentAnimClip >= animators.Length)
            {
                animationIsDone();
                return;
            }

            animators[currentAnimClip].SetTrigger("Fly");
            timer = 0;
        }
    }

    /**
     * Cleans up state after an animation, fly-in or fly-out has finished running.
     */
    private void animationIsDone()
    {
        if (currentState == State.FLYING_IN)
        {
            // intentionally empty
        }
        else if (currentState == State.FLYING_OUT)
        {
            GameManager gm = GameManager.i();
            gm.loadingScreen.fadePanel.OnFinish += OnFadeinFinished;
            gm.loadingScreen.fadeIn();
        }

        currentState = State.IDLE;
    }

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

        /*
         * There's a problem where show() is called first before the object is enabled. This creates a sort
         * of race condition where Start probably hasn't been called yet, but we're relying on that initialization first.
         * Putting the initialization here ensures the array is ready to go.
         */
        if (!initialized)
        {
            currentState = State.IDLE;
            animators = new Animator[]
                {
                    anim_time,
                    anim_collectables,
                    anim_deaths,
                    anim_nextLevel,
                    anim_mainMenu
                };
            initialized = true;
        }

        gameObject.SetActive(true);
        currentAnimClip = 0;
        animators[0].SetTrigger("Fly");
        currentState = State.FLYING_IN;
    }

    /**
     * Triggers the pink overlay panel to start being shown.
     */
    public void hide()
    {
        currentAnimClip = 0;
        animators[0].SetTrigger("Fly");
        currentState = State.FLYING_OUT;
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
        // reset UI animations
        foreach (Animator a in animators)
        {
            Debug.Log(a);
            a.SetTrigger("Fly");
        }
        gameObject.SetActive(false);
    }
}
