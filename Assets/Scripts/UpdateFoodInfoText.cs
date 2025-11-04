using UnityEngine;
using TMPro;

public class UpdateFoodInfoText : MonoBehaviour
{
    public TextMeshProUGUI text;

    public void Start()
    {
        UpdateResourceInfo(null, null);
        GameManager.Instance.resourceManager.OnFoodProductionChange += UpdateResourceInfo;
    }

    public void UpdateResourceInfo(object sender, System.EventArgs e)
    {
        text.text="Weekly production: " + GameManager.Instance.gameHandler.weeklyFoodProduction * GameManager.Instance.resourceManager.population
            + "\nWeekly total consumption: " + GameManager.Instance.gameHandler.dailyFoodConsumption * GameManager.Instance.resourceManager.population *7
            + "\nProduction/Person " + GameManager.Instance.gameHandler.weeklyFoodProduction;
    }
}
