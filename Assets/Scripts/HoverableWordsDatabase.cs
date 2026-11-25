using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[CreateAssetMenu(fileName = "HoverableWordsDatabase", menuName = "Databases/HoverableWordsDatabase")]
public class HoverableWordsDatabase : ScriptableObject
{

    /// <summary>
    /// Basic data structure representing a hoverable word or phrase, its description, and optional rich text formatting for display purposes. Each entry can have a main phrase and multiple variants that trigger the same description when hovered over.
    /// </summary>
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
    public Dictionary<string, List<KeyValuePair<string, HoverableWordEntry>>> phraseLookup = new();

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

    /// <summary>
    /// Populates the phrase lookup dictionary with entries and their variants, organizing them by the first word of
    /// each phrase.
    /// </summary>
    /// <remarks>This method processes all database entries and their phrase variants, ensuring that each is
    /// accessible via the first word in a case-insensitive manner. Existing entries in the lookup dictionary are
    /// preserved, and new buckets are created as needed. Phrases within each bucket are intended to be sorted by
    /// descending length for efficient lookup, although the sorting logic is not implemented in this method.</remarks>
    public void FillPhraseLookUpDictionary()
    {
        foreach (var entry in databaseEntries)
        {
            // The entry.phrase may be a phrase (“net food production”) or single word ("food")
            string firstWord = entry.phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0].Trim().ToLowerInvariant();

            // Check if the first word already has a bucket in the lookup dictionary; if not, create one
            if (!phraseLookup.TryGetValue(firstWord, out var list))
            {
                list = new();
                phraseLookup[firstWord.ToLowerInvariant()] = list;
            }

            // Add the main phrase to the bucket
            list.Add(new KeyValuePair<string, HoverableWordEntry>(entry.phrase, entry));

            // Add all variants too
            foreach (var variant in entry.phrase_variants)
            {
                string vFirst = variant.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0].Trim().ToLowerInvariant();

                if (!phraseLookup.TryGetValue(vFirst, out var variantList))
                {
                    variantList = new();
                    phraseLookup[vFirst.ToLowerInvariant()] = variantList;
                }

                variantList.Add(new KeyValuePair<string, HoverableWordEntry>(variant, entry));
            }
        }

        // Sort phrases within each bucket by descending phrase length
        var keys = phraseLookup.Keys.ToList();

        foreach (var key in keys)
        {
            var list = phraseLookup[key];
            phraseLookup[key] = list
                .OrderByDescending(kv => kv.Key?.Length ?? 0) //first by descending length
                .ThenBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase) //then, for equal length, by alphabet
                .ToList();
        }
        


        PrintPhraseLookup();
    }

    [ContextMenu("Print PhraseLookup")]

    public void PrintPhraseLookup()
    {
        foreach (var bucket in phraseLookup)
        {
            var sb = new StringBuilder(); sb.AppendLine($"Bucket: '{bucket.Key}' ({bucket.Value.Count} entries)");

            foreach (var kv in bucket.Value)
                sb.AppendLine($"  Phrase: '{kv.Key}' (len={kv.Key.Length}) -> Entry.phrase='{kv.Value.phrase}'");

            Debug.Log(sb.ToString(), this); // 'this' sets the Console context to the ScriptableObject } }

        }
    }

}
