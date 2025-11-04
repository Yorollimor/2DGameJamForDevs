using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public delegate void GameEvent(object sender, EventArgs e = null);


public class GameHandler : MonoBehaviour
{
    [Header("Game Status")]
    public int currentDay = 1; // Current day in the game
    public int currentWeek = 1; // Current week in the game
    public int maxDays = 30; // Maximum number of days in the game
    public int eventsCounter = 0; // Counter for the number of events that have occurred
    [Header("Daily Changes")]
    public int taxes = 0; // Amount of taxes collected each day
    public int dailyFoodConsumption = 0; // Amount of food consumed each day
    public int weeklyFoodProduction = 0; // Amount of food produced each day
    public int dailySoldierGrowth = 0; // Number of soldiers lost each day
    public int dailyPopulationGrowth = 0; // Number of populatino lost each day
    [Header("")]
    public bool advisingKing = true; // Flag to indicate if the advisor is currently advising the king


    [Header("Game Settings")]
    [Header("Initial Resources")]
    [SerializeField] int initialGold = 100; // Initial amount of gold
    [SerializeField] int initialFood = 100; // Initial amount of food
    [SerializeField] int initialSoldiers = 50; // Initial number of soldiers
    [SerializeField] int initialPeasants = 100; // Initial number of populatino
    [SerializeField] int inititalHealth = 10; // Initial amount of health
    [SerializeField] int initialAuthority = 0; // Initial number of sick people
    [SerializeField] int initialLoyaltyToKing = 50; // Initial acceptance of the king by the populatino
    
    [Header("Initial Daily Changes")]
    [SerializeField] int initialWeeklyTaxesPerPerson = 5; // Initial amount of taxes collected each day
    [SerializeField] int initialFoodConsumptionPerPerson = 1; // Initial amount of food consumed each day
    [SerializeField] int initialSoldierGrowth = 0; // Initial number of soldiers lost each day
    [SerializeField] int initialPopulationGrowth = 0 ; // Initial number of populatino lost each day
    [SerializeField] int initalWeeklyFoodProductionPerPerson = 3; // Initial number of events per turn

    [Header("Event Settings")]
    [SerializeField] int eventsBeforeSwitch = 3; // Number of events before switching from advising king to populatino
    public GameObject resourcePanelKing; // Panel for king's resources
    public GameObject resourcePanelPeasants; // Panel for populatino' resources
    public TextMeshProUGUI dayText; // Text to display the current day
    public int eventsAmountPerTurn = 3; // Number of events to process each turn
    public Canvas gameCanvas;
    public DayTransitionAnimation _dayTransitionAnimation;
    public WeekTransitionTextGenerator _weekTransitionAnimation;
    public GameObject[] backgroundArray;
    public UnityEngine.UI.Image backgroundImage;
    public List<Variable> collectedVariables;


    [Header("Characters")]
    public Character_Data king;
    public List<Character_Data> originalCharacters; // List of originalCharacters in the game
    public GameObject speakerPosition;
    private List<SpeakerType> introducedCharacters = new List<SpeakerType>(); // List of originalCharacters that have been introduced

    [Header("")]

    private GameManager gameManager;
    private EventManager eventManager;
    private ResourceManager resourceManager;


   // public GameEvent onDayChange;
   // public GameEvent onWeekChange;

    private void Start()
    {
        dayText.text = $"Day: {currentDay}"; // Update the day text display

        gameManager = GameManager.Instance;

        if (gameManager.Equals(null))
        {
            Debug.LogError("GameManager instance not found. Ensure it is initialized before EventManager.");
            return;
        }
        eventManager = gameManager.eventManager;
        resourceManager = gameManager.resourceManager;

        StartCoroutine(SetTransitionsInactiveAtEndOfFrame());

    }

    private IEnumerator SetTransitionsInactiveAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        _weekTransitionAnimation.gameObject.SetActive(false);
        _dayTransitionAnimation.gameObject.SetActive(false);
    }

    private void Update()
    {
        //return;// Disable for testing purposes
        if (Input.GetKeyDown(KeyCode.W))
        {
            StartWeekTransition();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            StartDayTransition();
        }
        if(Input.GetKeyDown(KeyCode.S))
        {
            PlaceNewCharacters();
        }

    }

    //GameObject tempHoverPopUp; 
    //public void ShowHoverInformation(GameObject parent)
    //{
    //    GameObject popUp = parent.GetComponent<ResourceHoverInformation>().infoPopUp;
    //    tempHoverPopUp = Instantiate(popUp, parent.GetComponent<ResourceHoverInformation>().popUpPosition.transform.position, Quaternion.identity);
    //    tempHoverPopUp.transform.SetParent(parent.transform);
    //    tempHoverPopUp.transform.localScale = Vector3.one;

    //    //TextMeshProUGUI dailyChangeInfoText = popUp.dailyChangeInfo.GetComponent<TextMeshProUGUI>();

    //    Vector3 pos = tempHoverPopUp.transform.localPosition;
    //    RectTransform rectTransformPopUp = popUp.GetComponent<RectTransform>();
    //    float posX = 0f;
    //    switch(rectTransformPopUp.anchorMin.x)
    //    {
    //        case 0f: // Left
    //            posX = pos.x;
    //            break;
    //        case 0.5f: // Center
    //            posX = 0f;
    //            break;
    //        case 1f: // Right
    //            posX = pos.x - parent.GetComponent<RectTransform>().sizeDelta.x / 2;
    //            break;
    //    }

    //    tempHoverPopUp.transform.localPosition = new Vector3(posX, pos.y- rectTransformPopUp.sizeDelta.y, 0); // Set the position of the popup
    //}

    //public void DeleteHoverInformationPopup()
    //{
    //    Destroy(tempHoverPopUp);
    //}

    public void StartNextDayIfAllCharactersAreGone()
    {
        //print("EventCounter increased");
        eventsCounter++;
        //if (instantiatedCharacters.Count > 0)
        //{   

        //    foreach (Character_Data c in instantiatedCharacters)
        //    {
        //        print("Remaining character: " + c.characterInfo.name_);
        //        if (c.characterInfo.characterType == SpeakerType.King) eventsCounter = eventsCounter > 1 ? eventsCounter -1 : eventsCounter; // if the king is still present, decrease the event counter by 1 to avoid switching after his event

        //    }
        //}
        if (instantiatedCharacters.Count == 0)
        {          
            if ((currentDay) % 7 == 0)
            {
                Debug.Log("Week Ends");
                StartWeekTransition();
            }
            else
            {         
                Debug.Log("Next Day Starts");
                StartDayTransition(); // Start the next day if the event counter reaches the threshold
            }         
        }
        //else if(instantiatedCharacters.Count == 0)
        //{
        //    Debug.Log("No more characters available, starting next day");

        //    if ((currentDay) % 7 == 0)
        //    {
        //        Debug.Log("Week Ends");
        //        StartWeekTransition();
        //    }
        //    else
        //    {
        //        Debug.Log("Next Day Starts");
        //        StartDayTransition(); // Start the next day if the event counter reaches the threshold
        //    }
        //}
    }
    public void StartNextDay()
    {
        GameManager.Instance.CheckGameOver();
        resourceManager.ResetDailyTracking();
        currentDay++; // Increment the day counter
        if (currentDay >= maxDays)
        {
            GameManager.Instance.WinGame();
            return;
        }

        if (currentDay % 7 == 1)
        {
            currentWeek++; // Increment the week counter
        }
        dayText.text = $"Day: {currentDay}"; // Update the day text display
        //onDayChange?.Invoke(this, EventArgs.Empty); // Notify subscribers about the day change
        eventsCounter = 0; // Reset counter after switching
        PlaceCharacters(); // Place originalCharacters for the new day
    }

    public void StartDayTransition()
    {
        TriggerExitAnimationForAllCharacters();

        GameManager.Instance.CheckGameOver();
        FMODUnity.RuntimeManager.PlayOneShot("event:/day transition");
        resourceManager.DailyAdjustments();

        _dayTransitionAnimation.gameObject.SetActive(true);
        _dayTransitionAnimation.StartTransition();
        SwitchAdvising();
        ChangeBackgroundToEventType();
    }


    private void StartWeekTransition()
    {
        TriggerExitAnimationForAllCharacters();

        GameManager.Instance.CheckGameOver();
        print("Weekly update");
        // onWeekChange?.Invoke(this, EventArgs.Empty); // Notify subscribers about the week end

        FMODUnity.RuntimeManager.PlayOneShot("event:/day transition");
        resourceManager.WeeklyAdjustments(); // Apply weekly adjustments to resources


        _weekTransitionAnimation.gameObject.SetActive(true);
        _weekTransitionAnimation.StartWeekTransition();
        //_weekTransitionAnimation.StartWeekTransition();
        SwitchAdvising();
        ChangeBackgroundToEventType();
    }

    private void TriggerExitAnimationForAllCharacters()
    {
        foreach (Character_Data c in instantiatedCharacters)
        {
            c.TriggerExitAnimation(EventManager.speakingCharacter, true);
            c.DisableCharacter();
        }
    }
    public void StartFirstDay()
    {
        ChangeBackgroundToEventType();

        PlaceKing();
    }

    public void PlaceNewCharacters()
    {
        print(gameCanvas.GetComponent<RectTransform>().sizeDelta.x);
        PlaceCharacters(); // Place originalCharacters for the new day
    }
    private void PlaceKing()
    {
        Character_Data temp = Instantiate<Character_Data>(king, new Vector3(gameCanvas.GetComponent<RectTransform>().sizeDelta.x+king.characterInfo.portrait.bounds.size.x, 0, 0), Quaternion.identity);
        temp.transform.SetParent(gameCanvas.transform, false); // Set the parent of the instantiated characterInfo to the game canvas
        instantiatedCharacters.Add(temp); // Add the instantiated characterInfo to the list
    }

    private void ChangeBackgroundToEventType()
    {
        switch(advisingKing) 
        {
            case true:
               backgroundArray[0].gameObject.SetActive(true);
                backgroundArray[1].gameObject.SetActive(false);
                break;
            case false:
                backgroundArray[0].gameObject.SetActive(false);
                backgroundArray[1].gameObject.SetActive(true);
                break;

        }
    }



    public void SwitchOnDialoguePanel()
    {
        GameManager.Instance.dialogueManager.dialoguePanel.SetActive(true); // Show the dialogue panel
    }

    public void SwitchOffDialoguePanel()
    {
         GameManager.Instance.dialogueManager.dialoguePanel.SetActive(false);
    }

    [SerializeField] List<Character_Data> DayAvailableCharacters = new List<Character_Data>(); //all potentially available originalCharacters this turn
    [SerializeField] List<Character_Data> instantiatedCharacters = new List<Character_Data>(); //the randomly chosen and not-yet-used originalCharacters this turn
    public List<Character_Data> GetActiveCharacters()
    {
        return instantiatedCharacters; // Return the list of originalCharacters available for the day
    }

    private void PlaceCharacters()
    {
        List<Character_Data> removeCharacters = instantiatedCharacters;
        for (int i = removeCharacters.Count - 1; i >= 0; i--)
        { 
            instantiatedCharacters[i].DeleteCharacter(); // Delete all originalCharacters that were previously instantiated
        }

        DayAvailableCharacters.Clear();
        instantiatedCharacters.Clear();

        Debug.Log("Placing characters now");

        List<EventData> eventsToRemove = new List<EventData>();
        switch (advisingKing)
        {
            case true:
                foreach(Character_Data character in originalCharacters)
                {
                    if(character.characterInfo.characterType == SpeakerType.King)
                    {
                        continue;
                    }
                    eventsToRemove.Clear();
                    if (character.hasKingEvents)
                    {
                        foreach (EventData e in character.associatedEvents)
                        { 
                            if(GameManager.Instance.eventManager.GetUsedEVents().Contains(e))
                            {
                                eventsToRemove.Add(e); // Remove used events from the characterInfo's associated events
                            }
                        }                       
                        if (character.associatedEvents.Count == eventsToRemove.Count)
                            continue;

                        DayAvailableCharacters.Add(character); // Add originalCharacters with king events to the list
                    }

                }
                break;
            case false:
               foreach(Character_Data character in originalCharacters)
                {
                    eventsToRemove.Clear();
                    if (character.hasPeasentEvents)
                    {
                        foreach (EventData e in character.associatedEvents)
                        {
                            if (GameManager.Instance.eventManager.GetUsedEVents().Contains(e))
                            {
                                eventsToRemove.Add(e); // Remove used events from the characterInfo's associated events
                            }
                        }

                        if (character.associatedEvents.Count == eventsToRemove.Count)
                            continue;

                        DayAvailableCharacters.Add(character); // Add originalCharacters with king events to the list
                    }                    
                }
                break;
        }

        if(currentDay != 1)
        {
            EventData gameOverEvent = null;
            foreach (Character_Data character in DayAvailableCharacters)
            {
                gameOverEvent = eventManager.CheckForGameOverEvent(eventManager.CheckForAvailableEvents(advisingKing ? EventType.KingEvent : EventType.PeasantEvent, character.associatedEvents));
            }
            if (gameOverEvent != null)
            {
                Character_Data temp = Instantiate(gameOverEvent.GetComponent<Character_Data>(), new Vector3(gameCanvas.GetComponent<RectTransform>().sizeDelta.x + gameOverEvent.GetComponent<Character_Data>().characterInfo.portrait.bounds.size.x, 0, 0), Quaternion.identity);
                temp.transform.SetParent(gameCanvas.transform, false); // Set the parent of the instantiated characterInfo to the game canvas
                instantiatedCharacters.Add(temp); // Add the instantiated characterInfo to the list
                return;
            }
        }


        // Ensure we only have the specified number of events for the day
        if (DayAvailableCharacters.Count > eventsAmountPerTurn)
        {
            int difference = DayAvailableCharacters.Count - eventsAmountPerTurn;
            for(int i = 0; i < difference; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, DayAvailableCharacters.Count);
                DayAvailableCharacters.RemoveAt(randomIndex); // Remove random originalCharacters to match the events amount per turn
            }
        }
        foreach (Character_Data character in DayAvailableCharacters)
        {
            Character_Data temp = Instantiate<Character_Data>(character,new Vector3(gameCanvas.GetComponent<RectTransform>().sizeDelta.x + character.characterInfo.portrait.bounds.size.x, 0,0),Quaternion.identity);
            temp.transform.SetParent(gameCanvas.transform, false); // Set the parent of the instantiated characterInfo to the game canvas
            instantiatedCharacters.Add(temp); // Add the instantiated characterInfo to the list
        }
        if(currentDay == 7)
        {
            PlaceKing();
        }
    }

    public void AddToIntroduced(Character_Data character)
    {
        introducedCharacters.Add(character.characterInfo.characterType); // Add the characterInfo to the list of introduced originalCharacters 
    }
    public List<SpeakerType> GetIntroducedCharacters()
    {
        return introducedCharacters; // Return the list of introduced originalCharacters
    }

    /// <summary>
    /// Removes the specified characterInfo from the day's available characterInfo list or the instantiated characterInfo list.
    /// </summary>
    /// <remarks>If the characterInfo is found in the day's available characterInfo list, it is removed from that
    /// list.  If the characterInfo is not in the day's available characterInfo list but is in the instantiated characterInfo list, 
    /// it is removed from the instantiated list, and its associated resources are cleaned up.  If the characterInfo is not
    /// found in either list, a warning is logged.</remarks>
    /// <param name="character">The characterInfo to be removed. Must not be <see langword="null"/>.</param>
    public void RemoveCharacterFromDayList(Character_Data character)
    {
        if (DayAvailableCharacters.Contains(character))
        {
            DayAvailableCharacters.Remove(character); // Remove the characterInfo from the day's available originalCharacters
        }
        else if (instantiatedCharacters.Contains(character))
        {
            instantiatedCharacters.Remove(character); // Remove the characterInfo from the day's available originalCharacters
            character.GetComponent<MoveToCharacterPosition>().DestroyThis();
        }
        else
        {
            Debug.LogWarning("Character not found in today's list: " + character.characterInfo.name);
        }
    }

    private void SwitchAdvising()
    {
        advisingKing = !advisingKing;
        if(advisingKing)
        {
            gameManager.currentEventType = EventType.KingEvent; // Switch to King events
        }
        else
        {
            gameManager.currentEventType = EventType.PeasantEvent; // Switch to Peasant events
        }

        print("Switching advising to: " + (advisingKing ? "King" : "Peasants"));
    }

    public void ApplyStartSettings()
    {
        currentDay = 1;
        taxes = initialWeeklyTaxesPerPerson;
        dailyFoodConsumption = initialFoodConsumptionPerPerson;
        dailySoldierGrowth = initialSoldierGrowth;
        dailyPopulationGrowth = initialPopulationGrowth;
        weeklyFoodProduction = initalWeeklyFoodProductionPerPerson;

        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            int initialAmount = 0;
            switch(type)
            {
                case ResourceType.Gold:
                    initialAmount = initialGold;
                    break;
                case ResourceType.Food:
                    initialAmount = initialFood;
                    break;
                case ResourceType.Soldiers:
                    initialAmount = initialSoldiers;
                    break;
                case ResourceType.Population:
                    initialAmount = initialPeasants;
                    break;
                case ResourceType.Health:
                    initialAmount = inititalHealth;
                    break;
                case ResourceType.Authority:
                    initialAmount = initialAuthority;
                    break;
                case ResourceType.LoyaltyToKing:
                    initialAmount = initialLoyaltyToKing;
                    break;
            }

            GameManager.Instance.resourceManager.SetResource(type, initialAmount);
        }

    }

}
