using UnityEngine;

public class HideWhilePanelOpen : MonoBehaviour
{
    [SerializeField] private GameObject target;    
    [SerializeField] private GameObject[] panels; 

    private void Update()
    {
        bool anyOpen = false;
        foreach (var panel in panels)
        {
            if (panel != null && panel.activeSelf) { anyOpen = true; break; }
        }

        if (target != null && target.activeSelf == anyOpen)
            target.SetActive(!anyOpen);
    }
}
