using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public ResourceManager resourceManager;
    public EventManager eventManager;
    public DialogueManager_SelfMade dialogueManager;
    public GameHandler gameHandler;

    public GameObject gameOverScreen;
    public GameObject winScreen;

    public EventType currentEventType = EventType.PeasantEvent; // Current event type, starts with King events
    public bool isGameOver = false;

    public bool isFirstTurn = true;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        resourceManager = GetComponent<ResourceManager>();
        eventManager = GetComponent<EventManager>();
        dialogueManager = GetComponent<DialogueManager_SelfMade>();
        gameHandler = GetComponent<GameHandler>();
        if (dialogueManager.dialoguePanel == null) Debug.LogError("DialoguePanel is NULL inside DialogueManager");
    }

    int startCounter = 0;
    void Start()
    {
        gameOverScreen.SetActive(false);
        winScreen.SetActive(false);
        print("Start function called!" + startCounter);
        startCounter++;
        StartGame();
    }

    public void StartGame()
    {
        gameHandler.SwitchOffDialoguePanel();

        isGameOver = false;
        gameHandler.ApplyStartSettings();
        if (isFirstTurn)
        {
            gameHandler.StartFirstDay(); // Start the first day of the game
        }
    }

    public void NextTurn(List<EventData> possibleEvents, Character_Data speaker)
    {
        if (isGameOver) return;

        CheckGameOver();

        gameHandler.SwitchOnDialoguePanel();
        var nextEvent = eventManager.GetNextEvent(currentEventType, possibleEvents, speaker);
        if(nextEvent == null)
        {
            Debug.LogWarning("No valid event found for the current event type and speaker.");
            return;
        }

        if (nextEvent != null)
        {
            dialogueManager.StartEventDialogue(nextEvent);
        }
    }

    private void Update()
    {
        /*
        if(Input.GetKeyDown(KeyCode.G))
        {
            CheckGameOver(true, "You pressed the G key to end the game.");
        }
        if(Input.GetKeyDown(KeyCode.W))
        {
            WinGame();
        }
        */
    }

    public void WinGame()
    {
        winScreen.SetActive(true);
        winScreen.GetComponentInChildren<GameOverStats>().SetGameOverMessage("Congratulations! You have successfully led your kingdom to prosperity!");
    }

    public void CheckGameOver(bool isOver = false, string overMessage = "")
    {
        if(isOver)
        {
            EndGame(overMessage);
            return;
        }
        
        if (resourceManager.food <= 0 ||
            resourceManager.population <= 0)
        {
            EndGame("Your kingdom has fallen due to lack of essential resources!");
        }
    }

    public void EndGame(string reason)
    {
        isGameOver = true;
        Debug.Log($"GAME OVER: {reason}");
        gameOverScreen.SetActive(true);
        gameOverScreen.GetComponentInChildren<GameOverStats>().SetGameOverMessage(reason);
        // TODO: Display game over UI
    }
}
