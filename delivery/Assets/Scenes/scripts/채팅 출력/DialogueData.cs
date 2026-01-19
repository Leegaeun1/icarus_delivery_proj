using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 대화의 한 줄을 정의하는 구조체
[System.Serializable]
public class DialogueData
{
    // 화자 구분을 위한 Enum (선택사항, string으로 해도 됨)
    public enum Speaker { ALIEN, PLAYER, SYSTEM }

    public Speaker speaker;      // 누가 말하는가 (외계인, 플레이어 등)
    public string message;       // 대화 내용
    public bool activateChoice;   // 이 대화 직후 선택지 UI를 활성화할지 여부
    public int nextDialogueID;   // (선택사항) 이 대화가 끝나고 이어질 다음 대화 묶음의 ID
}

// 전체 대화 흐름을 담는 묶음
[System.Serializable]
public class DialogueBlock
{
    public int id;
    public List<DialogueData> dialogues;
}