using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private List<EventData> events;
    public static EventData currentEvent;
    private List<EventData> usedEvents = new List<EventData>();
    private EventType currentEventType;

    public static Character_Data speakingCharacter;
    public EventData GetNextEvent(EventType eventType, List<EventData> possibleEvents, Character_Data speaker)
    {
        currentEventType = eventType;
        speakingCharacter = speaker;
        events = possibleEvents;

        List<EventData> filteredEvents = new List<EventData>();

        //checks which events of the characterInfo have to correct type and have the correct variables collected to show up
        filteredEvents = CheckForAvailableEvents(eventType, possibleEvents);
        EventData gameOverData = CheckForGameOverEvent(filteredEvents);
        if(gameOverData != null)
        {
            currentEvent = gameOverData;
            usedEvents.Add(gameOverData);
            return gameOverData;
        }

        if (filteredEvents.Count == 0)
        {
            Debug.LogWarning("No events available for the current event type and conditions.");
            return null;
        }


        int randomIndex = Random.Range(0, filteredEvents.Count - 1);
        if (!IsNewEvent(filteredEvents[randomIndex]))
        {
            randomIndex = SearchNewEventIndex(filteredEvents, randomIndex);
        }

        if (!filteredEvents[randomIndex].isRepeatable && !usedEvents.Contains(filteredEvents[randomIndex]))
        {
            usedEvents.Add(filteredEvents[randomIndex]);
        }


        currentEvent = filteredEvents[randomIndex];

        return filteredEvents[randomIndex];
    }

    public List<EventData> CheckForAvailableEvents(EventType eventType, List<EventData> possibleEvents)
    {
        List<EventData> tempFilteredEvents = new();
        foreach (EventData e in possibleEvents)
        {
            if (e.availableAfterWeeks > GameManager.Instance.gameHandler.currentWeek) continue; //if the event is not yet available, skip it
            if (e.conditionToHide != null)
            {
                if (GameManager.Instance.gameHandler.collectedVariables.Contains(e.conditionToHide))
                {
                    print("Event: " + e.title + " was hidden due to collected variable");
                    continue; //skip the event if the hide condition variable has been collected
                }
            }
            if (e.eventType == eventType || e.eventType == EventType.Cutscene || e.eventType == EventType.Tutorial)
            {
                bool conditionalResourceExists = e.resourceCondition.resourceConditionToShow.amount > 0 ? true : e.resourceCondition.resourceConditionToShow.amount < 0 ? true : e.resourceCondition.changeConditionToShow.amount < 0 ? true : e.resourceCondition.changeConditionToShow.amount > 0 ? true : false;
                if (e.conditionToShow == null && !conditionalResourceExists && e.priorEvent == null)
                {
                    tempFilteredEvents.Add(e);
                }
                else
                {
                    if (e.conditionType != ConditionType.None)
                    {
                        if ((e.priorEvent != null && usedEvents.Contains(e.priorEvent)) && (e.conditionType == ConditionType.EitherOr || e.conditionType == ConditionType.PriorEvent))
                        {
                            print("Event: " + e.title + " contained correct prior event");
                            if (conditionalResourceExists)
                            {
                                CheckForResourceConditionals(tempFilteredEvents, e, conditionalResourceExists); //checks if the resource condition is also met and adds the event if so
                            }
                            else
                                tempFilteredEvents.Add(e);
                        }
                        if (e.conditionToShow != null && e.conditionType == ConditionType.EitherOr || e.conditionType == ConditionType.Variable)
                        {
                            if (GameManager.Instance.gameHandler.collectedVariables.Contains(e.conditionToShow))
                            {
                                print("Event: " + e.title + " contained correct variable");
                                if (conditionalResourceExists)
                                {
                                    CheckForResourceConditionals(tempFilteredEvents, e, conditionalResourceExists); //checks if the resource condition is also met and adds the event if so
                                }
                                else
                                    tempFilteredEvents.Add(e);
                            }
                            else print("Event: " + e.title + " did not contain correct variable");
                        }
                        if (e.conditionType == ConditionType.Both)
                        {
                            if (e.priorEvent != null && usedEvents.Contains(e.priorEvent) && e.conditionToShow != null && GameManager.Instance.gameHandler.collectedVariables.Contains(e.conditionToShow))
                            {
                                print("Event: " + e.title + " contained correct variable and prior event");
                                if (conditionalResourceExists)
                                {
                                    CheckForResourceConditionals(tempFilteredEvents, e, conditionalResourceExists); //checks if the resource condition is also met and adds the event if so
                                }
                                else
                                    tempFilteredEvents.Add(e);
                            }
                        }
                    }
                    else if (conditionalResourceExists)
                    {
                        CheckForResourceConditionals(tempFilteredEvents, e, conditionalResourceExists);
                    }
                    else
                    {
                        Debug.LogWarning("Event: " + e.title + " has a conditionToShow or PriorEvent but no ConditionType Operator set. Added to possible events.");
                        tempFilteredEvents.Add(e);
                    }

                }
            }
        }
        return tempFilteredEvents;
    }

    public EventData CheckForGameOverEvent(List<EventData> filteredEvents)
    {
        foreach(EventData e in filteredEvents)
        {
            if(e.isGameOverEvent)
            {
                return e;
            }
        }
        return null;
    }

    private void CheckForResourceConditionals(List<EventData> filteredEvents, EventData e, bool conditionalResourceAmount)
    {
        if (conditionalResourceAmount)
        {
            ResourceManager rM = GameManager.Instance.resourceManager;
            int resourceAmount = rM.GetResource(e.resourceCondition.resourceConditionToShow.type);
            int changeAmount = rM.GetResource(e.resourceCondition.changeConditionToShow.type);

            if (e.resourceCondition.resourceConditionToShow.amount != 0)
            {
                switch (e.resourceCondition.comparisionOperator)
                {
                    case ComparisionOperators.GreaterThan:
                        if (resourceAmount >= e.resourceCondition.resourceConditionToShow.amount)
                            filteredEvents.Add(e);
                        break;

                    case ComparisionOperators.LessThan:
                        if (resourceAmount < e.resourceCondition.resourceConditionToShow.amount)
                            filteredEvents.Add(e);
                        break;
                    case ComparisionOperators.EqualTo:
                        if (resourceAmount == e.resourceCondition.resourceConditionToShow.amount)
                            filteredEvents.Add(e);
                        break;
                    case ComparisionOperators.NotEqualTo:
                        if (resourceAmount != e.resourceCondition.resourceConditionToShow.amount)
                            filteredEvents.Add(e);
                        break;
                }
            }
            else if (e.resourceCondition.changeConditionToShow.amount != 0)
            {
                switch (e.resourceCondition.comparisionOperator)
                {
                    case ComparisionOperators.GreaterThan:
                        if (changeAmount >= e.resourceCondition.changeConditionToShow.amount)
                            filteredEvents.Add(e);
                        break;
                    case ComparisionOperators.LessThan:
                        if (changeAmount < e.resourceCondition.changeConditionToShow.amount)
                            filteredEvents.Add(e);
                        break;
                    case ComparisionOperators.EqualTo:
                        if (changeAmount == e.resourceCondition.changeConditionToShow.amount)
                            filteredEvents.Add(e);
                        break;
                    case ComparisionOperators.NotEqualTo:
                        if (changeAmount != e.resourceCondition.changeConditionToShow.amount)
                            filteredEvents.Add(e);
                        break;
                }
            }
        }
    }

    private bool IsNewEvent(EventData currentSelection)
    {
        foreach (EventData usedEvent in usedEvents)
        {
            if (usedEvent.Equals(currentSelection))
            {
                return false; // Event has already been used
            }
        }
        return true; // Event is new
    }

    private int SearchNewEventIndex(List<EventData> filteredEvents, int original)
    {
        bool allUsed = true;
        foreach (EventData eventData in filteredEvents)
        {
            if (!usedEvents.Contains(eventData))
            {
                allUsed = false;
                return filteredEvents.IndexOf(eventData);
            }
        }
        if (allUsed) // Reset the used events of the current speaker if all events have been used
        {
            Debug.LogWarning("All events for the current speaker have been used. Resetting used events for this speaker: " + speakingCharacter);
            foreach (EventData eventData in usedEvents)
            {
                if (eventData.dialogue.speakingCharacters[0].speaker == speakingCharacter) // first speaker is always the main speaker of the event
                {
                    usedEvents.Remove(eventData);
                    break;
                }
            }
        }
        return Random.Range(0, CheckForAvailableEvents(currentEventType, events).Count -1); // After resetting, return a new random index of the applicable events.
    }

    public List<EventData> GetUsedEVents()
    {
        return usedEvents;
    }

    public void ResetEvents() { usedEvents = new List<EventData>(); }


}
