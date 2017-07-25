/**
 * Fascilitates an easing mechanism to help with animations.
 */
using UnityEngine;

public abstract class Easing : MonoBehaviour
{
    public AnimationCurve easingCurve;

    [Tooltip("How long the easing should take")]
    public float animationLength;

    [Tooltip("If the gameobject should deactivate after easing.")]
    public bool hideOnFinish = false;

    [Tooltip("If the easing animation should play backwards.")]
    public bool playBackwards = false;

    public delegate void FinsihedEvent();
    public event FinsihedEvent OnFinish;

    private float timer;
    private bool isPlaying = false;

    void Update()
    {
        if (!isPlaying)
            return;
        if (playBackwards)
        {
            updateBackwards();
            return;
        }

        timer += Time.deltaTime;
        if (timer > animationLength)
            timer = animationLength;

        updateAnimation(timer);

        if (timer >= animationLength)
        {
            isPlaying = false;
            if (OnFinish != null)
                OnFinish();
            if (hideOnFinish)
                gameObject.SetActive(false);
        }
    }

    private void updateBackwards()
    {
        timer -= Time.deltaTime;
        if (timer < 0)
            timer = 0;

        updateAnimation(timer);

        if (timer <= 0)
        {
            isPlaying = false;
            if (OnFinish != null)
                OnFinish();
            if (hideOnFinish)
                gameObject.SetActive(false);
        }
    }

    protected abstract void updateAnimation(float timer);

    /**
     * Plays the animation. If a function(s) has been assigned to OnFinish, then they will be called
     * after the animation finishes.
     */
    public void Play()
    {
        Reset();

        isPlaying = true;
        gameObject.SetActive(true);
    }

    /**
     * Resets the animation so it's ready to be played again. This function is first called by Play before the
     * animation starts.
     */
    public void Reset()
    {
        timer = playBackwards ? animationLength : 0;
        resetAnimation();
    }

    protected abstract void resetAnimation();
}
