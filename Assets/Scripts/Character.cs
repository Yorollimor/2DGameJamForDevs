using System.Xml;
using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "KingAndPeasants/Character")]
public class Character : ScriptableObject
{
    public int id; // Unique identifier for the character
    public SpeakerType character;
    public string name_;
    public string description;
    public Sprite portrait; // Character's portrait image
    public Color color = Color.white;
}
