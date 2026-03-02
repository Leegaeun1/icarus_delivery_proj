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

            // 생성만 합니다. 판정은 FoodDragHandler가 담당합니다.
            Instantiate(selectedFood, foodContainer);

            Debug.Log($"<color=orange>음식 생성 완료 (드래그 가능):</color> {selectedFood.name}");
        }
    }
}