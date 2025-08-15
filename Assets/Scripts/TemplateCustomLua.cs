using UnityEngine;
using PixelCrushers.DialogueSystem;

// Rename this class to the same name that you used for the script file.
// Add the script to your Dialogue Manager. You can optionally make this 
// a static class and remove the inheritance from MonoBehaviour, in which
// case you won't add it to the Dialogue Manager.
//
// This class registers two example functions: 
//
// - DebugLog(string) writes a string to the Console using Unity's Debug.Log().
// - AddOne(double) returns the value plus one.
//
// You can use these functions as models and then replace them with your own.
public class OnChoiceSelected : MonoBehaviour // Rename this class.
{
    [Tooltip("Typically leave unticked so temporary Dialogue Managers don't unregister your functions.")]
    public bool unregisterOnDisable = false;



    void OnEnable()
    {
        print("registering OnChoiceSelectedTrigger");
        // Make the functions available to Lua: (Replace these lines with your own.)
        Lua.RegisterFunction(nameof(OnChoiceSelectedTrigger), this, SymbolExtensions.GetMethodInfo(() => OnChoiceSelectedTrigger((double)0)));

    }

    void OnDisable()
    {
        if (unregisterOnDisable)
        {
            // Remove the functions from Lua: (Replace these lines with your own.)
            Lua.UnregisterFunction(nameof(OnChoiceSelectedTrigger));        }
    }

  public void OnChoiceSelectedTrigger( double choiceID)
    {
        //dialogueManagerWrapper.OnChoiceSelected(choiceID);
    }

}



/**/
