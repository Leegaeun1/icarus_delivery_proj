using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FoodManager : MonoBehaviour
{
    [Header("음식 프리팹 설정")]
    public GameObject[] foodPrefabs;

    [Header("생성 위치")]
    public Transform foodContainer;

    [Header("생성 시간 설정")]
    public float minWait = 0f;
    public float maxWait = 3f;

    void Start()
    {
        if (CheckMenu.selectedNames != null && CheckMenu.selectedNames.Count > 0)
        {
            StartCoroutine(SpawnFoodRoutine());
        }
        else
        {
            Debug.Log("<color=white>선택된 항목이 없어 음식을 생성하지 않습니다.</color>");
        }
    }

    IEnumerator SpawnFoodRoutine()
    {
        float randomWait = Random.Range(minWait, maxWait);
        yield return new WaitForSeconds(randomWait);

        if (foodPrefabs != null && foodPrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, foodPrefabs.Length);
            GameObject selectedFood = foodPrefabs[randomIndex];

            // 1. 음식 프리팹 생성 및 변수에 할당
            GameObject spawnedFood = Instantiate(selectedFood, foodContainer);

            // 2. 생성된 음식에서 버튼 컴포넌트를 찾아 이벤트 연결
            Button foodButton = spawnedFood.GetComponent<Button>();
            if (foodButton != null)
            {
                foodButton.onClick.RemoveAllListeners();
                foodButton.onClick.AddListener(() => {
                    if (DeliveryFinalManager.Instance != null)
                    {
                        DeliveryFinalManager.Instance.ConfirmDelivery();
                        Debug.Log("<color=cyan>음식 봉투 클릭:</color> 검증 프로세스 시작");
                        Destroy(spawnedFood);
                    }
                });
            }

            Debug.Log($"<color=orange>음식 생성 완료:</color> {selectedFood.name} (대기시간: {randomWait:F2}초)");
        }
        else
        {
            Debug.LogWarning("Food Prefabs 배열이 비어있습니다!");
        }
    }
}