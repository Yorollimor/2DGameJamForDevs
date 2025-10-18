using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeekTransitionTextGenerator : MonoBehaviour
{
    public GameObject tableContainer; //grid data
    public TextBoxWriter weekTextPrefab;
    public TextMeshProUGUI headerText; //"This weeks results"
    public TextMeshProUGUI nextWeekText; //"Week 2"
    public Sequence animationSequence;
    private Sequence nextWeekAnimation;
    public bool isTextDone = false;
    private bool transitionDone = false;
    private bool fadeOutStarted = false;
    Vector2 originalHeaderPos = Vector2.zero;
    Vector2 originalGridContainerPos = Vector2.zero;
    Vector2 originalNextWeekTextPos;

    public int offsetY = -20;
    public float animationDuration = 0.5f;

    TextBoxWriter[,] grid;

    public void StartWeekTransition()
    {
        if (originalHeaderPos.y == 0) //saving the positions only the first time it's called
        {
            originalHeaderPos = headerText.rectTransform.anchoredPosition;
            originalGridContainerPos = tableContainer.GetComponent<RectTransform>().anchoredPosition;
            originalNextWeekTextPos = nextWeekText.rectTransform.anchoredPosition;
        }
        isTextDone = false;
        transitionDone = false;
        fadeOutStarted = false;

        gameObject.GetComponent<Image>().DOFade(0.95f, animationDuration);

        animationSequence = DOTween.Sequence().SetAutoKill(false)
            .Append(headerText.rectTransform.DOAnchorPosY(originalHeaderPos.y + offsetY, animationDuration).From())
            .Join(headerText.DOFade(1, animationDuration))
            .AppendInterval(animationDuration / 2f)
            .Append(tableContainer.GetComponent<RectTransform>().DOAnchorPosY(originalGridContainerPos.y + offsetY, animationDuration).From())
            .Join(tableContainer.GetComponent<CanvasGroup>().DOFade(1, animationDuration));

        print("Started Week transition");
        nextWeekText.text = "Starting week: " + (GameManager.Instance.gameHandler.currentWeek + 1).ToString();

        StartEntryAnimationSequence();
    }

    private void StartEntryAnimationSequence()
    {
        headerText.color = new Color(1, 1, 1, 0);
        tableContainer.GetComponent<CanvasGroup>().alpha = 0f;
        nextWeekText.color = new Color(1, 1, 1, 0);

        print(originalHeaderPos);

        animationSequence.Play();
        animationSequence.OnComplete(GenerateText);
    }

    private void StartExitAnimationSequence()
    {
        animationSequence.PlayBackwards();
        nextWeekAnimation = DOTween.Sequence();

        nextWeekAnimation.Append(nextWeekText.DOFade(1, animationDuration))
        .SetDelay(animationSequence.Duration()+animationDuration)
        .OnComplete(() => {transitionDone = true ; animationSequence.Kill();});

        nextWeekAnimation.Play();
    }

    private void FadeWeekTextOut()
    {
        transitionDone = false;
        fadeOutStarted = true;

        Debug.LogWarning("FadeWeekTextOut");


        nextWeekAnimation = DOTween.Sequence();

        nextWeekAnimation.Append(nextWeekText.DOFade(0, animationDuration))
        .OnComplete(() => {
            gameObject.GetComponent<Image>().DOFade(0, animationDuration).OnComplete(StartNextDay).SetDelay(animationDuration / 2f);
            nextWeekAnimation.Kill();
        });      

        nextWeekAnimation.Play();
    }


    private void Update()
    {
        if((Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space)) && !fadeOutStarted)
        {
            if (isTextDone && !transitionDone) StartExitAnimationSequence();
            else if (transitionDone) { FadeWeekTextOut(); }
            else if(grid != null && !isTextDone && !transitionDone) //skip text animation
            {  
                bool wasUnfinished = false;
                for(int i = 0; i < grid.GetLength(0); i++)
                {
                    for(int j = 0; j < grid.GetLength(1); j++)
                    {
                        if(grid[i, j].textBox.maxVisibleCharacters < grid[i,j].textBox.text.Length)
                        {
                            print("Text Was Unfinished");
                            grid[i, j].GetComponent<TextBoxWriter>().SkipLine();
                            wasUnfinished = true;
                        }
                    }
                }
                if (!wasUnfinished) StartExitAnimationSequence();

                isTextDone = true;
            }
        }
    }



    private void StartNextDay()
    {
        nextWeekText.color = new Color(1, 1, 1, 0);
        CleanUp();
        GameManager.Instance.gameHandler.StartNextDay();
        gameObject.SetActive(false);
    }

     public void GenerateText()
    {
        if(grid != null)
            CleanUp();
        print("Generating weekly text");
        GameHandler gameHandler = GameManager.Instance.gameHandler;
        ResourceManager resourceManager = GameManager.Instance.resourceManager;

        Dictionary<string, float> dayChange = resourceManager.GetDailyResourceChanges();

        Dictionary<string, float> filteredDictionary = new Dictionary<string, float>();

        foreach (KeyValuePair<string, float> entry in dayChange)
        {
            if (entry.Value != 0)
            {
                filteredDictionary.Add(entry.Key, entry.Value);
            }
        }

        bool highlight = true;

        int totalFields = filteredDictionary.Count + 2; // +2 for extra lines
        grid = new TextBoxWriter[totalFields, 3];

        Vector2 cellSize = tableContainer.GetComponentInParent<GridLayoutGroup>().cellSize;

        // --- Create empty grid cells ---
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                grid[i, j] = Instantiate(weekTextPrefab, tableContainer.transform);
                grid[i, j].textBox.text = "";
                grid[i, j].background.color = new Color(0, 0, 0, 0);
                grid[i, j].background.GetComponent<RectTransform>().sizeDelta = cellSize;
            }
        }

        int rowIndex = 0;

        // --- Fill data ---
        foreach (string key in filteredDictionary.Keys)
        {
            highlight = !highlight;
            HighlightTextbox(rowIndex, highlight);
            int netFoodProduction = gameHandler.weeklyFoodProduction * resourceManager.population
                                  - gameHandler.dailyFoodConsumption * resourceManager.population * 7;

            float totalAmount = 0;

            // --- Column 0: Base change ---
            grid[rowIndex, 0].textBox.text = filteredDictionary[key] > 0 ? $"+ {filteredDictionary[key]} <sprite name={key}>" : filteredDictionary[key].ToString() + $" <sprite name={key}>";
            totalAmount += filteredDictionary[key];
            grid[rowIndex, 0].GetComponent<TextBoxWriter>().WriteText();

            // --- Column 1: Key-specific logic ---
            switch (key)
            {
                case "Authority":
                    grid[rowIndex, 1].textBox.text = $"+{Mathf.RoundToInt(resourceManager.soldiersToAuthorityTransfer * resourceManager.soldiers)} | {resourceManager.soldiersToAuthorityTransfer * 100}% / <sprite name=Soldiers>";
                    grid[rowIndex, 1].GetComponent<TextBoxWriter>().WriteText();
                    totalAmount += Mathf.RoundToInt(resourceManager.soldiersToAuthorityTransfer * resourceManager.soldiers);

                    rowIndex++;

                    HighlightTextbox(rowIndex, highlight);
                    grid[rowIndex, 1].textBox.text = $"+{Mathf.RoundToInt(resourceManager.goldToAuthorityTransfer * resourceManager.gold)} | {resourceManager.goldToAuthorityTransfer * 100}% / <sprite name=Gold>";
                    grid[rowIndex, 1].GetComponent<TextBoxWriter>().WriteText();
                    totalAmount += Mathf.RoundToInt(resourceManager.goldToAuthorityTransfer * resourceManager.gold);


                    break;

                case "Population Growth":
                    if (resourceManager.health < resourceManager.healthToPopulationLossThreshold)
                    {
                        grid[rowIndex, 1].textBox.text = $"-{Mathf.RoundToInt(resourceManager.CalculateHealthToPopulationGrowthTransfer() * resourceManager.health)} | {resourceManager.CalculateHealthToPopulationGrowthTransfer() * 100}% / low <sprite name=Health>";
                    }
                    else if (resourceManager.health > resourceManager.healthToPopulationGrowthThreshold)
                    {
                        grid[rowIndex, 1].textBox.text = $"+{Mathf.RoundToInt(resourceManager.CalculateHealthToPopulationGrowthTransfer() * resourceManager.health)} | {resourceManager.CalculateHealthToPopulationGrowthTransfer() * 100}% / high <sprite name=Health>";
                    }
                    grid[rowIndex, 1].GetComponent<TextBoxWriter>().WriteText();
                    totalAmount += Mathf.RoundToInt(resourceManager.CalculateHealthToPopulationGrowthTransfer() * resourceManager.health);

                    rowIndex++;
                    HighlightTextbox(rowIndex, highlight);
                    if (netFoodProduction < 0)
                    {
                        grid[rowIndex, 1].textBox.text = $"{Mathf.RoundToInt(resourceManager.foodGrowthToPopulationGrowthTransfer * netFoodProduction)} | {resourceManager.foodGrowthToPopulationGrowthTransfer * 100}% / net <sprite name=Food Production>";
                    }
                    else
                    {
                        grid[rowIndex, 1].textBox.text = $"+{Mathf.RoundToInt(resourceManager.foodGrowthToPopulationGrowthTransfer * netFoodProduction)} | {resourceManager.foodGrowthToPopulationGrowthTransfer * 100}% / net <sprite name=Food Production>";
                    }
                    grid[rowIndex, 1].GetComponent<TextBoxWriter>().WriteText();
                    totalAmount += Mathf.RoundToInt(resourceManager.foodGrowthToPopulationGrowthTransfer * netFoodProduction);

                    break;

                case "Soldier Growth":
                    grid[rowIndex, 1].textBox.text = $"+{Mathf.RoundToInt(resourceManager.foodGrowthToPopulationGrowthTransfer * 0.5f * resourceManager.food)} | {resourceManager.foodGrowthToPopulationGrowthTransfer * 0.5f * 100}% / <sprite name=Food>";
                    grid[rowIndex, 1].GetComponent<TextBoxWriter>().WriteText();
                    totalAmount += Mathf.RoundToInt(resourceManager.foodGrowthToPopulationGrowthTransfer * 0.5f * resourceManager.food);
                    break;

                case "Health":
                    if (netFoodProduction < 0)
                    {
                        grid[rowIndex, 1].textBox.text = $"{Mathf.RoundToInt(resourceManager.foodToHealthTransfer * netFoodProduction)} | {resourceManager.foodToHealthTransfer * 100}% / net <sprite name=Food Production>";
                    }
                    else
                    {
                        grid[rowIndex, 1].textBox.text = $"+{Mathf.RoundToInt(resourceManager.foodToHealthTransfer * netFoodProduction)} | {resourceManager.foodToHealthTransfer * 100}% / net <sprite name=Food Production>";
                    }
                    grid[rowIndex, 1].GetComponent<TextBoxWriter>().WriteText();
                    totalAmount += Mathf.RoundToInt(resourceManager.foodToHealthTransfer * netFoodProduction);
                    break;

                case "Loyalty to King":
                    grid[rowIndex, 1].textBox.text = $"+{Mathf.RoundToInt(resourceManager.foodToLoyaltyTransfer * resourceManager.food)} | {resourceManager.foodToLoyaltyTransfer * 100}% / <sprite name=Food>";
                    grid[rowIndex, 1].GetComponent<TextBoxWriter>().WriteText();
                    totalAmount += Mathf.RoundToInt(resourceManager.foodToLoyaltyTransfer * resourceManager.food);
                    break;
            }

            // --- Column 2: Total ---
            grid[rowIndex, 2].textBox.text = $"Total: " + (totalAmount > 0 ? $"<color=green>+{Mathf.RoundToInt(totalAmount)}</color>" : "<color=red>" + Mathf.RoundToInt(totalAmount).ToString()) + "</color>";
            grid[rowIndex, 2].textBox.text +=$" <sprite name={key}>";
            grid[rowIndex, 2].GetComponent<TextBoxWriter>().WriteText();

            rowIndex++;
        }
    }

    public void CleanUp()
    {
        for(int i = grid.GetLength(0)-1; i >=0; i--)
        {
            for(int j = grid.GetLength(1)-1; j >= 0; j--)
            {
                if (grid[i,j] != null)
                    Destroy(grid[i, j].gameObject);
            }
        }
    }

    private void HighlightTextbox(int rowIndex, bool highlight)
    {
        Color highlightColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        if(highlight)
        {
            grid[rowIndex, 0].background.color = highlightColor;
            grid[rowIndex, 1].background.color = highlightColor;
            grid[rowIndex, 2].background.color = highlightColor;
        }
        else
        {
            grid[rowIndex, 0].background.color = new Color (0,0,0,0);
            grid[rowIndex, 1].background.color = new Color(0, 0, 0, 0);
            grid[rowIndex, 2].background.color = new Color(0, 0, 0, 0);
        }

    }

}
