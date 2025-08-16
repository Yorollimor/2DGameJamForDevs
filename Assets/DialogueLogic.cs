using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using JetBrains.Annotations;

public class DialogueLogic : MonoBehaviour
{
    public bool isEvent = false; // True if this currentDialogueLineData is part of an event
    public TextMeshProUGUI textComponent;
    public TextMeshProUGUI speakerName;
    public List<String> lines;
    public EventData eventData; // Used to store the event DialogueDataContainer for the current currentDialogueLineData
    public float textSpeed;
    public bool stopInteraction = false; // If true, the player cannot interact with the game while this currentDialogueLineData is active

    public Image portraitImage;

    public delegate void DialogueEventHandler(object sender, EventArgs e);
    public event DialogueEventHandler OnDialogueEnd;
    public event DialogueEventHandler OnDialogueStart;
    private event DialogueEventHandler OnLineEnd;

    private int index;
    bool dialogueEndTriggered = false;
    private int[,] speakerCounts = new int[2, 2]; // [speakerID, lineCount]
    bool isDialogueDone = false;

    DialogueLine currentDialogueLineData = new DialogueLine();

    // Start is called before the first frame update
    void Start()
    {
        textComponent.text = string.Empty; 
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !stopInteraction)
        {
            if(isDialogueDone)
            {
                TriggerDialogueEnd();
                return; // Exit if currentDialogueLineData is already done
            }
            if (textComponent.text == lines[index])
            {
                NextLine(currentDialogueLineData);
            }
            else
            {
                print("skipping text");
                StopAllCoroutines(); // Stop any ongoing typing coroutine
                textComponent.text = lines[index];
                if(index >= lines.Count - 1)
                {
                    NextLine(currentDialogueLineData);
                }
             }
        }

    }

    public void InitializeEventData(EventData eventData)
    {
        this.eventData = eventData;
        ResetCounters(); // Reset counters for the new event
        PrepareDialogueLines();
    }

    int totalLinesCount = 0; // Total amount of lines
    public void PrepareDialogueLines()
    {
        DialogueDataContainer currentDialogue = eventData.dialogue;

        totalLinesCount = 0; //total amount of lines
        int currentLineCount = 0;


        int arrayCounter = 0;
        foreach (CharacterDialogue ds in currentDialogue.speakingCharacters)
        {
            totalLinesCount += ds.dialogueLines.Length;
            speakerCounts[arrayCounter, 0] = ds.speaker.id; // Store the speaker index
            speakerCounts[arrayCounter, 1] = 0; // Store the speaker index
            arrayCounter++;
        }

        int currentSpeaker = 0;
        bool exit = false;
        while (currentLineCount < totalLinesCount) //add the lines of the individual speakers in the correct order
        {
            for (int i = 0; i < speakerCounts.GetLength(0); i++)//traverse the speakerCounts array to find the current speaker
            {
                DialogueLine dialogue = GetSpeakerData(eventData, ref currentLineCount, currentSpeaker, ref speakerCounts[currentSpeaker, 1]);
                lines.Add(dialogue.line); // Add the current line to the lines list
                if (currentLineCount >= totalLinesCount)
                {
                    exit = true; // Exit if we have processed all lines
                    break;
                }
                if (dialogue.isFinalLine) //if the currentDialogueLineData line is the last line for the current speaker
                {
                    if (dialogue.nextSpeakerID != -1)// If the currentDialogueLineData line specifies a next speaker
                    {
                        for (int j = 0; j < speakerCounts.GetLength(0); j++) //Find the next speaker in the speakerCounts array based on the nextSpeakerID
                        {
                            if (speakerCounts[j, 0] == dialogue.nextSpeakerID)
                            {
                                currentSpeaker = j; // Switch to the next speaker specified in the currentDialogueLineData line
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

        StartDialogue(); // Start the currentDialogueLineData after preparing the lines
    }

    private void DeleteEmptyEntries()
    {
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            if (string.IsNullOrEmpty(lines[i]))
            {
                lines.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// used for starting a currentDialogueLineData that is part of an event.
    /// </summary>
    public void StartDialogue(bool isChoiceResponse = false)
    {
        OnDialogueStart?.Invoke(this, EventArgs.Empty); // Notify subscribers that currentDialogueLineData has started

        print("Starting dialogue");
        DeleteEmptyEntries();
        StartCoroutine(TypeLine(eventData, isChoiceResponse));
    }
   
    private void ResetCounters()
    {         
        index = 0; // Reset the index to the first line
        totalLinesCount = 0; // Reset the total lines count
        currentLineCount = 0; // Reset the current line count
        currentSpeaker = 0; // Reset the current speaker index
        dialogueEndTriggered = false; // Reset currentDialogueLineData end trigger
    }

    int currentLineCount = 0; // Current line count for the currentDialogueLineData
    int currentSpeaker = 0; // Current speaker index



    IEnumerator TypeLine(EventData eventData, bool isChoiceResponse = false)
    {       
        if (lines.Count == 0)
        {
            print("No lines to display");
            NextLine(currentDialogueLineData);           
            yield break; // Exit if there are no lines to display
        }

        if (currentLineCount < totalLinesCount)//
        {
            textComponent.text = ""; // Clear previous text
            currentDialogueLineData = GetSpeakerData(eventData, ref currentLineCount, currentSpeaker, ref speakerCounts[currentSpeaker, 1]);
        }

        if (!isChoiceResponse && currentLineCount < totalLinesCount)
        {
            if (currentDialogueLineData.isFinalLine) //if the currentDialogueLineData line is the last line for the current speaker and there are more lines to process
            {
                if (currentDialogueLineData.nextSpeakerID != -1)// If the currentDialogueLineData line specifies a next speaker
                {
                    for (int j = 0; j < speakerCounts.GetLength(0); j++) //Find the next speaker in the speakerCounts array based on the nextSpeakerID
                    {
                        if (speakerCounts[j, 0] == currentDialogueLineData.nextSpeakerID)
                        {
                            currentSpeaker = j; // Switch to the next speaker specified in the currentDialogueLineData line
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

        string textBuffer = null; // Buffer to hold the current line text -> if not, buffer problems might ommit characters
        foreach (char c in lines[index].ToCharArray())
        {
            textBuffer += c;
            textComponent.text = textBuffer;
            yield return new WaitForSeconds(textSpeed);
        }

        //TODO: After choice response, and no lines left, we should not call NextLine, but rather end the currentDialogueLineData
        if (isChoiceResponse && index >= lines.Count - 1)
        {
            print("End of dialogue reached after choice response");
            isDialogueDone = true;
            TriggerDialogueEnd();
            yield break; // Exit if the currentDialogueLineData is done
        }

        if (index >= lines.Count - 1)
        {
            NextLine(currentDialogueLineData);
        }
    }

    public void DisplayChoices(List<ChoiceData> choicesList)
    {
        DestroyDialogueOptions();
        // Populate options
        stopInteraction = true; // Disable interaction while choices are displayed

        var choices = choicesList;

        for (int i = 0; i < choices.Count; i++)
        {
            GameObject option = Instantiate(GameManager.Instance.dialogueManager.optionsPanel, this.transform, false);
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
    }

    private void OnChoiceSelected(ChoiceData data)
    {
        print("Choice Selected: " + data);

        if (eventData == null) return;

        if (data.responseToChoice != null && data.responseToChoice.Length > 0)
        {
            lines.Clear();
            lines.Add(data.responseToChoice); // Clear previous lines and add the response
            StartDialogue(true); // Start the currentDialogueLineData with the response
        }
        else
        {
            lines.Clear();
            lines.Add("..."); // Clear previous lines and add the response
            StartDialogue(true); // Start the currentDialogueLineData with the response
        }

        foreach (var change in data.consequences)
        {
            GameManager.Instance.resourceManager.ModifyResource(change.type, change.amount);
        }

        foreach (var change in data.dailyChanges)
        {
            GameManager.Instance.resourceManager.ModifyResource(change.type, change.amount);
        }

        stopInteraction = false; // Re-enable interaction after choice is made
        DestroyDialogueOptions(); // Clean up options after choice is made
    }

    private void DestroyDialogueOptions()
    {
        GameObject[] oldOptions = GameObject.FindGameObjectsWithTag("DialogueOption");
        foreach (GameObject option in oldOptions)
        {
            option.GetComponent<MoveToAnchor>().DestroyThis();
        }
    }
 

    /// <summary>
    /// Retrieves the currentDialogueLineData line DialogueDataContainer for a specific speaker in a non-event currentDialogueLineData sequence.
    /// </summary>
    /// <remarks>This method updates the speaker's name and portrait in the UI based on the provided <paramref
    /// name="eventData"/> DialogueDataContainer.</remarks>
    /// <param name="eventData">The <see cref="EventData"/> object containing the non-event currentDialogueLineData DialogueDataContainer.</param>
    /// <param name="currentLineCount">A reference to the current line count, which will be incremented by this method.</param>
    /// <param name="speakerId">The index of the speaker whose currentDialogueLineData line is being retrieved.</param>
    /// <param name="speaker_lines_count">A reference to the line count for the specified speaker, which will be incremented by this method.</param>
    /// <returns>The <see cref="DialogueLine"/> object representing the current currentDialogueLineData line for the specified speaker.</returns>
    private DialogueLine GetSpeakerData(EventData eventData, ref int currentLineCount, int speakerId, ref int speaker_lines_count)
    {
        CharacterDialogue sequence = eventData.dialogue.speakingCharacters[speakerId];
        DialogueLine dialogue = sequence.dialogueLines[speaker_lines_count];
        string current_line = dialogue.line;
        
        speaker_lines_count++;
        currentLineCount++;
        
        speakerName.text = sequence.speaker.name; // Set the speaker name
        portraitImage.sprite =sequence.speaker.portrait; // Set the speaker portrait if available
        return dialogue;
    }

    public void NextLine(DialogueLine line)
    {
        if (index < lines.Count - 1)
        {
            index++;
            textComponent.text = string.Empty;
            if (line.choices != null && line.choices.Count > 0)
            {
                DisplayChoices(line.choices); // Display choices if available
                return; // Exit to allow choices to be displayed
            }
            StartCoroutine(TypeLine(eventData));        
        }
        else
        {
            if (line.choices != null && line.choices.Count > 0)
            {
                DisplayChoices(line.choices); // Display choices if available
                return; // Exit to allow choices to be displayed
            }
            print("End of dialogue");
            isDialogueDone = true;
            if(lines.Count == 0)
            {
                TriggerDialogueEnd();
            }
        }

    }

    void TriggerDialogueEnd()
    {
        if(!dialogueEndTriggered)
        {
            print("Dialoge end invoked");
            dialogueEndTriggered = true;
            OnDialogueEnd?.Invoke(this, EventArgs.Empty);
            isDialogueDone = false;
        }
           
    }
}

