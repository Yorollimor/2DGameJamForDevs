using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
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
    [SerializeField] private bool isWeeklyRecap;

    public bool darkMode = false;
    private float hoverInteractionDelay = 0.35f;

    private TMP_Text textComponent;
    private List<GameObject> spawnedPopupList = new();
    private GameObject spawnedPopup = null;
    private PopUpInteraction this_popUp = null;
    private int lastWordIndex = -1;
    private int lastCharIndex = -1;
    private TMP_SpriteCharacter lastSpriteCharacter= null;
    private string originalText;

    private Vector3 mousePos;
    private int wordIndex = -1;
    private int charIndex = -1;
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
        charIndex = TMP_TextUtilities.FindIntersectingCharacter(textComponent, mousePos, null, true);

        if (charIndex != lastCharIndex)
        {

            textComponent.ForceMeshUpdate();
            var charInfo = textComponent.textInfo.characterInfo[charIndex];
            if(charInfo.elementType == TMP_TextElementType.Sprite)
            {
                wordIndex = -1; //if the hovered character is a sprite, do not consider it a hovered word

                var spriteChar = charInfo.textElement as TMPro.TMP_SpriteCharacter;
                string spriteName = spriteChar?.name;

                if (lastSpriteCharacter == spriteChar) return;

                if (!string.IsNullOrEmpty(spriteName))
                {
                    // Lookup in der bestehenden Datenbank
                    var entry = database.GetEntry(spriteName);
                    if (entry != null)
                    {
                        StartCoroutine(InteractionAfterDelay(hoverInteractionDelay, entry, wordIndex));
                    }
                }
                lastCharIndex = charIndex;
                lastSpriteCharacter = spriteChar;
            }
        }
    


        //apply text formatting if text has changed
        if (originalText != textComponent.text)
        {
            Debug.Log("----TEXT CHANGED, FORMATTING NEW -----");
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

    private IEnumerator InteractionAfterDelay(float delay, HoverableWordsDatabase.HoverableWordEntry entry, int index = -1, int charIndex = -1)
    {
        yield return new WaitForSeconds(delay);
        if(index != -1 && wordIndex != index) yield break; //hoveredWord has changed, do not show popup
        if (charIndex != -1 && lastCharIndex != charIndex) yield break;//hoveredWord has changed, do not show popup
       
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

    /// <summary>
    /// Strips the given string of punctuation characters, while preserving end punctuation marks such as '?', '.', '!', and ',' at the end of words.
    /// This method iterates through each character in the input string and constructs a new string that excludes punctuation characters, except for those
    /// specified end punctuation marks when they appear at the end of words. The resulting string is returned without leading or trailing whitespace.
    /// </summary>
    /// <param name="s">The input string to be processed.</param>
    /// <returns>A new string with punctuation removed, except for specified end punctuation marks.</returns>
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

            if(char.IsSeparator(c)| char.IsWhiteSpace(c) | char.IsPunctuation(c))
            {
                sb.Append(' ');
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

    private List<string> GetPhrasesFromTextThatIncludesWord(string text, string word)
    {
        string phraseInText = null;

        List<string> phrasesContainingTheWord = new List<string>();

        //Check which phrases in the database contain the word, including variants
        foreach(var entry in database.databaseEntries)
        {
            if (entry.phrase.Contains(word, StringComparison.OrdinalIgnoreCase))
            {
                phrasesContainingTheWord.Add(entry.phrase);
            }
            if (entry.phrase_variants != null)
            {
                foreach (var variant in entry.phrase_variants)
                {
                    if (variant.Contains(word, StringComparison.OrdinalIgnoreCase))
                    {
                        phrasesContainingTheWord.Add(variant);
                    }
                }
            }
        }

        phrasesContainingTheWord.Sort((a, b) => a.Length.CompareTo(b.Length)); //Sort by length ascending to prioritize shorter phrases
        foreach (var phrase in phrasesContainingTheWord)
        {
            Debug.Log("Phrase containing the word: " + phrase);
        }

        //Store those phrases in a list
        //Check if any of those phrases are in the text, if yes, add that phrase

        List<string> matchingPhrasesInText = new();

        foreach (var phrase in phrasesContainingTheWord)
        {
            Debug.Log("Checking if phrase is in text: " + phrase);
            if (Regex.IsMatch(text, @"(?<!\w)" + Regex.Escape(phrase) + @"(?!\w)", RegexOptions.IgnoreCase))
            {
                Debug.Log("Phrase found in text:");
                phraseInText = Regex.Match(text, @"(?<!\w)" + Regex.Escape(phrase) + @"(?!\w)", RegexOptions.IgnoreCase).Value;
                Debug.Log($"{phraseInText}");

                if (phraseInText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 1)
                {
                    matchingPhrasesInText.Add(phraseInText);
                }
            }
        }

        return matchingPhrasesInText;
    }

    string AutomaticFormatting(string text)
    {
        if (isWeeklyRecap) return text;

        textComponent.ForceMeshUpdate(true); //force update to ensure all tags are registered in textInfo
        string formattedText = text;
        formattedText = CleanStringFromRichTags(formattedText);

        int wordCount = formattedText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        int wordCountTMP = textComponent.GetTextInfo(textComponent.text).wordCount;
        bool banned = false;
        
        Dictionary<string, List<KeyValuePair<string, HoverableWordsDatabase.HoverableWordEntry>>> lookup = database.phraseLookup;

        foreach (string bucket in lookup.Keys)
        {

            if (!Regex.IsMatch(formattedText, @"(?<!\w)" + Regex.Escape(bucket) + @"(?!\w)", RegexOptions.IgnoreCase)) continue;

            Debug.LogFormat("Text contains bucket: {0}", bucket);

            List<string> phrasesInTextContainingWord = GetPhrasesFromTextThatIncludesWord(text, bucket);
            Debug.Log("Phrases in text that includes bucket: " + string.Join(", ", phrasesInTextContainingWord));

            //First, do all the formatting for the shortest phrases/single words.
            //Then go through the text again, check if any phrases contain prior formatted words/phrases, and if so, remove the formatting for those words/phrases that are part of a longer phrase in the text.
            foreach (var entry in lookup[bucket])
            {
                if(entry.Value.phrase == bannedWord)
                {
                    banned = true;
                    continue;
                }
                Debug.Log("Currently formatting phrase: " + entry.Value.phrase);

                string pattern = $@"(?<!\w){Regex.Escape(entry.Value.phrase).Replace(" ", @"[\s\p{P}]+", StringComparison.OrdinalIgnoreCase)}(?!\w)";
                string styledWord = entry.Value.phrase;
                styledWord = $"{entry.Value.richTextOpenTag}{styledWord}";
                styledWord = ReplaceColorTag(styledWord, database) + entry.Value.richTextCloseTag;

                //!-----this currently completely replaces the phrase in the text with the phrase in the database; ideally, it should place the tags around the original text in the text, not replace it with the database phrase. Need to fix this.
                formattedText = Regex.Replace(formattedText, pattern, styledWord, RegexOptions.IgnoreCase);

                foreach(var variant in entry.Value.phrase_variants)
                {
                    string patternVariant = $@"(?<!\w){Regex.Escape(variant).Replace(" ", @"[\s\p{P}]+", StringComparison.OrdinalIgnoreCase)}(?!\w)";
                    string styledVariant = variant;
                    styledVariant = $"{entry.Value.richTextOpenTag}{styledVariant}";
                    styledVariant = ReplaceColorTag(styledVariant, database) + entry.Value.richTextCloseTag;
                    formattedText = Regex.Replace(formattedText, patternVariant, styledVariant, RegexOptions.IgnoreCase);
                }

                Debug.LogFormat("---INBETWEEN Formatted text: {0}", formattedText);
            }

            if (phrasesInTextContainingWord.Count > 0 && !banned)
            {
                foreach (var phrase in phrasesInTextContainingWord)
                {

                    Debug.Log("Removing formatting for phrase in text: " + phrase);
                    formattedText = RemoveTagsInsideOutsidePhrase(formattedText, phrase);


                    HoverableWordsDatabase.HoverableWordEntry entry = database.GetEntry(phrase);

                    Debug.Log("Entry for phrase: " + (entry != null ? entry.phrase : "null"));

                    //---------------------it replaces "food production" with "Net Food Production", leading to "net Net Food Production" when using the database entry. 
                    //------------ fix right now is to use the original phrase from the text.
                    string pattern = $@"(?<!\w){Regex.Escape(phrase)}(?!\w)";
                    string styledWord = phrase;
                    styledWord = $"{entry.richTextOpenTag}{styledWord}";
                    styledWord = ReplaceColorTag(styledWord, database) + entry.richTextCloseTag;

                    formattedText = Regex.Replace(formattedText, pattern, styledWord, RegexOptions.IgnoreCase);
                }
            }

        }

        // print("Word count normal: " + wordCount);
        //print("Word count TMP: " + wordCountTMP);

        //List<HoverableWordsDatabase.HoverableWordEntry> phrases;
        // List<int> phraseStartEndIDs = new();

        //go through each entry in the database and see if it exists in the text
        //foreach (var entry in database.databaseEntries)
        //{
        //    //Debug.Log("Currently banned word: " + bannedWord);
        //    if (entry.phrase == bannedWord) continue; //skip formatting for banned Word

        //    skip = true;

        //    //-----Checks if the main word or any variants have matches in the text, if not -> skip entry


        //    if (!Regex.IsMatch(formattedText, @"\b" + Regex.Escape(entry.phrase) + @"\b", RegexOptions.IgnoreCase)) //if the main phrase is not in the text
        //    {
        //        if (entry.phrase_variants != null) //check for variants
        //        {
        //            foreach (var variant in entry.phrase_variants)
        //            {
        //                //if a variant is found, do not skip the text and continue formatting
        //                if (Regex.IsMatch(formattedText, @"\b" + Regex.Escape(variant) + @"\b", RegexOptions.IgnoreCase) && StripPunctuation(variant).Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 1)
        //                {
        //                    // print("Text contains variant phrase: " + variant);
        //                    skip = false;
        //                }
        //            }
        //        }
        //        if (skip) continue; //skip
        //    }

        //    Debug.Log("Text contains main phrase: " + entry.phrase);

        //    //food -> should not format in "net food production"
        //    //can't be skipped completely or it would skip all other "food" as well
        //    //skip formatting only at places where it's part of another phrase

        //    //get all phrases that contain the word
        //    //check each match if it is part of any of those phrase
        //    //if yes, skip the formatting

        //    phrasesContainingTheWord.Clear();

        //    AddToListIfDatabasePhraseContainsWord(entry.phrase, ref phrasesContainingTheWord);

        //    foreach (var entryVariantPhrase in entry.phrase_variants)
        //    {
        //        AddToListIfDatabasePhraseContainsWord(entryVariantPhrase, ref phrasesContainingTheWord);
        //    }

        //    Debug.Log("----List of PHRASES that contain the word " + entry.phrase);
        //    foreach (var w in phrasesContainingTheWord) Debug.Log(w + " | " + "\n");

        //    // print("Text contains main phrase: " + entry.phrase);

        //    // print("Text contains phrase: " + entry.phrase);

        //    //var words = StripPunctuation(entry.phrase).Split(' ', StringSplitOptions.RemoveEmptyEntries);


        //    //if(words.Length > 1)
        //    //    foreach (string wordInPhrase in words)
        //    //    {
        //    //        formattedText = RemoveTagsAroundWord(formattedText, wordInPhrase);
        //    //    }


        //    //foreach(string variant in entry.phrase_variants.Where(p => StripPunctuation(p).Split(" ").Length > 1))
        //    //{
        //    //    if (!Regex.IsMatch(formattedText, @"\b" + Regex.Escape(variant) + @"\b", RegexOptions.IgnoreCase)) continue;

        //    //    foreach(string word in StripPunctuation(variant).Split(' '))
        //    //    {
        //    //        formattedText = RemoveTagsAroundWord(formattedText, word);
        //    //    }
        //    //}

        //    //Need to check if current entry.phrase (and it's variants) are inside longer existing phrases in the text -> if yes, skip
        //    HoverableWordsDatabase.HoverableWordEntry phrase = IsWordPartOfPhraseInEntry(entry.phrase);
        //    if (phrase != null)
        //    {
        //        Debug.Log("Word " + entry.phrase + " is part of phrase: " + phrase.phrase);
        //        continue; //skip formatting for this entry
        //    }
        //    else
        //    {
        //        Debug.Log("Word " + entry.phrase + " is NOT part of phrase [phrase = null]");
        //    }

        //    //if (phrase != null)
        //    //    Debug.Log("Word " + entry.phrase + " is part of phrase: " + phrase.phrase);
        //    //else
        //    //    Debug.Log("Word " + entry.phrase + " is NOT part of phrase [phrase = null]");

        //    //if (phrase == null || (phrase != null && phrase != entry)) continue; //skip;

        //    //checking variants and formatting them as needed
        //    if (entry.phrase_variants != null)
        //    {
        //        foreach (var variant in entry.phrase_variants)
        //        {
        //            string styledVariant = $"{entry.richTextOpenTag}{variant}"; //<color=PrimaryColor>...</color> -> <color=PrimaryColor>... (omitted closing tag due to string parsing)
        //            styledVariant = ReplaceColorTag(styledVariant, database) + entry.richTextCloseTag;

        //            string patternVariant = $@"(?<!\w){Regex.Escape(variant).Replace("\\ ", @"[\s\p{P}]+")}(?!\w)";
        //            //formattedText = Regex.Replace(formattedText,patternVariant,styledVariant, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        //        }
        //    }
        //    string pattern = $@"(?<!\w){Regex.Escape(entry.phrase).Replace("\\ ", @"[\s\p{P}]+")}(?!\w)";
        //    string styledWord = entry.phrase;

        //    //print("Word to be styled: " + styledWord);
        //    styledWord = $"{entry.richTextOpenTag}{styledWord}";
        //    styledWord = ReplaceColorTag(styledWord, database) + entry.richTextCloseTag;

        //    //print("Styled phrase: " + styledWord);

        //    //formattedText = Regex.Replace(formattedText, pattern, styledWord, RegexOptions.IgnoreCase);
        //}


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

    string RemoveTagsInsideOutsidePhrase(string text, string phrase)
    {
        string pattern = "";

        var wordsInPhrase = StripPunctuation(phrase).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int i = 0;
        foreach (var word in wordsInPhrase)
        {
            if(i == wordsInPhrase.Length - 1)
            {
                pattern += $@"(<[^>]*>)?{Regex.Escape(word)}(<[^>]*>)?"; //to account for the last word not needing a space after it; Makes punctuation afterwords valid
                break;
            }
            pattern += $@"(<[^>]*>)?{Regex.Escape(word)}(<[^>]*>)? ";
            i++;
        }

        //Debug.Log("Pattern for removing tags inside/outside phrase: " + pattern);
        return Regex.Replace(text, pattern, phrase, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Determines whether a specified word is part of a multi-word phrase or its multi-word variants within a given entry.
    /// </summary>
    /// <remarks>This method checks both the main phrase and any phrase variants within the entry to determine
    /// if the specified word is included. It considers phrases with multiple words and uses regular expressions to
    /// match the word boundaries.</remarks>
    /// <param name="hoveredWord">The word to check for inclusion in the phrase or its variants.</param>
    /// <returns>The database entry if the specified word is part of the main phrase or any of its variants; otherwise, <see
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

            //remove punctuation from the phrase and split it into words to check if it's a multi-word phrase
            var strippedPhrase = StripPunctuation(entry.phrase).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string splitPhrase = string.Join(" | ", strippedPhrase);

            if (splitPhrase.Contains(hoveredWord, StringComparison.OrdinalIgnoreCase))
            {
                //Debug.Log("HOVERED WORD: " + hoveredWord + " is part of phrase: " + entry.phrase);
            }
            else continue;
               

            //print($"--- Checking entry: {entry.phrase} ---");
            //checking generally if any multi-word phrases exist in this entry
            if (strippedPhrase.Length == 1)
            {
                bool _multiWordExists = false;
  
                foreach (string word in entry.phrase_variants.Where(w => StripPunctuation(w).Split(' ', StringSplitOptions.RemoveEmptyEntries).Length > 1))
                {
                    Debug.Log(word + " VARIANT WORD is part of PHRASE");
                    _multiWordExists = true;
                    if (Regex.IsMatch(word, $@"\b{Regex.Escape(hoveredWord)}\b"))
                    {
                        // print(hoveredWord + " is part of phrase: " + phrase);
                        PhraseVariantsExist = true;
                        break;
                    }
                }
                if (!_multiWordExists)
                {
                    Debug.Log("No multi-word phrases exist in this entry: " + entry.phrase);
                    continue;
                }
            }


            if (strippedPhrase.Length == 1)
            {
                print($"Main entry '{entry.phrase}' is no phrase, checking for phrase-variants.");
                foreach (var variantEntry in entry.phrase_variants.Where(word => StripPunctuation(word).Split(' ').Length > 1))
                {
                    print($"'{variantEntry}' is being checked for containing '{hoveredWord}'");
                    //check if the hovered phrase is part of the variantEntry and only then continue
                    if (Regex.IsMatch(variantEntry, $@"\b{Regex.Escape(hoveredWord)}\b"))
                    {
                        print(hoveredWord + " is part of phrase: " + variantEntry);
                        PhraseVariantsExist = true;
                        break;
                    }
                }
                if (!PhraseVariantsExist)
                {
                    print("No potential matching phrases in "+ entry.phrase + " or variants have been found");
                    continue; //can't be a phrase
                }
            }

            wordMatchPlacement = CheckingForWordPositionInPhrase(hoveredWord, entry);
            print("WordMatchPlacement: " + wordMatchPlacement + " in phrase: " + entry.phrase);

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
                print("WordPlacement was negative");
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
                print($"Entry '{entry.phrase}' not part of any valid phrase with the hovered phrase '{hoveredWord}'");
                continue; //hoveredWord not part of phrase
            }

            phraseAroundWordExists = FoundMatchingPhraseAroundWord(entry, wordMatchPlacement, variantPhraseMatches);
            if (phraseAroundWordExists) return entry;
        }
        //print("No phrase matches the word: " + hoveredWord);
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
            foreach (string w in StripPunctuation(entry.phrase).Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                wordsInPhrase.Add(w);
            }

            int wordInTextIndex = wordIndex;
           // print("Word in text index: " + wordInTextIndex);

            wordInTextIndex -= wordMatchPlacement; //move index to start of phrase in text
           // print("Word in text index after correction: " + wordInTextIndex);
           // print("Wordcount of text: " + textComponent.textInfo.wordCount);

            if (wordInTextIndex >= 0 && (wordInTextIndex + wordsInPhrase.Count) <= textComponent.textInfo.wordCount) //checks if the wordInTextIndex is within the bounds of the text and if the phrase would fit in the text
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
            //for each phrase containing the word, check if the hoveredWord is part of that phrase and if the full phrase matches the text
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
                if (wordInTextIndex >= 0 && (wordInTextIndex + wordsInPhrase.Count) <= textComponent.textInfo.wordCount)
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
        Debug.Log("Text to be formatted: " + text); 

        var textNew = Regex.Replace(text, @"<[^>]*>", string.Empty, RegexOptions.Singleline);

        Debug.Log("Text after removing RichTags: " + textNew);
        return textNew;
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
