using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChatBubbleGenerator : MonoBehaviour
{
    // 인스펙터에 연결할 프리팹
    public GameObject alienBubblePrefab; // 외계인(상대방)이 말할 때 사용할 버블 프리팹
    public GameObject playerBubblePrefab; // 플레이어(나)가 말할 때 사용할 버블 프리팹
    public RectTransform contentRect; // 채팅 버블들이 들어갈 Content 영역

    // 대화 데이터를 받아 버블을 생성하고 스크롤을 최하단으로 내리는 함수
    public void GenerateBubble(DialogueData data)
    {
        GameObject bubbleGO = null;

        // 화자에 따라 사용할 프리팹 결정
        if (data.speaker == DialogueData.Speaker.ALIEN)
        {
            bubbleGO = Instantiate(alienBubblePrefab, contentRect);
        }
        else if (data.speaker == DialogueData.Speaker.PLAYER)
        {
            bubbleGO = Instantiate(playerBubblePrefab, contentRect);
        }
        // SYSTEM 메시지 등 추가 가능

        if (bubbleGO != null)
        {
            // 버블 프리팹 내부에 있는 TextMeshPro 컴포넌트를 찾아서 텍스트 설정
            TMP_Text textComponent = bubbleGO.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = data.message;
            }

            // 버블 생성 후 스크롤을 최하단으로 자동 이동 (UI 로직 필요)
            // Layout Group의 Rebuild가 필요하며, 일반적으로 ScrollRect 컴포넌트의 기능을 활용함.
            // (여기서는 ScrollRect 로직은 생략하고 생성까지만 구현)
        }
    }
}