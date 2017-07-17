﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
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
        MAIN_MENU,
        LEVEL_LOADING,
        LEVEL_STARTING,
        IN_LEVEL,
        LEVEL_ENDING,
        PAUSED
    }

    [Header("Per Level")]
    public GameObject spawnPoint = null;
    public GameObject lastCheckpoint = null;
    public float levelTimer = 0;
    public int deaths = 0;
    public LevelMetadata levelMetadata;

    [Space]
    [Header("Game Wide")]
    public State currentState = State.IN_LEVEL;
    public Camera cam;
    public Follow cameraFollowing;
    public GameObject player;
    public LevelEnd levelEnd;
    public LevelStart levelStart;
    public GameObject eventSystem;
    public string nextLevel;
    public FMOD.Studio.VCA vca_SFX;
    public Wolf playerWolf;

    [Space]
    [Header("Debug")]
    public bool debugMode = true;


    void Start ()
    {
        cameraFollowing = cam.GetComponent<Follow>();
        playerWolf = player.GetComponent<Wolf>();
        FMODUnity.RuntimeManager.StudioSystem.getVCA("SFX", out vca_SFX);
        Debug.Log("VCA: " + vca_SFX);

        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(player);
        DontDestroyOnLoad(cam);
        DontDestroyOnLoad(levelEnd.gameObject);
        DontDestroyOnLoad(eventSystem);
        SceneManager.sceneLoaded += onSceneLoaded;

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
            currentState = State.MAIN_MENU;
            SceneManager.LoadScene("Title");
        }
	}
	
	void Update ()
    {
		switch (currentState)
        {
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

    private void updateLevelLoading() { }

    private void updateStartingLevel()
    {
        // TODO -Fast- countdown timer, then transition to IN LEVEL
    }

    private void updateInLevel()
    {
        levelTimer += Time.deltaTime;
    }

    private void updateEndingLevel() { }
    private void updateMainMenu() { }
    private void updatePaused() { }

    /**
     * Cleanup from this current state.
     */
    private void leaveState()
    {
        if (currentState == State.MAIN_MENU)
        {
            cam.enabled = true;
            player.SetActive(true);
        }
        else if (currentState == State.IN_LEVEL)
        {
            spawnPoint = null;
            lastCheckpoint = null;
        }
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
            levelTimer = 0;
            deaths = 0;
            levelMetadata = FindObjectOfType<LevelMetadata>();
            SpawnPoint spawn = FindObjectOfType<SpawnPoint>();
            spawnPoint = spawn.gameObject;
            player.GetComponent<Wolf>().turn(spawnPoint.transform.position, spawn.faceLeft);
        }
        else if (nextState == State.IN_LEVEL)
        {
            // TODO start timer
        }
        else if (nextState == State.LEVEL_ENDING)
        {
            levelEnd.show();
        }
        else if (nextState == State.MAIN_MENU)
        {
            cam.gameObject.SetActive(false);
            player.SetActive(false);
            SceneManager.LoadScene("Title");
        }

        currentState = nextState;
    }

    public void triggerLevelEnd(string nextLevel)
    {
        this.nextLevel = nextLevel;
        transitionState(State.LEVEL_ENDING);
    }

    public void triggerGamestart()
    {
        transitionState(State.LEVEL_STARTING);
        cam.gameObject.SetActive(true);
        player.SetActive(true);
        SceneManager.LoadScene("Level_001");
    }

    public void loadLevel(string levelName)
    {
        if (levelName == "")
        {
            transitionState(State.MAIN_MENU);
            return;
        }

        // TODO check first if we're in the

        transitionState(State.LEVEL_LOADING);
        SceneManager.LoadScene(levelName);
    }

    public void loadNextLevel()
    {
        if (nextLevel == "")
        {
            Debug.LogWarning("Tried to load next level, but no next level set!");
            return;
        }

        loadLevel(nextLevel);
        nextLevel = "";
    }

    public void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (currentState == State.LEVEL_LOADING)
        {
            transitionState(State.LEVEL_STARTING); // TODO transition to LEVEL_STARTING first, then after into go to IN_LEVEL
        }
    }

    private void checkForCheats()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Goal g = FindObjectOfType<Goal>();
            triggerLevelEnd(g.nextLevel);
        }
    }
}