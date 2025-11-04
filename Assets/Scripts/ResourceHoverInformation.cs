using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ResourceHoverInformation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public PopUpInteraction infoPopUp;
    public GameObject popUpPosition;
    public GameObject dailyChangeInfo;
    private GameObject spawnedPopUp;

    private void EmptySpawnedPopUp(PopUpInteraction popUp)
    {
        spawnedPopUp = null;
    }

    private void SpawnPopUp()
    {
        GameObject tempHoverPopUp = Instantiate(infoPopUp.gameObject, popUpPosition.transform.position, Quaternion.identity);
        tempHoverPopUp.transform.SetParent(transform);
        tempHoverPopUp.transform.SetAsFirstSibling();
        tempHoverPopUp.transform.localScale = Vector3.one;

        Vector3 pos = tempHoverPopUp.transform.localPosition;
        RectTransform rectTransformPopUp = infoPopUp.GetComponent<RectTransform>();
        float posX = 0f;
        switch (rectTransformPopUp.anchorMin.x)
        {
            case 0f: // Left
                posX = pos.x;
                break;
            case 0.5f: // Center
                posX = 0f;
                break;
            case 1f: // Right
                posX = pos.x - GetComponent<RectTransform>().sizeDelta.x / 2;
                break;
        }

        tempHoverPopUp.transform.localPosition = new Vector3(posX, pos.y - rectTransformPopUp.sizeDelta.y, 0); // Set the position of the pop
        spawnedPopUp = tempHoverPopUp;
        spawnedPopUp.GetComponent<PopUpInteraction>().OnPopUpClosed += EmptySpawnedPopUp;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (spawnedPopUp != null)
              return;

        print("ResourceHoverInformation: Spawning pop-up");
        SpawnPopUp();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (spawnedPopUp != null)
        {
            spawnedPopUp.GetComponent<PopUpInteraction>().StartDeletionOfPopUp();
        }
    }
}
