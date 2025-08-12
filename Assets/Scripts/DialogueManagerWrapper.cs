using PixelCrushers.DialogueSystem;
using System.Linq;
using UnityEngine;

public class DialogueManagerWrapper : MonoBehaviour
{
    private EventData currentEvent;

    public void StartEventDialogue(EventData eventData)
    {
        
       // print("Starting event dialogue for: " + eventData.title);
        if (eventData == null)
        {
            Debug.LogError("EventData is null!");
            return;
        }
        currentEvent = eventData;

        // Create a DialogueEntry programmatically
        var conversation = DialogueManager.MasterDatabase.GetConversation("EventConversation");
       // print("Found conversation: " + conversation.Title);
        if (conversation == null)
        {
            Debug.LogError("Conversation 'EventConversation' not found in database!");
            return;
        }
        var startEntry = conversation.dialogueEntries.FirstOrDefault(e => e.id == 0);

        // Clear old responses
        startEntry.outgoingLinks.Clear();

        // Add a response for each choice in the event
        for (int i = 0; i < eventData.choices.Count; i++)
        {
            var choice = eventData.choices[i];
            var response = new Link
            {
                destinationConversationID = conversation.id,
                destinationDialogueID = 1, // You can route to a generic "end" node
                isConnector = false
            };
            startEntry.outgoingLinks.Add(response);

            // Store choice text and index in Lua variables so Dialogue System can show it
            Lua.Run($"Variable['ChoiceText_{i}'] = \"{choice.choiceText}\"");
        }

        // Store description
        Lua.Run($"Variable['EventDescription'] = \"{eventData.description}\"");
        print(eventData.description);

        // Start the conversation
        DialogueManager.StartConversation("EventConversation");
    }

    public void OnChoiceSelected(int choiceIndex)
    {
        if (currentEvent == null || choiceIndex < 0 || choiceIndex >= currentEvent.choices.Count) return;

        var choice = currentEvent.choices[choiceIndex];

        foreach (var change in choice.consequences)
        {
            GameManager.Instance.resourceManager.ModifyResource(change.type, change.amount);
        }

        GameManager.Instance.NextTurn();
    }
}
