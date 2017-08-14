/**
 * Game Manager manages game state and facilitates running the entire show.
 * 
 * @author Anthony 'Bitzawolf' Pepe
 * @date 2017
 */

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /* Based on Unity's default unit to real-world size, I apparently made the assets huge.
     * This means HUGE forces are needed to move them appropriately.*/
    public static float FORCE_MULT = 1000000;

    private static GameManager instance = null;

    private void OnEnable()
    {
        if (instance == null)
            instance = this;
    }

    public static GameManager i()
    {
        return instance;
    }

    /******************
     * Rub-a-dub pubs *
     ******************/
    public enum State
    {
        GAME_INIT,
        MAIN_MENU,
        LEVEL_LOADING,
        LEVEL_STARTING,
        IN_LEVEL,
        LEVEL_ENDING,
        PAUSED
    }

    // These properties pertain to each individual level
    [Header("Per Level")]
    public GameObject spawnPoint = null;
    public GameObject lastCheckpoint = null;
    public float levelTimer = 0;
    public int deaths = 0;
    public int collectablesGot = 0;
    public LevelMetadata levelMetadata;

    // These properties remain the same over the course of the entire game
    [Space]
    [Header("Game Wide")]
    public State currentState = State.GAME_INIT;
    public Camera cam;
    public Follow cameraFollowing;
    public GameObject player;
    public LevelEnd levelEnd;
    public LevelStart levelStart;
    public MainMenu mainMenu;
    public PauseMenu pauseMenu;
    public LoadingScreen loadingScreen;
    public GameObject UIlevelTimer;
    public GameObject eventSystem;
    public string nextLevel;
    public FMOD.Studio.VCA vca_SFX;
    public Wolf playerWolf;

    // Debug settings
    [Space]
    [Header("Debug")]
    public bool debugMode = true;

    /************
     * Privates *
     ************/
    private State previousState = State.GAME_INIT;


    void Start ()
    {
        // Cache important Game State objects
        cameraFollowing = cam.GetComponent<Follow>();
        playerWolf = player.GetComponent<Wolf>();
        FMODUnity.RuntimeManager.StudioSystem.getVCA("SFX", out vca_SFX);
        Debug.Log("VCA: " + vca_SFX);

        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(player);
        DontDestroyOnLoad(cam);
        DontDestroyOnLoad(levelEnd.gameObject);
        DontDestroyOnLoad(levelStart.gameObject);
        DontDestroyOnLoad(mainMenu.gameObject);
        DontDestroyOnLoad(loadingScreen.gameObject);
        DontDestroyOnLoad(pauseMenu.gameObject);
        DontDestroyOnLoad(UIlevelTimer);
        DontDestroyOnLoad(eventSystem);
        SceneManager.sceneLoaded += onSceneLoaded;

        // If we're in debug mode, don't set the current state nor load the Title screen.
        if (debugMode)
        {
            SpawnPoint spawps = FindObjectOfType<SpawnPoint>();
            if (spawps == null)
                Debug.LogWarning("No spawn point found!");
            else
                spawnPoint = spawps.gameObject;

            levelMetadata = FindObjectOfType<LevelMetadata>();
            if (levelMetadata == null)
                Debug.LogWarning("No Level Metadata found!");
        }
        else
        {
            currentState = State.GAME_INIT;
            loadingScreen.gameObject.SetActive(true);
            loadLevel("Title");
        }
	}
	
	void Update ()
    {
		switch (currentState)
        {
            case State.GAME_INIT: break; // no updates
            case State.MAIN_MENU: updateMainMenu(); break;
            case State.LEVEL_LOADING: updateLevelLoading(); break;
            case State.LEVEL_STARTING: updateStartingLevel(); break;
            case State.IN_LEVEL: updateInLevel(); break;
            case State.PAUSED: updatePaused(); break;
            case State.LEVEL_ENDING: updateEndingLevel(); break;
        }
        if (debugMode)
            checkForCheats();
	}

    private void updateMainMenu() { }
    private void updateLevelLoading() { }
    private void updateStartingLevel() { } // see LevelStart

    private void updateInLevel()
    {
        levelTimer += Time.deltaTime;
        if (Input.GetButtonDown("Cancel"))
        {
            pauseGame();
        }
    }

    private void updateEndingLevel() { } // see LevelEnd
    private void updatePaused()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            unPauseGame();
        }
    }

    /**
     * Perform cleanup required for leaving whatever state we're in.
     */
    private void leaveState()
    {
        if (currentState == State.GAME_INIT) { } // intentionally empty
        else if (currentState == State.MAIN_MENU)  { }
        else if (currentState == State.LEVEL_STARTING)
        {
            playerWolf.triggerLevelStarted();
        }
        else if (currentState == State.IN_LEVEL)
        {
            spawnPoint = null;
            lastCheckpoint = null;
        }
        else if (currentState == State.PAUSED)
        {
            playerWolf.setPaused(false);
        }
        previousState = currentState;
    }

    /**
     * Setup and prepare for the next state
     */
    private void transitionState(State nextState)
    {
        leaveState();

        if (nextState == State.LEVEL_STARTING)
        {
            cameraFollowing.target = player;
            levelMetadata = FindObjectOfType<LevelMetadata>();
            SpawnPoint spawn = FindObjectOfType<SpawnPoint>();
            spawnPoint = spawn.gameObject;
            levelTimer = 0;
            deaths = 0;
            playerWolf.setFacingDirection(spawn.facingDirection);
            playerWolf.setPosition(spawn.transform.position);
            playerWolf.setToLevelStartState();
            levelStart.show();
            UIlevelTimer.SetActive(true);
        }
        else if (nextState == State.IN_LEVEL)
        {

        }
        else if (nextState == State.PAUSED)
        {
            playerWolf.setPaused(true);
        }
        else if (nextState == State.LEVEL_ENDING)
        {
            playerWolf.setToLevelEndState();
            levelEnd.show();
        }
        else if (nextState == State.LEVEL_LOADING)
        {
            UIlevelTimer.SetActive(false);
        }
        else if (nextState == State.MAIN_MENU) { }// intentionally empty

        currentState = nextState;
    }

    /**
     * Triggers a level to end.
     * See transitionState for cleanup procedure.
     */
    public void triggerLevelEnd(string nextLevel)
    {
        this.nextLevel = nextLevel;
        transitionState(State.LEVEL_ENDING);
    }

    public void triggerGameStart()
    {
        transitionState(State.IN_LEVEL);
    }

    /**
     * Loads the indicated level, or the main menu if an
     * empty string is passed (default action for Goal objects without
     * an assigned next level.
     */
    public void loadLevel(string levelName)
    {
        if (levelName == "" || levelName == "Title")
        {
            transitionState(State.MAIN_MENU);
            SceneManager.LoadScene("Title");
            return;
        }

        transitionState(State.LEVEL_LOADING);
        SceneManager.LoadScene(levelName);
    }

    /**
     * Loads the next level which is stored inside this manager.
     * If no nextLevel has been assigned yet, a warning is produced
     * and nothing happens.
     */
    public void loadNextLevel()
    {
        if (nextLevel == "")
        {
            Debug.LogWarning("Tried to load next level, but no next level set!");
        }

        loadLevel(nextLevel);
        nextLevel = "";
    }

    /**
     * Opens the pause menu and changes the game state to paused, saving whatever state the game is currently in.
     * If the game state is already paused, nothing happens.
     */
    public void pauseGame()
    {
        if (currentState == State.PAUSED)
            return;

        pauseMenu.Show();
        transitionState(State.PAUSED);
    }

    /**
     * Closes the pause menu and resumes the current level/state.
     * If the game state is not currently paused, this method does nothing.
     */
    public void unPauseGame()
    {
        if (currentState != State.PAUSED)
            return;

        pauseMenu.Hide();
        transitionState(previousState);
    }

    /**
     * Immediately ends the game, cleaning up any resources as needed.
     */
    public void quitGame()
    {
        Debug.Log("Quitting the game.");
        Application.Quit();
    }

    /**
     * This is called from Unity's C# delegate event for a scene
     * being loaded.
     */
    public void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        loadingScreen.fadeOut();
        if (currentState == State.LEVEL_LOADING)
        {
            transitionState(State.LEVEL_STARTING);
        }
        else if (currentState == State.MAIN_MENU)
        {
            cameraFollowing.target = player;
            playerWolf.setToLevelStartState();
            SpawnPoint spawn = FindObjectOfType<SpawnPoint>();
            spawnPoint = spawn.gameObject;
            playerWolf.setFacingDirection(spawn.facingDirection);
            playerWolf.setPosition(spawn.transform.position);
            loadingScreen.fadePanel.OnFinish += OnMainMenuFadeFinished;
        }
    }

    private void OnMainMenuFadeFinished()
    {
        loadingScreen.fadePanel.OnFinish -= OnMainMenuFadeFinished;
        mainMenu.show();
    }

    /**
     * This is a list of cheats developers can use to assist in developing
     * the game and level(s). This function is only called from the update loop
     * if debugMode is true!
     */
    private void checkForCheats()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {

        }
        if (Input.GetKeyDown(KeyCode.F2))
        {

        }
    }
}
