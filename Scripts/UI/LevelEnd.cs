/**
 * Level end controls the UI at the end of a level. It gets activated by the Game Manager
 * and runs until one of its UI buttons are clicked.
 */
using UnityEngine;
using UnityEngine.UI;

public class LevelEnd : MonoBehaviour
{
    enum State
    {
        Showing,
        Hiding
    };
    public Image fadePanel, fadeoutPanel;
    public Text txt_name, txt_time, txt_collectables, txt_deaths;

    [Tooltip("How long the UI should take to fade in.")]
    public float levelEndLength = 2f;

    [Tooltip("How long the UI should take to fade to pink (menu ending).")]
    public float levelEndOutLength = 0.5f;
    
    [Tooltip("How translucent the UI's Background should be")]
    [Range(0, 1)]
    public float maxAlpha = 0.576f;
    
    private float timer = 0;
    private State curState = State.Showing;

    void Update()
    {
        switch (curState)
        {
            case State.Showing: updateShowing(); break;
            case State.Hiding: updateHiding(); break;
        }
    }

    /**
     * Slowly ramps up the UI's background panel alpha until it's at maxAlpha.
     */
    private void updateShowing()
    {
        if (timer >= levelEndLength)
            return;

        timer += Time.deltaTime;
        if (timer > levelEndLength)
            timer = levelEndLength;

        float alpha = (timer / levelEndLength) * maxAlpha;
        Color c = fadePanel.color;
        c.a = alpha;
        fadePanel.color = c;
	}

    /**
     * Slowly ramps up the pink UI panel overlay, which is used to mask a
     * level unloading and then loading.
     */
    private void updateHiding()
    {
        if (timer >= levelEndOutLength)
        {
            GameManager.i().loadNextLevel();
            fadeoutPanel.gameObject.SetActive(false);
            gameObject.SetActive(false);
            return;
        }

        timer += Time.deltaTime;
        if (timer > levelEndOutLength)
            timer = levelEndOutLength;

        float alpha = (timer / levelEndOutLength);
        Color c = fadeoutPanel.color;
        c.a = alpha;
        fadeoutPanel.color = c;
    }

    /**
     * Triggers the Level End UI panel to start being shown.
     * See updateShowing()
     */
    public void show()
    {
        Color c = fadePanel.color;
        c.a = 0;
        fadePanel.color = c;
        timer = 0;
        curState = State.Showing;

        GameManager gm = GameManager.i();
        System.TimeSpan span = System.TimeSpan.FromSeconds(gm.levelTimer);
        txt_time.text = string.Format("{0:D2}:{1:D2}:{2:D3}", span.Minutes, span.Seconds, span.Milliseconds);

        txt_name.text = gm.levelMetadata.levelName;
        txt_collectables.text = string.Format("{0} / {1}", gm.collectablesGot, gm.levelMetadata.totalCollectables);

        txt_deaths.text = "" + gm.deaths;

        gameObject.SetActive(true);
    }

    /**
     * Triggers the pink overlay panel to start being shown.
     * The pink panel is used to mask the game unloading and loading
     * scenes.
     * See updateHiding()
     */
    public void hide()
    {
        fadeoutPanel.gameObject.SetActive(true);
        timer = 0;
        curState = State.Hiding;
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
}
