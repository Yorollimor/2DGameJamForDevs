using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private List<EventData> events;
    public EventData currentEvent;
    private List<EventData> usedEvents = new List<EventData>();

    public Character_Data speakingCharacter;
    public EventData GetNextEvent(EventType eventType, List<EventData> possibleEvents, Character_Data speaker)
    {
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
        if (!CheckIfNewEvent(filteredEvents[randomIndex]))
        {
            randomIndex = SearchNewEventIndex(filteredEvents, randomIndex);
        }
        else
        {
            usedEvents.Add(filteredEvents[randomIndex]);
        }

        currentEvent = filteredEvents[randomIndex];

        return filteredEvents[randomIndex];
    }

    public List<EventData> CheckForAvailableEvents(EventType eventType, List<EventData> possibleEvents)
    {
        List<EventData> tempFilteredEvents = new List<EventData>();
        foreach (EventData e in possibleEvents)
        {

            if (e.eventType == eventType || e.eventType == EventType.Cutscene)
            {

                bool conditionalResourceAmount = e.resourceCondition.resourceConditionToShow.amount > 0 ? true : e.resourceCondition.resourceConditionToShow.amount < 0 ? true : e.resourceCondition.changeConditionToShow.amount < 0 ? true : e.resourceCondition.changeConditionToShow.amount > 0 ? true : false;
                if (e.conditionToShow == null && !conditionalResourceAmount && e.priorEvent == null)
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
                            if (conditionalResourceAmount)
                            {
                                CheckForResourceConditinals(tempFilteredEvents, e, conditionalResourceAmount); //checks if the resource condition is also met and adds the event if so
                            }
                            else
                                tempFilteredEvents.Add(e);
                        }
                        if (e.conditionToShow != null && e.conditionType == ConditionType.EitherOr || e.conditionType == ConditionType.Variable)
                        {
                            if (GameManager.Instance.gameHandler.collectedVariables.Contains(e.conditionToShow))
                            {
                                print("Event: " + e.title + " contained correct variable");
                                if (conditionalResourceAmount)
                                {
                                    CheckForResourceConditinals(tempFilteredEvents, e, conditionalResourceAmount); //checks if the resource condition is also met and adds the event if so
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
                                if (conditionalResourceAmount)
                                {
                                    CheckForResourceConditinals(tempFilteredEvents, e, conditionalResourceAmount); //checks if the resource condition is also met and adds the event if so
                                }
                                else
                                    tempFilteredEvents.Add(e);
                            }
                        }
                    }
                    else if (conditionalResourceAmount)
                    {
                        CheckForResourceConditinals(tempFilteredEvents, e, conditionalResourceAmount);
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

    private void CheckForResourceConditinals(List<EventData> filteredEvents, EventData e, bool conditionalResourceAmount)
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

    private bool CheckIfNewEvent(EventData currentSelection)
    {
        foreach (EventData usedIndex in usedEvents)
        {
            if (usedIndex.Equals(currentSelection))
            {
                return false; // Event has already been used
            }
        }
        return true; // Event is new
    }

    private int SearchNewEventIndex(List<EventData> filteredEvents, int original)
    {
        int totalEvents = events.Count;
        foreach (EventData eventData in filteredEvents)
        {
            if (!usedEvents.Contains(eventData))
            {
                usedEvents.Add(eventData);
                return filteredEvents.IndexOf(eventData);
            }
        }
        return original; // If all events have been used, return the original index
    }

    public List<EventData> GetUsedEVents()
    {
        return usedEvents;
    }

}
