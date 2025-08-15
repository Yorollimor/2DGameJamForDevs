using System;
using TMPro;
using UnityEngine;

public delegate void GameEvent(object sender, EventArgs e = null);


public class GameHandler : MonoBehaviour
{
    [Header("Game Status")]
    public int currentDay = 1; // Current day in the game
    public int maxDays = 30; // Maximum number of days in the game
    public int eventsCounter = 0; // Counter for the number of events that have occurred
    [Header("Daily Changes")]
    public int taxes = 0; // Amount of taxes collected each day
    public int dailyFoodConsumption = 0; // Amount of food consumed each day
    public int dailySoldierLosses = 0; // Number of soldiers lost each day
    public int dailyPeasantLosses = 0; // Number of peasants lost each day
    [Header("")]
    public bool advisingKing = true; // Flag to indicate if the advisor is currently advising the king


    [Header("Game Settings")]
    [Header("Initial Resources")]
    [SerializeField] int initialGold = 100; // Initial amount of gold
    [SerializeField] int initialFood = 100; // Initial amount of food
    [SerializeField] int initialSoldiers = 50; // Initial number of soldiers
    [SerializeField] int initialPeasants = 100; // Initial number of peasants
    [SerializeField] int initialMedicine = 10; // Initial amount of medicine
    [SerializeField] int initialSickPeople = 0; // Initial number of sick people
    [SerializeField] int initialAcceptanceOfKing = 50; // Initial acceptance of the king by the peasants
    
    [Header("Initial Daily Changes")]
    [SerializeField] int initialTaxes = 5; // Initial amount of taxes collected each day
    [SerializeField] int initialFoodConsumption = -2; // Initial amount of food consumed each day
    [SerializeField] int initialSoldierLosses = -1; // Initial number of soldiers lost each day
    [SerializeField] int initialPeasantLosses = 1; // Initial number of peasants lost each day
    
    [Header("Event Settings")]
    [SerializeField] int eventsBeforeSwitch = 3; // Number of events before switching from advising king to peasants
    public GameObject resourcePanelKing; // Panel for king's resources
    public GameObject resourcePanelPeasants; // Panel for peasants' resources
    public TextMeshProUGUI dayText; // Text to display the current day

    private GameManager gameManager;
    private EventManager eventManager;
    private ResourceManager resourceManager;


    public GameEvent onDayChange;

    public void IncreaseEventCounter()
    {
        eventsCounter++;
        if (eventsCounter >= eventsBeforeSwitch)
        {
            SwitchAdvising();
            currentDay++; // Increment the day counter
            dayText.text = $"Day: {currentDay}"; // Update the day text display
            onDayChange?.Invoke(this, EventArgs.Empty); // Notify subscribers about the day change
            eventsCounter = 0; // Reset counter after switching
        }
    }

    private void Start()
    {
        dayText.text = $"Day: {currentDay}"; // Update the day text display

        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("GameManager instance not found. Ensure it is initialized before EventManager.");
            return;
        }
        eventManager = gameManager.eventManager;
        resourceManager = gameManager.resourceManager;
    }

    private void SwitchAdvising()
    {
        advisingKing = !advisingKing;
        gameManager.isKingTurn = advisingKing; // Update GameManager's turn state
        print("Switching advising to: " + (advisingKing ? "King" : "Peasants"));
        if (advisingKing)
        {
            resourcePanelKing.SetActive(true);
            resourcePanelPeasants.SetActive(false);
        }
        else
        {
            resourcePanelKing.SetActive(false);
            resourcePanelPeasants.SetActive(true);
        }
    }

    public void ApplyStartSettings()
    {
        currentDay = 1;
        taxes = initialTaxes;
        dailyFoodConsumption = initialFoodConsumption;
        dailySoldierLosses = initialSoldierLosses;
        dailyPeasantLosses = initialPeasantLosses;

        foreach(ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            int initialAmount = 0;
            switch(type)
            {
                case ResourceType.Gold:
                    print("Setting initial gold: " + initialGold);
                    initialAmount = initialGold;
                    break;
                case ResourceType.Food:
                    initialAmount = initialFood;
                    break;
                case ResourceType.Soldiers:
                    initialAmount = initialSoldiers;
                    break;
                case ResourceType.Peasants:
                    initialAmount = initialPeasants;
                    break;
                case ResourceType.Medicine:
                    initialAmount = initialMedicine;
                    break;
                case ResourceType.SickPeople:
                    initialAmount = initialSickPeople;
                    break;
                case ResourceType.AcceptanceOfKing:
                    initialAmount = initialAcceptanceOfKing;
                    break;
            }

            if(resourceManager == null)
            {
                Debug.LogError("ResourceManager instance not found. Ensure it is initialized before setting resources.");
                return;
            }
            resourceManager.SetResource(type, initialAmount);
        }

    }

}
