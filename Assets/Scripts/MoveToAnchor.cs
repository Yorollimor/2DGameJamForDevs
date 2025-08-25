using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MoveToAnchor : MonoBehaviour
{
    public OptionAnchor defaultAnchor;
    public OptionAnchor myAnchor;
    public GameObject anchorParent;
    public RectTransform rectTransform;
    public Animator animator;
    public Button button;

    private void OnEnable()
    {
        if (myAnchor != null) return;

        StartCoroutine(LerpToAnchor());

    }
    private void FindFirstAvailableAnchor()
    {
        anchorParent = GameObject.FindGameObjectWithTag("AnchorParent");

        /*
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
                Debug.LogWarning("No available anchors found for: " + gameObject.name);
        */

        myAnchor = Instantiate(defaultAnchor, anchorParent.transform);

    }

    private IEnumerator LerpToAnchor()
    {
        float lerpCounter = 0f;
        float animationCounter = 0f;
        float randomDelay = Random.Range(0.2f, 0.5f);
        bool arrived = false;
        bool animationStarted = false;


        if (myAnchor == null)
        {
            FindFirstAvailableAnchor();
        }


        while (animationCounter < randomDelay)
        {
            animationCounter += Time.deltaTime;
            yield return null;
        }


        if (!animationStarted && animationCounter >= randomDelay)
        {
            animator.SetTrigger("Show");
            animationStarted = true;
        }

        if (animationCounter >= randomDelay)
        {
            while (myAnchor != null && !arrived)
            {
                lerpCounter += Time.deltaTime;
                rectTransform.position = Vector3.Lerp(rectTransform.position, myAnchor.GetComponent<RectTransform>().position, lerpCounter * 2);

                if (lerpCounter >= 0.5f)
                {
                    arrived = true;
                    if (button != null)
                    {
                        print("Set button active");
                        button.interactable = true;
                    }

                }
                    


                yield return null;
            }

        }


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
