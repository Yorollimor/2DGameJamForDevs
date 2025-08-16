using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager_SelfMade : MonoBehaviour
{
    [Header("Event Settings")]
    private EventData currentEvent;
    public GameObject dialoguePanel;
    public GameObject optionsPanel;

    [Header("Normal Dialogue Settings")]
    public GameObject normalDialoguePanel;
    public GameObject normalPlayerDialoguePanel;

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

    public void StartEventDialogue(EventData eventData)
    {
        ResetDialogueEvent();
        dialogueComponent.InitializeEventData(eventData);
        currentEvent = eventData;      
    } 

    private void OnNonEventEnd(object sender, System.EventArgs e)
    {
        print("Non-event dialogue ended.");
        dialogueComponent.OnDialogueEnd -= OnNonEventEnd;
        dialogueComponent.OnDialogueEnd += CallNextTurn; // Subscribe to the dialogue end event to call next turn
    }

    private void CallNextTurn(object sender, System.EventArgs e)
    {
        dialogueComponent.OnDialogueEnd -= CallNextTurn;
        TriggerExitAnimation();
        StartCoroutine(NextTurnAfterAnimationEnds()); // Ensure the exit animation is complete before proceeding
    }

    private IEnumerator NextTurnAfterAnimationEnds()
    {
        float animationDuration = dialoguePanel.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;
        float timer = 0f;
        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        print("Dialogue ended, calling next turn.");
        GameManager.Instance.NextTurn();
    }

    private void TriggerExitAnimation()
    {
        dialoguePanel.GetComponent<Animator>().SetTrigger("Exit_Speaker");
    }

    /// <summary>
    /// Resets the dialogue event by clearing all existing dialogue options and dialogue lines.
    /// </summary>
    /// <remarks>This method removes all game objects tagged as "DialogueOption" from the scene and clears the
    /// dialogue lines stored in the associated dialogue component. Use this method to reset the dialogue system to its
    /// initial state before starting a new dialogue event.</remarks>
    private void ResetDialogueEvent()
    {
        DestroyDialogueOptions();

        dialogueComponent.lines.Clear();
    }

    private void DestroyDialogueOptions()
    {
        GameObject[] oldOptions = GameObject.FindGameObjectsWithTag("DialogueOption");
        foreach (GameObject option in oldOptions)
        {
            option.GetComponent<MoveToAnchor>().DestroyThis();
        }
    }

    public void CreateDialoguePanels()
    {

    }
}




