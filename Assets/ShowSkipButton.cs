using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ShowSkipButton : MonoBehaviour
{

    bool isVisible = false;
    Vector2 originalPos = Vector2.zero;

    private void Start()
    {
        originalPos = transform.localPosition;

    }

    private void OnEnable()
    {
        DialogueLogic.OnDialogueStart += ShowButton;
        DialogueLogic.OnDialogueEnd += HideButton;
        GetComponent<Button>().interactable = false;
    }

    private void OnDisable()
    {
        DialogueLogic.OnDialogueStart -= ShowButton;
        DialogueLogic.OnDialogueEnd -= HideButton;
    }
    private void ShowButton(Character_Data character, bool specialCall)
    {
        if((EventManager.currentEvent.eventType == EventType.Tutorial || EventManager.currentEvent.eventType == EventType.Introduction) && !isVisible)
        {
            GetComponent<Button>().interactable = true;
            isVisible = true;
            Sequence sequ = DOTween.Sequence();
            sequ.Append(transform.DOLocalMoveY(290, 0.5f).SetEase(Ease.OutBack))
                .Join(GetComponent<CanvasGroup>().DOFade(1, 0.5f));

            sequ.Play();
        }
    }

    private void HideButton(Character_Data character, bool specialCall)
    {
        if ((EventManager.currentEvent.eventType == EventType.Tutorial || EventManager.currentEvent.eventType == EventType.Introduction) && isVisible)
        {
            isVisible = false;
            Sequence sequ = DOTween.Sequence();
            sequ.Append(transform.DOLocalMoveY(originalPos.y, 0.5f).SetEase(Ease.InBack))
                .Join(GetComponent<CanvasGroup>().DOFade(0, 0.5f));

            sequ.Play();
        }
    }

    public void Skip()
    {
        DialogueLogic.TriggerEventSkip();
        DialogueLogic.DestroyDialogueOptions();
    }
}
