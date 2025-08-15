using FMODUnity;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public ResourceManager resourceManager;
    public EventManager eventManager;
    public DialogueManager_SelfMade dialogueManager;
    public GameHandler gameHandler;

    public bool isKingTurn = true;
    public bool isGameOver = false;

    public bool isFirstTurn = true;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        isGameOver = false;
        gameHandler.ApplyStartSettings();
        NextTurn();
    }

    public void NextTurn()
    {
        if (isGameOver) return;

        CheckGameOver();
        gameHandler.IncreaseEventCounter(); // Increment the event counter each time a new event is fetched
        var nextEvent = eventManager.GetNextEvent(isKingTurn);

        if (nextEvent != null)
        {
            dialogueManager.StartEventDialogue(nextEvent);
        }
    }

    public void CheckGameOver()
    {
        if (resourceManager.gold <= 0 ||
            resourceManager.food <= 0 ||
            resourceManager.soldiers <= 0 ||
            resourceManager.peasants <= 0)
        {
            EndGame("Your kingdom has fallen due to lack of resources!");
        }
    }

    public void EndGame(string reason)
    {
        isGameOver = true;
        Debug.Log($"GAME OVER: {reason}");
        // TODO: Display game over UI
    }
}
