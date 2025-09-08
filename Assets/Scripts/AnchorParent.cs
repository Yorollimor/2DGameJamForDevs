using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class AnchorParent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OptionAnchor[] OpAnchors;

        OpAnchors = GetComponentsInChildren<OptionAnchor>();

        List<Image> images = new List<Image>();

        foreach (var opAnchor in OpAnchors) 
        {
            if(opAnchor.GetComponent<Image>())
                images.Add(opAnchor.GetComponent<Image>());
        }
        foreach (Image image in images)
        {
            image.enabled = false;
        }
    }
}
