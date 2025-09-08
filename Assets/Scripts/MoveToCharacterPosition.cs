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
        //StartCoroutine(LerpToAnchor());
        LerpToAnchor();
    }

    private void LerpToAnchor()
    {
        /*
        float lerpCounter = 0f;
        float animationCounter = 0f;
        float randomDelay = Random.Range(0.2f, 0.5f);
        bool arrived = false;
        bool animationStarted = false;
        */

        if (myAnchor == null)
        {
            FindFirstAvailableAnchor();
        }
        Tween tween = DOTween.Sequence()
           .Append(transform.DOMove(myAnchor.transform.position, 0.5f).SetEase(Ease.InOutBack).SetDelay(Random.Range(0.1f, 0.5f)))
           .Join(transform.DOScale(1, 0.5f));

        tween.Play();
       
        /*
                while (animationCounter < randomDelay)
                {
                    animationCounter += Time.deltaTime;
                    yield return null;
                }


                if (!animationStarted && animationCounter >= randomDelay)
                {
                    animationStarted = true;
                }

                if (animationCounter >= randomDelay)
                {
                    while (myAnchor != null && !arrived)
                    {
                        lerpCounter += Time.deltaTime;
                        rectTransform.position = Vector3.Lerp(rectTransform.position, myAnchor.GetComponent<RectTransform>().position, lerpCounter * 2);

                        if (lerpCounter > 1f)
                            arrived = true;


                        yield return null;
                    }
                }
        */
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
