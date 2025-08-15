using System;
using System.Collections.Generic;
using UnityEngine;

public enum  DailyChangeType { Taxes, FoodConsumption, SoldierLoss, PeasantLoss }
public enum SpeakerType { King, Advisor, Peasants, HeadDoctor, General, Narrator, Treasurer}
public enum EventType { KingEvent, PeasantEvent, DialogueKing, DialoguePeasants, Cutscene}

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

[CreateAssetMenu(fileName = "NewEvent", menuName = "KingAndPeasants/EventData")]
public class EventData : ScriptableObject
{
    public string title;
    public Character speaker; // Speaker for this choice
    public EventType eventType; // Type of the event (KingEvent, PeasantEvent, DialogueKing, DialoguePeasants)

    [TextArea] public string[] descriptionDialogue;
    public bool isKingEvent = true; //True if this event involves the King, false if it involves peasants
    public List<ChoiceData> choices;

    public NonEventDialogue nonEventDialogue; // Dialogue to show if no choices are available
}

