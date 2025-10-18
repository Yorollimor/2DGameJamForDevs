using UnityEngine;
using TMPro;

using System.Collections;


public class GetDailyChange : MonoBehaviour
{
    GameManager gameManager;
    GameHandler gameHandler;
    public DailyChangeType type; 


    TextMeshProUGUI text;

    private void Start()
    {
        gameManager = GameManager.Instance;
        gameHandler = gameManager.gameHandler;
        text = GetComponent<TextMeshProUGUI>();
    }

    public void StartUpdateChangeText(int amountMin, int amountMax = 0)
    {
        StartCoroutine(UpdateChangeText(amountMin, amountMax));
    }

    private IEnumerator UpdateChangeText(int amountMin, int amountMax = 0)
    {
        while (gameManager == null || gameHandler == null || text == null)
        {
            yield return new WaitForFixedUpdate();
        }

        if (text == null || gameHandler == null) Debug.LogWarning("Text or gamehandler null");

        switch (type)
        {
            case DailyChangeType.Taxes:
                text.color = SwitchColorBasedOnValue(amountMax == 0 ? amountMin : (amountMin+amountMax)/2);
                text.text = $"{amountMin} - {amountMax}/W";
            break;

            case DailyChangeType.FoodProduction:
                text.color = SwitchColorBasedOnValue(amountMax == 0 ? amountMin : (amountMin + amountMax) / 2);
                text.text = $"{amountMin}/W";
                break;

            case DailyChangeType.SoldierGrowth:
                text.color = SwitchColorBasedOnValue(amountMax == 0 ? amountMin : (amountMin + amountMax) / 2);
                text.text = amountMin.ToString() + "/D";
                break;

            case DailyChangeType.PopulationGrowth:
                text.color = SwitchColorBasedOnValue(amountMax == 0 ? amountMin : (amountMin + amountMax) / 2);
                text.text = amountMin.ToString() + "/D";
                break;

        }
    }

    private Color SwitchColorBasedOnValue(float value)
    {
        float valueMapped = Mathf.InverseLerp(-1000, 1000, value);
        Color c = Color.Lerp(Color.red, Color.green, valueMapped);

        if (value > -2 && value < 2)
        {
            return Color.white; // Neutral values in white
        }
        else return c;
    }

}
