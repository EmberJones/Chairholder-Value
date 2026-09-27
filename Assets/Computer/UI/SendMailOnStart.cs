using System.Collections;
using UnityEngine;

// Sends the test emails automatically when the game starts,
// so the inbox isn't empty (replaces pressing the old Send Mail button).
public class SendMailOnStart : MonoBehaviour
{
    [SerializeField] private MailTest mailTest;
    [Tooltip("How many times to call SendMail. One press of the old button = 1.")]
    [SerializeField] private int times = 1;

    private IEnumerator Start()
    {
        yield return null; // wait one frame so the Mail Manager is ready

        for (int i = 0; i < times; i++)
            mailTest.SendMail();
    }
}
