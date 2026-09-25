using UnityEngine;

[CreateAssetMenu(fileName = "NewCrimeTag", menuName = "CV System/Crime Tag")]
public class CrimeTag : ScriptableObject
{
    [Tooltip("Display name shown to designers and optionally in-game (e.g. 'Financial Fraud').")]
    public string DisplayName;

    [TextArea]
    [Tooltip("Optional designer notes about what this tag represents / examples of offenses that belong under it.")]
    public string Notes;

    public override string ToString() => string.IsNullOrEmpty(DisplayName) ? name : DisplayName;
}
