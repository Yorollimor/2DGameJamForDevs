using DG.Tweening;
using System.Collections;
using UnityEngine;

public class MoveToCharacterPosition : MonoBehaviour
{
    public OptionAnchor myAnchor;
    public GameObject anchorParent;
    public RectTransform rectTransform;
    public Animator animator;

    private void OnEnable()
    {
        if (myAnchor != null) return;

       LerpToAnchor();

    }


    private void FindFirstAvailableAnchor()
    {
        anchorParent = GameObject.FindGameObjectWithTag("CharacterAnchorPoint");

        OptionAnchor[] anchors = anchorParent.GetComponentsInChildren<OptionAnchor>();
        foreach (OptionAnchor a in anchors)
        {
            if (!a.isTaken)
            {
                myAnchor = a;
                myAnchor.TakeAnchor();
                return;
            }
        }
        Debug.LogWarning("No available anchors found.");
    }

    public void MoveToAnchor()
    {
        LerpToAnchor();
    }

    private void LerpToAnchor()
    {

        if (myAnchor == null)
        {
            FindFirstAvailableAnchor();
        }
        Tween tween = DOTween.Sequence()
           .Append(transform.DOMove(myAnchor.transform.position, 0.5f).SetEase(Ease.InOutBack).SetDelay(Random.Range(0.1f, 0.5f)))
           .Join(transform.DOScale(1, 0.5f));

        tween.Play();
       
    }

    /// <summary>
    /// Destroys the current GameObject and releases the associated anchor, if any.
    /// </summary>
    /// <remarks>This method removes the GameObject to which this script is attached from the scene and
    /// releases any associated anchor resources. Ensure that this operation is intended, as it cannot be
    /// undone.</remarks>
    public void DestroyThis()
    {
        myAnchor?.ReleaseAnchor();
        Destroy(this.gameObject);
    }
}
