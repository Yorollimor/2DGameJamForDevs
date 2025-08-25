using System.Xml;
using UnityEngine;


public enum SpeakerType { King, Advisor, Peasants, HeadDoctor, General, Narrator, Treasurer }

[CreateAssetMenu(fileName = "Character", menuName = "KingAndPeasants/Character")]
public class Character : ScriptableObject
{
    public int id; // Unique identifier for the characterInfo
    public SpeakerType characterType;
    public string name_;
    public string description;
    public Sprite portrait; // Character's portrait image
    public Color color = Color.white;
}
