using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class HandMovement : MonoBehaviour
{
    Vector2 originalPos = Vector2.zero;

    [Tooltip("Percentage of total screen width")]
    public float moveXRange = 33;
    Vector2 mousePos;
    Image sprite;
    bool isLeftHand = false;
    bool isClicking = false;

    private void Start()
    {
        originalPos = transform.position;

        sprite = GetComponent<Image>();
        if(transform.position.x <= Screen.width/2f) isLeftHand = true;
    }

    private void OnEnable()
    {
        DialogueLogic.OnChoiceChosen += ClickAnimation;
    }

    private void OnDisable()
    {
        DialogueLogic.OnChoiceChosen -= ClickAnimation;
    }
    // Update is called once per frame

    Vector2 priorMousePos = Vector2.zero;
    void Update()
    {
        mousePos = Input.mousePosition;
        float scaledMoveRangeX = Screen.width * (moveXRange / 100f);
        if (mousePos.x > originalPos.x)
        {
            if (isLeftHand && mousePos.x > originalPos.x + scaledMoveRangeX)
                mousePos.x = originalPos.x + scaledMoveRangeX;
            else if (!isLeftHand) mousePos.x = originalPos.x;
        }
        if (mousePos.x < originalPos.x)
        {
            if (!isLeftHand && mousePos.x < originalPos.x - scaledMoveRangeX)
                mousePos.x = originalPos.x - scaledMoveRangeX;
            else if(isLeftHand) mousePos.x = originalPos.x;
        }

        if (mousePos != priorMousePos && !isClicking)
            transform.DOMoveX(mousePos.x, 3f).SetEase(Ease.OutCubic);
        priorMousePos = mousePos;
    }

    private void ClickAnimation(GameObject panel)
    {
        bool mouseInLeftHalf =Input.mousePosition.x <= Screen.width / 2f;

        if(isLeftHand && mouseInLeftHalf || !isLeftHand && !mouseInLeftHalf)
        {
            isClicking = true;
            Vector2 target = new Vector2(Input.mousePosition.x, transform.position.y + sprite.GetComponent<RectTransform>().sizeDelta.y / Screen.height);
            Sequence sequ = DOTween.Sequence();

            sequ.Append(transform.DOMove(target, 0.3f).SetEase(Ease.OutQuad))
                .Join(transform.DOScale(0.9f, 0.3f))
                .OnComplete(MoveToOriginalY);

            sequ.Play();
        }

        
    }
    
    private void MoveToOriginalY()
    {
        Sequence sequ = DOTween.Sequence();

        sequ.Append(transform.DOMoveY(originalPos.y, 0.5f).SetEase(Ease.InOutQuad))
            .Join(transform.DOScale(1,0.5f))
            .OnComplete(() => isClicking = false);

        sequ.Play();
    }

}
