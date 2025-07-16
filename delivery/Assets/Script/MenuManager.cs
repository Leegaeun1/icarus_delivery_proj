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
    public GameObject correctIngredient;
    public Transform cardStackParent; // ���� �߰��� ī�� ������ �θ� ������Ʈ

    [Header("ī�� ��ġ ����")]
    public int cardsToDisplay = 3; // ���� ������������ ǥ���� ī�� ��
    public float cardSpacing = 20f; // ī��� ī�� ������ ���� (�ȼ�)
    public float dealInterval = 0.5f; // ī�尡 �������� �ð� ����

    private List<GameObject> cardStack; // ���̿� �׾Ƶ� ī�� ����Ʈ

    private int[] num; // �ùٸ� �޴�(craken)�� �ƴ� ī����� �ε���
    private int correctCardIndex = -1; //  ī���� ���� special_menus �� �ε���

    string correctmenu ;

    void Awake()
    {
        // Awake���� �̸� �ʱ�ȭ
        check_menu.gameObject.SetActive(true);
        select_menu.gameObject.SetActive(false);

        // �� �κп��� special_menus �迭�� null�� �ƴ��� �ٽ� �� �� Ȯ���ϴ� ��� �ڵ� �߰�
        if (special_menus != null && special_menus.Length > 0)
        {
            correctmenu = special_menus[Random.Range(0, special_menus.Length)].name;
        }
        else
        {
            Debug.LogError("special_menus �迭�� ����ֽ��ϴ�. �����Ϳ��� �����յ��� �Ҵ����ּ���.");
        }
    }
    void Start()
    {
        //check_menu.gameObject.SetActive(true);
        //select_menu.gameObject.SetActive(false);

        //correctmenu = special_menus[Random.Range(0,special_menus.Length)].name; //  �� ���Ŀ� �޴��� �˸°� �ٲ�� �ؾ���. ������ ������

        List<int> tempNumList = new List<int>();
        for (int i = 0; i < special_menus.Length; i++)
        {
            if (special_menus[i].name == correctmenu)
            {
                // ī���� �ε����� ����
                correctCardIndex = i;
            }
            else // �ùٸ� �޴�(craken)�� �ƴ� �͸� ����Ʈ�� �߰�
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
        correctIngredient.GetComponent<TextMeshProUGUI>().text = correctmenu;
    }

    public void random_card()
    {
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
        // cardsToDisplay�� �ùٸ� ��� ī�� ������ �� ��ġ ��
        // �ùٸ� ��Ḧ ������ ������ ī��� num.Length(�������� ������ �� �ִ� ī�� ��) ������ ����
        int actualCardsToDisplay = Mathf.Min(cardsToDisplay, num.Length + 1); // +1�� �ùٸ� ��� ī�带 ����
        if (actualCardsToDisplay == 0)
        {
            Debug.LogWarning("ǥ���� ī�尡 �����ϴ� (cardsToDisplay ���� �Ǵ� ��� ������ ī�� ����).");
            return;
        }
        // ���� cardsToDisplay�� 1���ε� �װ� �ùٸ� ��ᰡ �ƴ϶�� ���
        if (actualCardsToDisplay == 1 && (cardsToDisplay > 1 || num.Length == 0)) // num.Length == 0�� craken�� �ִٴ� �ǹ�
        {
            Debug.LogWarning($"cardsToDisplay�� 1�̸�, {correctmenu}�� ��ġ�˴ϴ�.");
        }


        // ��ü ī�� �׷��� ������ �ʺ� ���
        float totalGroupWidth = (actualCardsToDisplay * cardWidth) + ((actualCardsToDisplay - 1) * cardSpacing);
        float startPosX = -totalGroupWidth / 2f + cardWidth / 2f;

        // ��ġ�� ī����� ���� �ε����� ���� ����Ʈ
        List<int> cardIndicesToPlace = new List<int>();

        // 1. ī�带 ��ġ�� ����Ʈ�� �߰� (������ ����)
        cardIndicesToPlace.Add(correctCardIndex);

        // 2. ������ �ڸ��� �� ���� ī����� num �迭���� ����
        // �ùٸ� ��Ḧ �����ϰ� ��ġ�ؾ� �� ī�� ����
        int remainingSlots = actualCardsToDisplay - 1;

        // num �迭 (�ùٸ� ��� ���� ī���)���� �������� remainingSlots ������ŭ ����
        List<int> tempAvailableNums = new List<int>(num); // �ߺ� ������ ���� �ӽ� ����Ʈ
        for (int k = 0; k < remainingSlots; k++)
        {
            if (tempAvailableNums.Count == 0)
            {
                Debug.LogWarning($"�������� ��ġ�� ī�尡 �� �̻� �����ϴ�. {correctmenu}�� �Բ� ǥ�õ� ���� ī�� ���� �����մϴ�.");
                break;
            }
            int randomIdx = Random.Range(0, tempAvailableNums.Count);
            cardIndicesToPlace.Add(tempAvailableNums[randomIdx]);
            tempAvailableNums.RemoveAt(randomIdx); // �ߺ� ����
        }

        // ���� ��ġ�� ī����� ������ ���� (�ùٸ� ��ᰡ ���� ��ġ�� ���̵���)
        cardIndicesToPlace = cardIndicesToPlace.OrderBy(x => Random.value).ToList();

        // ���� ���������� ��ġ�� cardIndicesToPlace ����Ʈ�� ����Ͽ� ī�� ��ġ
        for (int i = 0; i < cardIndicesToPlace.Count; i++)
        {
            int cardPrefabIndex = cardIndicesToPlace[i]; // special_menus �迭���� ������ ���� �ε���

            GameObject instantiatedMenu = Instantiate(special_menus[cardPrefabIndex]);
            instantiatedMenu.transform.SetParent(canvasRectTransform, false);

            CardGo cardGoComponent = instantiatedMenu.GetComponent<CardGo>();

            // 2. CardGo ������Ʈ�� �����Ѵٸ� �ش� �ν��Ͻ��� Flip �޼��带 ȣ���մϴ�.
            if (cardGoComponent != null)
            {
                cardGoComponent.Invoke("Flip", 1f);  // ���� �� ī�� �ν��Ͻ��� Flip �޼��尡 ȣ��˴ϴ�.
                print("Filp����");
            }
            else
            {
                Debug.LogWarning($"���: {instantiatedMenu.name} ������Ʈ�� CardGo ������Ʈ�� �����ϴ�.");
            }

            RectTransform instantiatedRectTransform = instantiatedMenu.GetComponent<RectTransform>();
            if (instantiatedRectTransform == null)
            {
                Debug.LogError($"�ν��Ͻ�ȭ�� ������Ʈ {instantiatedMenu.name}�� RectTransform ������Ʈ�� �����ϴ�.");
                Destroy(instantiatedMenu);
                continue;
            }

            // ��� ī�忡 �߾� Anchor �� Pivot ����
            instantiatedRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            instantiatedRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            instantiatedRectTransform.pivot = new Vector2(0.5f, 0.5f);

            float currentPosX = startPosX + i * (cardWidth + cardSpacing);
            instantiatedRectTransform.anchoredPosition = new Vector2(currentPosX, 0f);

            instantiatedRectTransform.localScale = Vector3.one;
            instantiatedRectTransform.localRotation = Quaternion.identity;

            Debug.Log($"�ν��Ͻ�ȭ��: {special_menus[cardPrefabIndex].name} ��ġ: {instantiatedRectTransform.anchoredPosition}");
        }
    }
}