using UnityEngine;

public class CVSpawnPoint : MonoBehaviour
{
    public static CVSpawnPoint Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }
}
