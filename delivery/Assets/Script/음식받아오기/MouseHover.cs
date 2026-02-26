using UnityEngine;
using TMPro;
using UnityEngine.EventSystems; // 마우스 이벤트 처리를 위해 필요 [cite: 2026-02-12]

public class MouseHoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 설정")]
    public GameObject tooltipPanel; // 보여줄 UI 패널
    public TextMeshProUGUI infoText; // 데이터를 출력할 TMP

    void Start()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    // 마우스를 올렸을 때 실행
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GlobalDataManager.Instance != null)
        {
            if (tooltipPanel != null) tooltipPanel.SetActive(true);

            // 다른 씬에서 보존된 변수 값을 가져와서 표시 [cite: 2026-02-12]
            if (infoText != null)
                infoText.text = GlobalDataManager.Instance.sharedValue;
        }
    }

    // 마우스가 나갔을 때 실행
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }
}