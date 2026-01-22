using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ReadSpreadSheets : MonoBehaviour
{
    [Header("Google Sheet Settings")]
    public readonly string ADDRESS = "https://docs.google.com/spreadsheets/d/1SUvkrIiBEfRl-J2_887gtT8MJNJgfKvjd-QEr4NglY0";
    public readonly string RANGE = "A2:B";
    public readonly long SHEET_ID_MATERIAL = 0;          // 재료 시트
    public readonly long SHEET_ID_SALES = 899576211;     // 판매 가격 시트 (2열: food_name, price)

    [Header("Data Lists")]
    public List<Food_material> materials;  // 재료 리스트
    public List<Food_product> productPrices; // 완성품 가격 리스트

    [Header("Game State")]
    public TextMeshProUGUI mineral;
    public TextMeshProUGUI money_effect;
    public int money;
    public int usedMoney = 0;

    // 통계용 변수 (메모리에만 저장, 일차 종료 시 SaveDailyData로 저장)
    public int totalSpent = 0;   // 총 지출
    public int totalRevenue = 0; // 총 수익
    public int save_Spent = 0;

    // 오늘 하루 동안 번 돈 (정산 전)
    public int dailyRevenue = 0;

    // [테스트용] 현재 손님이 요청한 메뉴 이름 (인스펙터에서 직접 입력하여 테스트)
    public string currentRequestName = "kraken_sand";

    private bool dataReady = false;
    private string last_name = string.Empty;

    private Button timeManager;


    [System.Serializable]
    public class Food_material
    {
        public string name;
        public int cost;
    }

    [System.Serializable]
    public class Food_product // 완성품 (수익)
    {
        public string food_name;
        public int price;
    }

    void Start()
    {

        if (mineral == null)
        {
            Debug.Log("[ReadSpreadSheet] 미네랄 텍스트가 등록되어있지 않습니다.");
            mineral = GameObject.Find("money_txt").GetComponent<TextMeshProUGUI>();
        }
        timeManager = GameObject.Find("TimeManager").GetComponent<Button>();
        money_effect = GameObject.Find("money_effect").GetComponent<TextMeshProUGUI>();
        // 시작 시 효과 텍스트 투명하게 초기화
        if (money_effect != null)
        {
            Color c = money_effect.color;
            c.a = 0f;
            money_effect.color = c;
        }
        // 개발 테스트용 초기화
        //PlayerPrefs.DeleteKey("Gold");
        //PlayerPrefs.DeleteKey("TotalSpent");

        // 하루 시작 시 일일 수익 초기화
        dailyRevenue = 0;

        // 저장된 돈 불러오기 (없으면 기본값 1000으로 시작한다고 가정 - 값 조정 필요)
        money = PlayerPrefs.GetInt("Gold", 40);
        totalSpent = PlayerPrefs.GetInt("TotalSpent", 0); // 누적 지출 불러오기
        totalRevenue = PlayerPrefs.GetInt("TotalRevenue", 0); // 수익 불러오기

        mineral.text = money.ToString();

        // 게임 시작 시 장바구니 리스트 초기화 (이전 저장 내역 무시)
        CheckMenu.selectedNames.Clear();
        usedMoney = 0;

        StartCoroutine(LoadAllData());
    }

    public static string GetTSVAddress(string address, string range, long sheetId)
    {
        return $"{address}/export?format=tsv&range={range}&gid={sheetId}";
    }

    // 두 개의 시트를 순차적으로 로딩
    private IEnumerator LoadAllData()
    {
        // 1. 재료 데이터 로드 (기존)
        UnityWebRequest wwwMaterial = UnityWebRequest.Get(GetTSVAddress(ADDRESS, RANGE, SHEET_ID_MATERIAL));
        yield return wwwMaterial.SendWebRequest();
        materials = GetDatas<Food_material>(wwwMaterial.downloadHandler.text);
        Debug.Log("재료 데이터 로드 완료");

        // 2. 판매 가격 데이터 로드
        UnityWebRequest wwwSales = UnityWebRequest.Get(GetTSVAddress(ADDRESS, RANGE, SHEET_ID_SALES));
        yield return wwwSales.SendWebRequest();
        productPrices = GetDatas<Food_product>(wwwSales.downloadHandler.text);
        Debug.Log("판매 가격 데이터 로드 완료");

        dataReady = true;
    }
    T GetData<T>(string[] datas) // TSV 한 행을 T타입 객체로 변환하는 함수 
    {
        object data = Activator.CreateInstance(typeof(T));
        FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        for (int i = 0; i < fields.Length; i++)
        {
            try
            {
                Type type = fields[i].FieldType;

                if (string.IsNullOrEmpty(datas[i]))
                    continue;

                if (type == typeof(int))
                    fields[i].SetValue(data, int.Parse(datas[i]));
                else if (type == typeof(float))
                    fields[i].SetValue(data, float.Parse(datas[i]));
                else if (type == typeof(bool))
                    fields[i].SetValue(data, bool.Parse(datas[i]));
                else if (type == typeof(string))
                    fields[i].SetValue(data, datas[i]);
                else
                    fields[i].SetValue(data, Enum.Parse(type, datas[i]));
            }
            catch (Exception e)
            {
                Debug.LogError($"SpreadSheet Error : {e.Message}");
            }
        }

        return (T)data;
    }

    List<T> GetDatas<T>(string data)
    {
        List<T> returnList = new List<T>();
        string[] splitedData = data.Split('\n');

        foreach (string element in splitedData)
        {
            string[] datas = element.Split('\t');
            returnList.Add(GetData<T>(datas));
        }
        return returnList;
    }
    // 수익 계산 로직
    // 요청한 메뉴 이름(requestName)에 맞는 재료가 포함되어 있는지 검사
    private int CalculateSalesRevenue(string requestName)
    {
        // 1. 요청한 메뉴의 가격 정보 찾기
        var product = productPrices.Find(p => p.food_name == requestName);
        if (product == null)
        {
            Debug.LogError($"[오류] 요청한 메뉴 '{requestName}'가 가격표(Sheet)에 없습니다.");
            return 0;
        }

        // 2. 검증 로직: "요청 메뉴 이름" 안에 "선택된 재료 이름"이 포함되어 있는지 확인
        // 예: request="kraken_sand" 이고, 재료에 "kraken"이 있으면 성공
        bool isMatch = false;
        foreach (string ingredient in CheckMenu.selectedNames)
        {
            if (requestName.Contains(ingredient))
            {
                isMatch = true;
                Debug.Log($"[매칭 성공] 요청: {requestName} / 핵심재료: {ingredient}");
                break;
            }
        }

        if (isMatch)
        {
            return product.price;
        }
        else
        {
            Debug.Log($"[매칭 실패] 요청은 '{requestName}'였으나, 핵심 재료가 포함되지 않았습니다.");
            return 0; // 요청 불일치 시 수익 없음
        }
    }

    public void OnIngredientToggled(string ingredientName, bool isSelected)
    {
        if (!dataReady) return;

        var material = materials.Find(m => m.name == ingredientName);
        if (material == null) return;

        if (isSelected)
        {
            if (!CheckMenu.selectedNames.Contains(ingredientName))
            {
                CheckMenu.selectedNames.Add(ingredientName);

                last_name = ingredientName;

                usedMoney += material.cost;
            }
        }
        else
        {
            if (CheckMenu.selectedNames.Contains(ingredientName))
            {
                CheckMenu.selectedNames.Remove(ingredientName);
                usedMoney -= material.cost;
            }
        }
        Debug.Log($"현재 선택 총액: {usedMoney} / 마지막 추가: {last_name}");
    }

    // 결제 처리 함수
    public bool ApplyPurchase()
    {
        // 1. 잔액 확인
        if (money < usedMoney)
        {
            Debug.LogWarning("잔액 부족!");

            // 돈 부족 시 마지막 재료 취소
            if (!string.IsNullOrEmpty(last_name) && CheckMenu.selectedNames.Contains(last_name))
            {
                CheckMenu.selectedNames.Remove(last_name);
                var material = materials.Find(m => m.name == last_name);
                if (material != null)
                {
                    usedMoney -= material.cost;
                }
                last_name = string.Empty;
            }
            return false;
        }
        int currentSpent = usedMoney;
        int currentRevenue = 0;

        // 2. 결제 진행
        money -= usedMoney;        // 현재 잔액 차감
        totalSpent += usedMoney;   // 총 지출액에 누적

        // 3. 수익 처리 (요청한 메뉴와 일치하는지 확인 후 돈 지급)
        // 현재는 Inspector에 있는 currentRequestName을 사용 (나중에 손님 시스템과 연동 필요)
        int revenue = CalculateSalesRevenue(currentRequestName);

        if (revenue > 0)
        {
            dailyRevenue += revenue;
            totalRevenue += revenue; 
            Debug.Log($"[수익] 메뉴 완성! {revenue} 골드 획득. (요청: {currentRequestName})");
            // 돈이 추가되는것을 보여줌
            money += revenue;
            currentRevenue = revenue;
            //money_effect.text = "+" + revenue.ToString();
            //money_effect.color = Color.green;
            //// 투명도 1f -> 0f로 돌아가기 
            //StartCoroutine(FadeInAndOutCoroutine());

        }
        else
        {
            Debug.Log("[수익] 요청한 메뉴가 아니므로 수익이 발생하지 않았습니다.");
        }

        // 4. UI 및 저장
        mineral.text = money.ToString();

        Debug.Log($"결제 성공! 지불액: {usedMoney} / 남은 금액 : {money}/ 총 누적 지출: {totalSpent}");
        save_Spent = currentSpent;
        // 4. 다음 결제를 위해 사용된 금액(장바구니 금액) 초기화
        // 리스트(selectedNames)는 유지되지만, 비용은 지불했으므로 0으로 만듦
        usedMoney = 0;
        StartCoroutine(SequenceTransactionEffect(currentSpent, currentRevenue));

        return true;
    }

    // 지출과 수익을 순서대로 보여주는 코루틴
    IEnumerator SequenceTransactionEffect(int spent, int revenue)
    {
        // 1. 지출 이펙트 (빨강)
        if (spent > 0)
        {
            money_effect.text = "-" + spent.ToString();
            money_effect.color = Color.red;

            // 페이드 인/아웃 실행하고 끝날 때까지 대기
            yield return StartCoroutine(FadeEffectProcess());
        }

        // 2. 수익이 있다면 지출 이펙트 끝난 후 실행 (초록)
        if (revenue > 0)
        {
            money_effect.text = "+" + revenue.ToString();
            money_effect.color = Color.green;

            yield return StartCoroutine(FadeEffectProcess());
        }
    }
    public void SaveDailyData() // 하루가 끝날 때 저장!!!!
    {
        // 1. 모아둔 일일 수익을 플레이어 돈에 합산
        //money += dailyRevenue;
        // 2. UI 갱신 (정산된 금액 표시)
        mineral.text = money.ToString();
        


        // 3. 데이터 저장
        PlayerPrefs.SetInt("Gold", money);
        PlayerPrefs.SetInt("TotalSpent", totalSpent);
        PlayerPrefs.SetInt("TotalRevenue", totalRevenue); // 수익도 저장
        PlayerPrefs.Save();
        Debug.Log($"[일차 마감] 총 {dailyRevenue} 골드 수익 정산 완료. 현재 자산: {money}");
    }

    // 페이드 인/아웃 로직 하나만 남김
    IEnumerator FadeEffectProcess()
    {
        Color c = money_effect.color;
        c.a = 0f;
        money_effect.color = c;

        float speed = 3f; // 속도 조절

        // Fade In
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * speed;
            c.a = Mathf.Lerp(0f, 1f, timer);
            money_effect.color = c;
            yield return null;
        }
        c.a = 1f;
        money_effect.color = c;

        yield return new WaitForSeconds(0.2f); // 잠깐 대기

        // Fade Out
        timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * speed;
            c.a = Mathf.Lerp(1f, 0f, timer);
            money_effect.color = c;
            yield return null;
        }
        c.a = 0f;
        money_effect.color = c;
    }
}