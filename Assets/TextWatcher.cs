using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextWatcher : MonoBehaviour
{
    private TextMeshProUGUI tmp;
    private string lastText;

    private void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        lastText = tmp.text;
    }

    private void LateUpdate()
    {
        if (tmp.text != lastText)
        {
            Debug.LogWarning(
                $"[TextWatcher] '{gameObject.name}' text changed from '{lastText}' to '{tmp.text}' at frame {Time.frameCount}.\nStack:\n" +
                System.Environment.StackTrace,
                this
            );
            lastText = tmp.text;
        }
    }
}
