using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCVFormat", menuName = "CV System/CV Format")]
public class CVFormat : ScriptableObject
{
    [Header("Display")]
    public string ProfileName;

    [Range(0f, 100f)]
    [Tooltip("Overall presentation quality score, 0-100. " +
             "e.g. Excellent=90-100, Good=70-89, Mediocre=45-69, Poor=20-44, Very Poor=0-19.")]
    public float FormatScore = 100f;

    [TextArea]
    [Tooltip("Short list of the specific issues (or strengths) this profile represents, " +
             "shown to the player as feedback after they pick, e.g. 'No section headings', " +
             "'Inconsistent dates', 'Unprofessional contact email'.")]
    public List<string> Notes = new List<string>();
    public Sprite DeskSprite;
}
