using System;
using System.Linq;
using UnityEngine;
/*
public class DialogueManagerWrapper : MonoBehaviour
{
    private EventData currentEvent;

    public void StartEventDialogue(EventData DialogueDataContainer)
    {
        DialogueManager.StopAllConversations(); // Stop any active conversations before starting a new one

        var database = DialogueManager.MasterDatabase;
        Template template = Template.FromDefault();

        // print("Starting event dialogue for: " + DialogueDataContainer.title);
        if (DialogueDataContainer == null)
        {
            Debug.LogError("EventData is null!");
            return;
        }
        currentEvent = DialogueDataContainer;

        // Create a DialogueEntry programmatically
        var conversation = database.GetConversation("EventConversation");
       // print("Found conversation: " + conversation.Title);
        if (conversation == null)
        {
            Debug.LogError("Conversation 'EventConversation' not found in database!");
            return;
        }
        var startEntry = conversation.dialogueEntries.FirstOrDefault(e => e.id == 0);

        // Clear old responses
        startEntry.outgoingLinks.Clear();


        if (database.GetActor(DialogueDataContainer.actorName) == null) // Check if the speaker actor exists and create it if not
        {
            database.actors.Add(template.CreateActor(template.GetNextActorID(database), DialogueDataContainer.actorName, false));
        }

        conversation.ActorID = database.GetActor(DialogueDataContainer.actorName).id;

        if (database.GetActor(DialogueDataContainer.conversantName) == null) // Check if the conversant actor exists and create it if not
        {
            database.actors.Add(template.CreateActor(template.GetNextActorID(database), DialogueDataContainer.conversantName, true));
        }

        conversation.ConversantID = database.GetActor(DialogueDataContainer.conversantName).id; // Set the conversant to the event's conversant

        startEntry.ActorID = conversation.ActorID;
        startEntry.ConversantID = conversation.ConversantID;

        // Create the introduction node
        var introductionText = template.GetNextDialogueEntryID(conversation);
        var introductionNode = template.CreateDialogueEntry(introductionText, conversation.id, string.Empty);
        introductionNode.DialogueText = DialogueDataContainer.description;
        introductionNode.ActorID = conversation.ActorID;
        introductionNode.ConversantID = conversation.ConversantID;
        conversation.dialogueEntries.Add(introductionNode);

        // Link the start entry to the introduction node
        startEntry.outgoingLinks.Add(new Link
        {
            originConversationID = conversation.id,
            originDialogueID = startEntry.id,
            destinationConversationID = conversation.id,
            destinationDialogueID = introductionNode.id,
            isConnector = false
        });

        // Add a response for each choice in the event
        for (int i = 0; i < DialogueDataContainer.choices.Count; i++)
        {
            ChoiceData choice = DialogueDataContainer.choices[i];
            // Create a new dialogue entry for the choice
            var newNode = template.CreateDialogueEntry(template.GetNextDialogueEntryID(conversation), conversation.id, string.Empty);
            newNode.ActorID = conversation.ConversantID;
            newNode.ConversantID = conversation.ActorID;
            newNode.DialogueText = choice.choiceText;

            for (int y = 0; y < choice.consequences.Count; y++)
            {
                ResourceChange change = choice.consequences[y];
                newNode.DialogueText = newNode.DialogueText + "\n\n\n" + change.type.ToString() + ": " + change.amount.ToString();
            }

                newNode.userScript = "OnChoiceSelectedTrigger(" + i + ")"; // Set the user script to call OnChoiceSelected with the index of the choice
            conversation.dialogueEntries.Add(newNode);

            // Create a link from the introduction node to the new choice node
            Link response = new Link
            {
                originConversationID = conversation.id,
                originDialogueID = introductionNode.id,
                destinationConversationID = conversation.id,
                destinationDialogueID = newNode.id,
                isConnector = false
            };
            introductionNode.outgoingLinks.Add(response);
        }

        // Start the conversation
        if(!DialogueManager.IsConversationActive)
        {
            DialogueManager.StartConversation("EventConversation");
        }

    }

    public void OnChoiceSelected(double choiceIndex)
    {
        print("Choice Selected: " + choiceIndex);

        if (currentEvent == null || choiceIndex < 0 || choiceIndex >= currentEvent.choices.Count) return;

        var choice = currentEvent.choices[(int)choiceIndex];

        foreach (var change in choice.consequences)
        {
            GameManager.Instance.resourceManager.ModifyResource(change.type, change.amount);
        }

        GameManager.Instance.NextTurn();
    }
}
*/