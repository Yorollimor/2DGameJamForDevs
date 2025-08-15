using System;
using System.Collections.Generic;
using UnityEngine;

public enum DailyChangeType { Taxes, FoodConsumption, SoldierLoss, PeasantLoss }
public enum SpeakerType { King, Advisor, Peasants, HeadDoctor, General, Narrator, Treasurer }
public enum EventType { KingEvent, PeasantEvent, DialogueKing, DialoguePeasants, Cutscene }

[Serializable]
public class ResourceChange
{
    public ResourceType type;
    public int amount;
}

[Serializable]
public class DailyChangeChanges
{
    public DailyChangeType type;
    public int amount;
}

[Serializable]
public class ChoiceData
{
    public string choiceText;
    public string responseToChoice; // Text shown after the choice is made
    public List<ResourceChange> consequences;
    public List<DailyChangeChanges> dailyChanges; // Changes to the daily resource fluctuations. 
}

[Serializable]
public class DialogueLine
{
    [TextArea] public string line;
    public List<ChoiceData> choices; // List of choices available after this line
    public bool isFinalLine = false; // True if this is the last line of dialogue in the sequence
    public int nextSpeakerID = -1; // id of the next speaker in the sequence, -1 if no next speaker
}

[Serializable]
public class CharacterDialogue
{
    public Character speaker;
    public DialogueLine[] dialogueLines;
}

[CreateAssetMenu(fileName = "NewEvent", menuName = "KingAndPeasants/DialogueData")]
public class DialogueDataContainer : ScriptableObject
{
    public string title; // Title of the dialogue
    public CharacterDialogue[] speakingCharacters; // Array of dialogue sequences
}

[CreateAssetMenu(fileName = "NewEvent", menuName = "KingAndPeasants/EventData")]
public class EventData : ScriptableObject
{
    public string title;
    public EventType eventType; // Type of the event (KingEvent, PeasantEvent, DialogueKing, DialoguePeasants)

    public DialogueDataContainer dialogue;
    public bool isKingEvent = true; //True if this event involves the King, false if it involves peasants
}

