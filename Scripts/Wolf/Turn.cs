/**
 * Turn triggers help guide the player in turning left or right so they
 * don't fall off the track as easily. If the player could turn whenever they
 * wanted, it would be too easy to fall off the track due to the visuals
 * giving a poor depth perception.
 * 
 * There are two main categories of turns in the game, separate tracks to
 * turn down and large areas where the player is free to turn as much as they
 * want.
 * 
 * Only turning once, centering on the turn, and limiting the turn direction options
 * is best for turning down the small track paths. The free turning area would definitely
 * not want to be centered on turning nor limit the number of turns nor directions.
 */
using UnityEngine;

public class Turn : MonoBehaviour
{
    static Color gizmoColor = new Color(66f / 255, 244f / 255, 232f / 255, 0.5f);

    [Tooltip("If the player can only turn once using this trigger.")]
    public bool onlyTurnOnce = true;
    [Tooltip("If the player should be centered to the trigger after turning.")]
    public bool centerOnTurn = true;
    [Header("Limit Turn Directions")]
    [Tooltip("Allow the player to face positive X")]
    public bool allowUpRight = true;
    [Tooltip("Allow the player to face positive Z")]
    public bool allowUpLeft = true;
    [Tooltip("Allow the player to face negative Z")]
    public bool allowDownRight = true;
    [Tooltip("Allow the player to face negative X")]
    public bool allowDownLeft = true;


    /**
     * Checks to see if this turn trigger lets objects turn to face the intended direction.
     * Returns true if this turn trigger will allow the object to face the intended direction.
     */
    public bool allowsDirection(Wolf.FacingDir direction)
    {
        switch(direction)
        {
            case Wolf.FacingDir.UP_RIGHT:   return allowUpRight;
            case Wolf.FacingDir.UP_LEFT:    return allowUpLeft;
            case Wolf.FacingDir.DOWN_RIGHT: return allowDownRight;
            case Wolf.FacingDir.DOWN_LEFT:  return allowDownLeft;
            default:
                Debug.LogWarning("Unknown direction in Turn.allowsDirection: " + direction + ".");
                return false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}
