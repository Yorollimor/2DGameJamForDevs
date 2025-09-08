using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public enum ResourceType { 
    Gold, Authority, Soldiers, 
    Food, Population, Health, LoyaltyToKing }

public class ResourceManager : MonoBehaviour
{
    public int gold = 50;
    public int soldiers = 20;
    public int authority = 80;
    public int population = 50;
    public int food = 50;
    public int health = 0;
    public int loyaltyToKing = 50; // Percentage of acceptance by the populatino

    public float goldToAuthorityTransfer = 0.3f;
    public float soldiersToAuthorityTransfer = 0.3f;
    public float healthToPopulationGrowthThreshold = 0.6f; // Percentage of health needed for population growth
    public float healthToPopulationLossThreshold = 0.3f; // Percentage of health needed for population loss
    public float foodToHealthTransfer = 0.2f; // Percentage of food that transfers to health
    public float foodGrowthToPopulationGrowthTransfer = 0.3f;
    public float foodToLoyaltyTransfer = 0.2f; // Percentage of food that transfers to loyalty


    public TextMeshProUGUI goldText;
    public TextMeshProUGUI authorityText;
    public TextMeshProUGUI soldiersText;

    public TextMeshProUGUI populationText;
    public TextMeshProUGUI foodText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI loyaltyToKingText;

    public TextMeshProUGUI baseTaxesText;
    public TextMeshProUGUI baseFoodUseText;

    public List<GetDailyChange> dailyChangeTextList = new List<GetDailyChange>();

    private GameHandler gameHandler;

    private Dictionary<string, float> dailyChangeSummary = new Dictionary<string, float>();
    private Dictionary<string, float> weeklyChangeSummary = new Dictionary<string, float>();
    public Dictionary<string, Image> resourceIconsDictionary = new Dictionary<string, Image>();

    public delegate void OnResourceChanged(object sender, EventArgs args);
    public event OnResourceChanged onFoodProductionChange;
    private void Start()
    {
        gameHandler = GameManager.Instance.gameHandler;

        dailyChangeSummary.Add("Gold", 0);
        dailyChangeSummary.Add("Authority", 0);
        dailyChangeSummary.Add("Population", 0);
        dailyChangeSummary.Add("Food", 0);
        dailyChangeSummary.Add("Loyalty To King", 0);
        dailyChangeSummary.Add("Health", 0);
        dailyChangeSummary.Add("Soldiers", 0);

        dailyChangeSummary.Add("Taxes", 0);
        dailyChangeSummary.Add("Population Growth", 0);
        dailyChangeSummary.Add("Soldier Growth", 0);
        dailyChangeSummary.Add("Food Consumption", 0);
        dailyChangeSummary.Add("Food Production", 0);



        // deprecated code part
        weeklyChangeSummary.Add("Gold", 0);
        weeklyChangeSummary.Add("Authority", 0);
        weeklyChangeSummary.Add("Population", 0);
        weeklyChangeSummary.Add("Food", 0);
        weeklyChangeSummary.Add("Loyalty To King", 0);
        weeklyChangeSummary.Add("Health", 0);
        weeklyChangeSummary.Add("Soldiers", 0);

        weeklyChangeSummary.Add("Taxes", 0);
        weeklyChangeSummary.Add("Population Growth", 0);
        weeklyChangeSummary.Add("Soldier Growth", 0);
        weeklyChangeSummary.Add("Food Consumption", 0);
        weeklyChangeSummary.Add("Food Production", 0);
    }

    public Dictionary<string, float> GetDailyResourceChanges()
    { return dailyChangeSummary; }

    public Dictionary<string, float> GetWeeklyResourceChanges()
    { return weeklyChangeSummary; }


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
            case ResourceType.Gold:
                gold += amount;
                dailyChangeSummary["Gold"] += amount;
                weeklyChangeSummary["Gold"] += amount;
                break;
            case ResourceType.Food:
                food += amount;
                if(food < 0) food = 0;
                dailyChangeSummary["Food"] += amount;
                weeklyChangeSummary["Food"] += amount;
                break;
            case ResourceType.Soldiers: 
                soldiers += amount;
                if(soldiers < 0) soldiers = 0;
                dailyChangeSummary["Soldiers"] +=  amount;
                weeklyChangeSummary["Soldiers"] +=  amount;
                break;
            case ResourceType.Population:
                population += amount;
                onFoodProductionChange?.Invoke(this, EventArgs.Empty);
                if (population < 0) population = 0;
                dailyChangeSummary["Population"] += amount;
                weeklyChangeSummary["Population"] += amount;
                break;
            case ResourceType.Health:
                health += amount;
                if(health > 100) health = 100;
                if(health < 0) health = 0;
                dailyChangeSummary["Health"] += amount;
                weeklyChangeSummary["Health"] += amount;
                break;

            case ResourceType.LoyaltyToKing:
                loyaltyToKing += amount;
                if(loyaltyToKing > 100) loyaltyToKing = 100;
                if(loyaltyToKing < 0) loyaltyToKing = 0;
                dailyChangeSummary["Loyalty To King"] += amount;
                weeklyChangeSummary["Loyalty To King"] += amount;
                break;
            case ResourceType.Authority:
                authority += amount;
                if(authority > 100) authority = 100;
                if(authority < 0) authority = 0;
                dailyChangeSummary["Authority"] += amount;
                weeklyChangeSummary["Authority"] += amount;
                break;
        }
        UpdateUI();
    }

    public void ResetDailyTracking()
    {
        Dictionary<string, float> temp = new Dictionary<string, float>(dailyChangeSummary);
        foreach (string k in temp.Keys)
        {
            dailyChangeSummary[k] = 0;
        }
    }

    public void ResetWeeklyTracking()
    { 
        Dictionary<string, float> temp = new Dictionary<string, float>(weeklyChangeSummary);
        foreach (string k in temp.Keys)
        {
            weeklyChangeSummary[k] = 0;
        }
    }

    public void ModifyResource(DailyChangeType type, int amount)
    {
        switch (type)
        {
            case DailyChangeType.Taxes:
                gameHandler.taxes += amount;
                if(gameHandler.taxes <0)
                {
                    gameHandler.taxes = 0; // Ensure taxes cannot go negative
                }
                dailyChangeSummary["Taxes"] += amount;
                weeklyChangeSummary["Taxes"] += amount;
                break;
            case DailyChangeType.FoodProduction:
                gameHandler.weeklyFoodProduction += amount;
                onFoodProductionChange?.Invoke(this, EventArgs.Empty);
                dailyChangeSummary["Food Production"] += amount;
                weeklyChangeSummary["Food Production"] += amount;
                break;
            case DailyChangeType.SoldierGrowth:
                gameHandler.dailySoldierGrowth += amount;
                dailyChangeSummary["Soldier Growth"] += amount;
                weeklyChangeSummary["Soldier Growth"] += amount;
                break;
            case DailyChangeType.PopulationGrowth:
                gameHandler.dailyPopulationGrowth += amount;
                dailyChangeSummary["Population Growth"] += amount;
                weeklyChangeSummary["Population Growth"] += amount;
                break;
                case DailyChangeType.FoodConsumption:
                gameHandler.dailyFoodConsumption += amount;
                onFoodProductionChange?.Invoke(this, EventArgs.Empty);
                dailyChangeSummary["Food Consumption"] += amount;
                weeklyChangeSummary["Food Consumption"] += amount;
                break;
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
            ResourceType.Population => population,
            ResourceType.Health => health,
            ResourceType.Authority => authority,
            ResourceType.LoyaltyToKing => loyaltyToKing,
            _ => 0
        };
    }
    public int GetResource(DailyChangeType type)
    {
        return type switch
        {
            DailyChangeType.SoldierGrowth => gameHandler.dailySoldierGrowth,
            DailyChangeType.FoodProduction => gameHandler.dailyFoodConsumption,
            DailyChangeType.PopulationGrowth => gameHandler.dailyPopulationGrowth,
            DailyChangeType.FoodConsumption => gameHandler.dailyFoodConsumption,
            DailyChangeType.Taxes => gameHandler.taxes,
            _ => 0
        };
    }

    private void UpdateHealthAuthorityLoyaltyColors()
    {
        int[] resources = { health, authority, loyaltyToKing };
        TextMeshProUGUI[] texts = { healthText, authorityText, loyaltyToKingText };
        for (int i = 0; i < resources.Length; i++)
        {
            if (resources[i] >= 75)
                texts[i].color = Color.green;
            else if (resources[i] >= 50)
                texts[i].color = Color.yellow;
            else if (resources[i] >= 25)
                texts[i].color = new Color(1f, 0.64f, 0f); // Orange color
            else
                texts[i].color = Color.red;
        }
    }

    public void UpdateUI()
    {
        if (goldText != null) goldText.text = gold.ToString();
        if (authorityText != null) authorityText.text = $"{authority}%";
        if (soldiersText != null) soldiersText.text = $"{soldiers}";
        if (populationText != null) populationText.text = $"{population}";
        if (foodText != null) foodText.text = $"{food}";
        if (healthText != null) healthText.text = $"{health}%";
        if (loyaltyToKingText != null) loyaltyToKingText.text = $"{loyaltyToKing}%";

        UpdateHealthAuthorityLoyaltyColors();

        foreach (GetDailyChange changeText in dailyChangeTextList)
        {
            int result = 0;
            switch (changeText.type)
            {
                case DailyChangeType.Taxes:
                    baseTaxesText.text = "Base tax: "+GameManager.Instance.gameHandler.taxes.ToString() + "/Person";
                    changeText.StartUpdateChangeText(CalculateTaxesMinAmount(), CalculateTaxesMaxAmount());
                    break;
                case DailyChangeType.PopulationGrowth:
                    changeText.StartUpdateChangeText(GameManager.Instance.gameHandler.dailyPopulationGrowth);
                    break;
                case DailyChangeType.FoodProduction:
                    result = GameManager.Instance.gameHandler.weeklyFoodProduction * population - GameManager.Instance.gameHandler.dailyFoodConsumption * population*7;
                    changeText.StartUpdateChangeText(result);

                    baseFoodUseText.text = "Food use: " + GameManager.Instance.gameHandler.dailyFoodConsumption.ToString() + "/Person";
                    break;

                case DailyChangeType.SoldierGrowth:
                    changeText.StartUpdateChangeText(GameManager.Instance.gameHandler.dailySoldierGrowth);
                    break;
            }
        }

    }

    public void SetResource(ResourceType type, int value)
    {
        switch (type)
        {
            case ResourceType.Gold: gold = value; break;
            case ResourceType.Authority: authority = value; break;
            case ResourceType.Food: food = value; break;
            case ResourceType.Soldiers: soldiers = value; break;
            case ResourceType.Population: population = value; break;
            case ResourceType.Health: health = value; break;
            case ResourceType.LoyaltyToKing: loyaltyToKing = value; break;
            default: print("resource no match"); break;
        }
        UpdateUI();
    }

    public void DailyAdjustments()
    {
        // Adjust resources based on daily changes       
        ConsumeFood();
        SoldierChange();
        PopulationChange();

        // Update UI after adjustments
    }

    public void WeeklyAdjustments()
    {
        print("Weekly adjustments");
        ProduceFood();
        ConsumeFood();
        SoldierChange();
        PopulationChange();
        AdjustResourcesToThemselves();

        CollectTaxes(); //weekly taxes
    }

    private void CollectTaxes()
    {
        print("Taxes: " + CalculateTaxesAmount() + " collected!");
        ModifyResource(ResourceType.Gold, CalculateTaxesAmount());
        ModifyResource(ResourceType.LoyaltyToKing, Mathf.RoundToInt(-GameManager.Instance.gameHandler.taxes * population * 0.01f));
    }

    private void AdjustResourcesToThemselves()
    {
        int netFoodProduction = gameHandler.weeklyFoodProduction * population - gameHandler.dailyFoodConsumption * population * 7;
        //TODO: Adjust the resources that affect eachother with their affection rate and update the UI
        //Authority affects tax payment; Loyalty affects tax payment; Health affects population growth; Gold affects authority; Food affects health, population growth, and loyalty;
        //population affects food consumption and tax payment; soldiers affect authority;
        ModifyResource(ResourceType.Authority, Mathf.RoundToInt(gold * goldToAuthorityTransfer));
        ModifyResource(ResourceType.Authority, Mathf.RoundToInt(soldiers * soldiersToAuthorityTransfer));
        ModifyResource(ResourceType.Health, Mathf.RoundToInt(netFoodProduction * foodToHealthTransfer));
        ModifyResource(ResourceType.LoyaltyToKing, Mathf.RoundToInt(netFoodProduction * foodToLoyaltyTransfer));
        ModifyResource(DailyChangeType.PopulationGrowth, Mathf.RoundToInt(CalculateHealthToPopulationGrowthTransfer()));
        ModifyResource(DailyChangeType.PopulationGrowth, Mathf.RoundToInt(netFoodProduction * foodGrowthToPopulationGrowthTransfer));
        ModifyResource(DailyChangeType.SoldierGrowth, Mathf.RoundToInt(netFoodProduction * foodGrowthToPopulationGrowthTransfer * 0.5f));
    }

    private void ProduceFood()
    {
        ModifyResource(ResourceType.Food, gameHandler.weeklyFoodProduction * population); // Example: Produce 5 food daily
    }

    public float CalculateHealthToPopulationGrowthTransfer()
    {
        if (health < healthToPopulationGrowthThreshold * 100)
        {
            if (health <= healthToPopulationLossThreshold * 100)
            {
                float mappedValue = Mathf.InverseLerp(0f, healthToPopulationLossThreshold * 100f, health);
                float mappedTransferValue = Mathf.InverseLerp(-100, 0f, mappedValue);
                return mappedTransferValue;
            }
            else
                return 0f; // if the health is between the thresholds
        }
        else
        {
            float mappedValue = Mathf.InverseLerp(healthToPopulationGrowthThreshold * 100, 100f, health);
            float mappedTransferValue = Mathf.InverseLerp(0f, 50f, mappedValue);
            return mappedTransferValue;
        }
    }

    private int CalculateTaxesAmount()
    {
        int taxAmount= 0;
        float chance = (authority / 100f + loyaltyToKing / 100f)/2f;
        if (chance < 0.01f) chance = 0.01f; 
        for (int i = 0; i<population; i++)
        {
            if(UnityEngine.Random.Range(0f,1f) <= chance)
                taxAmount += GameManager.Instance.gameHandler.taxes;
        }

        return taxAmount;
    }

    private int CalculateTaxesMinAmount()
    {
        float chance = authority / 100f * loyaltyToKing / 100f;
        if (chance < 0.01f) chance = 0.01f;

        return Mathf.RoundToInt(GameManager.Instance.gameHandler.taxes * population * chance);
    }
    private int CalculateTaxesMaxAmount()
    {
        return Mathf.RoundToInt(GameManager.Instance.gameHandler.taxes * population);
    }

    private void ConsumeFood()
    {
        if (food - gameHandler.dailyFoodConsumption*population < 0)
        {
            print("Food in the negative!");
            ModifyResource(ResourceType.Food, -GameManager.Instance.gameHandler.dailyFoodConsumption * population);
            ModifyResource(ResourceType.LoyaltyToKing, food - GameManager.Instance.gameHandler.dailyFoodConsumption * population);
            ModifyResource(ResourceType.Health, food - GameManager.Instance.gameHandler.dailyFoodConsumption * population / 2);
        }
        else
            ModifyResource(ResourceType.Food, -GameManager.Instance.gameHandler.dailyFoodConsumption*population); // Example: Consume 2 food daily
    }

    private void SoldierChange()
    {
        ModifyResource(ResourceType.Soldiers, GameManager.Instance.gameHandler.dailySoldierGrowth); // Example: Lose 1 soldier daily
    }

    private void PopulationChange()
    {
        ModifyResource(ResourceType.Population, GameManager.Instance.gameHandler.dailyPopulationGrowth); // Example: Lose 1 peasant daily
    }
}
