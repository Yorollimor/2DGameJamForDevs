using System;
using System.Collections.Generic;
using UnityEngine;

public enum DailyChangeType { Taxes, FoodProduction, SoldierGrowth, PopulationGrowth, FoodConsumption }
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
    public Variable conditionToShow = null;
    public Variable conditionToHide = null;
    public string choiceText;
    public string responseToChoice; // Text shown after the choice is made
    public List<ResourceChange> consequences;
    public List<DailyChangeChanges> dailyChanges; // Changes to the daily resource fluctuations.
    public Variable receivingVaribale = null;
    public Variable removingVariable = null;
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

[CreateAssetMenu(fileName = "NewEvent", menuName = "KingAndPeasants/DialogueDataContainer")]
public class DialogueDataContainer : ScriptableObject
{
    public string title; // Title of the dialogue
    public CharacterDialogue[] speakingCharacters; // Array of dialogue sequences
}