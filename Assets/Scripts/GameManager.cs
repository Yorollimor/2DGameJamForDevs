using FMODUnity;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public ResourceManager resourceManager;
    public EventManager eventManager;
    public DialogueManagerWrapper dialogueManager;

    private bool isKingTurn = true;
    public bool isGameOver = false;

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
        isKingTurn = true;
        NextTurn();
    }

    public void NextTurn()
    {
        if (isGameOver) return;

        CheckGameOver();

        var nextEvent = eventManager.GetNextEvent(isKingTurn);
        if (nextEvent != null)
        {
            dialogueManager.StartEventDialogue(nextEvent);
        }

        isKingTurn = !isKingTurn; // Alternate turns
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
