using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.UIElements;
public class GameOverStats : MonoBehaviour
{
    public TextMeshProUGUI statsText;
    public float textSpeed = 0.05f;

    private string gameOverMessage = "Game Over";

    public void SetGameOverMessage(string message)
    {
        gameOverMessage = message;

        int totalEvents = 0;
        int completedEvents = 0;

        string temp;

        completedEvents = GameManager.Instance.eventManager.GetUsedEVents().Count;

        foreach (Character_Data c in GameManager.Instance.gameHandler.originalCharacters)
        {
            totalEvents += c.associatedEvents.Count;
        }

        temp = $"\nDays managed: {GameManager.Instance.gameHandler.currentDay}" +
            $"\nEvents: {completedEvents} of {totalEvents}" +
            $"\n\n{gameOverMessage}";

        StartCoroutine(TypeText(temp, statsText));
    }

    IEnumerator TypeText(string text, TextMeshProUGUI textHolder)
    {
        bool enteredTag = false;
        bool exitedTag = false;
        string buffer = textHolder.text;
        foreach (char c in text.ToCharArray())
        {
            if (c == '\n') // Handle new line
            {
                buffer += c;
                textHolder.text = buffer; // Update text immediately for new lines
                yield return new WaitForSeconds(textSpeed);
                continue;
            }
            else if (c == '<' || enteredTag && !exitedTag) // Handle color tags
            {
                enteredTag = true;
                exitedTag = false;
                buffer += c;
                if (c == '>') // If we reached the end of a tag
                {
                    exitedTag = true;
                    enteredTag = false;
                }
                continue;
            }

            buffer += c;
            textHolder.text = buffer;
            yield return new WaitForSeconds(textSpeed);
        }

    }
}
