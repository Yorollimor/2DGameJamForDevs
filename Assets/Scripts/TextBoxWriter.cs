using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TextBoxWriter : MonoBehaviour
{
    public TextMeshProUGUI textBox;
    public float textSpeed = 0.03f;
    public Image background;

    static bool isPrinting = false;
    bool continuePrinting = false;
    static int longestText = 0;

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
        float tempTextSpeed = textSpeed;
        int messageLength = fullText.Length;
        textBox.text = fullText;
        textBox.maxVisibleCharacters = 0;

        yield return new WaitForEndOfFrame();
        if (longestText < messageLength) { longestText = messageLength; }


        while (textBox.maxVisibleCharacters < messageLength)
        {

            if (skipLine)
            {
                tempTextSpeed = 0.01f;
                //textBox.maxVisibleCharacters = messageLength;
                //yield break; // Exit if skipLine is true
            }

            textBox.maxVisibleCharacters++;
            if (longestText == messageLength &&(!isPrinting || continuePrinting))
            {
                isPrinting = true;
                continuePrinting = true;
                print("Length: " + messageLength + "Visible Characters: " + textBox.maxVisibleCharacters);
            }
            yield return new WaitForSeconds(tempTextSpeed);
        }
    }
}
