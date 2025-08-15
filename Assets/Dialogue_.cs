using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using PixelCrushers.DialogueSystem;

public class Dialogue_ : MonoBehaviour
{
    public bool isEvent = false; // True if this dialogue is part of an event
    public TextMeshProUGUI textComponent;
    public TextMeshProUGUI speakerName;
    public List<String> lines;
    public float textSpeed;
    public bool stopInteraction = false; // If true, the player cannot interact with the game while this dialogue is active

    public Image portraitImage;

    public delegate void DialogueEventHandler(object sender, EventArgs e);
    public event DialogueEventHandler OnDialogueEnd;
    public event DialogueEventHandler OnDialogueStart;

    private int index;
    bool dialogueEndTriggered = false;
    private int[,] speakerCounts = new int[2, 2]; // [speakerID, lineCount]
    bool isDialogueDone = false;

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
                return; // Exit if dialogue is already done
            }
            if (textComponent.text == lines[index])
            {
                NextLine();
            }
            else
            {
                print("skipping text");
                StopAllCoroutines(); // Stop any ongoing typing coroutine
                textComponent.text = lines[index];
                if(index >= lines.Count - 1)
                {
                    NextLine();
                }
             }
        }

    }
    /// <summary>
    /// used for starting a dialogue that is part of an event.
    /// </summary>
    public void StartDialogue()
    {
        OnDialogueStart?.Invoke(this, EventArgs.Empty); // Notify subscribers that dialogue has started

        print("Starting dialogue");
        SetCountsToZero();
        DeleteEmptyEntries();
        isEvent = true; // Set to true since this is an event dialogue
        dialogueEndTriggered = false;
        index = 0;
        StartCoroutine(TypeLine());
    }

    int currentLineCount = 0;
    int currentSpeaker = 0;
    int totalLinesCount = 0;

    public void SetCountsToZero()
    {
        totalLinesCount = 0; // Reset total lines count
        currentLineCount = 0;
        currentSpeaker = 0;
    }

    /// <summary>
    /// used for starting a dialogue that is not part of an event.
    /// </summary>
    /// <param name="nonEDial"></param>
    public void StartDialogue(EventData nonEDial)
    {
        print("Starting non-event dialogue");
        SetCountsToZero(); // Reset counts to zero before starting a new dialogue
        DeleteEmptyEntries();

        dialogueEndTriggered = false;
        index = 0;

        speakerCounts = new int[nonEDial.nonEventDialogue.speakingCharacters.Length, 2];

        int arrayCounter = 0;
        foreach (CharacterDialogue ds in nonEDial.nonEventDialogue.speakingCharacters)
        {
            totalLinesCount += ds.dialogueLines.Length;
            speakerCounts[arrayCounter, 0] = ds.speaker.id; // Store the speaker index
            speakerCounts[arrayCounter, 1] = 0; // Initialize the line count for this speaker
            arrayCounter++;
        }
        isEvent = false; // Set to false since this is a non-event dialogue



        StartCoroutine(TypeLine(nonEDial));
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

    IEnumerator TypeLine()
    {
        if (lines.Count == 0)
        {
            NextLine();
            print("No lines to display");
            yield break; // Exit if there are no lines to display
        }

        textComponent.text = ""; // Clear previous text


        string textBuffer = null; // Buffer to hold the current line text -> if not, buffer problems might ommit characters
        foreach (char c in lines[index].ToCharArray())
        {
            textBuffer += c;
            textComponent.text = textBuffer;
            yield return new WaitForSeconds(textSpeed);
        }

        if (index >= lines.Count - 1)
        {
            NextLine();
        }
    }

    /// <summary>
    /// Displays the next line of dialogue - used for both event and non-event dialogues. Ignores the EventData if there is no event with choices. (KingEvent or PeasantEvent)
    /// </summary>
    /// <param name="nonEDial"></param>
    public void CallTypeLine(EventData nonEDial = null)
    {
        if (nonEDial.eventType == EventType.KingEvent || nonEDial.eventType == EventType.PeasantEvent)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            StartCoroutine(TypeLine(nonEDial));
        }
    }

    public void CallSingleTypeLine(string line)
    {
        StartCoroutine(TypeSingleLine(line));
    }

    IEnumerator TypeSingleLine(string line)
    {

        if (lines.Count == 0)
        {
            print("No lines to display");
            yield break; // Exit if there are no lines to display
        }  
        textComponent.text = ""; // Clear previous text

        string textBuffer = null; // Buffer to hold the current line text -> if not, buffer problems might ommit characters
        foreach (char c in line.ToCharArray())
        {
            textBuffer += c;
            textComponent.text = textBuffer;
            yield return new WaitForSeconds(textSpeed);
        }

            TriggerDialogueEnd();
    }

    IEnumerator TypeLine(EventData nonEDial)
    {       

        if (lines.Count == 0)
        {
            print("No lines to display");
            NextLine();           
            yield break; // Exit if there are no lines to display
        }

        textComponent.text = ""; // Clear previous text

        dialogueLine dialogue = GetSpeakerData(nonEDial, ref currentLineCount, currentSpeaker, ref speakerCounts[currentSpeaker, 1]);

            if (dialogue.isFinalLine && currentLineCount < totalLinesCount) //if the dialogue line is the last line for the current speaker and there are more lines to process
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

        string textBuffer = null; // Buffer to hold the current line text -> if not, buffer problems might ommit characters
        foreach (char c in lines[index].ToCharArray())
        {
            textBuffer += c;
            textComponent.text = textBuffer;
            yield return new WaitForSeconds(textSpeed);
        }

        if (index >= lines.Count - 1)
        {
            NextLine();
        }
    }

    /// <summary>
    /// Retrieves the dialogue line data for a specific speaker in a non-event dialogue sequence.
    /// </summary>
    /// <remarks>This method updates the speaker's name and portrait in the UI based on the provided <paramref
    /// name="nonEDial"/> data.</remarks>
    /// <param name="nonEDial">The <see cref="EventData"/> object containing the non-event dialogue data.</param>
    /// <param name="currentLineCount">A reference to the current line count, which will be incremented by this method.</param>
    /// <param name="speakerId">The index of the speaker whose dialogue line is being retrieved.</param>
    /// <param name="speaker_lines_count">A reference to the line count for the specified speaker, which will be incremented by this method.</param>
    /// <returns>The <see cref="dialogueLine"/> object representing the current dialogue line for the specified speaker.</returns>
    private dialogueLine GetSpeakerData(EventData nonEDial, ref int currentLineCount, int speakerId, ref int speaker_lines_count)
    {
        CharacterDialogue sequence = nonEDial.nonEventDialogue.speakingCharacters[speakerId];
        dialogueLine dialogue = sequence.dialogueLines[speaker_lines_count];
        string current_line = dialogue.line;
        
        speaker_lines_count++;
        currentLineCount++;
        
        speakerName.text = sequence.speaker.name; // Set the speaker name
        portraitImage.sprite =sequence.speaker.portrait; // Set the speaker portrait if available
        return dialogue;
    }

    public void NextLine()
    {
        if (index < lines.Count - 1)
        {
            index++;
            textComponent.text = string.Empty;
            if(isEvent)
                StartCoroutine(TypeLine());
            else 
            { 
                EventData nonEDial = GameManager.Instance.eventManager.currentEvent;
                if (nonEDial != null)
                {
                    StartCoroutine(TypeLine(nonEDial));
                }
            }
        }
        else
        {
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

