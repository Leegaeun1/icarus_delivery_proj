using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class ReadSpreadSheets : MonoBehaviour
{
    public readonly string ADDRESS = "https://docs.google.com/spreadsheets/d/1SUvkrIiBEfRl-J2_887gtT8MJNJgfKvjd-QEr4NglY0";
    public readonly string RANGE = "A2:B";
    public readonly long SHEET_ID = 0;
    public List<Food_material> materials;
    public TextMeshProUGUI mineral;

    public int money;      // 현재 보유한 돈
    public int usedMoney = 0; // 선택한 재료들의 총 비용 (장바구니 금액)

    // 총 지출액과 총 수익을 저장할 변수
    public int totalSpent = 0;
    // public int totalRevenue = 0; // 필요시 총 수익도 추가 가능

    private bool dataReady = false; //  로딩 완료 플래그

    private string last_name = string.Empty;

    [System.Serializable]
    public class Food_material
    {
        public string name;
        public int cost;
    }

    void Start()
    {

        if (mineral == null)
        {
            Debug.Log("[ReadSpreadSheet] 미네랄 텍스트가 등록되어있지 않습니다.");
        }

        // 개발 테스트용 초기화
        //PlayerPrefs.DeleteKey("Gold");
        //PlayerPrefs.DeleteKey("TotalSpent");

        // 저장된 돈 불러오기 (없으면 기본값 1000으로 시작한다고 가정 - 값 조정 필요)
        money = PlayerPrefs.GetInt("Gold", 20);
        totalSpent = PlayerPrefs.GetInt("TotalSpent", 0); // 누적 지출 불러오기

        mineral.text = money.ToString();

        // 게임 시작 시 장바구니 리스트 초기화 (이전 저장 내역 무시)
        CheckMenu.selectedNames.Clear();
        usedMoney = 0;

        StartCoroutine(LoadData());
    }

    public static string GetTSVAddress(string address, string range, long sheetId)
    {
        return $"{address}/export?format=tsv&range={range}&gid={sheetId}";
    }

    private IEnumerator LoadData()
    {
        UnityWebRequest www = UnityWebRequest.Get(GetTSVAddress(ADDRESS, RANGE, SHEET_ID));
        yield return www.SendWebRequest();

        Debug.Log(www.downloadHandler.text);
        materials = GetDatas<Food_material>(www.downloadHandler.text);
        dataReady = true; // 데이터 준비 완료 

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

        // 2. 결제 진행
        money -= usedMoney;        // 현재 잔액 차감
        totalSpent += usedMoney;   // 총 지출액에 누적

        // 3. UI 및 저장
        mineral.text = money.ToString();

        PlayerPrefs.SetInt("Gold", money);
        //PlayerPrefs.SetInt("TotalSpent", totalSpent); // 지출 내역 저장
        PlayerPrefs.Save();

        Debug.Log($"결제 성공! 지불액: {usedMoney} / 남은 금액 : {money}/ 총 누적 지출: {totalSpent}");

        // 4. 다음 결제를 위해 사용된 금액(장바구니 금액) 초기화
        // 리스트(selectedNames)는 유지되지만, 비용은 지불했으므로 0으로 만듦
        usedMoney = 0;

        return true;
    }
    public void SaveDailyData()
    {
        //PlayerPrefs.SetInt("Gold", money);
        PlayerPrefs.SetInt("TotalSpent", totalSpent);
        PlayerPrefs.Save();
        Debug.Log("일차 종료. 데이터 저장 완료.");
    }
}