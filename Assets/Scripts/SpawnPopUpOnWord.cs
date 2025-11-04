using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TextMeshProUGUI))]
///<summary>This class is put on objects that display text. It automatically formats words that are in the given database and spawns a corresponding on entry-hover.</summary>
public class SpawnPopUpOnWord : MonoBehaviour
{
    //1. format text
    //2. detect hovered word in text
    //3. check for database entry
    //4. spawn popUp
    //5. check if word was part of a prior pop-up
    //5a. if so, connect pop-ups


    [SerializeField] private HoverableWordsDatabase database;
    [SerializeField] private GameObject popupPrefab;

    public bool darkMode = false;
    private float hoverInteractionDelay = 0.1f;

    private TMP_Text textComponent;
    private GameObject spawnedPopup = null;
    private PopUpInteraction this_popUp = null;
    private int lastWordIndex = -1;
    private string originalText;

    private Vector3 mousePos;
    private int wordIndex = -1;
    private string hoveredWord = "";

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        database = Resources.Load<HoverableWordsDatabase>("HoverWordsDatabase");
    }

    void Update()
    {
        mousePos = Input.mousePosition;
        wordIndex = TMP_TextUtilities.FindIntersectingWord(textComponent, mousePos, null);

        //apply text formatting if text has changed
        if (originalText != textComponent.text)
        {
            textComponent.text = AutomaticFormatting(textComponent.text);
            originalText = textComponent.text;
        }

        if(wordIndex == -1)
        {
        //    //if there is an existing popup, start deleting it
        //    if (spawnedPopup != null)
        //    {
        //        spawnedPopup.GetComponent<PopUpInteraction>().StartDeletionOfPopUp();
        //    }
            lastWordIndex = -1;
            return;
        }

        //if a word was found and it was not the same as before
        if (wordIndex != lastWordIndex)
        {
            ////if there is an existing popup, start deleting it
            //if (spawnedPopup != null)
            //{
            //    spawnedPopup.GetComponent<PopUpInteraction>().StartDeletionOfPopUp();
            //}

            //get the actual word from the text
            hoveredWord = textComponent.textInfo.wordInfo[wordIndex].GetWord();

            //compare the word to the database entries
            var entry = database.GetEntry(hoveredWord);

            //if the word is in the database, instantiate new popup
            if (entry != null)
            {
                Vector3 position = new();

                //setting x position of the popup based on mouse position and screen bounds
                if (mousePos.x - popupPrefab.GetComponent<RectTransform>().rect.width < 0)
                {
                   position.x = mousePos.x + popupPrefab.GetComponent<RectTransform>().rect.width;
                }
                else if(mousePos.x + popupPrefab.GetComponent<RectTransform>().rect.width > Screen.width)
                {
                    position.x = Screen.width - popupPrefab.GetComponent<RectTransform>().rect.width;
                }
                else
                {
                    position.x = mousePos.x;
                }

                //setting y position of the popup based on mouse position and screen bounds
                if (mousePos.y - popupPrefab.GetComponent<RectTransform>().rect.height < 0)
                {
                    position.y = mousePos.y + popupPrefab.GetComponent<RectTransform>().rect.height;
                }
                else if (mousePos.y + popupPrefab.GetComponent<RectTransform>().rect.height > Screen.height)
                {
                    position.y = Screen.height - popupPrefab.GetComponent<RectTransform>().rect.height;
                }
                else
                {
                    position.y = mousePos.y;
                }
                /*
                position.y = mousePos.y > Screen.height / 2 ? position.y - popupPrefab.GetComponent<RectTransform>().rect.height/2f : position.y + popupPrefab.GetComponent<RectTransform>().rect.height / 2f;
                position.x = mousePos.x > Screen.width / 2 ? position.x - popupPrefab.GetComponent<RectTransform>().rect.width/2f : position.x + popupPrefab.GetComponent<RectTransform>().rect.width / 2f;
                */

                ShowPopup(entry, position);
            }
            lastWordIndex = wordIndex;
        }
    }

    private IEnumerator InteractionAfterDelay(float delay)
    {
               yield return new WaitForSeconds(delay);
    }

    private void OnEnable()
    {
        originalText = textComponent.text;
        textComponent.text = AutomaticFormatting(textComponent.text);
    }

    void ShowPopup(HoverableWordsDatabase.HoverableWordEntry entry, Vector3 position)
    {
        //check if this object or it's parent or somewhere else has a PopUpInteraction component
        if(GetComponentInParent<PopUpInteraction>() != null)
        {
            this_popUp = GetComponentInParent<PopUpInteraction>();
        }

        print("Prior pop-up: " + (this_popUp != null ? this_popUp.name : "null"));

        spawnedPopup = Instantiate(popupPrefab, transform); 


        spawnedPopup.GetComponent<PopUpInteraction>().SetSpawnText(textComponent);

        //setting position
        print("Setting pop-up position to: " + position);
        spawnedPopup.transform.position = position;
        //setting text
        spawnedPopup.GetComponentInChildren<TMP_Text>().text = entry.description;
        //making sure it's active
        spawnedPopup.SetActive(true);

        if (this_popUp != null)
        {
            print("Setting up parent-child connection between pop-ups");
            spawnedPopup.GetComponent<PopUpInteraction>().ConnectPopUp(this_popUp);

            PopUpInteraction rootInteraction = this_popUp;
            while (rootInteraction != null && rootInteraction.GetConnectedChildPopUp() != null)
            {
                rootInteraction = rootInteraction.GetConnectedChildPopUp();
            }

            spawnedPopup.transform.SetParent(rootInteraction.transform.parent);
            spawnedPopup.GetComponent<Canvas>().sortingOrder = this_popUp.GetComponent<Canvas>().sortingOrder + 1;
        }
        
        spawnedPopup.transform.SetAsLastSibling();
    }
    string AutomaticFormatting(string text)
    {
        string result = text;

        foreach (var entry in database.databaseEntries)
        {
            if(entry.word_variants != null)
            {
                foreach (var variant in entry.word_variants)
                {
                    string patternVariant = $@"\b{Regex.Escape(variant)}\b";
                    string styledVariant = $"{entry.richTextOpenTag}{variant}{entry.richTextCloseTag}";
                    result = Regex.Replace(result, patternVariant, styledVariant, RegexOptions.IgnoreCase);
                }
            }
            string pattern = $@"\b{Regex.Escape(entry.word)}\b";
            string styledWord = entry.word;
            styledWord = $"{entry.richTextOpenTag}{styledWord}{entry.richTextCloseTag}";

            result = Regex.Replace(result, pattern, styledWord, RegexOptions.IgnoreCase);
        }

        result = ReplaceColorTag(result, database);
        return result;
    }

    string ReplaceColorTag(string text, HoverableWordsDatabase database)
    {
        Dictionary<string, Color> hoverColors = database.hoverColors;
        string result = text;

        foreach (KeyValuePair<string, Color> kvp in hoverColors)
        {
            string tagName = kvp.Key;
            if (!result.Contains(tagName)) continue;
            Color color = kvp.Value;

            if(!darkMode)
            {
                color = Color.Lerp(color, Color.black, 0.35f);
            }


            string colorHex = ColorUtility.ToHtmlStringRGB(color);
            result = result.Replace($"<color={tagName}>", $"<color=#{colorHex}>");
        }

        return result;
    }


}
