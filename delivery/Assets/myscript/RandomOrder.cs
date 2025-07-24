using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RandomOrder : MonoBehaviour
{
    [Header("UI ��ҵ�")]
    public GameObject popupPrefab; // �˾� ������
    public Canvas mainCanvas; // ���� ĵ����
    public Button startButton; // ���� ��ư
    public TextMeshProUGUI statusText; // ���� �ؽ�Ʈ

    [Header("�ֹ� ����")]
    public string[] orderMenus = { "apple", "melon", "grape" }; // �ֹ� �޴���
    public float minDelay = 2f; // �ּ� ��� �ð�
    public float maxDelay = 5f; // �ִ� ��� �ð�
    public float popupDuration = 3f; // �˾��� ���̴� �ð�

    private bool isProcessing = false;
    private int currentOrderIndex = 0;

    void Start()
    {
        // ���� ��ư �̺�Ʈ ����
        if (startButton != null)
            startButton.onClick.AddListener(StartOrderSequence);

        UpdateStatusText("�ֹ��� �����Ϸ��� ��ư�� Ŭ���ϼ���");
    }

    public void StartOrderSequence()
    {
        if (isProcessing) return;

        isProcessing = true;
        currentOrderIndex = 0;
        startButton.interactable = false;

        UpdateStatusText("�ֹ� ó�� ��...");
        StartCoroutine(ProcessOrdersSequentially());
    }

    IEnumerator ProcessOrdersSequentially()
    {
        for (int i = 0; i < orderMenus.Length; i++)
        {
            // ������ �ð� ���
            float randomDelay = Random.Range(minDelay, maxDelay);
            UpdateStatusText($"���� �ֹ����� {randomDelay:F1}�� ��� ��... ({i + 1}/{orderMenus.Length})");

            yield return new WaitForSeconds(randomDelay);

            // �˾� ���� �� ǥ��
            ShowOrderPopup(orderMenus[i], i + 1);

            // �˾��� ���̴� �ð� ���
            yield return new WaitForSeconds(popupDuration);
        }

        // ��� �ֹ� �Ϸ�
        isProcessing = false;
        startButton.interactable = true;
        UpdateStatusText("��� �ֹ��� �Ϸ�Ǿ����ϴ�!");
    }

    void ShowOrderPopup(string menuName, int orderNumber)
    {
        // �˾� ����
        GameObject popup = Instantiate(popupPrefab, mainCanvas.transform);

        // �˾� ����
        OrderPopup popupScript = popup.GetComponent<OrderPopup>();
        if (popupScript != null)
        {
            popupScript.SetupPopup(menuName, orderNumber, popupDuration);
        }

        UpdateStatusText($"�ֹ� #{orderNumber}: {menuName}");
    }

    void UpdateStatusText(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}

// ���� �˾��� �����ϴ� ��ũ��Ʈ
public class OrderPopup : MonoBehaviour
{
    [Header("�˾� UI ��ҵ�")]
    public TextMeshProUGUI menuNameText;
    public TextMeshProUGUI orderNumberText;
    public Button confirmButton;
    public Button cancelButton;
    public Image backgroundImage;

    [Header("�ִϸ��̼�")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.3f;

    private CanvasGroup canvasGroup;
    private float displayDuration;

    void Awake()
    {
        // CanvasGroup ������Ʈ �������� (������ �߰�)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;

        // ��ư �̺�Ʈ ����
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmClicked);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);
    }

    public void SetupPopup(string menuName, int orderNumber, float duration)
    {
        // �ؽ�Ʈ ����
        if (menuNameText != null)
            menuNameText.text = menuName;

        if (orderNumberText != null)
            orderNumberText.text = $"�ֹ� #{orderNumber}";

        displayDuration = duration;

        // �˾� ǥ�� ����
        StartCoroutine(ShowPopupSequence());
    }

    IEnumerator ShowPopupSequence()
    {
        // ���̵� ��
        yield return StartCoroutine(FadeIn());

        // ������ �ð���ŭ ��� (��ư Ŭ������ ���� ���� ����)
        float timer = 0f;
        while (timer < displayDuration && gameObject != null)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // ���̵� �ƿ� �� ����
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
        Debug.Log($"�ֹ� Ȯ��: {menuNameText.text}");
        StopAllCoroutines();
        StartCoroutine(FadeOutAndDestroy());
    }

    void OnCancelClicked()
    {
        Debug.Log($"�ֹ� ���: {menuNameText.text}");
        StopAllCoroutines();
        StartCoroutine(FadeOutAndDestroy());
    }

    IEnumerator FadeOutAndDestroy()
    {
        yield return StartCoroutine(FadeOut());
        Destroy(gameObject);
    }
}