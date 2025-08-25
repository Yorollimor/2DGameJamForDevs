using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class Character_Data : MonoBehaviour
{
    public Character characterInfo; // Character associated with this data
    public Animator animator;

    public bool hasIntroduced= false; // Flag to check if the characterInfo has been introduced
    public bool isSpeaking = false;

    public EventData introductionEvent; // Event that introduces this characterInfo

    public List<EventData> associatedEvents; // List of events associated with this characterInfo
    public bool hasPeasentEvents = false;
    public bool hasKingEvents = false;

    Image portraitImage; // UI Image component to display the characterInfo's portrait
    Button button; // Button component to trigger characterInfo interactions

    private void Awake()
    {
        button = GetComponent<Button>(); // Get the Button component from this GameObject
        portraitImage = GetComponent<Image>(); // Get the Image component from this GameObject
        portraitImage.sprite = characterInfo.portrait; // Get the Image component from this GameObject
    }
    private void Start()
    {   
        ColorBlock colorBlock = button.colors; // Get the current color block of the button
        colorBlock.highlightedColor = characterInfo.color; // Set the highlighted color of the button to the characterInfo's color
        button.colors = colorBlock; // Apply the modified color block back to the button
        button.image.sprite = characterInfo.portrait; // Set the button's image sprite to the characterInfo's portrait
        button.image.alphaHitTestMinimumThreshold = 0.9f;// Set the alpha hit test minimum threshold for the button's image
        button.onClick.AddListener(OnCharacterClick); // Add a listener to the button's click event

    }
    private void OnCharacterClick()
    {
        if(!isSpeaking)
        {
            StartCoroutine(MoveCharacterToSpeakerPosition());
            button.interactable = false;
            GetComponent<FMODUnity.StudioEventEmitter>().enabled = false;
        }

    }

    private void NotSpeakingAnymore(object sender, EventArgs e)
    {
        GameManager.Instance.dialogueManager.GetDialogueLogicHandler().OnDialogueEnd -= NotSpeakingAnymore;
        GetComponent<MoveToCharacterPosition>().MoveToAnchor();
        button.interactable = true;
        GetComponent<FMODUnity.StudioEventEmitter>().enabled = true;
        isSpeaking = false;
    }

    System.Collections.IEnumerator MoveCharacterToSpeakerPosition()
    {
        GameManager.Instance.dialogueManager.dialoguePanel.GetComponent<DialogueLogic>().stopInteraction = true;
        isSpeaking = true;
        transform.SetAsLastSibling();
        float timer = 0f;
        float speed = 0.5f;

        DisableAllOtherCharactersInScene();

        animator.SetTrigger("Start");

        while (timer / speed <= 1f)
        {
            transform.position = Vector3.Lerp(transform.position, GameManager.Instance.gameHandler.speakerPosition.transform.position, timer / speed);
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1.2f, 1.2f, 1), timer / speed);
            timer += Time.deltaTime;
            yield return null;
        }
        animator.enabled = false;

        if (!hasIntroduced && !GameManager.Instance.gameHandler.GetIntroducedCharacters().Contains(characterInfo.characterType))
        {
            print("Introduce character");
            hasIntroduced = true; // Set the flag to true to indicate that the characterInfo has been introduced
            GameManager.Instance.eventManager.speakingCharacter = this;
            GameManager.Instance.dialogueManager.StartEventDialogue(introductionEvent); // Start the introduction event dialogue         
            GameManager.Instance.dialogueManager.GetDialogueLogicHandler().OnDialogueEnd += NotSpeakingAnymore;
            GameManager.Instance.gameHandler.AddToIntroduced(this);
        }
        else
        {
            print("Start next turn with character");
            GameManager.Instance.NextTurn(associatedEvents, this); // Call the NextTurn method from GameManager when the characterInfo is clicked
            GameManager.Instance.dialogueManager.GetDialogueLogicHandler().OnDialogueEnd += NotSpeakingAnymore;
            GameManager.Instance.dialogueManager.onExitAnimationDone += DeleteCharacter;
        }
    }

    public void DisableCharacter()
    {
        if (button == null)
        {
            Debug.LogWarning("Button is not assigned in Character_Data.");
        }
        else button.interactable = false; // Disable the button to prevent further interactions
        if (portraitImage == null)
        {
            Debug.LogWarning("Image is not assigned in Character_Data.");
            return; // Exit if button or portraitImage is not assigned
        }       
       
        FMODUnity.StudioEventEmitter emitter = GetComponent<FMODUnity.StudioEventEmitter>();
        emitter.enabled = false;
        portraitImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Change the portrait image color to invisible
    }
    private void DisableAllOtherCharactersInScene()
    {
        foreach (Character_Data characterData in GameManager.Instance.gameHandler.GetActiveCharacters())
        {
            if(characterData != this)
                characterData.DisableCharacter(); // Disable each characterInfo's button and portrait image
        }
    }
    public void EnableCharacter()
    {
        FMODUnity.StudioEventEmitter emitter = GetComponent<FMODUnity.StudioEventEmitter>();
        emitter.enabled = true;
        button.interactable = true; // Enable the button for interactions
        portraitImage.color = new Color(1f, 1f, 1f, 1f); // Change the portrait image color to visible
    }

    private void DeleteCharacter(object sender, EventArgs e)
    {
        GameManager.Instance.dialogueManager.onExitAnimationDone -= DeleteCharacter;

        var args = e as DialogueManagerEventArgs;

        if(args != null)
            if (args.sentEvent.eventType == EventType.Cutscene) return;
        
        GameManager.Instance.gameHandler.RemoveCharacterFromDayList(this);
    }

    public void DeleteCharacter()
    {
        GameManager.Instance.dialogueManager.onExitAnimationDone -= DeleteCharacter;

        GameManager.Instance.gameHandler.RemoveCharacterFromDayList(this);
        Destroy(gameObject); // Destroy the characterInfo GameObject
    }


 
}
