using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "HoverableWordsDatabase", menuName = "Databases/HoverableWordsDatabase")]
public class HoverableWordsDatabase : ScriptableObject
{
    [System.Serializable]
    public class HoverableWordEntry
    {
        public string phrase;
        public string[] phrase_variants;
        [TextArea] public string description;
        public string richTextOpenTag;
        public string richTextCloseTag;


        public HoverableWordEntry(string word, string description)
        {
            this.phrase = word;
            this.description = description;
        }
    }

    public Color primaryResourceColor = Color.softYellow;
    public Color secondaryResourceColor = Color.blueViolet;


    public Dictionary<string, Color> hoverColors = new();
    public Dictionary<string, SortedDictionary<string, HoverableWordEntry>> phraseLookup = new();

    private void OnEnable()
    {
        hoverColors["PrimaryResource"] = primaryResourceColor;
        hoverColors["SecondaryResource"] = secondaryResourceColor;

        FillPhraseLookUpDictionary();
    }

    public HoverableWordEntry[] databaseEntries;
    
    /// <summary>
    /// Retrieves a <see cref="HoverableWordEntry"/> from the database that matches the specified phrase.
    /// </summary>
    /// <param name="word">The phrase to search for in the database. This search is case-insensitive.</param>
    /// <returns>A <see cref="HoverableWordEntry"/> that matches the specified phrase or one of its variants. Returns <see
    /// langword="null"/> if no matching entry is found.</returns>
    public HoverableWordEntry GetEntry(string word)
    {
        foreach (var entry in databaseEntries)
        {
            //check main phrase
            if (entry.phrase.Equals(word, System.StringComparison.OrdinalIgnoreCase))
            {
                return entry;
            }
            //Check variants
            if (entry.phrase_variants != null)
            {
                foreach (var variant in entry.phrase_variants)
                {
                    if (variant.Equals(word, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return entry;
                    }
                }
            }
        }
        return null;
    }

    private void FillPhraseLookUpDictionary()
    {
        foreach (var entry in databaseEntries)
        {
            // The entry.phrase may be a phrase (“net food production”) or single word ("food")
            string firstWord = entry.phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0].Trim().ToLowerInvariant();

            if (!phraseLookup.TryGetValue(firstWord, out var dict))
            {
                dict = new();
                phraseLookup[firstWord] = dict;
            }

            dict[entry.phrase] = entry;

            // Add all variants too
            foreach (var variant in entry.phrase_variants)
            {
                string vFirst = variant.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0].Trim().ToLowerInvariant();

                if (!phraseLookup.TryGetValue(vFirst, out var vDict))
                {
                    vDict = new();
                    phraseLookup[vFirst] = vDict;
                }

                vDict[variant] = entry;
            }
        }

        // Sort phrases within each bucket by descending phrase length
        foreach (var kvp in phraseLookup)
        {
            kvp.
            foreach(var v_kvp in kvp.Value)
            {
                       
            }
            kvp.Value.Sort((a, b) =>
                PhraseLength(b).CompareTo(PhraseLength(a))); // descending
        }
    }

    private static int PhraseLength(HoverableWordsDatabase.HoverableWordEntry entry)
    {
        return entry.phrase.Length;
    }

    /// <summary>
    /// Returns all phrases represented by an entry.
    /// Includes main phrase and all multi-word variants.
    /// </summary>
    private static IEnumerable<string> GetAllPhrases(
        HoverableWordsDatabase.HoverableWordEntry entry)
    {
        yield return entry.phrase;

        if (entry.phrase_variants == null)
            yield break;

        foreach (var variant in entry.phrase_variants)
            yield return variant;
    }

}
