using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FoodOrder
{
    public ReadSpreadSheet.Food_material sandwich;
    public ReadSpreadSheet.Food_material drink;
    public ReadSpreadSheet.Food_material cookie;

    public string GetOrderText()
    {
        return $"손님: {sandwich.name}, {drink.name}, {cookie.name} 주세요!";
    }
}

public class OrderManager : MonoBehaviour
{
    public ReadSpreadSheet sheetLoader;  // ReadSpreadSheet 연결
    public SpeechBubble speechBubble;    // SpeechBubble 연결

    private List<ReadSpreadSheet.Food_material> sandwiches;
    private List<ReadSpreadSheet.Food_material> drinks;
    private List<ReadSpreadSheet.Food_material> cookies;

    private bool isReady = false;

    IEnumerator Start()
    {
        // 데이터 로딩 대기
        while (!sheetLoader.DataReady)
            yield return null;

        // 이름 규칙으로 카테고리 분류
        sandwiches = sheetLoader.materials.FindAll(m => m.name.Contains("샌드위치"));
        drinks = sheetLoader.materials.FindAll(m => m.name.Contains("드링크"));
        cookies = sheetLoader.materials.FindAll(m => m.name.Contains("쿠키"));

        if (sandwiches.Count == 0 || drinks.Count == 0 || cookies.Count == 0)
        {
            Debug.LogError("[OrderManager] 카테고리별 데이터가 충분하지 않습니다.");
            yield break;
        }

        // 랜덤 주문 생성
        FoodOrder order = MakeRandomOrder();

        // SpeechBubble로 출력
        if (speechBubble != null)
        {
            speechBubble.ShowMessage(order.GetOrderText(), 4f); // 4초간 표시
        }
    }

    private FoodOrder MakeRandomOrder()
    {
        FoodOrder order = new FoodOrder();
        order.sandwich = sandwiches[Random.Range(0, sandwiches.Count)];
        order.drink = drinks[Random.Range(0, drinks.Count)];
        order.cookie = cookies[Random.Range(0, cookies.Count)];
        return order;
    }

    public string GetRandomOrderText()
    {
        if (!isReady) return "데이터 로딩중...";
        return MakeRandomOrder().GetOrderText();
    }
}
