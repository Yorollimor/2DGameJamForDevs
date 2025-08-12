using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    public int gold = 50;
    public int food = 50;
    public int soldiers = 20;
    public int peasants = 50;

    public TextMeshProUGUI goldText;
    public TextMeshProUGUI foodText;
    public TextMeshProUGUI soldiersText;
    public TextMeshProUGUI peasantsText;

    private void Start()
    {
        UpdateUI();
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
            _ => 0
        };
    }

    public void UpdateUI()
    {
        if (goldText != null) goldText.text = $"Gold: {gold}";
        if (foodText != null) foodText.text = $"Food: {food}";
        if (soldiersText != null) soldiersText.text = $"Soldiers: {soldiers}";
        if (peasantsText != null) peasantsText.text = $"Peasants: {peasants}";
    }
}
