using System;
using UnityEngine;

[Serializable]
public class dialogueLine
{
    [TextArea] public string line;
    public bool isFinalLine = false; // True if this is the last line of dialogue in the sequence
    public int nextSpeakerID = -1; // id of the next speaker in the sequence, -1 if no next speaker
}

[Serializable]
public class CharacterDialogue
{
    public Character speaker;
    public dialogueLine[] dialogueLines;

}

[CreateAssetMenu(fileName = "NonEventDialogue", menuName = "KingAndPeasants/NonEventDialogue")]
public class NonEventDialogue : ScriptableObject
{
    public string title; // Title of the dialogue
    public CharacterDialogue[] speakingCharacters; // Array of dialogue sequences
}
