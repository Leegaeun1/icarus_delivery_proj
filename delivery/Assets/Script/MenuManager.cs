using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject check_menu;
    public GameObject select_menu;
    public GameObject[] special_menus; // ī�� �����յ�
    public RectTransform canvasRectTransform; // Canvas�� RectTransform
    //public GameObject correctIngredient;
    public Transform cardStackParent; // ���� �߰��� ī�� ������ �θ� ������Ʈ
    public TextMeshProUGUI correctName;

    [Header("ī�� ��ġ ����")]
    public int cardsToDisplay = 3; // ���� ������������ ǥ���� ī�� ��
    public float cardSpacing = 20f; // ī��� ī�� ������ ���� (�ȼ�)
    public float dealInterval = 0.5f; // ī�尡 �������� �ð� ����

    private List<GameObject> cardStack; // ���̿� �׾Ƶ� ī�� ����Ʈ

    private int[] num; // �ùٸ� �޴��� �ƴ� ī����� �ε���
    private int correctCardIndex = -1; //  ī���� ���� special_menus �� �ε���

    string correctmenu;

    void Start()
    {
        check_menu.gameObject.SetActive(true);
        select_menu.gameObject.SetActive(false);

        // �� �κп��� special_menus �迭�� null�� �ƴ��� �ٽ� �� �� Ȯ��
        if (special_menus != null && special_menus.Length > 0)
        {
            correctCardIndex = Random.Range(0, special_menus.Length);
            correctmenu = special_menus[correctCardIndex].name;
            correctName.text = special_menus[correctCardIndex].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text;
        }
        else
        {
            Debug.LogError("special_menus �迭�� ����ֽ��ϴ�. �����Ϳ��� �����յ��� �Ҵ����ּ���.");
        }

        List<int> tempNumList = new List<int>();
        for (int i = 0; i < special_menus.Length; i++)
        {
            if (i != correctCardIndex)// �ùٸ� �޴�(craken)�� �ƴ� �͸� ����Ʈ�� �߰�
            {
                tempNumList.Add(i);
            }
        }
        num = tempNumList.ToArray();

        // �ùٸ� ��� ī�尡 special_menus�� �����ϴ��� Ȯ��
        if (correctCardIndex == -1)
        {
            Debug.LogError($"'{correctmenu}' ī�带 special_menus �迭���� ã�� �� �����ϴ�. �̸� �Ǵ� �迭 �ε����� Ȯ�����ּ���.");
        }
        //correctIngredient.GetComponent<TextMeshProUGUI>().text = correctmenu;
    }

    public void CreateCardStack()
    {
        Debug.Log("CreateCardStack �Լ��� ȣ��Ǿ����ϴ�."); // �� ���� �߰��մϴ�.
        // ��ġ�� ī�尡 ���ų�, special_menus�� ��������� ����
        if (num.Length == 0 || special_menus.Length == 0 || correctCardIndex == -1)
        {
            Debug.LogWarning($"��ġ�� ī�� �������� �����ϰų�, '{correctmenu}' ī�带 ã�� �� �����ϴ�.");
            return;
        }

        // ù ��° ī�� �����տ��� �ʺ� ������ ��� ī���� �ʺ�� �����մϴ�.
        float cardWidth = 0f;
        if (special_menus[0].TryGetComponent<RectTransform>(out RectTransform firstCardRect))
        {
            cardWidth = firstCardRect.rect.width;
        }
        else
        {
            Debug.LogError("special_menus �迭�� ù ��° �����տ� RectTransform�� �����ϴ�. UI ���������� Ȯ���ϼ���.");
            return;
        }

        // ���� ��ġ�� ī�� �� ����:
        int actualCardsToDisplay = Mathf.Min(cardsToDisplay, num.Length + 1);
        if (actualCardsToDisplay == 0)
        {
            Debug.LogWarning("ǥ���� ī�尡 �����ϴ� (cardsToDisplay ���� �Ǵ� ��� ������ ī�� ����).");
            return;
        }

        // ��ġ�� ī����� ���� �ε����� ���� ����Ʈ
        List<int> cardIndicesToPlace = new List<int>();

        // 1. �ùٸ� ī�带 ��ġ�� ����Ʈ�� �߰� (������ ����)
        cardIndicesToPlace.Add(correctCardIndex);

        // 2. ������ �ڸ��� �� ���� ī����� num �迭���� ����
        int remainingSlots = actualCardsToDisplay - 1;
        List<int> tempAvailableNums = new List<int>(num);
        for (int k = 0; k < remainingSlots; k++)
        {
            if (tempAvailableNums.Count == 0)
            {
                Debug.LogWarning($"�������� ��ġ�� ī�尡 �� �̻� �����ϴ�. {correctmenu}�� �Բ� ǥ�õ� ���� ī�� ���� �����մϴ�.");
                break;
            }
            int randomIdx = Random.Range(0, tempAvailableNums.Count);
            cardIndicesToPlace.Add(tempAvailableNums[randomIdx]);
            tempAvailableNums.RemoveAt(randomIdx);
        }

        // ���� ��ġ�� ī����� ������ ���� (�ùٸ� ��ᰡ ���� ��ġ�� ���̵���)
        cardIndicesToPlace = cardIndicesToPlace.OrderBy(x => Random.value).ToList();

        // ���� ���������� ��ġ�� cardIndicesToPlace ����Ʈ�� ����Ͽ� ī�� ������ ����ϴ�.
        cardStack = new List<GameObject>();
        foreach (var cardIndex in cardIndicesToPlace)
        {
            cardStack.Add(special_menus[cardIndex]);
        }

        // ī�� ��ġ�� �ڷ�ƾ ����
        StartCoroutine(DealCards(cardWidth, actualCardsToDisplay));
    }

    // ī�带 �ϳ��� ��ġ�� �ڷ�ƾ
    IEnumerator DealCards(float cardWidth, int actualCardsToDisplay)
    {
        // ��ü ī�� �׷��� ������ �ʺ� ���
        float totalGroupWidth = (actualCardsToDisplay * cardWidth) + ((actualCardsToDisplay - 1) * cardSpacing);
        float startPosX = -totalGroupWidth / 2f + cardWidth / 2f;

        for (int i = 0; i < cardStack.Count; i++)
        {
            // 1. ī�� �ν��Ͻ�ȭ
            GameObject instantiatedMenu = Instantiate(cardStack[i], cardStackParent);
            instantiatedMenu.transform.SetSiblingIndex(i);
            // 2. CardGo ������Ʈ�� �����Ѵٸ� Flip �޼��带 ȣ���մϴ�.
            CardGo cardGoComponent = instantiatedMenu.GetComponent<CardGo>();
            if (cardGoComponent != null)
            {
                cardGoComponent.Invoke("Flip", 1f);
                print("Flip����");
            }
            else
            {
                Debug.LogWarning($"���: {instantiatedMenu.name} ������Ʈ�� CardGo ������Ʈ�� �����ϴ�.");
            }

            // 3. RectTransform ����
            RectTransform instantiatedRectTransform = instantiatedMenu.GetComponent<RectTransform>();
            if (instantiatedRectTransform == null)
            {
                Debug.LogError($"�ν��Ͻ�ȭ�� ������Ʈ {instantiatedMenu.name}�� RectTransform ������Ʈ�� �����ϴ�.");
                Destroy(instantiatedMenu);
                continue;
            }

            instantiatedRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            instantiatedRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            instantiatedRectTransform.pivot = new Vector2(0.5f, 0.5f);
            instantiatedRectTransform.localScale = Vector3.one;
            instantiatedRectTransform.localRotation = Quaternion.identity;

            // 4. ī���� ���� ��ġ ���
            float currentPosX = startPosX + i * (cardWidth + cardSpacing);
            Vector2 finalPosition = new Vector2(currentPosX, 0f);

            // 5. �ִϸ��̼� �ڷ�ƾ ����
            StartCoroutine(AnimateCard(instantiatedRectTransform, finalPosition, dealInterval));

            // ���� ī�尡 ����������� ���
            yield return new WaitForSeconds(dealInterval);
        }
    }

    // ī�� �̵� �ִϸ��̼� �ڷ�ƾ
    IEnumerator AnimateCard(RectTransform card, Vector2 targetPosition, float duration)
    {
        Vector2 startPosition = card.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            // Lerp �Լ��� ����Ͽ� ���� ��ġ���� ��ǥ ��ġ���� �ε巴�� �̵�
            float t = elapsedTime / duration;
            card.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null; // ���� �����ӱ��� ���
        }

        // �ִϸ��̼� �Ϸ� �� ���� ��ġ ����
        card.anchoredPosition = targetPosition;
    }
}