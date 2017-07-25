/**
 * Changes the alpha color over a period of time following an animation curve.
 */
using UnityEngine;
using UnityEngine.UI;

public class EasingTransparency : Easing
{
    public Text text = null;
    public Image image = null;

    protected override void updateAnimation(float timer)
    {
        float alpha = easingCurve.Evaluate(timer / animationLength);
        Debug.Log("a:" + alpha);

        if (text != null)
        {
            Color c = text.color;
            c.a = alpha;
            text.color = c;
        }

        if (image != null)
        {
            Color c = image.color;
            c.a = alpha;
            image.color = c;
        }
    }

    /**
     * Resets the animation so it's ready to be played again. This function is first called by Play before the
     * animation starts.
     */
    protected override void resetAnimation()
    {
        float resetVal = playBackwards ? animationLength : 0;

        if (text != null)
        {
            Color c = text.color;
            c.a = easingCurve.Evaluate(resetVal);
            text.color = c;
        }

        if (image != null)
        {
            Color c = image.color;
            c.a = easingCurve.Evaluate(resetVal);
            image.color = c;
        }
    }
}
