using UnityEngine;
using TMPro;
using Language.Lua;

public class GetDailyChange : MonoBehaviour
{
    GameManager gameManager;
    GameHandler gameHandler;
    public DailyChangeType dailyChangeType;


    TextMeshProUGUI text;

    private void Start()
    {
        gameManager = GameManager.Instance;
        gameHandler = gameManager.gameHandler;
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (gameHandler == null || gameManager == null)
        {
            return; // Ensure gameHandler and gameManager are initialized
        }
        switch (dailyChangeType)
        {
            case DailyChangeType.Taxes:
                text.color = SwitchColorBasedOnValue(gameHandler.taxes);
                text.text = gameHandler.taxes.ToString();
            break;

            case DailyChangeType.FoodConsumption:
                text.color = SwitchColorBasedOnValue(gameHandler.dailyFoodConsumption);
                text.text = gameHandler.dailyFoodConsumption.ToString();
            break;

            case DailyChangeType.SoldierLoss:
                text.color = SwitchColorBasedOnValue(gameHandler.dailySoldierLosses);
                text.text = gameHandler.dailySoldierLosses.ToString();
            break;

            case DailyChangeType.PeasantLoss:
                text.color = SwitchColorBasedOnValue(gameHandler.dailyPeasantLosses);
                text.text = gameHandler.dailyPeasantLosses.ToString();
            break;

        }
    }

    private Color SwitchColorBasedOnValue(int value)
    {
        if (value < 0)
        {
            return Color.red; // Negative values in red
        }
        else if (value > 0)
        {
            return Color.green; // Positive values in green
        }
        else
        {
            return Color.white; // Neutral values in white
        }
    }

}
