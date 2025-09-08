using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "ResourceImageContainer", menuName = "Scriptable Objects/ResourceImageContainer")]
public class ResourceImageContainer : ScriptableObject
{
    public ResourceType resourceType;
    public DailyChangeType changeType;
    public Image icon;
    public bool isResource;
}
