using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MoveToAnchor : MonoBehaviour
{
    public OptionAnchor defaultAnchor;
    public OptionAnchor myAnchor;
    public GameObject anchorParent;
    public RectTransform rectTransform;
    public Button button;

    private void OnEnable()
    {
        if (myAnchor != null) return;

        StartCoroutine(LerpToAnchor());

    }
    private void FindFirstAvailableAnchor()
    {
        anchorParent = GameObject.FindGameObjectWithTag("AnchorParent");
        myAnchor = Instantiate(defaultAnchor, anchorParent.transform);
    }

    private IEnumerator LerpToAnchor()
    {
        float randomDelay = Random.Range(0.1f, 0.3f);
        yield return null;
        button.interactable = false;

        if (myAnchor == null)
        {
            FindFirstAvailableAnchor();
        }

        yield return null;
        Sequence sequ = DOTween.Sequence().SetAutoKill(true);
        sequ.Append(rectTransform.DOMove(myAnchor.GetComponent<RectTransform>().position, 0.5f)).SetEase(Ease.InBack)
            //.AppendInterval(0.3f)
            .OnComplete(() => { if (GetComponent<ValidChoice>().isValidChoice) { button.interactable = true; } });

        sequ.Play();


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
