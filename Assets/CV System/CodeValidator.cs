using System.Runtime.CompilerServices;
using UnityEngine;
using TMPro;

public class CodeValidator : MonoBehaviour
{
    [SerializeField] private TMP_Text OutPutTextField;
    private string output;
    public void OnCodeChanged(string code)
    {
        output = "";

        if (code.Trim() == "")
        {
            OutPutTextField.text = "Please enter a criminal code";
            return;
        }

        if (BackgroundCheckRegistry.TryLookup(code.ToUpper(), out var result))
        {
            output = "Listed criminal offenses:\n";

            if (result.Offenses.Count == 0)
            {
                output += "No criminal acts on record";
                OutPutTextField.text = output;
                return;
            }

            foreach (var offense in result.Offenses)
            {
                output += offense.name + "\n";
            }

            OutPutTextField.text = output;
            return;
        } else
        {
            OutPutTextField.text = "Invalid Code entered";
        }
    }
}
