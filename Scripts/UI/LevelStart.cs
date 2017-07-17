using UnityEngine;
using UnityEngine.UI;

public class LevelStart : MonoBehaviour
{
    enum State
    {
        SHOWING,
        COUNT_DOWN,
        HIDING
    };
    public Image fadePanel;
    public Text txt_name, txt_countdown;
    public float fadeLength = 0.5f;
    public float countdownLength = 3f;

    private float timer = 0;
    private State curState = State.SHOWING;

    // Use this for initialization
    void Start ()
    {
		
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
        // fade out pink overlay
        // Have title already set and visible
    }

    private void updateCountdown()
    {
        //"Ready?" -- "Wolf!"
        /*
         * 1) Animate in "Ready"
         * 2) Animate out "Ready" - Start Fade out Title
         * 3) Animate in "Wolf!"
         * 4) Start level
         * 5) Transition to hiding
         */
    }

    private void updateHiding()
    {
        // Animate "Wolf!" out of the way quickly
    }

    private void transitionState(State nextSate)
    {
        switch (nextSate)
        {

        }
    }
}
