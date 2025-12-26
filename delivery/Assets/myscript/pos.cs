using UnityEngine;
using UnityEngine.UI;

public class pos : MonoBehaviour
{
    // 기본 POS 버튼 및 패널 변수만 유지
    public Button posButton;
    public GameObject shimPanel;
    public GameObject popupPanel;

    void Awake()
    {
        // 초기화 시 팝업과 배경 숨김
        shimPanel.SetActive(false);
        popupPanel.SetActive(false);
    }

    void Start()
    {
        // 버튼 클릭 시 팝업 열기 연결
        posButton.onClick.AddListener(OpenPopup);
        // 배경(shim) 클릭 시 팝업 닫기 연결
        shimPanel.GetComponent<Button>().onClick.AddListener(ClosePopup);
    }

    public void OpenPopup()
    {
        shimPanel.SetActive(true);
        popupPanel.SetActive(true);
        // 기존에 있던 다른 버튼들을 켜는 로직 제거됨
    }

    public void ClosePopup()
    {
        shimPanel.SetActive(false);
        popupPanel.SetActive(false);
        // 기존에 있던 다른 버튼들을 끄는 로직 제거됨
    }
}