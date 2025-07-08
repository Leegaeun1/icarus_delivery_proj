using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // OrderBy ���
using DG.Tweening;

public class CookManager : MonoBehaviour
{
    private List<string> selectedIngredientsNames;
    public GameObject sandwichContainer; // ������ġ ������ �θ� ������Ʈ

    [Header("������ġ ��� �ִϸ��̼� ����")]
    public float dropStartYOffset = 500f; // ȭ�� ��� ������ ��Ḧ ���� Y ������ (�ȼ� ����)
    public float dropDuration = 0.5f;    // �� ��ᰡ �������� �� �ɸ��� �ð�
    public float delayBetweenDrops = 0.2f; // �� ��ᰡ �������� �����ϴ� �ð� ����

    [Header("�� �̸� ����")]
    public string bottomBreadName = "bread2"; // ���� �Ʒ� ���� �� �̸�
    public string topBreadName = "bread";   // ���� ���� ���� �� �̸�

    void Start()
    {
        selectedIngredientsNames = CheckMenu.selectedNames;
        print("���õ� ����: " + string.Join(',', selectedIngredientsNames));

        if (sandwichContainer == null)
        {
            Debug.LogError("CookManager: 'Sandwich Container' GameObject�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        // ��� ��Ḧ �ϴ� ��Ȱ��ȭ�մϴ�. (�� ���� �� ��� ��ᰡ ��Ȱ�� ���¶�� ����)
        for (int i = 0; i < sandwichContainer.transform.childCount; i++)
        {
            sandwichContainer.transform.GetChild(i).gameObject.SetActive(false);
        }

        StartCoroutine(AnimateSandwichDrops());
    }

    IEnumerator AnimateSandwichDrops()
    {
        // 1. ��� ��� ������Ʈ�� �̸����� ã�� ���� ��ųʸ� ���� (���� ����ȭ)
        Dictionary<string, GameObject> allIngredientsDict = new Dictionary<string, GameObject>();
        for (int i = 0; i < sandwichContainer.transform.childCount; i++)
        {
            GameObject child = sandwichContainer.transform.GetChild(i).gameObject;
            allIngredientsDict[child.name] = child;
        }

        // 2. ��� �ִϸ��̼ǿ� ����� ���� ���ĵ� ��� ����Ʈ
        List<GameObject> dropOrderIngredients = new List<GameObject>();

        GameObject bottomBread = null;
        GameObject topBread = null;

        // 3. ���� ���� ó���Ͽ� ��� ���� ����Ʈ�� �߰�
        if (allIngredientsDict.TryGetValue(bottomBreadName, out bottomBread))
        {
            dropOrderIngredients.Add(bottomBread);
        }
        else
        {
            Debug.LogWarning($"'{bottomBreadName}' (�ٴ� ��)�� ������ġ �����̳ʿ��� ã�� �� �����ϴ�.");
        }

        // 4. ���õ� ���� �� ���� �����ϰ� �߰� ���鸸 ���͸��Ͽ� Y������ ����
        List<GameObject> middleIngredients = new List<GameObject>();
        foreach (string selectedName in selectedIngredientsNames)
        {
            // ���õ� ��� �̸��� �� �̸��� ��ġ���� �ʴ� ��츸 �߰�
            if (selectedName != bottomBreadName && selectedName != topBreadName)
            {
                // ���� GameObject�� ��ųʸ����� ã�� �߰� (�̸�.Contains() ����� ��Ȯ�� �̸����� ����)
                GameObject ingredient = allIngredientsDict.FirstOrDefault(pair => pair.Key.Contains(selectedName)).Value;
                // �Ǵ� ��Ȯ�� �̸� ��ġ: GameObject ingredient = allIngredientsDict.GetValueOrDefault(selectedName);
                if (ingredient != null)
                {
                    middleIngredients.Add(ingredient);
                }
            }
        }

        // �߰� ������ Y�� ��ġ�� ���� ���� (���� �Ʒ� ���� ������)
        // �� �κ��� ��� �����յ��� ���������� ���� Y ��ġ�� ���� ���ĵ˴ϴ�.
        middleIngredients = middleIngredients.OrderBy(go => go.transform.position.y).ToList();
        dropOrderIngredients.AddRange(middleIngredients); // �߰� ������ ��� ���� ����Ʈ�� �߰�

        // 5. ������ �� ó��
        if (allIngredientsDict.TryGetValue(topBreadName, out topBread))
        {
            dropOrderIngredients.Add(topBread);
        }
        else
        {
            Debug.LogWarning($"'{topBreadName}' (�� ��)�� ������ġ �����̳ʿ��� ã�� �� �����ϴ�.");
        }

        //// ���� ��� ���� Ȯ�� (����� ��)
        //Debug.Log("���� ��� ����:");
        //foreach (var ing in dropOrderIngredients)
        //{
        //    Debug.Log($"- {ing.name} (Y: {ing.transform.position.y})");
        //}


        // �� ��ῡ ���� �ִϸ��̼� ����
        foreach (GameObject ingredient in dropOrderIngredients)
        {
            // �ִϸ��̼� ���� �� �ʱ� ��ġ ���� (���� ��ġ + dropStartYOffset)
            Vector3 originalPos = ingredient.transform.position; // ���������� ���� ��ġ
            Vector3 startPos = originalPos;
            startPos.y += dropStartYOffset; // ���� Y ��ġ (ȭ�� ��� ��)

            ingredient.transform.position = startPos; // �ʱ� ��ġ ����
            ingredient.SetActive(true); // ��� Ȱ��ȭ

            // �ִϸ��̼� ����: ������ �Ʒ��� ����������
            ingredient.transform.DOMove(originalPos, dropDuration).SetEase(Ease.OutBounce);

            // ���� ��ᰡ ����������� ��� ���
            yield return new WaitForSeconds(delayBetweenDrops);
        }
    }
}