using UnityEngine;

[CreateAssetMenu(fileName = "NewSkillTag", menuName = "CV System/Skill Tag")]
public class SkillTag : ScriptableObject
{
    [Tooltip("Display name shown to designers and optionally in-game (e.g. 'Information Technology').")]
    public string DisplayName;

    [TextArea]
    [Tooltip("Optional designer notes about what this tag represents / examples of what belongs under it.")]
    public string Notes;

    public override string ToString() => string.IsNullOrEmpty(DisplayName) ? name : DisplayName;
}
