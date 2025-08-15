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
    Dialogue_ dialogueComponent;

    /// <summary>
    /// The first dimension represents the speaker index, and the second dimension represents the line count for that speaker.
    /// </summary>
    int[,] speakerCounts = new int[2,2];

    private void Awake()
    {
        dialogueComponent = dialoguePanel.GetComponent<Dialogue_>();
    }

    public void StartEventDialogue(EventData eventData)
    {
        ResetDialogueEvent();
        dialoguePanel.GetComponent<Animator>().SetTrigger("Intro_Speaker");

        currentEvent = eventData;

        // if it's a non-event dialogue, we need to set the speaker counts dynamically based on the characters involved
        if (eventData.nonEventDialogue != null)
        {
            speakerCounts = new int[eventData.nonEventDialogue.speakingCharacters.Length, 2];
        }

        if (!eventData.descriptionDialogue.IsUnityNull())
        {
            // Setting the description 
            foreach (string line in eventData.descriptionDialogue)
            {
                dialogueComponent.lines.Add(line);
            }
        }
        else print("No description dialogue provided for the event.");

        dialogueComponent.speakerName.text = eventData.speaker.name;
        dialogueComponent.portraitImage.sprite = eventData.speaker.portrait;

        switch(eventData.eventType)
        {
            case EventType.KingEvent:
                HandleKingEvent();
                break;
            case EventType.PeasantEvent:
                HandlePeasantEvent();
                break;
            case EventType.DialogueKing:
                HandleDialogueKing();
                break;
            case EventType.DialoguePeasants:
                HandleDialoguePeasants();
                break;
        }

        if(eventData.nonEventDialogue != null)
        {
            dialogueComponent.StartDialogue(eventData);
            return;
        }
        dialogueComponent.StartDialogue();
    }

    public void HandleKingEvent()
    {
        dialogueComponent.OnDialogueEnd += ShowEventChoices;
    }
    public void HandlePeasantEvent()
    {
        dialogueComponent.OnDialogueEnd += ShowEventChoices;
    }

    public void HandleDialogueKing()
    {
        //dialogueComponent.OnDialogueStart += PrepareDialogueLines;
        dialogueComponent.OnDialogueEnd += PrepareDialogueLines;
    }
    public void HandleDialoguePeasants()
    {
        dialogueComponent.OnDialogueEnd += PrepareDialogueLines;
    }  

    private void OnNonEventEnd(object sender, System.EventArgs e)
    {
        print("Non-event dialogue ended.");
        dialogueComponent.OnDialogueEnd -= OnNonEventEnd;
        dialogueComponent.OnDialogueEnd += CallNextTurn; // Subscribe to the dialogue end event to call next turn
    }

    private void PrepareDialogueLines(object sender, System.EventArgs e)
    {
        print("Preparing dialogue lines for non-event dialogue.");
        NonEventDialogue currentDialogue = currentEvent.nonEventDialogue;

        int totalLinesCount = 0; //total amount of lines

        int currentLineCount = 0;


        int arrayCounter = 0;
        foreach (CharacterDialogue ds in currentDialogue.speakingCharacters)
        {
            totalLinesCount += ds.dialogueLines.Length;
            speakerCounts[arrayCounter,0] = ds.speaker.id; // Store the speaker index
            speakerCounts[arrayCounter, 1] = 0; // Store the speaker index
            arrayCounter++;
        }

        int currentSpeaker = 0;
        bool exit = false;
        while (currentLineCount < totalLinesCount) //add the lines of the individual speakers in the correct order
        {
            for (int i = 0; i < speakerCounts.GetLength(0); i++)//traverse the speakerCounts array to find the current speaker
            {
                dialogueLine dialogue = GetSpeakerData(currentDialogue, ref currentLineCount, currentSpeaker, ref speakerCounts[currentSpeaker, 1]);
                if (currentLineCount >= totalLinesCount)
                {
                    exit = true; // Exit if we have processed all lines
                    break;
                }
                if (dialogue.isFinalLine) //if the dialogue line is the last line for the current speaker
                {
                    if (dialogue.nextSpeakerID != -1)// If the dialogue line specifies a next speaker
                    {
                        for (int j = 0; j < speakerCounts.GetLength(0); j++) //Find the next speaker in the speakerCounts array based on the nextSpeakerID
                        {
                            if (speakerCounts[j, 0] == dialogue.nextSpeakerID)
                            {
                                currentSpeaker = j; // Switch to the next speaker specified in the dialogue line
                                break;
                            }
                        }
                    }
                    else //If no next speaker is specified
                    {
                        // Switch to the next speaker in the array
                        currentSpeaker = (currentSpeaker + 1) % speakerCounts.GetLength(0);
                    }
                }

            }
            if (exit) break; // Exit the loop if we have processed all lines
        }
        

        dialogueComponent.OnDialogueEnd -= PrepareDialogueLines;
        dialogueComponent.OnDialogueEnd += CallNextTurn;
        dialogueComponent.StartDialogue(currentEvent);
    }

    /// <summary>
    /// Retrieves the dialogue line data for a specific speaker from the current dialogue sequence. Increments the current line count and the speaker's line count.
    /// Furthermore, it updates the dialogue component's lines with the current line text.
    /// </summary>
    /// <param name="currentDialogue">The current dialogue sequence containing all speakers and their dialogue lines.</param>
    /// <param name="currentLineCount">A reference to the total count of dialogue lines processed so far. This value is incremented by the method.</param>
    /// <param name="speakerIndex">The index of the speaker whose dialogue line is being retrieved.</param>
    /// <param name="speaker_lines_count">A reference to the count of lines processed for the specified speaker. This value is incremented by the method.</param>
    /// <returns>The <see cref="dialogueLine"/> object representing the dialogue line data for the specified speaker.</returns>
    private dialogueLine GetSpeakerData(NonEventDialogue currentDialogue, ref int currentLineCount, int speakerIndex, ref int speaker_lines_count)
    {

        dialogueLine dialogue = currentDialogue.speakingCharacters[speakerIndex].dialogueLines[speaker_lines_count];
        string current_line = dialogue.line;
        speaker_lines_count++;
        currentLineCount++;
        dialogueComponent.lines.Add(current_line);
        return dialogue;
    }

    private void ShowEventChoices(object sender, System.EventArgs e)
    {
        DestroyDialogueOptions();
        // Populate options
        dialogueComponent.stopInteraction = true; // Disable interaction while choices are displayed

        var choices = currentEvent.choices;

        for (int i = 0; i < choices.Count; i++)
        {
            GameObject option = Instantiate<GameObject>(optionsPanel,this.transform,false);
            TextMeshProUGUI[] contents = option.GetComponentsInChildren<TextMeshProUGUI>();
            contents[0].text = choices[i].choiceText; // Assuming the first TextMeshProUGUI is for the choice text

            contents[1].text = ""; // Clear previous consequences text
            foreach (ResourceChange change in choices[i].consequences)
            {
                // Handle resource changes here, e.g., update UI or game state
                contents[1].text += change.type.ToString() + ": ";
                contents[1].text += change.amount < 0 ? change.amount.ToString() + "\n" : "+" + change.amount.ToString() + "\n";
                contents[1].color = change.amount < 0 ? Color.red : Color.green; // Change color based on amount
            }
            foreach (DailyChangeChanges change in choices[i].dailyChanges)
            {
                // Handle resource changes here, e.g., update UI or game state
                contents[1].text += change.type.ToString() + ": ";
                contents[1].text += change.amount < 0 ? change.amount.ToString() + "\n" : "+" + change.amount.ToString() + "\n";
                contents[1].color = change.amount < 0 ? Color.red : Color.green; // Change color based on amount
            }

            int x = i; // Capture the current index for the listener

            option.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
        
                OnChoiceSelected(choices[x]);
            });
        }
        
        dialogueComponent.OnDialogueEnd -= ShowEventChoices;
    }
    public void OnChoiceSelected(ChoiceData data)
    {
        print("Choice Selected: " + data);

        if (currentEvent == null) return;


        dialogueComponent.OnDialogueEnd += CallNextTurn; // Subscribe to the dialogue end event to call next turn

        if ( data.responseToChoice != null && data.responseToChoice.Length > 0)
        {
            dialogueComponent.lines.Clear();
            dialogueComponent.lines.Add(data.responseToChoice); // Clear previous lines and add the response
            dialogueComponent.StartDialogue(); // Start the dialogue with the response
        }
        else
        {
            dialogueComponent.lines.Clear();
            dialogueComponent.lines.Add("..."); // Clear previous lines and add the response
            dialogueComponent.StartDialogue(); // Start the dialogue with the response
        }       

        foreach (var change in data.consequences)
        {
            GameManager.Instance.resourceManager.ModifyResource(change.type, change.amount);
        }

        foreach (var change in data.dailyChanges)
        {
            GameManager.Instance.resourceManager.ModifyResource(change.type, change.amount);
        }

        dialogueComponent.stopInteraction = false; // Re-enable interaction after choice is made
        DestroyDialogueOptions(); // Clean up options after choice is made
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




