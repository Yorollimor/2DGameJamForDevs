using System;
using System.Collections;
using UnityEngine;

public class DialogueManagerEventArgs : EventArgs
{
    public EventData sentEvent { get; set; }
    public DialogueManagerEventArgs(EventData eventData)
    {
        sentEvent = eventData;
    }
}
public class DialogueManager_SelfMade : MonoBehaviour
{
    [Header("Event Settings")]
    private EventData currentEvent;
    public GameObject dialoguePanel;
    public GameObject optionsPanel;

    [Header("Normal Dialogue Settings")]
    public GameObject normalDialoguePanel;
    public GameObject normalPlayerDialoguePanel;

    public delegate void ExitEvent(object sender, EventArgs e);
    public ExitEvent onExitAnimationDone;

    /// <summary>
    /// Represents the dialogue component used to manage and display dialogue in the application.
    /// </summary>
    /// <remarks>This component is typically used to handle dialogue interactions, including displaying text,
    /// managing dialogue sequences.</remarks>
    DialogueLogic dialogueComponent;

    /// <summary>
    /// The first dimension represents the speaker index, and the second dimension represents the line count for that speaker.
    /// </summary>
    int[,] speakerCounts = new int[2,2];

    private void Awake()
    {
        dialogueComponent = dialoguePanel.GetComponent<DialogueLogic>();
    }

    public DialogueLogic GetDialogueLogicHandler()
    { return dialogueComponent; }

    public void StartEventDialogue(EventData eventData)
    {
        ResetDialogueEvent();

        dialogueComponent.InitializeEventData(eventData);

        dialogueComponent.OnDialogueEnd += CallNextTurn; // Subscribe to the dialogue end event
        currentEvent = eventData;      
    } 

    private void CallNextTurn(object sender, System.EventArgs e)
    {
        dialogueComponent.OnDialogueEnd -= CallNextTurn;
        TriggerExitAnimation();
        StartCoroutine(NextTurnAfterAnimationEnds()); // Ensure the exit animation is complete before proceeding
    }

    private IEnumerator NextTurnAfterAnimationEnds()
    {
        dialogueComponent.stopInteraction = true;
        float animationDuration = GameManager.Instance.eventManager.speakingCharacter.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;
        float timer = 0f;
        //float lerpCounter = 0f;

        Transform speakingCharacter = GameManager.Instance.eventManager.speakingCharacter.transform;

        while (timer < 0.5f)
        {
            speakingCharacter.position = Vector3.Lerp(speakingCharacter.position, GameManager.Instance.gameHandler.speakerPosition.transform.position, timer * 2);
            speakingCharacter.localScale = Vector3.Lerp(speakingCharacter.localScale, new Vector3(1f, 1f, 1), timer *2);
            timer += Time.deltaTime;
            yield return null;
        }

       onExitAnimationDone?.Invoke(this, new DialogueManagerEventArgs(currentEvent)); 

        /*
        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
         */
        print("Calling next turn.");
        ExitDialogue();
    }

    private void TriggerExitAnimation()
    {
        GameManager.Instance.eventManager.speakingCharacter.GetComponent<Animator>().SetTrigger("Exit_Speaker");
    }

    /// <summary>
    /// Resets the dialogue event by clearing all existing dialogue options and dialogue linesList.
    /// </summary>
    /// <remarks>This method removes all game objects tagged as "DialogueOption" from the scene and clears the
    /// dialogue linesList stored in the associated dialogue component. Use this method to reset the dialogue system to its
    /// initial state before starting a new dialogue event.</remarks>
    private void ResetDialogueEvent()
    {
        dialogueComponent.linesList.Clear();
        dialogueComponent.stopInteraction = false;
    }

    private void ExitDialogue()
    {
        GameHandler gameHandler = GameManager.Instance.gameHandler;

        gameHandler.SwitchOffDialoguePanel();

        if(currentEvent.isGameOverEvent)
        {
            GameManager.Instance.isGameOver = true;
            GameManager.Instance.CheckGameOver(true, currentEvent.title);
            return;
        }

        if (GameManager.Instance.isFirstTurn)
        {
            GameManager.Instance.isFirstTurn = false;
            gameHandler.PlaceNewCharacters();
            return;
        }

        gameHandler.GetActiveCharacters().ForEach(character => character.EnableCharacter());

        if(currentEvent.eventType != EventType.Cutscene && currentEvent.eventType != EventType.Introduction)
            gameHandler.IncreaseEventCounter();

        /*if (gameHandler.GetActiveCharacters().Count <= 0)
        {
            gameHandler.StartDayTransition();
            return;
        }
        */

   
    }
}




