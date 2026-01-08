using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[RequireComponent(typeof(TextMeshProUGUI))]
///<summary>This class is put on objects that display text. It automatically formats words that are in the given database and spawns a corresponding pop-up on entry-hover.</summary>
public class SpawnPopUpOnWord : MonoBehaviour
{
    //1. format text
    //2. detect hovered hoveredWord in text
    //3. check for database entry
    //4. spawn popUp
    //5. check if hoveredWord was part of a prior pop-up
    //5a. if so, connect pop-ups


    [SerializeField] private HoverableWordsDatabase database;
    [SerializeField] private GameObject popupPrefab;

    public bool darkMode = false;
    private float hoverInteractionDelay = 0.35f;

    private TMP_Text textComponent;
    private List<GameObject> spawnedPopupList = new();
    private GameObject spawnedPopup = null;
    private PopUpInteraction this_popUp = null;
    private int lastWordIndex = -1;
    private string originalText;

    private Vector3 mousePos;
    private int wordIndex = -1;
    private string hoveredWord = "";
    private string bannedWord = "";

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        database = Resources.Load<HoverableWordsDatabase>("HoverWordsDatabase");
    }
    private void OnEnable()
    {
        textComponent.text = AutomaticFormatting(textComponent.text);
        originalText = textComponent.text;
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
            lastWordIndex = -1;
            return;
        }

        //if a hoveredWord was found and it was not the same as before
        if (wordIndex != lastWordIndex)
        {
            //get the actual hoveredWord from the text
            hoveredWord = textComponent.textInfo.wordInfo[wordIndex].GetWord();

            HoverableWordsDatabase.HoverableWordEntry entry = IsWordPartOfPhraseInEntry(hoveredWord);
            //check if the hoveredWord is part of a phrase in the database
                if (entry != null)
                {
                    print(hoveredWord + " is part of phrase" + entry.phrase);
                }

            //compare the hoveredWord to the database entries
            if (entry == null)
            {
                entry = database.GetEntry(hoveredWord);
            }

            //if the hoveredWord is in the database, instantiate new popup
            if (entry != null)
            {
                if(entry.phrase == bannedWord) //do not show popup for banned hoveredWord
                {
                    lastWordIndex = wordIndex;
                    return;
                }
                StartCoroutine(InteractionAfterDelay(hoverInteractionDelay, entry, wordIndex));
            }
            lastWordIndex = wordIndex;
        }
    }

    private IEnumerator InteractionAfterDelay(float delay, HoverableWordsDatabase.HoverableWordEntry entry, int index)
    {
        yield return new WaitForSeconds(delay);
        if (wordIndex != index) yield break;//hoveredWord has changed, do not show popup
       
        StartCoroutine(ShowPopup(entry));
    }

    private Vector3 CalculatePosition(GameObject popUp)
    {
        Vector3 position = mousePos;

        RectTransform rect = popUp.GetComponent<RectTransform>();
        float popUpWidth = rect.rect.width;
        float popUpHeight = rect.rect.height;

        // Clamp X position to screen bounds
        position.x = Mathf.Clamp(position.x, popUpWidth / 2f, Screen.width - popUpWidth/2f);

        // Clamp Y position to screen bounds
        position.y = Mathf.Clamp(position.y, popUpHeight / 2f, Screen.height - popUpHeight/2f);
        //pos 800
        //popupHeight = 500
        //screenHeight = 1500
        //1400, 250, 1000

        if (mousePos.y > Screen.height / 2)
        {
            if (mousePos.y == position.y) position.y -= popUpHeight / 2;
        }
        else
        {
            if (mousePos.y == position.y) position.y += popUpHeight / 2;
        }

            return position;
    }

    private string StripPunctuation(string s)
    {
        var sb = new StringBuilder();
        foreach (char c in s)
        {
            if(IsEndPunctuation(c))
            {
                sb.Append(c);
                continue;
            }

            if (!char.IsPunctuation(c))
                sb.Append(c);
        }
        return sb.ToString();
    }
    private bool IsEndPunctuation(char c)
    {
        return c == '?' ? true : c == '.' ? true : c == '!' ? true : c == ',' ? true : false;
    }

    IEnumerator ShowPopup(HoverableWordsDatabase.HoverableWordEntry entry)
    {
        //check if this object or it's parent or somewhere else has a PopUpInteraction component
        if(GetComponentInParent<PopUpInteraction>() != null)
        {
            this_popUp = GetComponentInParent<PopUpInteraction>();
        }

       // print("Prior pop-up: " + (this_popUp != null ? this_popUp.name : "null"));

        spawnedPopup = Instantiate(popupPrefab, transform); 
        spawnedPopup.transform.localPosition = Vector3.zero;
        spawnedPopup.transform.position = mousePos;

        //setting text
        spawnedPopup.GetComponentInChildren<TMP_Text>().text = entry.description;
        spawnedPopup.GetComponent<PopUpInteraction>().SetSpawnText(textComponent);
        spawnedPopup.GetComponent<PopUpInteraction>().SetPopUpTitle(entry.phrase);
        spawnedPopup.GetComponentInChildren<SpawnPopUpOnWord>().SetBannedWord(entry.phrase);


        //making sure it's active
        spawnedPopup.SetActive(true);

        if (this_popUp != null)
        {
            //print("Setting up parent-child connection between pop-ups");
            spawnedPopup.GetComponent<PopUpInteraction>().ConnectPopUp(this_popUp);

            PopUpInteraction rootInteraction = this_popUp;
            while (rootInteraction != null && rootInteraction.GetConnectedChildPopUp() != null)
            {
                rootInteraction = rootInteraction.GetConnectedChildPopUp();
            }

            spawnedPopup.transform.SetParent(rootInteraction.transform.parent);
        }

        yield return new WaitUntil(() => spawnedPopup.GetComponent<RectTransform>().rect.width != 0); //wait for end of frame to ensure popup size is calculated correctly

        Vector3 position = CalculatePosition(spawnedPopup);

        //setting position
        //print("Setting pop-up position to: " + position);
        spawnedPopup.transform.position = position;
        //print("Pop-up position set to: " + spawnedPopup.transform.position);

        spawnedPopup.transform.SetAsLastSibling();
        spawnedPopupList.Add(spawnedPopup);
    }
    string AutomaticFormatting(string text)
    {
        string formattedText = CleanStringFromRichTags(text);
        print("Text without tags: " + formattedText);
        bool skip = true;
        string matchedText = string.Empty;
        List<string> phrasesContainingTheWord = new();
        int wordCount = StripPunctuation(formattedText).Split(' ').Length;
        int wordCountTMP = textComponent.GetTextInfo(textComponent.text).wordCount;
        print("Word count normal: " + wordCount);
        print("Word count TMP: " + wordCountTMP);

        List<HoverableWordsDatabase.HoverableWordEntry> phrases;
        List<int> phraseStartEndIDs = new();

        for (int index = 0; index < wordCount; index ++)
        {
            if (!database.phraseLookup.TryGetValue(textComponent.textInfo.wordInfo[index].GetWord(), out List<HoverableWordsDatabase.HoverableWordEntry> matches))
            {
                continue; 
            }

            foreach(var entry in matches)
            {

                foreach(string phrase in entry.phrase_variants)
            }
        }


        foreach (var entry in database.databaseEntries)
        {
            if (entry.phrase == bannedWord) continue; //skip formatting for banned Word

            skip = true;

            //-----Checks if the main word or any variants have matches in the text, if not -> skip entry
            if (!Regex.IsMatch(text, @"\b" + Regex.Escape(entry.phrase) + @"\b", RegexOptions.IgnoreCase)) //if the main phrase is not in the text
            {
                if (entry.phrase_variants != null) //check for variants
                {
                    foreach (var variant in entry.phrase_variants)
                    {
                        //if a variant is found, do not skip the text and continue formatting
                        if (Regex.IsMatch(text, @"\b" + Regex.Escape(variant) + @"\b", RegexOptions.IgnoreCase) && StripPunctuation(variant).Split(' ').Length > 1)
                        {
                           // print("Text contains variant phrase: " + variant);
                            skip = false;
                        }
                    }
                }
                if(skip) continue; //skip
            }

            //food -> should not format in "net food production"
            //can't be skipped completely or it would skip all other "food" as well
            //skip formatting only at places where it's part of another phrase

            //get all phrases that contain the otherPhrase
            //check each match if it is part of any of those phrase
            //if yes, skip the formatting

            phrasesContainingTheWord.Clear();

            AddToListIfDatabasePhraseContainsWord(entry.phrase, ref phrasesContainingTheWord);

            foreach (var entryVariantPhrase in entry.phrase_variants)
            {
                AddToListIfDatabasePhraseContainsWord(entryVariantPhrase, ref phrasesContainingTheWord);
            }

            print("----List of entries that contain the word " + entry.phrase);
            foreach (var w in phrasesContainingTheWord) print(w + " ");

            // print("Text contains main phrase: " + entry.phrase);

            // print("Text contains phrase: " + entry.phrase);

            foreach (string wordInPhrase in StripPunctuation(entry.phrase).Split(' ').Where(p => StripPunctuation(p).Split(" ").Length > 1))
            {
                formattedText = RemoveTagsAroundWord(formattedText, wordInPhrase);
            }

            foreach(string variant in entry.phrase_variants.Where(p => StripPunctuation(p).Split(" ").Length > 1))
            {
                if (!Regex.IsMatch(formattedText, @"\b" + Regex.Escape(variant) + @"\b", RegexOptions.IgnoreCase)) continue;

                foreach(string word in StripPunctuation(variant).Split(' '))
                {
                    formattedText = RemoveTagsAroundWord(formattedText, word);
                }
            }

            HoverableWordsDatabase.HoverableWordEntry phrase = IsWordPartOfPhraseInEntry(entry.phrase);
            if (phrase == null || (phrase != null && phrase != entry)) continue; //skip;

            ////go through each word in the text
            //foreach (var currentWord in textComponent.textInfo.wordInfo)
            //{
            //    string firstWordInEntry = StripPunctuation(entry.phrase).Split(' ')[0];
            //    if (firstWordInEntry.Equals(currentWord.GetWord(), StringComparison.OrdinalIgnoreCase))
            //    {

            //    }               
            //}

            //checking variants and formatting them as needed
            if (entry.phrase_variants != null)
            {
                foreach (var variant in entry.phrase_variants)
                {
                    //-----This removes tags around all matching words of the phrase in the text, not just in the found phrase -> INVALID

                    //if (Regex.IsMatch(text, @"\b" + Regex.Escape(variant) + @"\b", RegexOptions.IgnoreCase) && StripPunctuation(variant).Split(' ').Length > 1)
                    //{
                    //    foreach (string wordInPhrase in StripPunctuation(variant).Split(' '))
                    //    {
                    //        formattedText = RemoveTagsAroundWord(formattedText, wordInPhrase);
                    //    }
                    //}

                    string patternVariant = $@"\b{Regex.Escape(variant).Replace("\\ ", "\\s+")}\b";
                    string styledVariant = $"{entry.richTextOpenTag}{variant}"; //<color=PrimaryColor>...</color> -> <color=PrimaryColor>... (omitted closing tag due to string parsing)
                    styledVariant = ReplaceColorTag(styledVariant, database) + entry.richTextCloseTag;
                    formattedText = Regex.Replace(formattedText,patternVariant,styledVariant, RegexOptions.IgnoreCase);
                }
            }
            string pattern = $@"\b{Regex.Escape(entry.phrase).Replace("\\ ", "\\s+")}\b";
            string styledWord = entry.phrase;

            //print("Word to be styled: " + styledWord);
            styledWord = $"{entry.richTextOpenTag}{styledWord}";
            styledWord = ReplaceColorTag(styledWord, database) + entry.richTextCloseTag;

            //print("Styled phrase: " + styledWord);

            formattedText = Regex.Replace(formattedText, pattern, styledWord, RegexOptions.IgnoreCase);
        }

        print("Formatted text: " + formattedText);
        return formattedText;
    }

    /// <summary>
    /// Adds phrases or their variants from the database to the specified list if they contain the given word.
    /// </summary>
    /// <remarks>This method searches the database for phrases and their variants that contain the specified
    /// word, ignoring case. Phrases and variants with only one word are excluded from the search. The method ensures
    /// that punctuation is stripped before performing comparisons.</remarks>
    /// <param name="word">The word to search for within the database phrases and their variants. Cannot be null.</param>
    /// <param name="filledList">The list to which matching phrases or variants will be added. Must be initialized before calling this method.</param>
    private void AddToListIfDatabasePhraseContainsWord(string word, ref List<string> filledList)
    {
        foreach (var otherPhrase in database.databaseEntries.Where(x => StripPunctuation(x.phrase) != StripPunctuation(word) && StripPunctuation(x.phrase).Split(' ').Length > 1))
        {
            if (otherPhrase.phrase.Contains(word, StringComparison.OrdinalIgnoreCase))
            {
                filledList.Add(otherPhrase.phrase);
            }
            foreach (var variant in otherPhrase.phrase_variants.Where(x => StripPunctuation(x).Split(' ').Length > 1))
            {
                if (variant.Contains(word, StringComparison.OrdinalIgnoreCase))
                {
                    filledList.Add(variant);
                }
            }
        }
    }
    string RemoveTagsAroundWord(string text, string word)
    {
        string pattern = $@"<[^>]*>{Regex.Escape(word)}<[^>]*>";
        return Regex.Replace(text, pattern, word, RegexOptions.IgnoreCase);
    }
 
    /// <summary>
    /// Determines whether a specified otherPhrase is part of a multi-otherPhrase phrase or its multi-otherPhrase variants within a given entry.
    /// </summary>
    /// <remarks>This method checks both the main phrase and any phrase variants within the entry to determine
    /// if the specified otherPhrase is included. It considers phrases with multiple words and uses regular expressions to
    /// match the otherPhrase boundaries.</remarks>
    /// <param name="hoveredWord">The otherPhrase to check for inclusion in the phrase or its variants.</param>
    /// <returns>The database entry if the specified otherPhrase is part of the main phrase or any of its variants; otherwise, <see
    /// langword="null"/>.</returns>
    HoverableWordsDatabase.HoverableWordEntry IsWordPartOfPhraseInEntry(string hoveredWord)
    {
        int wordMatchPlacement = -1;
        bool PhraseVariantsExist = false;
        bool phraseAroundWordExists = false;

        foreach(HoverableWordsDatabase.HoverableWordEntry entry in database.databaseEntries)
        {
             wordMatchPlacement = -1;
             PhraseVariantsExist = false;
             phraseAroundWordExists = false;

            print($"--- Checking entry: {entry.phrase} ---");
            //checking generally if any multi-otherPhrase phrases exist in this entry
            if (StripPunctuation(entry.phrase).Split(' ').Length == 1)
            {
                bool _multiWordExists = false;
                foreach (string word in entry.phrase_variants.Where(word => StripPunctuation(word).Split(' ').Length > 1))
                {
                    _multiWordExists = true;
                    if (Regex.IsMatch(word, $@"\b{Regex.Escape(hoveredWord)}\b"))
                    {
                        // print(hoveredWord + " is part of phrase: " + phrase);
                        PhraseVariantsExist = true;
                        break;
                    }
                }
                if (!_multiWordExists) continue;
            }


            print("Checking if word is part of phrase: " + entry.phrase);
            if (StripPunctuation(entry.phrase).Split(' ').Length == 1)
            {
               // print($"Main phrase '{entry.phrase}' is no phrase, checking for phrase-variants.");
                foreach (var variantEntry in entry.phrase_variants.Where(word => StripPunctuation(word).Split(' ').Length > 1))
                {
                    // print($"'{phrase}' is being checked for containing '{hoveredWord}'");
                    //check if the hovered phrase is part of the phrase and only then continue
                    if (Regex.IsMatch(variantEntry, $@"\b{Regex.Escape(hoveredWord)}\b"))
                    {
                       // print(hoveredWord + " is part of phrase: " + phrase);
                        PhraseVariantsExist = true;
                        break;
                    }
                }
                if (!PhraseVariantsExist)
                {
                    //print("No potential matching phrases in "+ entry.phrase + " or variants have been found");
                    continue; //can't be a phrase
                }
            }

            wordMatchPlacement = CheckingForWordPositionInPhrase(hoveredWord, entry);
            // print("WordMatchPlacement: " + wordMatchPlacement);

            //for variants make a list of all phrases
            //-> make a list of matched phrases
            //-> go through all phrases and add them to the list if the hovered phrase is part of the phrase
            //-> go through matched list and find a phrase that fits 100% -> formate and trigger popup

            //------------------> MAKE SURE THERE IS NO OTHER LONGER PHRASE THAT MATCHES <<<<<<<---------------------------------------------------------------------------------
            //------------------> MAKE SURE THERE IS NO OTHER LONGER PHRASE THAT MATCHES <<<<<<<---------------------------------------------------------------------------------
            //------------------> MAKE SURE THERE IS NO OTHER LONGER PHRASE THAT MATCHES <<<<<<<---------------------------------------------------------------------------------

            List<string> variantPhraseMatches = new();

            if (wordMatchPlacement == -1)
            {
                //print("WordPlacement was negative");
                if (!PhraseVariantsExist) continue;
                else
                {
                    List<string> possiblePhrases = new();
                    //go through each entry that has multiple words and contains the hovered phrase and add it to the list of possible match phrases
                    foreach (var word in entry.phrase_variants.Where(word => (StripPunctuation(word).Split(' ').Length > 1) && (Regex.IsMatch(word, $@"\b{Regex.Escape(hoveredWord)}\b", RegexOptions.IgnoreCase))))
                    {
                        print(hoveredWord + " is part of this variant phrase: " + word);
                        possiblePhrases.Add(word);
                    }
                    variantPhraseMatches = possiblePhrases;
                }
            }

            if (wordMatchPlacement == -1 && (variantPhraseMatches == null || variantPhraseMatches.Count == 0))
            {
                //print($"Entry '{entry.phrase}' not part of any valid phrase with the hovered phrase '{hoveredWord}'");
                continue; //hoveredWord not part of phrase
            }

            phraseAroundWordExists = FoundMatchingPhraseAroundWord(entry, wordMatchPlacement, variantPhraseMatches);
            if (phraseAroundWordExists) return entry;
        }
        print("No phrase matches the word: " + hoveredWord);
        return null;
    }

    int CheckingForWordPositionInPhrase(string hoveredWord, HoverableWordsDatabase.HoverableWordEntry entry)
    {
        int wordMatchPlacement = -1;
        bool matchFound = false;
        if (StripPunctuation(entry.phrase).Split(' ').Length > 1)
        {
            foreach (string wordInPhrase in StripPunctuation(entry.phrase).Split(' '))
            {
                // print("Current phrase in phrase: " + wordInPhrase);
                wordMatchPlacement++;
                if (hoveredWord.Equals(wordInPhrase, System.StringComparison.OrdinalIgnoreCase))
                {
                    matchFound = true;
                    //   print(hoveredWord + " is part of phrase: "   + entry.phrase);
                    break;
                }
            }
        }

        if (matchFound) return wordMatchPlacement;
        else return -1;
    }

    bool FoundMatchingPhraseAroundWord(HoverableWordsDatabase.HoverableWordEntry entry, int wordMatchPlacement, List<string> possibleMatchesList)
    {
       // print("Finding matching phrase around phrase");
        bool foundMatchingPhrase = true;

        List<string> wordsInPhrase = new();
       // print("Word Match Placement is: " + wordMatchPlacement);
        if (wordMatchPlacement != -1)
        {
            wordsInPhrase = new();
            foreach (string w in StripPunctuation(entry.phrase).Split(' '))
            {
                wordsInPhrase.Add(w);
            }

            int wordInTextIndex = wordIndex;
           // print("Word in text index: " + wordInTextIndex);

            wordInTextIndex -= wordMatchPlacement; //move index to start of phrase in text
           // print("Word in text index after correction: " + wordInTextIndex);
           // print("Wordcount of text: " + textComponent.textInfo.wordCount);

            if (wordInTextIndex >= 0 && (wordInTextIndex + wordsInPhrase.Count) <= textComponent.textInfo.wordCount)
            {
                int i = 0;
                while (i < wordsInPhrase.Count)
                {
                    string priorWord = textComponent.textInfo.wordInfo[wordInTextIndex + i].GetWord();
                    if (wordsInPhrase[i].Equals(priorWord, System.StringComparison.OrdinalIgnoreCase))
                    {

                       // print("Checking phrase: " + priorWord + "with phrase phrase: " + wordsInPhrase[i] + "of phrase: " + entry.phrase);
                        i++;
                        continue;
                    }
                    else
                    {
                       // print("Word does not match");
                        foundMatchingPhrase = false;
                        break;
                    }
                }
            }
            else
            {
               // print("Phrase would be out of bounds");
                foundMatchingPhrase = false;
            }
        }

        if (possibleMatchesList != null && foundMatchingPhrase == false)
        {
            //for each phrase containing the phrase
            foreach (string phrase in possibleMatchesList)
            {
                foundMatchingPhrase = true;
                wordsInPhrase = new();
                foreach (string w in StripPunctuation(phrase).Split(' '))
                {
                    wordsInPhrase.Add(w);
                }

                int wordInTextIndex = wordIndex;

                wordInTextIndex -= wordMatchPlacement; //move index to start of phrase in text

                //check if the full phrase matches 
                if (wordInTextIndex !< 0 && (wordInTextIndex + wordsInPhrase.Count) !> textComponent.textInfo.wordCount)
                {
                    int i = 0;
                    while (i < wordsInPhrase.Count)
                    {
                        string priorWord = textComponent.textInfo.wordInfo[wordInTextIndex + i].GetWord();
                        if (wordsInPhrase[i].Equals(priorWord, System.StringComparison.OrdinalIgnoreCase))
                        {
                            i++;
                           // print("Alternative Checking phrase: " + priorWord + "with phrase phrase: " + wordsInPhrase[i] + "of phrase: " + phrase);
                            continue;
                        }
                        else
                        {
                            //print("Alternative Word does not match");
                            foundMatchingPhrase = false;
                            break;
                        }
                    }
                }
                else
                {
                   // print("Alternative Phrase would be out of bounds");
                    foundMatchingPhrase = false;
                }

                if (foundMatchingPhrase) break;
            }
        }

        //print("Phrase found? " + foundMatchingPhrase);
        return foundMatchingPhrase;
    }

    string CleanStringFromRichTags(string text)
    {
        textComponent.ForceMeshUpdate(true);
        return Regex.Replace(text, @"<[^>]*>", string.Empty, RegexOptions.Singleline);
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

    public void SetBannedWord(string word)
    {
        bannedWord = word;
    }


}
