using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class PopUpInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool mouseEntered = false;
    private PopUpInteraction connectedPopUpChild = null;
    private PopUpInteraction connectedPopUpParent = null;
    private float timeToDestroy = 0.5f;

    private TMP_Text tmpSpawnedFrom = null;

    private int originalWordIndex = -1;
    private int currentWordIndex = -1;
    public event Action<PopUpInteraction> OnPopUpClosed;
    private Vector2 mousePos;

    private void OnEnable()
    {
        mouseEntered = false;
        UpdateWordIndex();
        originalWordIndex = currentWordIndex;
    }

    private void Update()
    {
        mousePos = Input.mousePosition;
        UpdateWordIndex();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseEntered = true;
        Debug.LogWarning("Mouse entered popup");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(mouseEntered)
           StartCoroutine(DeleteAfterDelay(timeToDestroy));

        Debug.LogWarning("Mouse exited popup");
        mouseEntered = false;
    }

    /// <summary>
    /// Connects the parameter as a child to this pop-up, and this pop-up as the parent to the parameter.
    /// </summary>
    /// <param name="otherPopUp"></param>
    public void ConnectPopUp(PopUpInteraction otherPopUp)
    {
        connectedPopUpChild = otherPopUp;
        otherPopUp.connectedPopUpParent = this;
        print("Connected " + otherPopUp.name + " as child to " + this.name);
    }
    public void DisconnectPopUpFromParent()
    {
        connectedPopUpParent = null;
        print("Disconnected pop-up from parent");
    }

    public void SetSpawnText(TMP_Text text)
    {
        tmpSpawnedFrom = text;
    }

    private void UpdateWordIndex()
    {
        if(tmpSpawnedFrom != null)
        {
            currentWordIndex = TMP_TextUtilities.FindIntersectingWord(tmpSpawnedFrom, mousePos, null);
            //print("Current WordIndex updated to: " + currentWordIndex);
        }
    }

    public void StartDeletionOfPopUp()
    {
          StartCoroutine(DeleteAfterDelay(timeToDestroy));
    }

    private IEnumerator DeleteAfterDelay(float delay)
    {
        print("Starting delete timer");
        float timer = 0f;

        yield return new WaitUntil(() => { print("Parent not null yet"); return connectedPopUpParent == null; }); 
        print("No parent pop-up detected, continuing delete timer");

        while (timer < delay)
        {
            if(originalWordIndex != -1)
            {
                print("Current word index: " + currentWordIndex);
                yield return new WaitUntil(() => originalWordIndex != currentWordIndex);
                print("Changed word index: " + currentWordIndex);
            }

            if (mouseEntered)
            {
                print("Mouse re-entered pop-up, cancelling delete timer");
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }
        print("Delete timer completed, destroying pop-up");
        connectedPopUpChild?.DisconnectPopUpFromParent();
        OnPopUpClosed?.Invoke(this);
        Destroy(gameObject);
    }

    public PopUpInteraction GetConnectedChildPopUp()
    {
        return connectedPopUpChild;
    }

    public PopUpInteraction GetConnectedParentPopUp()
    {
        return connectedPopUpParent;
    }
}
