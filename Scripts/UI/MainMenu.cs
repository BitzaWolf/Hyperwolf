/**
 * Manages the Main Menu UI. 
 */
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void onStartClicked()
    {
        GameManager gm = GameManager.i();
        // gm.loadLevel("001");
    }

    public void onOptionsClicked()
    {

    }

    public void onExitClicked()
    {
        Application.Quit();
    }
}
