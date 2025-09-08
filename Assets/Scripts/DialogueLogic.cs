using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueEventArgs: EventArgs
{
    public DialogueLine CurrentDialogueLine { get;}
    public bool IsChoiceResponse { get;}

    public bool SpeakerEnter { get;} // Animation name for the speaker change event
    public bool SpeakerExit { get; } // Animation name for the speaker change event
    public DialogueEventArgs(DialogueLine currentDialogueLine, bool isChoiceResponse)
    {
        CurrentDialogueLine = currentDialogueLine;
        IsChoiceResponse = isChoiceResponse;
    }

    public DialogueEventArgs(bool SpeakerEnter)
    {
        if(SpeakerEnter)
        {
            this.SpeakerEnter = true;
            this.SpeakerExit = false;
        }
        else
        {
            this.SpeakerExit = true;
            this.SpeakerEnter = false;
        }
    }
}



public class DialogueLogic : MonoBehaviour
{
    public bool isEvent = false; // True if this currentDialogueLineData is part of an event
    public TextMeshProUGUI textComponent;
    public TextMeshProUGUI speakerName;
    public List<String> linesList;
    public EventData eventData; // Used to store the event DialogueDataContainer for the current currentDialogueLineData
    public float textSpeed;
    public bool stopInteraction = false; // If true, the player cannot interact with the game while this currentDialogueLineData is active

    //public Image portraitImage;

    public delegate void DialogueEventHandler(Character_Data speaker);
    public static event DialogueEventHandler OnDialogueEnd;
    public event DialogueEventHandler OnDialogueStart;
    private event DialogueEventHandler OnSpeakerChange;
    DialogueEventArgs speakerExit = new DialogueEventArgs(false);
    DialogueEventArgs speakerEnter = new DialogueEventArgs(true);

    public static event Action<GameObject> OnChoiceChosen;


    private Animator animator;
    private int index;
    bool dialogueEndTriggered = false;
    private int[,] speakerCounts = new int[2, 2]; // [speakerID, lineCount]
    bool isDialogueDone = false;
    bool wasChoiceResponseLine = false; // Flag to indicate if the currentDialogueLineData line is a response to a choice
    ChoiceData currentChoiceData; // Used to store the current choice data if the currentDialogueLineData line is a response to a choice
    bool skipLine = false;

    public bool isGameOverDialogue = false;
    DialogueLine currentDialogueLineData = new DialogueLine();

    // Start is called before the first frame update
    void Start()
    {
        textComponent.text = string.Empty;
    }

    private void DialogueLogic_OnSpeakerChange(object sender, EventArgs e)
    {
        throw new NotImplementedException();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !stopInteraction)
        {
            if(isDialogueDone)
            {
                OnSpeakerChange?.Invoke(EventManager.speakingCharacter); // Notify subscribers that the speaker has exited

                TriggerDialogueEnd();
                return; // Exit if currentDialogueLineData is already done
            }
            if (IsLineFinished())
            {
                NextLine(currentDialogueLineData, wasChoiceResponseLine);
            }
            else
            {
                skipLine = true;
                //textComponent.text = FinishLine();
                if (index >= linesList.Count - 1)
                {
                    //NextLine(currentDialogueLineData, wasChoiceResponseLine);
                }
             }
        }

    }

    private bool IsLineFinished()
    {
        //was it a response to a choice? check if the line of the current choice is finished
        //was it a normal line? check if the currentDialogueLineData line is finished
        if (wasChoiceResponseLine)
        {
            return currentChoiceData.responseToChoice.Length == textComponent.maxVisibleCharacters; // Check if the choice response line is finished
        }
        else
        {
            return textComponent.maxVisibleCharacters == linesList[index].Length;
        }
    }

    public void InitializeEventData(EventData eventData)
    {
        isGameOverDialogue = eventData.isGameOverEvent;

        this.eventData = eventData;
        stopInteraction = true;
        ResetCounters(); // Reset counters for the new event
        PrepareDialogueLines();
    }

    int totalLinesCount = 0; // Total amount of linesList
    public void PrepareDialogueLines()
    {
        if (eventData == null) print("eventdata null");

        print(eventData.dialogue.title);
        DialogueDataContainer currentDialogue = eventData.dialogue;

        totalLinesCount = 0; //total amount of linesList
        int currentLineCount = 0; // Current line count for the PrepareSpeakerLineOrder method
        int currentSpeaker = 0;

        int arrayCounter = 0;

        speakerCounts = new int[currentDialogue.speakingCharacters.Length, 2]; // Initialize the speakerCounts array to hold speaker IDs and their line counts

        foreach (CharacterDialogue ds in currentDialogue.speakingCharacters)
        {
            totalLinesCount += ds.dialogueLines.Length;
            speakerCounts[arrayCounter, 0] = ds.speaker.id; // Store the speaker index
            speakerCounts[arrayCounter, 1] = 0; // initialize the line count for this speaker
            arrayCounter++;
        }
       // print("Total lines count: " + totalLinesCount);

        bool exit = false;
        while (currentLineCount < totalLinesCount) //add the linesList of the individual speakers in the correct order
        {
            for (int i = 0; i < speakerCounts.GetLength(0); i++)//traverse the speakerCounts array to find the current speaker
            {
                DialogueLine lineData = PrepareSpeakerLineOrder(eventData, ref currentLineCount, currentSpeaker, ref speakerCounts[currentSpeaker, 1]);
                linesList.Add(lineData.line); // Add the current line to the linesList list
                //print("Adding line: " + lineData.line + " for speaker: " + currentSpeaker);
                if (currentLineCount >= totalLinesCount)
                {
                    exit = true; // Exit if we have processed all linesList
                    break;
                }
                if (lineData.isFinalLine) //if the currentDialogueLineData line is the last line for the current speaker
                {
                    if (lineData.nextSpeakerID != -1)// If the currentDialogueLineData line specifies a next speaker
                    {
                        for (int j = 0; j < speakerCounts.GetLength(0); j++) //Find the next speaker in the speakerCounts array based on the nextSpeakerID
                        {
                            if (speakerCounts[j, 0] == lineData.nextSpeakerID)
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
            if (exit) break; // Exit the loop if we have processed all linesList
        }

        //resetting the current line of the speakers
        arrayCounter = 0; // Counter for the speakerCounts array
        foreach (CharacterDialogue ds in eventData.dialogue.speakingCharacters)
        {
            speakerCounts[arrayCounter, 1] = 0; // reset the line count for this speaker
            arrayCounter++;
        }

        StartDialogue(); // Start the currentDialogueLineData after preparing the linesList
    }


    /// <summary>
    /// used for starting a currentDialogueLineData that is part of an event.
    /// </summary>
    public void StartDialogue(string singleLine = null)
    {
        OnDialogueStart?.Invoke(EventManager.speakingCharacter); // Notify subscribers that currentDialogueLineData has started
        GameManager.Instance.gameHandler.SwitchOnDialoguePanel(); // Enable the dialogue panel
        print("Starting dialogue");
        //DeleteEmptyEntries();
        StartCoroutine(TypeLine(eventData, singleLine));
    }
   
    private void ResetCounters()
    {         
        index = 0; // Reset the index to the first line
        totalLinesCount = 0; // Reset the total linesList count
        currentLineCount = 0; // Reset the current line count
        currentSpeaker = 0; // Reset the current speaker index
        dialogueEndTriggered = false; // Reset currentDialogueLineData end trigger
        wasChoiceResponseLine = false; // Reset the choice response line flag
        isDialogueDone = false;
    }

    int currentLineCount = 0; // Current line count for the currentDialogueLineData
    int currentSpeaker = 0; // Current speaker index
    int priorSpeaker = 0; // Prior speaker index to track changes

    [Header("Dialogue Panel Animation")]
    public float lineShakeDuration = 0.5f;
    public float lineShakeStrength = 10f;
    public int lineShakeVibrato = 30;
    IEnumerator TypeLine(EventData eventData, string line = null)
    {
        transform.DOShakePosition(lineShakeDuration, lineShakeStrength, lineShakeVibrato);
        skipLine = false;
        if (linesList.Count == 0)
        {
            print("No lines to display");
            NextLine(currentDialogueLineData);           
            yield break; // Exit if there are no linesList to display
        }       

        if (string.IsNullOrEmpty(line) && currentLineCount < totalLinesCount)
        {
            currentDialogueLineData = GetSpeakerData(eventData, ref currentLineCount, currentSpeaker, ref speakerCounts[currentSpeaker, 1]);
            priorSpeaker = currentSpeaker; // Update prior speaker to the current one

            if (currentDialogueLineData.isFinalLine) //if the currentDialogueLineData line is the last line for the current speaker and there are more linesList to process
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

        if (EventManager.speakingCharacter == null) print("Character null");
        else if (GameManager.Instance.eventManager == null) print("EventManager null");

        stopInteraction = false;
        textComponent.text = ""; // Clear previous text
        if (!string.IsNullOrEmpty(line)) //check if it's choice response
        {
            int messageLength = line.Length;
            textComponent.text = line;
            textComponent.maxVisibleCharacters = 0;

            char currentCharacter = ' ';
            char[] charsArray = textComponent.text.ToCharArray();

            while (textComponent.maxVisibleCharacters < messageLength)
            {
                if (skipLine)
                {
                    textComponent.maxVisibleCharacters = messageLength;
                    
                    break; // Exit if skipLine is true
                }

                textComponent.maxVisibleCharacters++;
                currentCharacter = charsArray[textComponent.maxVisibleCharacters - 1];

                if(currentCharacter == ' ') FMODUnity.RuntimeManager.PlayOneShot("event:/DialogueSFX/Dialogue_1");

                yield return new WaitForSeconds(textSpeed);
            }
        }
        else //if it's a normal line
        {
            int messageLength = linesList[index].Length;
            textComponent.text = linesList[index];
            textComponent.maxVisibleCharacters = 0;

            char currentCharacter = ' ';
            char[] charsArray = textComponent.text.ToCharArray();

            while (textComponent.maxVisibleCharacters < messageLength)
            {
                if (skipLine)
                {
                    textComponent.maxVisibleCharacters = messageLength;
                    break; // Exit if skipLine is true
                }

                textComponent.maxVisibleCharacters++;
                currentCharacter = charsArray[textComponent.maxVisibleCharacters - 1];

                if (currentCharacter == ' ') FMODUnity.RuntimeManager.PlayOneShot("event:/DialogueSFX/Dialogue_1");

                yield return new WaitForSeconds(textSpeed);
            }
        }

        if (!string.IsNullOrEmpty(line) && index >= linesList.Count - 1)
        {
            print("End of dialogue reached after choice response");
            isDialogueDone = true;
            yield break; // Exit if the currentDialogueLineData is done
        }

        if (index >= linesList.Count - 1)
        {
            
            if (currentDialogueLineData.choices.Count > 0)
            { 
                print("Displaying choices after last line");
                DisplayChoices(currentDialogueLineData.choices); // Display choices if available
                yield break;
            }
            else
            {
                //print("End of dialogue reached");              
                isDialogueDone = true; // Set the currentDialogueLineData as done if no choices are available
            }

        }
    }

    public void DisplayChoices(List<ChoiceData> choicesList)
    {
        DestroyDialogueOptions();
        // Populate optionsList
        stopInteraction = true; // Disable interaction while choices are displayed

        var choices = choicesList;
        var filteredCchoices = new List<ChoiceData>();
        foreach (ChoiceData choice in choices)
        {
            if (choice.conditionToShow != null || choice.conditionToHide != null)
            {                     
                if (GameManager.Instance.gameHandler.collectedVariables.Contains(choice.conditionToShow) || !GameManager.Instance.gameHandler.collectedVariables.Contains(choice.conditionToHide))
                        filteredCchoices.Add(choice);
                else continue;
            }
            else filteredCchoices.Add(choice);
        }

        if (filteredCchoices.Count == 0)
        {
            Debug.LogWarning("Filtered choices are 0!");
        }

        List<GameObject> optionsList = new List<GameObject>();
        for (int i = 0; i < filteredCchoices.Count; i++)
        {
            GameObject option = Instantiate(GameManager.Instance.dialogueManager.optionsPanel, GameManager.Instance.gameHandler.gameCanvas.transform, false);
            option.GetComponentInChildren<Button>().interactable = false;
            optionsList.Add(option);
            TextMeshProUGUI[] contents = option.GetComponentsInChildren<TextMeshProUGUI>();
            contents[0].text = filteredCchoices[i].choiceText; // Assuming the first TextMeshProUGUI is for the choice text

            contents[1].text = ""; // Clear previous consequences text
            foreach (ResourceChange change in filteredCchoices[i].consequences)
            {
                contents[1].text += change.type.ToString() + ": ";
                contents[1].text += change.amount < 0 ? "<color=red>" + change.amount.ToString() + "</color>\n" : "<color=green>+" + change.amount.ToString() + "</color>\n";
            }
            foreach (DailyChangeChanges change in filteredCchoices[i].dailyChanges)
            {

                contents[1].text += change.type.ToString() + ": ";
                contents[1].text += change.amount < 0 ? "<color=red>" + change.amount.ToString() + "</color>\n" : "<color=green>+" + change.amount.ToString() + "</color>\n";
            }


            if(!EnoughResourceForChoice(filteredCchoices[i]) && optionsList.Count > 1)
            {
                option.GetComponent<ValidChoice>().isValidChoice = false;
                contents[1].color = Color.gray;
                contents[0].color = Color.gray;
            }
            int x = i; // Capture the current index for the listener

            option.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                OnChoiceSelected(filteredCchoices[x], option);
            });
        }
        bool oneChoice = false;
        foreach(GameObject option in optionsList)
        {
            if (option.GetComponent<ValidChoice>().isValidChoice) oneChoice = true;
            else option.GetComponentInChildren<Button>().interactable = false;
        }
        if(!oneChoice)
        {
            Debug.LogWarning("No valid choices available!");
            GameManager.Instance.isGameOver = true;
            TriggerDialogueEnd();
        }
    }

    private bool EnoughResourceForChoice(ChoiceData data)
    {
        foreach (ResourceChange change in data.consequences)
        {
            switch(change.type)
            {
                case ResourceType.Food:
                    if(GameManager.Instance.resourceManager.food - change.amount < 0)
                    {
                        Debug.Log("Not enough food for this choice!");
                        return false;
                    }
                    break;
                case ResourceType.Gold:
                    if (GameManager.Instance.resourceManager.gold < 0)
                    {
                        Debug.Log("Not enough gold for this choice!");
                        return false;
                    }
                    break;
                case ResourceType.Soldiers:
                    if (GameManager.Instance.resourceManager.soldiers - change.amount < 0)
                    {
                        Debug.Log("Not enough soldiers for this choice!");
                        return false;
                    }
                    break;
                case ResourceType.Population:
                    if (GameManager.Instance.resourceManager.population - change.amount < 0)
                    {
                        Debug.Log("Not enough population for this choice!");
                        return false;
                    }
                    break;
                default: return true;
            }           
        }
        return true;
    }

    private void OnChoiceSelected(ChoiceData data, GameObject panel)
    {
        if (eventData == null) return;

        OnChoiceChosen?.Invoke(panel);

        if(data.receivingVaribale != null) GameManager.Instance.gameHandler.collectedVariables.Add(data.receivingVaribale);

        currentChoiceData = data; // Store the current choice data for later use

        if (data.responseToChoice != null && data.responseToChoice.Length > 0)
        {
            StartCoroutine(TypeLine(eventData, data.responseToChoice)); // Start the currentDialogueLineData with the response
        }
        else
        {
            StartCoroutine(TypeLine(eventData, "...")); // Start the currentDialogueLineData with the response
            currentChoiceData.responseToChoice = "..."; // Set a default response if none is provided
        }

        foreach (ResourceChange change in data.consequences)
        {
            GameManager.Instance.resourceManager.ModifyResource(change.type, change.amount);
        }

        foreach (DailyChangeChanges change in data.dailyChanges)
        {
            GameManager.Instance.resourceManager.ModifyResource(change.type, change.amount);
        }

        stopInteraction = false; // Re-enable interaction after choice is made
        DestroyDialogueOptions(); // Clean up optionsList after choice is made
    }

    private void DestroyDialogueOptions()
    {
        GameObject[] oldOptions = GameObject.FindGameObjectsWithTag("DialogueOption");
        foreach (GameObject option in oldOptions)
        {
            option.GetComponent<MoveToAnchor>().myAnchor.DestroyAnchor();
            option.GetComponent<MoveToAnchor>().DestroyThis();
        }
    }

    /// <summary>
    /// Retrieves the currentDialogueLineData line DialogueDataContainer for a specific speaker in a non-event currentDialogueLineData characterData.
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
        CharacterDialogue characterData = eventData.dialogue.speakingCharacters[speakerId];
        DialogueLine line = characterData.dialogueLines[speaker_lines_count];
        string current_line = line.line;
        
        speaker_lines_count++;
        currentLineCount++;
        
        speakerName.text = characterData.speaker.name; // Set the speaker name
        //portraitImage.sprite =characterData.speaker.portrait; // Set the speaker portrait if available
        return line;
    }

    private DialogueLine PrepareSpeakerLineOrder(EventData eventData, ref int currentLineCount, int speakerId, ref int speaker_lines_count)
    {
        CharacterDialogue characterData = eventData.dialogue.speakingCharacters[speakerId];
        DialogueLine line = characterData.dialogueLines[speaker_lines_count];
        string current_line = line.line;

        speaker_lines_count++;
        currentLineCount++;
        return line;
    }

    public void NextLine(DialogueLine line, bool choiceResponse = false)
    {
        if (index < linesList.Count - 1)
        {
            if (line.choices != null && line.choices.Count > 0 && !wasChoiceResponseLine)
            {
                wasChoiceResponseLine = true;
                DisplayChoices(line.choices); // Display choices if available
                return; // Exit to allow choices to be displayed
            }
            index++;
            textComponent.text = string.Empty;
            wasChoiceResponseLine = false; // Reset the choice response flag for the next line
            StartCoroutine(TypeLine(eventData));        
        }
        else
        {
            if (line.choices != null && line.choices.Count > 0 && !wasChoiceResponseLine)
            {
                wasChoiceResponseLine = true;
                DisplayChoices(line.choices); // Display choices if available
                return; // Exit to allow choices to be displayed
            }
            isDialogueDone = true;
            if(linesList.Count == 0 || index >= linesList.Count -1)
            {
                TriggerDialogueEnd();
            }
        }

    }

    void TriggerDialogueEnd()
    {
        if(!dialogueEndTriggered)
        {
            if(eventData.conditionType != ConditionType.None)
            {
                if(eventData.conditionToShow != null)
                {
                    print("Prior variable removed from used events: " + eventData.conditionToShow.variableName);
                    GameManager.Instance.gameHandler.collectedVariables.Remove(eventData.conditionToShow);
                }
                if (eventData.priorEvent != null)
                {
                    print("Prior event removed from used events: " + eventData.priorEvent.title);
                    GameManager.Instance.eventManager.GetUsedEVents().Remove(eventData.priorEvent);
                }
            }
            print("Dialoge end invoked");
            dialogueEndTriggered = true;
            OnDialogueEnd?.Invoke(EventManager.speakingCharacter);
            isDialogueDone = false;
        }
           
    }
}

