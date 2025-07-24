using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RandomOrder : MonoBehaviour
{
    [Header("UI 요소들")]
    public GameObject popupPrefab; // 팝업 프리팹
    public Canvas mainCanvas; // 메인 캔버스
    public Button startButton; // 시작 버튼
    public TextMeshProUGUI statusText; // 상태 텍스트

    [Header("주문 설정")]
    public string[] orderMenus = { "apple", "melon", "grape" }; // 주문 메뉴들
    public float minDelay = 2f; // 최소 대기 시간
    public float maxDelay = 5f; // 최대 대기 시간
    public float popupDuration = 3f; // 팝업이 보이는 시간

    private bool isProcessing = false;
    private int currentOrderIndex = 0;

    void Start()
    {
        // 시작 버튼 이벤트 연결
        if (startButton != null)
            startButton.onClick.AddListener(StartOrderSequence);

        UpdateStatusText("주문을 시작하려면 버튼을 클릭하세요");
    }

    public void StartOrderSequence()
    {
        if (isProcessing) return;

        isProcessing = true;
        currentOrderIndex = 0;
        startButton.interactable = false;

        UpdateStatusText("주문 처리 중...");
        StartCoroutine(ProcessOrdersSequentially());
    }

    IEnumerator ProcessOrdersSequentially()
    {
        for (int i = 0; i < orderMenus.Length; i++)
        {
            // 랜덤한 시간 대기
            float randomDelay = Random.Range(minDelay, maxDelay);
            UpdateStatusText($"다음 주문까지 {randomDelay:F1}초 대기 중... ({i + 1}/{orderMenus.Length})");

            yield return new WaitForSeconds(randomDelay);

            // 팝업 생성 및 표시
            ShowOrderPopup(orderMenus[i], i + 1);

            // 팝업이 보이는 시간 대기
            yield return new WaitForSeconds(popupDuration);
        }

        // 모든 주문 완료
        isProcessing = false;
        startButton.interactable = true;
        UpdateStatusText("모든 주문이 완료되었습니다!");
    }

    void ShowOrderPopup(string menuName, int orderNumber)
    {
        // 팝업 생성
        GameObject popup = Instantiate(popupPrefab, mainCanvas.transform);

        // 팝업 설정
        OrderPopup popupScript = popup.GetComponent<OrderPopup>();
        if (popupScript != null)
        {
            popupScript.SetupPopup(menuName, orderNumber, popupDuration);
        }

        UpdateStatusText($"주문 #{orderNumber}: {menuName}");
    }

    void UpdateStatusText(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}

// 개별 팝업을 관리하는 스크립트
public class OrderPopup : MonoBehaviour
{
    [Header("팝업 UI 요소들")]
    public TextMeshProUGUI menuNameText;
    public TextMeshProUGUI orderNumberText;
    public Button confirmButton;
    public Button cancelButton;
    public Image backgroundImage;

    [Header("애니메이션")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.3f;

    private CanvasGroup canvasGroup;
    private float displayDuration;

    void Awake()
    {
        // CanvasGroup 컴포넌트 가져오기 (없으면 추가)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;

        // 버튼 이벤트 연결
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmClicked);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);
    }

    public void SetupPopup(string menuName, int orderNumber, float duration)
    {
        // 텍스트 설정
        if (menuNameText != null)
            menuNameText.text = menuName;

        if (orderNumberText != null)
            orderNumberText.text = $"주문 #{orderNumber}";

        displayDuration = duration;

        // 팝업 표시 시작
        StartCoroutine(ShowPopupSequence());
    }

    IEnumerator ShowPopupSequence()
    {
        // 페이드 인
        yield return StartCoroutine(FadeIn());

        // 지정된 시간만큼 대기 (버튼 클릭으로 일찍 종료 가능)
        float timer = 0f;
        while (timer < displayDuration && gameObject != null)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // 페이드 아웃 후 제거
        if (gameObject != null)
        {
            yield return StartCoroutine(FadeOut());
            Destroy(gameObject);
        }
    }

    IEnumerator FadeIn()
    {
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, timer / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
        transform.localScale = Vector3.one;
    }

    IEnumerator FadeOut()
    {
        float timer = 0f;
        Vector3 startScale = transform.localScale;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, timer / fadeOutDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        transform.localScale = Vector3.zero;
    }

    void OnConfirmClicked()
    {
        Debug.Log($"주문 확인: {menuNameText.text}");
        StopAllCoroutines();
        StartCoroutine(FadeOutAndDestroy());
    }

    void OnCancelClicked()
    {
        Debug.Log($"주문 취소: {menuNameText.text}");
        StopAllCoroutines();
        StartCoroutine(FadeOutAndDestroy());
    }

    IEnumerator FadeOutAndDestroy()
    {
        yield return StartCoroutine(FadeOut());
        Destroy(gameObject);
    }
}