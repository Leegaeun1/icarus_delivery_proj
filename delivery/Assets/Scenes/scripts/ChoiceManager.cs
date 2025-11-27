using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceManager : MonoBehaviour
{
    // DialogueManager 참조는 이 관리자에게만 연결
    public DialogueManager dialogueManager;

    // 이 함수를 각 버튼이 호출하게 됩니다.
    public void HandleChoiceSelected(int choiceNumber, int nextDialogueID)
    {
        // 1. 전체 선택지 패널 비활성화 (부모 GameObject를 끔)
        gameObject.SetActive(false);

        // 2. DialogueManager에게 선택 정보 전달
        dialogueManager.ReceiveChoice(nextDialogueID);

        Debug.Log($"Choice {choiceNumber} made. Starting dialogue ID: {nextDialogueID}");

        // *참고: 버튼 자체를 비활성화하는 로직은 필요 없습니다.
        // 왜냐하면 전체 패널(gameObject)이 꺼지기 때문입니다.
    }
}