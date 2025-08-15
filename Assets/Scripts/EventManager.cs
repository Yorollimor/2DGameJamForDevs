using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public List<EventData> events;
    public EventData currentEvent;
    private List<EventData> usedEvents = new List<EventData>();
  

    public EventData GetNextEvent(bool isKingTurn)
    {
       
       var filteredEvents = events.Where(e => e.isKingEvent == isKingTurn).ToList();
        print(filteredEvents.Count + " events found for " + (isKingTurn ? "King" : "Peasants"));
        if (filteredEvents.Count == 0) return null;

        int randomIndex = Random.Range(0, filteredEvents.Count-1);
        if (!CheckIfNewEvent(filteredEvents[randomIndex]))
        {
            randomIndex = SearchNewEventIndex(filteredEvents, randomIndex);
        }
        else { 
            usedEvents.Add(filteredEvents[randomIndex]);
        }

            foreach (EventData usedIndex in usedEvents)
            {
                print(usedIndex.title + " has been used.");
            }

        currentEvent = filteredEvents[randomIndex];
        return filteredEvents[randomIndex];
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
       foreach(EventData eventData in filteredEvents)
        {
            if (!usedEvents.Contains(eventData))
            {
                usedEvents.Add(eventData);
                return filteredEvents.IndexOf(eventData);
            }
        }
        return original; // If all events have been used, return the original index
    }
}
