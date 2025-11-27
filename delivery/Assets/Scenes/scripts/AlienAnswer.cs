using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class AlienAnswer : MonoBehaviour
{
    public AlienAppearStepped alien;
    //public SpeechBubble bubble;

    public DialogueManager dialogueManager;

    public void CallButtonPressed()
    {
        alien.SummonAlien(); // 외계인 등장
        //StartCoroutine(ShowSpeechDelayed());
    }

    /*IEnumerator ShowSpeechDelayed()
    {
        yield return new WaitForSeconds(0.5f); // 외계인 등장 후 약간 기다림
        bubble.ShowMessage("안녕, 지구인!", 3f);
    }*/
}
