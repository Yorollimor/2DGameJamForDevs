using DG.Tweening;
using UnityEngine;

public class MoveUpDownAnimation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.DOLocalMoveY(transform.localPosition.y + 5, 1). SetLoops(-1, LoopType.Yoyo);
    }

}
