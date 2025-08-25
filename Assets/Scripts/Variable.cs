using System;
using UnityEngine;

[Serializable]
public enum VariableType
{
   ItemGot,
   EventHappened,
   DialogueCompleted,
   ChoiceSelected
}

[CreateAssetMenu(fileName = "Variable", menuName = "Scriptable Objects/Variable")]

public class Variable : ScriptableObject
{
    public string variableName;
    public VariableType variableType;
}
