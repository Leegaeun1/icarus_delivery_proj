using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Choice : MonoBehaviour
{
    // 부모 관리자 컴포넌트를 참조
    public ChoiceManager choiceManager;

    // 이 버튼의 고유 ID와 다음 대화 ID
    public int choiceNumber; // 1, 2, 3, 4 중 하나
    public int nextDialogueID;

    // OnChoice1~4 함수를 하나로 통합 (모든 버튼이 이 함수를 호출)
    public void OnChoiceSelected()
    {
        // 버튼을 비활성화하는 로직 제거 (전체 패널이 꺼지므로 불필요)

        // 부모 관리자에게 선택 정보를 전달
        if (choiceManager != null)
        {
            choiceManager.HandleChoiceSelected(choiceNumber, nextDialogueID);
        }
    }

    // 기존의 OnChoice1, OnChoice2, OnChoice3, OnChoice4 함수는 제거합니다.
    // 모든 버튼이 OnChoiceSelected() 하나만 호출하도록 연결하면 됩니다.
}
