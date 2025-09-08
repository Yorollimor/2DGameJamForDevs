using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
public class DayTransitionAnimation : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] TextMeshProUGUI changes;
    [SerializeField] float textSpeed = 0.2f;
    [SerializeField] bool transitionDone = false;
    public bool isDayTransition = false;
    
    Animator animator;
    List<TextMeshProUGUI> changeTexts = new List<TextMeshProUGUI>();

    List<Coroutine> coroutines = new List<Coroutine>();

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void UpdateDayText()
    {
        text.text = "Day: " + (GameManager.Instance.gameHandler.currentDay + 1);
        if(!isDayTransition)
        {
            text.text = "Week: " + (GameManager.Instance.gameHandler.currentWeek + 1);
        }
    }

    private void Update()
    {
        if(Input.GetMouseButton(0) && transitionDone)
        {
            transitionDone = false;
            NextDay();
        }
    }


    int coroutineCount = 0;
    /// <summary>
    /// Is being called throuh the animation event in the animator - clears previous texts and couritine list, then writes the changes for the day or week.
    /// </summary>
    public void WriteChanges()
    {   
        Dictionary<string, float> tempDictionary = GameManager.Instance.resourceManager.GetDailyResourceChanges();
        if (!isDayTransition)
            tempDictionary = GameManager.Instance.resourceManager.GetWeeklyResourceChanges();

        changeTexts.Clear();
        coroutines.Clear();
        coroutineCount = 0;
        ResourceManager resourceManager = GameManager.Instance.resourceManager;
        GameHandler gameHandler = GameManager.Instance.gameHandler;

        foreach (string key in tempDictionary.Keys)
        {
            if (tempDictionary[key] != 0)
            {
                var temp = Instantiate(changes, transform);
                temp.fontSize = 50;
                changeTexts.Add(temp);

                if (tempDictionary[key] > 0)
                {
                    coroutines.Add(StartCoroutine(TypeText($"{key}: <color=green>+{tempDictionary[key]}</color>", temp)));
                }
                else
                {
                    coroutines.Add(StartCoroutine(TypeText($"{key}: <color=red>{tempDictionary[key]}</color>", temp)));
                }

                if (!isDayTransition)
                {
                    var sb_result = new System.Text.StringBuilder();

                    float foodNetProduction = gameHandler.weeklyFoodProduction * resourceManager.population - gameHandler.dailyFoodConsumption * resourceManager.population * 7;
                    switch (key)
                    {
                        case "Population Growth":

                            if (resourceManager.health < resourceManager.healthToPopulationLossThreshold)
                            {
                                print("Population Growth");
                                sb_result.Append($"({Mathf.RoundToInt(resourceManager.CalculateHealthToPopulationGrowthTransfer() * resourceManager.health)} | {resourceManager.CalculateHealthToPopulationGrowthTransfer()}% of current low health.");
                            }

                            else if(resourceManager.health >= resourceManager.healthToPopulationGrowthThreshold)
                            {
                                sb_result.Append($"(+{Mathf.RoundToInt(resourceManager.CalculateHealthToPopulationGrowthTransfer() * resourceManager.health)} | {resourceManager.CalculateHealthToPopulationGrowthTransfer()}% of current high health.");
                            }
                            
                            if (foodNetProduction < 0)
                            {
                                sb_result.AppendLine();
                                sb_result.Append($"{Mathf.RoundToInt(resourceManager.foodGrowthToPopulationGrowthTransfer * foodNetProduction)} | {resourceManager.foodGrowthToPopulationGrowthTransfer * 0.5f * 100}% of current net weekly food production.");
                            }
                            else
                            {
                                sb_result.AppendLine();
                                sb_result.Append($"+{Mathf.RoundToInt(resourceManager.foodGrowthToPopulationGrowthTransfer * foodNetProduction)} | {resourceManager.foodGrowthToPopulationGrowthTransfer * 0.5f * 100}% of current net weekly food production.");
                            }
                            break;

                        case "Soldier Growth":
                            if(foodNetProduction < 0)
                            {
                                sb_result.Append($"({Mathf.RoundToInt(resourceManager.foodGrowthToPopulationGrowthTransfer * 0.5f * foodNetProduction)} | {resourceManager.foodGrowthToPopulationGrowthTransfer * 0.5f * 100}% of current net weekly food production.");
                            }
                              
                            else
                            {
                                sb_result.Append($"(+{Mathf.RoundToInt(resourceManager.foodGrowthToPopulationGrowthTransfer * 0.5f * foodNetProduction)} | {resourceManager.foodGrowthToPopulationGrowthTransfer * 0.5f * 100}% of current net weekly food production.");
                            }
                            break;

                        case "Health":
                            if (gameHandler.weeklyFoodProduction * resourceManager.population - gameHandler.dailyFoodConsumption * resourceManager.population * 7 < 0)
                            {
                                sb_result.Append($"({Mathf.RoundToInt(resourceManager.foodToHealthTransfer * resourceManager.food)} | {resourceManager.foodToHealthTransfer * 100}% of current food in storage.");
                            }

                            else
                            {
                                sb_result.Append($"(+{Mathf.RoundToInt(resourceManager.foodToHealthTransfer * resourceManager.food)} | {resourceManager.foodToHealthTransfer * 100}% of current food in storage.");
                            }
                            break;

                        case "Authority":
                            sb_result.Append($"(+{Mathf.RoundToInt(resourceManager.soldiersToAuthorityTransfer * resourceManager.soldiers)} | {resourceManager.soldiersToAuthorityTransfer * 100}% of current soldier count.");

                            sb_result.AppendLine();
                            sb_result.Append($"+{Mathf.RoundToInt(resourceManager.goldToAuthorityTransfer * resourceManager.gold)} | {resourceManager.goldToAuthorityTransfer * 100}% of current gold amount.");
                            break;

                        case "Loyalty To King":
                            sb_result.Append($"(+{Mathf.RoundToInt(resourceManager.foodToLoyaltyTransfer * resourceManager.food)} | {resourceManager.foodToLoyaltyTransfer * 100}% of current food in storage.");
                            break;
                    }
                    string finalText = sb_result.ToString();         
                    if(finalText != "")
                    {
                        var weeklyText = Instantiate(changes, transform);
                        weeklyText.text = "";
                        weeklyText.fontSize = 35;
                        changeTexts.Add(weeklyText);
                        coroutines.Add(StartCoroutine(TypeText($"{finalText} Weekly adjustments)", weeklyText)));
                    }
                  
                }              
            }
        }
        if (coroutines.Count == 0) transitionDone = true;
    }


    IEnumerator TypeText(string text, TextMeshProUGUI textHolder)
    {

        textHolder.fontStyle = FontStyles.Normal;
        textHolder.color = Color.white;

        bool enteredTag = false;
        bool exitedTag = false;
        string buffer = "";
        int index = 0;
        foreach (char c in text.ToCharArray())
        {
            index++;
            if (c == '\n') // Handle new line
            {
                buffer += c;
                textHolder.text = buffer; // Update text immediately for new lines
                yield return new WaitForSeconds(textSpeed);
                continue;
            }
            else if (c == '<' || enteredTag && !exitedTag) // Handle color tags
            {
                enteredTag = true;
                exitedTag = false;
                buffer += c;
                if (c == '>') // If we reached the end of a tag
                {
                    exitedTag = true;
                    enteredTag = false;
                    if (index == text.Length)
                        textHolder.text = buffer;
                }

                continue;

            }
            buffer += c;

            textHolder.text = buffer;
            yield return new WaitForSeconds(textSpeed);
        }

        coroutineCount++;
        if (coroutines.Count == coroutineCount)
        {
            yield return new WaitForSeconds(0.1f);
            transitionDone = true;
        }

    }

    public void NextDay()
    {
        foreach(TextMeshProUGUI t in  changeTexts) 
            {
                Destroy(t.transform.gameObject);
            }

        animator.SetTrigger("Hide");
        GameManager.Instance.gameHandler.StartNextDay();
    }



}
