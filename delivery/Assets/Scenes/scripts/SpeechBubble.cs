using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeechBubble : MonoBehaviour
{
    public GameObject panel;      // ← Panel 오브젝트
    public TMP_Text speechText;   // ← Panel 하위 Text (TMP)

    public void ShowMessage(string message, float duration = 3f)
    {
        panel.SetActive(true);
        speechText.text = message;

        CancelInvoke();
        Invoke("HideMessage", duration);
    }

    private void HideMessage()
    {
        panel.SetActive(false);
    }
}
