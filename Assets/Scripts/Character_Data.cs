using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using DG.Tweening;
using Unity.VisualScripting;

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

    public event Action OnExitAnimationFinished;
    public event Action OnCharacterDeleted;

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
        button.image.alphaHitTestMinimumThreshold = 0.99f;// Set the alpha hit test minimum threshold for the button's image
        button.onClick.AddListener(OnCharacterClick); // Add a listener to the button's click event

    }

    private void OnEnable()
    {
        DialogueLogic.OnDialogueEnd += TriggerExitAnimation;
    }

    private void OnDisable()
    {
        DialogueLogic.OnDialogueEnd -= TriggerExitAnimation;
        DialogueManager_SelfMade.OnCharacterFinishedCutsceneOrIntroduction -= NotSpeakingAnymore;
    }
    private void OnCharacterClick()
    {
        if(!isSpeaking)
        {
            GameManager.Instance.dialogueManager.dialoguePanel.GetComponent<DialogueLogic>().stopInteraction = true;
            //StartCoroutine(MakeCharacterSpeakerAndGetEVent());
            transform.SetAsLastSibling();

            Tween tween = DOTween.Sequence()

            .Append(transform.DOMove(GameManager.Instance.gameHandler.speakerPosition.transform.position, 0.5f))
                .SetEase(Ease.InOutCubic)
                .Join(transform.DOScale(1.2f, 0.5f))
                .OnComplete(MakeCharacterSpeakerAndGetEvent);

            tween.Play();

            button.interactable = false;
            GetComponent<FMODUnity.StudioEventEmitter>().enabled = false;
        }

    }

    /// <summary>
    /// being called at day or week transition, and at the end of a dialogue
    /// </summary>
    public void TriggerExitAnimation(Character_Data character, bool specialCall = false)
    {
        if (character != this) return;

        //don't trigger the exit animation if the character stays in the scene -> After Cutscene & Introduction
        if (!specialCall)
            if ((EventManager.currentEvent.eventType == EventType.Cutscene || EventManager.currentEvent.eventType == EventType.Introduction))
            {
                if(EventManager.speakingCharacter.characterInfo.characterType != SpeakerType.King)
                {
                    OnExitAnimationFinished?.Invoke();
                    return;
                }
            }
        print("Exit animation triggered");

        Canvas canvas = GameManager.Instance.gameHandler.gameCanvas;
        RectTransform rT = GetComponent<RectTransform>();
        Vector2 exitPosition = new Vector2(Screen.width + rT.sizeDelta.x, rT.anchoredPosition.y + 50);

        Tween tween = DOTween.Sequence()
            .Append(rT.DOMove(exitPosition, 0.5f).SetEase(Ease.InOutCubic))
            .Join(rT.DOScale(1, 0.5f))
            .OnComplete(() => DeleteCharacterAfterDialogue(specialCall));

        tween.Play();
    }

    private void NotSpeakingAnymore(Character_Data speaker)
    {
        if (speaker != this) return;

        DialogueManager_SelfMade.OnCharacterFinishedCutsceneOrIntroduction -= NotSpeakingAnymore;
        GetComponent<MoveToCharacterPosition>().MoveToAnchor(); //move the character back to the original position
        button.interactable = true;
        GetComponent<FMODUnity.StudioEventEmitter>().enabled = true;
        isSpeaking = false;
    }

    private void MakeCharacterSpeakerAndGetEvent()
    {
        isSpeaking = true;
        DisableAllOtherCharactersInScene();

        if (!hasIntroduced && !GameManager.Instance.gameHandler.GetIntroducedCharacters().Contains(characterInfo.characterType))
        {
            print("Introduce character");
            hasIntroduced = true; // Set the flag to true to indicate that the characterInfo has been introduced
            EventManager.speakingCharacter = this;
            GameManager.Instance.dialogueManager.StartEventDialogue(introductionEvent); // Start the introduction event dialogue
            DialogueManager_SelfMade.OnCharacterFinishedCutsceneOrIntroduction += NotSpeakingAnymore;
            GameManager.Instance.gameHandler.AddToIntroduced(this);
        }
        else
        {
            print("Start next turn with character");
            GameManager.Instance.NextTurn(associatedEvents, this); // Call the NextTurn method from GameManager when the characterInfo is clicked
            DialogueManager_SelfMade.OnCharacterFinishedCutsceneOrIntroduction += NotSpeakingAnymore;
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

    public void DeleteCharacter()
    {
        GameManager.Instance.gameHandler.RemoveCharacterFromDayList(this);
    }

    /// <summary>
    /// removes the character from any list and deletes the object if the event was anything but a cutscene
    /// </summary>
    public void DeleteCharacterAfterDialogue(bool isSpecialCall = false)
    {
        if (EventManager.currentEvent.eventType == EventType.Cutscene) return;

        GameManager.Instance.gameHandler.RemoveCharacterFromDayList(this);
        if (isSpecialCall) return;

        print("InvokedExitAnimation");
        OnExitAnimationFinished?.Invoke();
    }

}
