using UnityEngine;

public class EnableDisableGameObject : MonoBehaviour
{
    public GameObject objToEnable;

    public void EnableObject(GameObject obj)
    {
        objToEnable = obj;
        if (objToEnable != null)
        {
            objToEnable.SetActive(true);
        }
    }

    public void DisableObject(GameObject obj)
    {
        objToEnable = obj;
        if (objToEnable != null)
        {
            objToEnable.SetActive(false);
        }
    }
}
