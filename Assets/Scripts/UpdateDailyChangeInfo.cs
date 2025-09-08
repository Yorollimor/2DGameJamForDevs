using UnityEngine;
using TMPro;

public class UpdateDailyChangeInfo : MonoBehaviour
{
    public TextMeshProUGUI dailyChangeText;
    public ResourceType resourceType;

    private ResourceManager resourceManager;

    private void Start()
    {
        resourceManager = GameManager.Instance.resourceManager;
    }
}
