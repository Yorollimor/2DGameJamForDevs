using System;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceType { Gold, Food, Soldiers, Peasants }

[Serializable]
public class ResourceChange
{
    public ResourceType type;
    public int amount;
}

[Serializable]
public class ChoiceData
{
    public string choiceText;
    public List<ResourceChange> consequences;
}

[CreateAssetMenu(fileName = "NewEvent", menuName = "KingAndPeasants/EventData")]
public class EventData : ScriptableObject
{
    public string title;
    [TextArea] public string description;
    public bool isKingEvent; // true = advising king, false = advising peasants
    public List<ChoiceData> choices;
}

