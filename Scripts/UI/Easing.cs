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

    public delegate void FinsihedEvent();
    public event FinsihedEvent OnFinish;

    private float timer;
    private bool isPlaying = false;

    void Update()
    {
        if (!isPlaying)
            return;

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
        timer = 0;
        resetAnimation();
    }

    protected abstract void resetAnimation();
}
