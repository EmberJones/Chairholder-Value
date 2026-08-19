using UnityEngine;
using TMPro;

public class RoleUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TextMeshProUGUI RoleText = gameObject.GetComponent<TextMeshProUGUI>();
        RoleText.text = GameManager.Instance.currentRole.RoleTitle;
    }
}
