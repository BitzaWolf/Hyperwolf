/**
 * Translates an object over a period of time following an animation curve.
 */
using UnityEngine;

public class EasingTranslate : Easing
{
    public Vector3 translateDirection;
    public float translationAmount;

    [Tooltip("If the translation should use its local coordinate system or global coordinates.")]
    public bool useLocalSpace = true;
    
    private float previousVal;
    private Space space;

    protected override void updateAnimation(float timer)
    {
        // traditionally, a translation is better done as an offset from some original
        // position, but with gameobjects that may move in other ways (like UI elements)
        // we can't just track an original position. It's better to apply a small translation
        // from wherever the object currently is. This means subtracting the translation
        // already accomplished from where it should be now, thus translation = previous - current.
        float val = easingCurve.Evaluate(timer / animationLength) * translationAmount;
        float translation = val - previousVal;
        
        gameObject.transform.Translate(translateDirection * translation, space);

        previousVal = val;
	}

    /**
     * Resets the animation so it's ready to be played again. This function is first called by Play before the
     * animation starts.
     */
    protected override void resetAnimation()
    {
        space = useLocalSpace ? Space.Self : Space.World;
        previousVal = easingCurve.Evaluate(0) * translationAmount;
    }

    /**
     * Subtracts the effect of this translation from the Game Object's current position. If called once
     * after the animation is completed, it'll undo the animation effect. If called multiple times, then
     * the game object will keep moving backwards. This is because we don't track the object's initial
     * position so there's no way to know for sure where the object was.
     */
    public void UndoTranslation()
    {
        gameObject.transform.Translate(translateDirection * -translationAmount, space);
    }
}
