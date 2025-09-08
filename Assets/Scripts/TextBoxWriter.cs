using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TextBoxWriter : MonoBehaviour
{
    public TextMeshProUGUI textBox;
    public float textSpeed = 0.03f;
    public Image background;

    bool skipLine = false;

    private string fullText;

    public void WriteText()
    {
        fullText = textBox.text;

        StartCoroutine(TypeText());
    }

    public void SkipLine()
    {
        skipLine = true;
    }

    public IEnumerator TypeText()
    {
        int messageLength = fullText.Length;
        textBox.text = fullText;
        textBox.maxVisibleCharacters = 0;

        while (textBox.maxVisibleCharacters < messageLength)
        {
            if (skipLine)
            {
                textBox.maxVisibleCharacters = messageLength;
                yield break; // Exit if skipLine is true
            }

            textBox.maxVisibleCharacters++;
            yield return new WaitForSeconds(textSpeed);
        }
    }
}
