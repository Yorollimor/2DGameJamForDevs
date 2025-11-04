using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HoverableWordsDatabase", menuName = "Databases/HoverableWordsDatabase")]
public class HoverableWordsDatabase : ScriptableObject
{
    [System.Serializable]
    public class HoverableWordEntry
    {
        public string word;
        public string[] word_variants;
        [TextArea] public string description;
        public string richTextOpenTag;
        public string richTextCloseTag;

        public HoverableWordEntry(string word, string description)
        {
            this.word = word;
            this.description = description;
        }
    }

    public Color primaryResourceColor = Color.softYellow;
    public Color secondaryResourceColor = Color.blueViolet;


    public Dictionary<string, Color> hoverColors = new();

    private void OnEnable()
    {
        hoverColors["PrimaryResource"] = primaryResourceColor;
        hoverColors["SecondaryResource"] = secondaryResourceColor;
    }

    public HoverableWordEntry[] databaseEntries;
    
    /// <summary>
    /// Retrieves a <see cref="HoverableWordEntry"/> from the database that matches the specified word.
    /// </summary>
    /// <param name="word">The word to search for in the database. This search is case-insensitive.</param>
    /// <returns>A <see cref="HoverableWordEntry"/> that matches the specified word or one of its variants. Returns <see
    /// langword="null"/> if no matching entry is found.</returns>
    public HoverableWordEntry GetEntry(string word)
    {
        foreach (var entry in databaseEntries)
        {
            //check main word
            if (entry.word.Equals(word, System.StringComparison.OrdinalIgnoreCase))
            {
                return entry;
            }
            //Check variants
            if (entry.word_variants != null)
            {
                foreach (var variant in entry.word_variants)
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


}
