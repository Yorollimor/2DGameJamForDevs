using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class DayTransitionAnimation : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nextDayText;
    [SerializeField] TextMeshProUGUI changesTextboxPrefab;
    [SerializeField] TextMeshProUGUI todaysImpactText;
    [SerializeField] GameObject dayImpactContainer;
    [SerializeField] Image bg;   
    [SerializeField] float textSpeed = 0.2f;
    [SerializeField] bool transitionDone = false;
    public bool isDayTransition = false;

    [SerializeField] Vector2 originalPosImpactText = Vector2.zero;
    Sequence transition;
    List<TextMeshProUGUI> changeTexts = new List<TextMeshProUGUI>();

    List<Coroutine> coroutines = new List<Coroutine>();

    private void Start()
    {

    }

    public void UpdateDayText()
    {
        nextDayText.text = "Day: " + (GameManager.Instance.gameHandler.currentDay + 1);
    }

    private void Update()
    {
        if((Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space)) && transitionDone)
        {
            transitionDone = false;
            NextDay();
        }
    }

    private void OnEnable()
    {
        print("Days Transition Enabled");
        bg.color = Color.clear;
        todaysImpactText.color = new Color(1,1,1,0);
        nextDayText.color = new Color(1, 1, 1, 0);
    }

    public void StartTransition()
    {
        UpdateDayText();       
        bg.DOFade(0.95f, 0.3f);        
        transition = DOTween.Sequence();
        transition.SetAutoKill(false);

        transition.Append(todaysImpactText.DOFade(1,0.3f))
            .Join(dayImpactContainer.GetComponent<RectTransform>().DOLocalMoveY(100,0.5f).SetRelative(true).SetEase(Ease.OutBack))
            .AppendInterval(textSpeed)
            .OnComplete(() => { StartCoroutine(WriteChanges()); });

        transition.Play();
    }

    int coroutineCount = 0;

    public IEnumerator WriteChanges()
    {   
        Dictionary<string, float> tempDictionary = GameManager.Instance.resourceManager.GetDailyResourceChanges();

        changeTexts.Clear();
        coroutines.Clear();
        coroutineCount = 0;
        ResourceManager resourceManager = GameManager.Instance.resourceManager;
        GameHandler gameHandler = GameManager.Instance.gameHandler;

        foreach (string key in tempDictionary.Keys)
        {
            if (tempDictionary[key] != 0)
            {
                var temp = Instantiate(changesTextboxPrefab, dayImpactContainer.transform);
                temp.transform.SetAsLastSibling();
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
            }
            yield return new WaitForSeconds(textSpeed);
        }
        if (coroutines.Count == 0) transitionDone = true;
    }


    IEnumerator TypeText(string fullText, TextMeshProUGUI textBox)
    {
        int index = 0;
 
        int messageLength = fullText.Length;
        textBox.text = fullText;
        textBox.maxVisibleCharacters = 0;

        while (textBox.maxVisibleCharacters < messageLength)
        {
            textBox.maxVisibleCharacters++;
            index++;
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
        Sequence exitTransition = DOTween.Sequence().SetAutoKill(false);

        exitTransition.Append(nextDayText.DOFade(1, 0.4f))
            .Join(nextDayText.transform.DOLocalMoveY(100, 0.4f).SetRelative(true).SetEase(Ease.OutBack))
            .AppendInterval(0.3f)
            .OnComplete(() => exitTransition.PlayBackwards());

        transition.PlayBackwards();
        StartCoroutine(StatsFlyOut());
        transition.OnRewind(() => { 
            exitTransition.Play().SetDelay(0.5f);
            transition.Kill();
            } );

        exitTransition.OnRewind(() => { bg.DOFade(0f, 0.3f).SetDelay(0.3f).OnComplete(GameManager.Instance.gameHandler.StartNextDay); exitTransition.Kill(); });
    }

    private IEnumerator StatsFlyOut()
    {
        foreach (TextMeshProUGUI t in changeTexts)
        {
            t.transform.DOLocalMoveY(-150f, 0.3f).SetRelative(true).SetEase(Ease.InBack);
            t.DOFade(0, 0.4f).OnComplete(() => Destroy(t.transform.gameObject));
            yield return new WaitForSeconds(textSpeed);
        }
    }



}
