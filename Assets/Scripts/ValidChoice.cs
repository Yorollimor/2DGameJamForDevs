using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class ValidChoice : MonoBehaviour
{
    public bool isValidChoice = true;

    [SerializeField] private List<TextMeshProUGUI> textToDisableList;
    [SerializeField] private GameObject invalidReasonTextContainer;

    private void OnEnable()
    {
        invalidReasonTextContainer.gameObject.SetActive(false);
        //isValidChoice = true;
    }

    public void MakeChoiceInvalid(string reason)
    {
        invalidReasonTextContainer.GetComponentInChildren<TextMeshProUGUI>().text = reason;
        invalidReasonTextContainer.gameObject.SetActive(true);
        isValidChoice = false;

        foreach(TextMeshProUGUI t in textToDisableList)
        {
            t.color = Color.gray;
        }
    }
}
