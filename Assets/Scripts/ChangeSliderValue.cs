using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class ChangeSliderValue : MonoBehaviour
{
    private Slider slider;
    public ResourceType resourceType;
    public Material backgroundMaterial;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private IEnumerator Start()
    {
        yield return null; // Wait one frame to ensure all systems are initialized
        GameManager.Instance.resourceManager.OnResourceChange += UpdateSlider;
        GameManager.Instance.resourceManager.InvokeResourceChangeEvent(this, resourceType);
        
        if(resourceType == ResourceType.Health)
        {
            backgroundMaterial.SetFloat("_PopGrowthMin", GameManager.Instance.resourceManager.healthToPopulationGrowthThreshold);
            backgroundMaterial.SetFloat("_PopLossMax", GameManager.Instance.resourceManager.healthToPopulationLossThreshold);
        }

    }

    private void OnDisable()
    {
        GameManager.Instance.resourceManager.OnResourceChange -= UpdateSlider;
    }

    private void UpdateSlider(object sender, EventArgs change)
    {
        ResourceChangeEventArg changeArg = change as ResourceChangeEventArg;

        if (changeArg.resourceType != resourceType) return;
        switch(resourceType)
        {
            case ResourceType.Health:
                slider.value = changeArg.newValue;
                break;
        }
    }
}
