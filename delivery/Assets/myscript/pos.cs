using UnityEngine;
using UnityEngine.UI;

public class pos : MonoBehaviour
{
    [Header("버튼 설정")]
    public Button OpenButton;      // POS 열기 버튼
    public Button closeButton;    // X (닫기) 버튼

    [Header("패널 설정")]
    public GameObject popupPanel; // 팝업 패널

    void Awake()
    {
        // 시작 시 팝업 숨김
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    void Start()
    {
        // POS 버튼 클릭 시 열기 기능 연결
        if (OpenButton != null)
        {
            OpenButton.onClick.AddListener(OpenPopup);
        }

        // X 버튼 클릭 시 닫기 기능 연결
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
        }
    }

    public void OpenPopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }
    }

    public void ClosePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }
}