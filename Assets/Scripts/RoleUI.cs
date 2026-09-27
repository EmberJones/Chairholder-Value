using UnityEngine;
using TMPro;

public class RoleUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TextMeshProUGUI RoleText = gameObject.GetComponent<TextMeshProUGUI>();
        if(GameManager.Instance != null)
            RoleText.text = GameManager.Instance.currentRole.RoleTitle;
        if(TutorialManager.Instance != null)
            RoleText.text = TutorialManager.Instance.currentRole.RoleTitle;
    }
}
