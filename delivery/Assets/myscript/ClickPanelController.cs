using UnityEngine;
using UnityEngine.UI;

public class ClickPanelController : MonoBehaviour
{
    public Button checkButton;      // 팝업을 여는 버튼
    public GameObject shimPanel;    // 배경 패널 (shim panel)
    public GameObject checkPanel;   // 체크 팝업 패널

    void Start()
    {
        // 체크 버튼 클릭 이벤트 추가
        checkButton.onClick.AddListener(OpenCheckPanel);

        // shim 패널 클릭 이벤트 추가 (팝업 닫기용)
        if (shimPanel != null)
        {
            shimPanel.GetComponent<Button>().onClick.AddListener(CloseCheckPanel);
        }

        // 처음에는 패널들이 보이지 않도록 설정
        shimPanel.SetActive(false);
        checkPanel.SetActive(false);
    }

    // 체크 패널을 여는 함수
    public void OpenCheckPanel()
    {
        shimPanel.SetActive(true);   // 배경 먼저 활성화
        checkPanel.SetActive(true);  // 체크 패널 활성화
    }

    // 체크 패널을 닫는 함수 (shim 패널 클릭 시 호출)
    public void CloseCheckPanel()
    {
        shimPanel.SetActive(false);  // 배경 비활성화
        checkPanel.SetActive(false); // 체크 패널 비활성화
    }
}