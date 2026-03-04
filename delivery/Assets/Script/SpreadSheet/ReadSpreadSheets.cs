using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ReadSpreadSheets : MonoBehaviour
{

    public static ReadSpreadSheets Instance;

    [Header("Google Sheet Settings")]
    public readonly string ADDRESS = "https://docs.google.com/spreadsheets/d/1SUvkrIiBEfRl-J2_887gtT8MJNJgfKvjd-QEr4NglY0";
    public readonly string RANGE = "A2:C";
    public readonly long SHEET_ID_MATERIAL = 0;          // 재료 시트
    public readonly long SHEET_ID_SALES = 899576211;     // 판매 가격 시트 (2열: food_name, price)

    [Header("Data Lists")]
    public List<Food_material> materials;  // 재료 리스트
    public List<Food_product> productPrices; // 완성품 가격 리스트
    public Dictionary<string, string> menuVocab = new Dictionary<string, string>();

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
    public int dailySpent = 0;

    

    private bool dataReady = false;
    private string last_name = string.Empty;

    private Button timeManager;

    [Header("별점 관리")]
    public bool is_menu_incorrect = false;
    public int menu_incorrect = 0;
    public int menu_num = 0;
    public int stand_money = 300;
    public List<string> tmp_selected = new List<string>();
    public int delivery_incorrect = 1;

    [Header("주문 관리")]
    public List<string> CurrentRequest_Include = new List<string>();
    public List<string> CurrentRequest_Exclude = new List<string>();
    // [테스트용] 현재 손님이 요청한 메뉴 이름 (인스펙터에서 직접 입력하여 테스트)
    public List<string> currentRequestName = new List<string> { };

    [System.Serializable]
    public class Food_material
    {
        public string name;
        public string kor_name;
        public int cost;
    }

    [System.Serializable]
    public class Food_product // 완성품 (수익)
    {
        public string food_name;
        public string kor_name;
        public int price;
    }
    public void ClearRequests()
    {
        // 리스트가 null일 경우를 대비해 null 체크를 하거나, 
        // Awake에서 이미 초기화되었다면 바로 Clear()만 해도 됩니다.

        if (CurrentRequest_Include != null) CurrentRequest_Include.Clear();
        if (CurrentRequest_Exclude != null) CurrentRequest_Exclude.Clear();
        if (currentRequestName != null) currentRequestName.Clear();

        Debug.Log("이전 주문 데이터를 모두 초기화했습니다.");
    }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
              DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // [핵심 수정] new List로 새로 만들지 말고, null 체크 후 Clear()로 청소만 합니다.
        // 이렇게 해야 인스펙터에 보이는 리스트와 코드가 쓰는 리스트가 일치하게 됩니다.

        if (currentRequestName == null) currentRequestName = new List<string>();
        currentRequestName.Clear();

        if (CurrentRequest_Include == null) CurrentRequest_Include = new List<string>();
        CurrentRequest_Include.Clear();

        if (CurrentRequest_Exclude == null) CurrentRequest_Exclude = new List<string>();
        CurrentRequest_Exclude.Clear();

        // 다른 리스트들도 안전하게 초기화
        if (CheckMenu.selectedNames == null) CheckMenu.selectedNames = new List<string>();
        CheckMenu.selectedNames.Clear();

        if (tmp_selected == null) tmp_selected = new List<string>();
        tmp_selected.Clear();

        // 일일 데이터 초기화
        dailyRevenue = 0;
        dailySpent = 0;
        usedMoney = 0;
        save_Spent = 0;

        Debug.Log(">> 모든 데이터 리스트가 깨끗하게 초기화되었습니다.");
    }

    // ReadSpreadSheets 클래스 안에 추가하세요.
    public void ResetGameData()
    {
        // 1. 디스크에 저장된 데이터 삭제
        PlayerPrefs.DeleteKey("Gold");
        PlayerPrefs.DeleteKey("TotalSpent");
        PlayerPrefs.DeleteKey("TotalRevenue");
        PlayerPrefs.Save(); // 즉시 반영

        // 2. 현재 메모리에 떠있는 변수 값들도 초기화
        money = 1000; // 초기 자금
        totalSpent = 0;
        totalRevenue = 0;
        dailyRevenue = 0;
        dailySpent = 0;
        usedMoney = 0;
        menu_num = 0;
        menu_incorrect = 0;
        delivery_incorrect = 0;
        if (currentRequestName != null) currentRequestName.Clear();
        if (CheckMenu.selectedNames != null) CheckMenu.selectedNames.Clear();
        if (tmp_selected != null) tmp_selected.Clear();
        // UI 갱신 (만약 현재 씬에 있다면)
        if (mineral != null) mineral.text = money.ToString();
        
        Debug.Log(">> 모든 데이터가 초기화되었습니다.");
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
        //PlayerPrefs.DeleteKey("TotalRevenue");
        // 저장된 돈 불러오기 (없으면 기본값 1000으로 시작한다고 가정 - 값 조정 필요)
        money = PlayerPrefs.GetInt("Gold", 1000);
        totalSpent = PlayerPrefs.GetInt("TotalSpent", 0); // 누적 지출 불러오기
        totalRevenue = PlayerPrefs.GetInt("TotalRevenue", 0); // 수익 불러오기

        mineral.text = money.ToString();

        StartCoroutine(LoadAllData());
    }
    public static string GetTSVAddress(string address, string range, long sheetId)
    {
        return $"{address}/export?format=tsv&range={range}&gid={sheetId}&dummy={System.DateTime.Now.Ticks}";
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

        // 3. 로드된 데이터를 바탕으로 사전(Dictionary) 만들기
        MakeDictionary();

        Debug.Log("데이터 로드 및 한글 매핑 완료");
        dataReady = true;
    }
    // 데이터를 딕셔너리에 몰아넣는 함수
    void MakeDictionary()
    {
        menuVocab.Clear();

        // 1) 재료 이름 등록 (예: "피클" -> "pickle")
        foreach (var mat in materials)
        {
            if (!menuVocab.ContainsKey(mat.kor_name))
            {
                menuVocab.Add(mat.kor_name, mat.name);
            }
        }

        // 2) 완성품 이름 등록 (예: "크라켄 샌드위치" -> "kraken_sand")
        // *주의: 판매 시트(Sheet_ID_SALES)에도 B열에 한글 이름을 추가해야 작동합니다.
        foreach (var prod in productPrices)
        {
            if (!menuVocab.ContainsKey(prod.kor_name))
            {
                menuVocab.Add(prod.kor_name, prod.food_name);
            }
        }

        // 3) 시트에 없는 특수 단어들만 수동으로 추가 (필요하다면)
        // 예: "빼고", "없이" 같은 문법적 단어는 시트에 없다면 여기서 추가
        //menuVocab.TryAdd("빼고", "EXCLUDE_KEYWORD"); 
    }
    T GetData<T>(string[] datas) // TSV 한 행을 T타입 객체로 변환하는 함수 
    {
        object data = Activator.CreateInstance(typeof(T));
        FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        for (int i = 0; i < fields.Length; i++)
        {
            try
            {
                // [방어 1] 데이터 개수보다 필드가 많으면 패스 (인덱스 초과 방지)
                if (i >= datas.Length) continue;

                // [방어 2: 핵심!] 앞뒤 공백 제거 후 빈 값 체크
                // datas[i]를 직접 쓰지 않고, 깨끗하게 씻은 cellData를 씁니다.
                string cellData = datas[i].Trim();
                if (string.IsNullOrEmpty(cellData)) continue;

                Type type = fields[i].FieldType;

                if (string.IsNullOrEmpty(datas[i]))
                    continue;

                // 아래부터는 datas[i] 대신 cellData를 사용합니다!
                if (type == typeof(int))
                    fields[i].SetValue(data, int.Parse(cellData));
                else if (type == typeof(float))
                    fields[i].SetValue(data, float.Parse(cellData));
                else if (type == typeof(bool))
                    fields[i].SetValue(data, bool.Parse(cellData));
                else if (type == typeof(string))
                    fields[i].SetValue(data, cellData);
                else
                    fields[i].SetValue(data, Enum.Parse(type, cellData));
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
    private int CalculateSalesRevenue(List<string> requestNameList)
    {
        int totalCalculatedRevenue = 0;

        // 1. 계산을 위해 선택된 재료 리스트를 복사해옵니다. (원본 리스트 훼손 방지)
        // 리스트를 복사하지 않으면 RemoveAt을 할 때 실제 선택된 재료가 사라져버립니다.
        List<string> tempMyIngredients = new List<string>(CheckMenu.selectedNames);

        // 2. 요청 리스트 순회
        foreach (string reqName in requestNameList)
        {
            if (string.IsNullOrEmpty(reqName)) continue;

            // 가격 정보 찾기
            var product = productPrices.Find(p => p.food_name == reqName);
            if (product == null) continue;

            // 3. 내 재료 중에 이 요청에 맞는 게 있는지 찾기
            // (예: reqName="kraken_sand" 일 때, tempMyIngredients에 "kraken"이 있는지)
            int foundIndex = -1;

            for (int i = 0; i < tempMyIngredients.Count; i++)
            {
                if (reqName.Contains(tempMyIngredients[i]))
                {
                    foundIndex = i;
                    break; // 찾았으면 루프 탈출
                }
            }

            // 4. 매칭되는 재료가 있다면?
            if (foundIndex != -1)
            {
                // 가격 더하기
                totalCalculatedRevenue += product.price;

                // [중요] 사용된 재료는 임시 리스트에서 제거하여 중복 계산 방지
                // (예: 참치 샌드위치 2개 주문인데 참치 1개만 있을 경우, 1개만 계산되도록)
                tempMyIngredients.RemoveAt(foundIndex);

                Debug.Log($"[수익 미리보기] {reqName} 매칭됨 (+{product.price})");
            }
            else
            {
                // 매칭 안 됨 -> 그냥 넘어감 (여기서 오답 처리 하지 않음!)
            }
        }

        return totalCalculatedRevenue;
    }

    // 요리가 끝난 시점(Finish 버튼 클릭)에 최종 점검 및 수익 계산을 하는 함수

    public int CheckFinalResult()
    {
        is_menu_incorrect = false;
        int finalRevenue = 0;

        // 1. 정답지 설정 (손님이 넣어달라고 한 것들)
        List<string> requestList = new List<string>(CurrentRequest_Include);

        // 2. 내 제출안 설정 (플레이어가 클릭한 것들)
        List<string> myIngredients = new List<string>(currentRequestName);

        // [검사 1] 제외 리스트(Exclude) 검사: 내가 선택한 것에 '빼야 할 것'이 들어있는가?
        foreach (string forbidden in CurrentRequest_Exclude)
        {
            if (myIngredients.Contains(forbidden))
            {
                Debug.Log($"[오답] 제외해야 할 재료({forbidden})를 넣었습니다.");
                is_menu_incorrect = true;
                break;
            }
        }
        if (is_menu_incorrect) return 0;

        // [검사 2] 포함 리스트(Include) 검사: 손님이 주문한 게 내 제출안에 다 있는가?
        for (int i = requestList.Count - 1; i >= 0; i--)
        {
            string req = requestList[i];
            bool found = false;

            for (int j = myIngredients.Count - 1; j >= 0; j--)
            {
                if (myIngredients[j] == req)
                {
                    // 수익 계산 (완성품 ID일 때만 가격 추가)
                    var product = productPrices.Find(p => p.food_name == req);
                    if (product != null) finalRevenue += product.price;

                    myIngredients.RemoveAt(j);
                    requestList.RemoveAt(i);
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                Debug.Log($"[오답] 필요한 재료/메뉴({req})가 없습니다.");
                is_menu_incorrect = true;
            }
        }

        // [검사 3] 불필요한 추가 재료 검사: 주문하지 않은 게 남아있는가?
        if (myIngredients.Count > 0)
        {
            Debug.Log($"[오답] 주문하지 않은 재료가 더 들어있습니다: {string.Join(", ", myIngredients)}");
            is_menu_incorrect = true;
        }

        // 최종 결과 적용
        if (is_menu_incorrect)
        {
            menu_incorrect++;
            Debug.Log(">> 최종 결과: 오답");
            return 0;
        }
        else
        {
            money += finalRevenue;
            mineral.text = money.ToString();
            dailyRevenue += finalRevenue;
            totalRevenue += finalRevenue;
            Debug.Log($">> 최종 결과: 정답! (+{finalRevenue} Gold)");
            StartCoroutine(DailyTransactionEffect(finalRevenue));
            return finalRevenue;
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

            // 돈 부족 시 마지막 재료 취소 로직
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

        // 2. 결제 진행
        money -= usedMoney;
        dailySpent += usedMoney;
        totalSpent += usedMoney;
        print(string.Join(", ", currentRequestName));
        
        // 4. UI 및 저장
        mineral.text = money.ToString();

        Debug.Log($"결제 성공! 지불액: {usedMoney} / 남은 금액 : {money}/ 총 누적 지출: {totalSpent}");
        save_Spent = currentSpent;

        // 초기화
        usedMoney = 0;
        if (this.gameObject != null && this.gameObject.activeInHierarchy)
        {
            StartCoroutine(SequenceTransactionEffect(currentSpent));
        }

        return true;
    }

    // 지출과 수익을 순서대로 보여주는 코루틴
    IEnumerator SequenceTransactionEffect(int spent)
    {
        // 1. 지출 이펙트 (빨강)
        if (spent > 0)
        {
            money_effect.text = "-" + spent.ToString();
            money_effect.color = Color.red;

            // 페이드 인/아웃 실행하고 끝날 때까지 대기
            yield return StartCoroutine(FadeEffectProcess());
        }

    }
    IEnumerator DailyTransactionEffect(int dailyRevenue)
    {
        // 1. 수익이 있다면 지출 이펙트 끝난 후 실행 (초록)
        if (dailyRevenue > 0)
        {
            money_effect.text = "+" + dailyRevenue.ToString();
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
        //StartCoroutine(DailyTransactionEffect(dailyRevenue)); 


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