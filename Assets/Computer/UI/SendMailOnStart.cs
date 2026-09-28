using System;
using System.Collections;
using UnityEngine;

public class SendMailOnStart : MonoBehaviour
{
    [Tooltip("How many times to call SendMail. One press of the old button = 1.")]
    [SerializeField] private int times = 1;

    private IEnumerator Start()
    {
        yield return null; 

        for (int i = 0; i < times; i++)
        {
            MailMessage message = new MailMessage(Convert.ToString(i), Convert.ToString(i), Convert.ToString(i));
            MailManager.Instance.ReceiveMail(message);
        }
    }
}
