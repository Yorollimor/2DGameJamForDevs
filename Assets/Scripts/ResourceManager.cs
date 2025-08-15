using UnityEngine;
using TMPro;
using System;

public enum ResourceType { 
    Gold, Food, Soldiers, Peasants, 
    Medicine, SickPeople, AcceptanceOfKing }

public class ResourceManager : MonoBehaviour
{
    public int gold = 50;
    public int food = 50;
    public int soldiers = 20;
    public int peasants = 50;
    public int medicine = 10; 
    public int sickPeople = 0;
    public int acceptanceOfKing = 50; // Percentage of acceptance by the peasants


    public TextMeshProUGUI goldText;
    public TextMeshProUGUI foodText;
    public TextMeshProUGUI soldiersText;
    public TextMeshProUGUI peasantsText;

    public TextMeshProUGUI medicineText;
    public TextMeshProUGUI sickPeopleText;
    public TextMeshProUGUI acceptanceOfKingText;

    private GameHandler gameHandler;

    private void Start()
    {
        UpdateUI();
        gameHandler = GameManager.Instance.gameHandler;
        gameHandler.onDayChange += DailyAdjustments; // Subscribe to the day change event
    }
    private void OnDisable()
    {
        if (gameHandler != null)
        {
            gameHandler.onDayChange -= DailyAdjustments; // Unsubscribe to avoid memory leaks
        }
    }

    /// <summary>
    /// Modifies the specified resource by adding the given amount.
    /// </summary>
    /// <remarks>This method adjusts the value of the specified resource by the provided amount.  Ensure that
    /// the <paramref name="type"/> parameter corresponds to a valid resource type.</remarks>
    /// <param name="type">The type of resource to modify. Must be one of the defined <see cref="ResourceType"/> values.</param>
    /// <param name="amount">The amount to add to the specified resource. Can be positive or negative.</param>
    public void ModifyResource(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Gold: gold += amount; break;
            case ResourceType.Food: food += amount; break;
            case ResourceType.Soldiers: soldiers += amount; break;
            case ResourceType.Peasants: peasants += amount; break;
            case ResourceType.Medicine: medicine += amount; break;
            case ResourceType.SickPeople: sickPeople += amount; break;
            case ResourceType.AcceptanceOfKing: acceptanceOfKing += amount; break;
        }
        UpdateUI();
    }

    public void ModifyResource(DailyChangeType type, int amount)
    {
        switch (type)
        {
            case DailyChangeType.Taxes: gameHandler.taxes += amount; break;
            case DailyChangeType.FoodConsumption: gameHandler.dailyFoodConsumption += amount; break;
            case DailyChangeType.SoldierLoss: gameHandler.dailySoldierLosses += amount; break;
            case DailyChangeType.PeasantLoss: gameHandler.dailyPeasantLosses += amount; break;
        }
        UpdateUI();
    }

    public int GetResource(ResourceType type)
    {
        return type switch
        {
            ResourceType.Gold => gold,
            ResourceType.Food => food,
            ResourceType.Soldiers => soldiers,
            ResourceType.Peasants => peasants,
            ResourceType.Medicine => medicine,
            ResourceType.SickPeople => sickPeople,
            ResourceType.AcceptanceOfKing => acceptanceOfKing,
            _ => 0
        };
    }

    public void UpdateUI()
    {
        if (goldText != null) goldText.text = $"Gold: {gold}";
        if (foodText != null) foodText.text = $"Food: {food}";
        if (soldiersText != null) soldiersText.text = $"Soldiers: {soldiers}";
        if (peasantsText != null) peasantsText.text = $"Peasants: {peasants}";
        if (medicineText != null) medicineText.text = $"Medicine: {medicine}";
        if (sickPeopleText != null) sickPeopleText.text = $"Sick People: {sickPeople}";
        if (acceptanceOfKingText != null) acceptanceOfKingText.text = $"Acceptance of King: {acceptanceOfKing}%";
    }

    public void SetResource(ResourceType type, int value)
    {
        switch (type)
        {
            case ResourceType.Gold: gold = value; break;
            case ResourceType.Food: food = value; break;
            case ResourceType.Soldiers: soldiers = value; break;
            case ResourceType.Peasants: peasants = value; break;
            case ResourceType.Medicine: medicine = value; break;
            case ResourceType.SickPeople: sickPeople = value; break;
            case ResourceType.AcceptanceOfKing: acceptanceOfKing = value; break;
            default: print("resource no match"); break;
        }
        UpdateUI();
    }

    public void DailyAdjustments(object sender, EventArgs e)
    {
        // Adjust resources based on daily changes
        ModifyResource(ResourceType.Gold, gameHandler.taxes); // Example: Add 5 gold daily
        ModifyResource(ResourceType.Food, -gameHandler.dailyFoodConsumption); // Example: Consume 2 food daily
        ModifyResource(ResourceType.Soldiers, -gameHandler.dailySoldierLosses); // Example: Lose 1 soldier daily
        ModifyResource(ResourceType.Peasants, -gameHandler.dailyPeasantLosses); // Example: Lose 1 peasant daily

        // Update UI after adjustments
        UpdateUI();
    }
}
