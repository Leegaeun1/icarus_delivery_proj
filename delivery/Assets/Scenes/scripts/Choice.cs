using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Choice : MonoBehaviour
{
    public SpeechBubble speechBubble;

    public void OnChoice1()
    {
        speechBubble.ShowMessage("1st", 3f);
        GetComponent<Button>().interactable = false;
    }

    public void OnChoice2()
    {
        speechBubble.ShowMessage("2nd", 3f);
        GetComponent<Button>().interactable = false;
    }

    public void OnChoice3()
    {
        speechBubble.ShowMessage("3rd", 3f);
        GetComponent<Button>().interactable = false;
    }
    public void OnChoice4()
    {
        speechBubble.ShowMessage("4th", 3f);
        GetComponent<Button>().interactable = false;
    }
}
