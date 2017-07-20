/**
 * Changes the alpha color over a period of time following an animation curve.
 */
using UnityEngine;
using UnityEngine.UI;

public class EasingTransparency : Easing
{
    public Text text;

    protected override void updateAnimation(float timer)
    {
        float alpha = easingCurve.Evaluate(timer / animationLength);
        Color c = text.color;
        c.a = alpha;
        text.color = c;
    }

    /**
     * Resets the animation so it's ready to be played again. This function is first called by Play before the
     * animation starts.
     */
    protected override void resetAnimation()
    {
        Color c = text.color;
        c.a = easingCurve.Evaluate(0);
        text.color = c;
    }
}
