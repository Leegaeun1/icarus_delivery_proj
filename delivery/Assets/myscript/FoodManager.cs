using UnityEngine;
using System.Collections;

public class FoodManager : MonoBehaviour
{
    [Header("음식 프리팹 설정")]
    // 여러 종류의 음식 프리팹을 넣을 수 있습니다 (Ramen, Pizza 등)
    public GameObject[] foodPrefabs;

    [Header("생성 위치")]
    public Transform foodContainer;

    [Header("생성 시간 설정")]
    public float minWait = 0f;
    public float maxWait = 3f;

    void Start()
    {
        // 게임이 시작되면 랜덤 생성 루틴을 시작합니다.
        StartCoroutine(SpawnFoodRoutine());
    }

    IEnumerator SpawnFoodRoutine()
    {
        // 1. 0~3초 사이의 랜덤한 시간을 기다립니다.
        float randomWait = Random.Range(minWait, maxWait);
        yield return new WaitForSeconds(randomWait);

        // 2. 프리팹 배열이 비어있는지 확인합니다.
        if (foodPrefabs != null && foodPrefabs.Length > 0)
        {
            // 3. 배열 중 하나를 랜덤으로 골라 생성합니다.
            int randomIndex = Random.Range(0, foodPrefabs.Length);
            GameObject selectedFood = foodPrefabs[randomIndex];

            Instantiate(selectedFood, foodContainer);
        }
        else
        {
            Debug.LogWarning("Food Prefabs 배열이 비어있습니다!");
        }
    }
}