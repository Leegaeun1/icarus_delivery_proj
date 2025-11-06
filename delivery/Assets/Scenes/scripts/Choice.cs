using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Choice : MonoBehaviour
{
    //public SpeechBubble speechBubble;
    public DialogueManager dialogueManager; // 새로 연결
                                            // 각 버튼에 매핑될 다음 대화 묶음의 ID
    public int nextDialogueID1;
    public int nextDialogueID2;
    public int nextDialogueID3;
    public int nextDialogueID4;


    public void OnChoice1()
    {
        //speechBubble.ShowMessage("1st", 3f);
        GetComponent<Button>().interactable = false;
        dialogueManager.ReceiveChoice(nextDialogueID1);
        gameObject.SetActive(false);
    }

    public void OnChoice2()
    {
        //speechBubble.ShowMessage("2nd", 3f);
        GetComponent<Button>().interactable = false;
        dialogueManager.ReceiveChoice(nextDialogueID2);
        gameObject.SetActive(false);
    }

    public void OnChoice3()
    {
        //speechBubble.ShowMessage("3rd", 3f);
        GetComponent<Button>().interactable = false;
        dialogueManager.ReceiveChoice(nextDialogueID3);
        gameObject.SetActive(false);
    }
    public void OnChoice4()
    {
        //speechBubble.ShowMessage("4th", 3f);
        GetComponent<Button>().interactable = false;
        dialogueManager.ReceiveChoice(nextDialogueID4);
        gameObject.SetActive(false);
    }
}
