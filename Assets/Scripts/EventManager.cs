using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public List<EventData> events;

    public EventData GetNextEvent(bool isKingTurn)
    {
        var filteredEvents = events.Where(e => e.isKingEvent == isKingTurn).ToList();
        if (filteredEvents.Count == 0) return null;

        int randomIndex = Random.Range(0, filteredEvents.Count);
        return filteredEvents[randomIndex];
    }
}
