using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // LINQ 사용

public class DialogueManager : MonoBehaviour
{
    // 외부 컴포넌트 연결
    public ChatBubbleGenerator bubbleGenerator;
    public Choice choiceUI; // 선택지 UI 관리 (Choice.cs)
    public OrderManager orderManager; // 주문 정보 필요

    // 현재 대화 데이터 (구글 시트에서 로드하거나 인스펙터에서 직접 정의)
    public List<DialogueBlock> dialogueBlocks;

    private Coroutine dialogueRoutine;

    // 현재 대화가 끝나고 다음 대화가 시작될 때까지 기다리는 Lock
    private bool isWaitingForChoice = false;

    // 초기 대화 시작 (AlienAnswer.cs에서 호출)
    public void StartInitialDialogue()
    {
        // 1. OrderManager에게 주문 텍스트를 받음
        string orderText = orderManager.GetRandomOrderText();

        // 2. 초기 대화 블록 (ID 0)을 찾아서 주문 텍스트로 메시지를 교체 (선택 사항)
        DialogueBlock initialBlock = dialogueBlocks.Find(b => b.id == 0);
        if (initialBlock != null && initialBlock.dialogues.Count > 0)
        {
            // 첫 번째 외계인 대화를 주문 텍스트로 설정
            initialBlock.dialogues[0].message = orderText;
            dialogueRoutine = StartCoroutine(RunDialogue(initialBlock.dialogues));
        }
    }

    // 선택지(Choice.cs) 클릭 시 호출됨
    public void ReceiveChoice(int nextDialogueID)
    {
        if (isWaitingForChoice)
        {
            isWaitingForChoice = false;
            // 이전 코루틴이 선택지 대기로 중단되었으므로, 새 대화 흐름 시작
            DialogueBlock nextBlock = dialogueBlocks.Find(b => b.id == nextDialogueID);
            if (nextBlock != null)
            {
                dialogueRoutine = StartCoroutine(RunDialogue(nextBlock.dialogues));
            }
        }
    }

    // 대화 한 묶음을 순차적으로 진행하는 코루틴
    IEnumerator RunDialogue(List<DialogueData> dialogues)
    {
        choiceUI.gameObject.SetActive(false); // 대화 시작 시 선택지 숨기기

        foreach (var data in dialogues)
        {
            // 1. 대화 버블 생성
            bubbleGenerator.GenerateBubble(data);

            // 2. 잠시 대기
            yield return new WaitForSeconds(data.speaker == DialogueData.Speaker.ALIEN ? 1.0f : 0.5f);
            // 외계인 대화는 조금 더 길게 보여줄 수 있음

            // 3. 선택지 활성화 여부 확인
            if (data.activateChoice)
            {
                isWaitingForChoice = true;
                choiceUI.gameObject.SetActive(true); // 선택지 UI를 화면에 표시

                // 선택이 이루어질 때까지 대기
                yield return new WaitUntil(() => isWaitingForChoice == false);
            }
        }
    }
}