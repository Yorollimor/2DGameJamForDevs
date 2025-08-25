using UnityEngine;
using UnityEngine.UI;

public class OptionAnchor : MonoBehaviour
{
    public bool isTaken = false;

    private void OnEnable()
    {
        isTaken = false;
    }

    public void TakeAnchor()
    {
        isTaken = true;
    }

    public void ReleaseAnchor()
    {
        isTaken = false;
    }

    public void DestroyAnchor()
    {
               Destroy(gameObject);
    }
}
