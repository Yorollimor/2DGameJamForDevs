using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class DialogueManager_SelfMade : MonoBehaviour
{
    [Header("Event Settings")]
    private EventData currentEvent;
    public GameObject dialoguePanel;
    public GameObject optionsPanel;

    [Header("Normal Dialogue Settings")]
    public GameObject normalDialoguePanel;
    public GameObject normalPlayerDialoguePanel;

    public static event Action<Character_Data> OnCharacterFinishedCutsceneOrIntroduction;

    /// <summary>
    /// Represents the dialogue component used to manage and display dialogue in the application.
    /// </summary>
    /// <remarks>This component is typically used to handle dialogue interactions, including displaying text,
    /// managing dialogue sequences.</remarks>
    DialogueLogic dialogueComponent;

    private void Awake()
    {
        dialogueComponent = dialoguePanel.GetComponent<DialogueLogic>();
    }

    public void StartEventDialogue(EventData eventData)
    {
        ResetDialogueEvent();

        EventManager.currentEvent = eventData;
        dialogueComponent.InitializeEventData(eventData);

        EventManager.speakingCharacter.OnExitAnimationFinished += ExitDialogue;
        currentEvent = eventData;      
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
        EventManager.speakingCharacter.OnExitAnimationFinished -= ExitDialogue;

        GameHandler gameHandler = GameManager.Instance.gameHandler;

        gameHandler.SwitchOffDialoguePanel(); //turn off dialogue panel

        if(currentEvent.isGameOverEvent) //check if the current event was a game over event
        {
            GameManager.Instance.isGameOver = true;
            GameManager.Instance.CheckGameOver(true, currentEvent.title);
            return;
        }

        if (GameManager.Instance.isFirstTurn) //Check if the current event was the first turn (tutorial) and then place new characters
        {
            GameManager.Instance.isFirstTurn = false;
            gameHandler.PlaceNewCharacters();
            return;
        }

        //---- Enabling all characters, increasing event-counter (if not cutscene or introduction), moves character back to original position if it was introduction or cutscene.------
        gameHandler.GetActiveCharacters().ForEach(character => character.EnableCharacter());

        //if(currentEvent.eventType == EventType.PeasantEvent || currentEvent.eventType == EventType.KingEvent)
            gameHandler.StartNextDayIfAllCharactersAreGone();
        if(currentEvent.eventType != EventType.PeasantEvent || currentEvent.eventType != EventType.KingEvent)
        {
            OnCharacterFinishedCutsceneOrIntroduction?.Invoke(EventManager.speakingCharacter);
        }
        
    }
}




