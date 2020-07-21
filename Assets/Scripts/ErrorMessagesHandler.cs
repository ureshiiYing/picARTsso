using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorMessagesHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject errorPanel;

    public void DisplayError(string msg)
    {
        StartCoroutine(ErrorPrompt(msg));
    }

    private IEnumerator ErrorPrompt(string message)
    {
      
        errorPanel.GetComponentInChildren<TMP_Text>().text = message;
        errorPanel.SetActive(true);
        yield return new WaitForSeconds(2f);
        errorPanel.SetActive(false);
    }
}
