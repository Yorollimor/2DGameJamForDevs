using System;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasGroup))]
public class PopUpInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool mouseEntered = false;
    private PopUpInteraction connectedPopUpChild = null;
    private List<PopUpInteraction> connectedPopUpParent = new();
    private GameObject objectSpawnedFrom = null;
    private float timeToDestroy = 0.05f;
    private int hierarchyLevel = 0;

    private static List<PopUpInteraction> allPopUps = new();

    [SerializeField] private TextMeshProUGUI PopUpTitle;

    private TMP_Text textObjectSpawnedFrom = null;

    private int originalWordIndex = -1;
    private int currentWordIndex = -1;
    public event Action<PopUpInteraction> OnPopUpClosed;
    private Vector2 mousePos;
    private Coroutine coroutine;
    private CanvasGroup canvasGroup;

    private void OnEnable()
    {

        canvasGroup = GetComponent<CanvasGroup>();

        if (PopUpTitle != null)
            PopUpTitle.text = "";

        mouseEntered = false;
        allPopUps.Add(this);
    }

    public int GetHierarchyLevel() { return hierarchyLevel; }

    private void Update()
    {
        mousePos = Input.mousePosition;
        UpdateWordIndex();

        if(!mouseEntered)
        {
            if(objectSpawnedFrom != null)
            {
                //Check if mouse is still over the object that spawned this pop-up
                RectTransform rectTransform = objectSpawnedFrom.GetComponent<RectTransform>();
                Vector2 localMousePos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePos, null, out localMousePos);
                if (rectTransform.rect.Contains(localMousePos))
                {
                    //print("Mouse is still over object that spawned pop-up. Not starting delete timer.");
                    return;
                }
            }
            if (textObjectSpawnedFrom != null)
            {
                if(originalWordIndex == currentWordIndex)
                {
                    //print("Mouse is outside pop-up, but still on original phrase. Not starting delete timer.");
                    return;
                }
            }
            if(coroutine == null)
            {
                coroutine = StartCoroutine(DeleteAfterDelay(timeToDestroy));
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseEntered = true;
        //Debug.LogWarning("Mouse entered popup");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseEntered = false;
    }

    /// <summary>
    /// Connects the parameter as a child to this pop-up, and this pop-up as the parent to the parameter.
    /// </summary>
    /// <param name="otherPopUp"></param>
    public void ConnectPopUp(PopUpInteraction otherPopUp)
    {
       // print("Connect popUp");
        hierarchyLevel = otherPopUp.hierarchyLevel + 1;
        connectedPopUpChild = otherPopUp;
        otherPopUp.connectedPopUpParent.Add(this);

        SetSortingOrderAndHierarchy();
       // print("Connected " + otherPopUp.name + " as child to " + this.name);
    }
    public void DisconnectPopUpFromParent(PopUpInteraction parent)
    {
        if(!connectedPopUpParent.Contains(parent))
        {
            Debug.LogWarning("Attempted to disconnect pop-up from a parent that is not connected.");
            return;
        }
        connectedPopUpParent.Remove(parent);
       // print("Disconnected pop-up from parent");
    }

    private void SetSortingOrderAndHierarchy()
    {
       // print("Hierarchy: " + hierarchyLevel);
      //  print("PopUpsCount: " + allPopUps.Count);

        GetComponent<Canvas>().sortingOrder = 5;

        //foreach (PopUpInteraction popUp in allPopUps)
        //{
        //    popUp.GetComponent<Canvas>().sortingOrder = popUp.GetComponent<Canvas>().sortingOrder > 5 ? popUp.GetComponent<Canvas>().sortingOrder - 1 : 5;
        //} 

        if (allPopUps.Count > 1)
        {
            //-2 to skip the current pop-up as the latest entry
            if (allPopUps[allPopUps.Count - 2].GetHierarchyLevel() < hierarchyLevel)
            {
                GetComponent<Canvas>().sortingOrder = allPopUps[allPopUps.Count - 2].GetComponent<Canvas>().sortingOrder + 1; //Ensure pop-ups always appear above previous pop-ups
            }
        }


    }

    public void SetSpawnText(TMP_Text text)
    {
        textObjectSpawnedFrom = text;
        StartCoroutine(DelayedSetWordIndex());
    }

    public void SetPopUpTitle(string title)
    {
        PopUpTitle.text = title;
    }

    private IEnumerator DelayedSetWordIndex()
    {
        yield return null; // wait one frame for TMP to update geometry
        UpdateWordIndex();
        originalWordIndex = currentWordIndex;
       // print($"Setting original phrase index to: {originalWordIndex} [{this.name}]");
    }

    private void UpdateWordIndex()
    {
        if(textObjectSpawnedFrom != null)
        {
            currentWordIndex = TMP_TextUtilities.FindIntersectingWord(textObjectSpawnedFrom, mousePos, null);
            //print("Current WordIndex updated to: " + currentWordIndex + $"[{this.name}]");
        }
    }

    public void StartDeletionOfPopUp()
    {
          StartCoroutine(DeleteAfterDelay(timeToDestroy));
    }

    private IEnumerator DeleteAfterDelay(float delay)
    {
       // print("Starting delete timer");
        float timer = 0f;
        float fadeStopThreshold = 0.5f;

        while (timer < delay)
        {
            if(originalWordIndex != -1)
            {
               // print("Current phrase index: " + currentWordIndex + $"[{this.name}]");
                yield return new WaitUntil(() => originalWordIndex != currentWordIndex);
                //print("Current phrase index does not equal original. Continueing deletion timer." + $"[{this.name}]");
            }
            if (TriggerMouseInsidePopUp())
            {
                yield break;
            }
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, fadeStopThreshold, timer / delay);
            //print(delay - timer + " seconds remaining on delete timer" + $"[{this.name}]");
            yield return null;
        }

        yield return new WaitUntil(() => {
        TriggerMouseInsidePopUp();
        return connectedPopUpParent.Count == 0; });

        //print("No parent pop-up detected, continuing delete timer");

        timer = 0f;
        float fadeDuration = 0.2f;
        while (!TriggerMouseInsidePopUp())
        {
            if (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(fadeStopThreshold, 0f, timer / fadeDuration);
            }
            else
            {
                //print("Delete timer completed, destroying pop-up" + $"[{this.name}]");
                connectedPopUpChild?.DisconnectPopUpFromParent(this);
                OnPopUpClosed?.Invoke(this);
                coroutine = null;
                allPopUps.Remove(this);
                Destroy(gameObject);
                yield break;
            }
            yield return null;
        }
        yield break;
    }

    private bool TriggerMouseInsidePopUp()
    {
        if (mouseEntered)
        {
            //print("Mouse re-entered pop-up, cancelling delete timer");
            ResetCoroutineChanges();
        }

        return mouseEntered;
    }

    private void ResetCoroutineChanges()
    {
        coroutine = null;
        canvasGroup.alpha = 1f;
    }

    public void SetObjectSpawnedFrom(GameObject obj)
    {
        objectSpawnedFrom = obj;
    }

    public PopUpInteraction GetConnectedChildPopUp()
    {
        return connectedPopUpChild;
    }

    public List<PopUpInteraction> GetConnectedParentPopUpList()
    {
        return connectedPopUpParent;
    }
}
