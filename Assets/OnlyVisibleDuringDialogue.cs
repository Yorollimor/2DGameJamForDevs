using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class OnlyVisibleDuringDialogue : MonoBehaviour
{

    CanvasGroup group;

    private void OnEnable()
    {
        DialogueLogic.OnDialogueStart += Show;
        DialogueLogic.OnDialogueEnd += Hide;
    }

    private void OnDisable()
    {
        DialogueLogic.OnDialogueStart -= Show;
        DialogueLogic.OnDialogueEnd -= Hide;
    }

    private void Start()
    {
       group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
    }

    private void Show(Character_Data character, bool isSpecialCall)
    {
        group.DOFade(1, 0.3f);
    }

    private void Hide(Character_Data character, bool isSpecialCall)
    {
        group.DOFade(0, 0.3f);
    }
}
