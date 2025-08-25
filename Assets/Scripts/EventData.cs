using System;
using System.Collections.Generic;
using UnityEngine;

public enum EventType { KingEvent, PeasantEvent, Cutscene, Introduction }
public enum ConditionType { Variable, PriorEvent, Both, EitherOr, None };

public enum  ComparisionOperators
{
    GreaterThan, LessThan, EqualTo, NotEqualTo
};

[Serializable]
public class ResourceCondition
{
    public DailyChangeChanges changeConditionToShow = null;
    public ResourceChange resourceConditionToShow = null;
    public ComparisionOperators comparisionOperator = ComparisionOperators.GreaterThan;

}

[CreateAssetMenu(fileName = "NewEvent", menuName = "KingAndPeasants/EventData")]
public class EventData : ScriptableObject
{
    [Header("Event Conditions")]
    public Variable conditionToShow = null;
    public EventData priorEvent = null; // Event that must be completed before this one can be shown
    public ConditionType conditionType = ConditionType.None;

    [Header("Resource Conditions")]
    public ResourceCondition resourceCondition = null;
    
    [Header("Event Data")]
    public string title;
    public EventType eventType; // Type of the event (KingEvent, PeasantEvent, DialogueKing, DialoguePeasants)

    public bool isGameOverEvent = false;
    public DialogueDataContainer dialogue;
}

