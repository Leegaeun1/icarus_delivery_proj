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

    private bool dataReady = false; //  로딩 완료 플래그


    [System.Serializable]
    public class Food_material
    {
        public string name;
        public int cost;
        
    }
    
    void Start()
    {

        PlayerPrefs.DeleteKey("SavedDeck");

        if (mineral == null)
        {
            Debug.Log("[ReadSpreadSheet] 미네랄 텍스트가 등록되어있지 않습니다.");
        }
        PlayerPrefs.DeleteKey("Gold");
        // 저장된 돈 불러오기 (없으면 기본값 1000으로 시작한다고 가정)
        money = PlayerPrefs.GetInt("Gold", 20);
        mineral.text = money.ToString();

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

        LoadSavedDeck();
    }
    // 마지막으로 저장된 덱을 불러오고 비용 계산
    public void LoadSavedDeck()
    {
        // 1. 저장된 문자열 가져오기
        string savedString = PlayerPrefs.GetString("SavedDeck", "");

        if (!string.IsNullOrEmpty(savedString))
        {
            // 2. 리스트로 변환하여 static 리스트에 복원
            string[] savedItems = savedString.Split(',');

            CheckMenu.selectedNames.Clear(); // 불러오기 전에 현재 상태 초기화 (중복 방지)
            CheckMenu.selectedNames.AddRange(savedItems);

            // 3. 복원된 아이템들의 가격 합산 (usedMoney 복구)
            usedMoney = 0;
            foreach (string itemName in CheckMenu.selectedNames)
            {
                var mat = materials.Find(m => m.name == itemName);
                if (mat != null) usedMoney += mat.cost;
            }

            Debug.Log($"[로드 완료] 불러온 목록: {savedString} / 총 비용: {usedMoney}");
        }
    }
    T GetData<T>(string[] datas) // TSV 한 행을 T타입 객체로 변환하는 함수 
    {
        object data = Activator.CreateInstance(typeof(T)); // T 타입의 기본 생성자로 객체를 하나 만듬

        // 클래스에 있는 변수들을 순서대로 저장한 배열. 모든 필드를 배열로 받음 
        FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        // GetFields(..) : T 타입 안에 있는 필드 목록을 리플렉션으로 가져옴 

        for (int i = 0; i < fields.Length; i++)
        {
            try
            {
                // 현재 i번째 필드의 자료형 가져옴
                Type type = fields[i].FieldType;

                if (string.IsNullOrEmpty(datas[i])) //빈 문자열일때 건너뜀
                    continue;

                // 변수에 맞는 자료형으로 파싱해서 넣는다.
                // 만든 data 객체의 i번째 필드에 값을 대입함 
                if (type == typeof(int))
                    fields[i].SetValue(data, int.Parse(datas[i]));

                else if (type == typeof(float))
                    fields[i].SetValue(data, float.Parse(datas[i]));

                else if (type == typeof(bool))
                    fields[i].SetValue(data, bool.Parse(datas[i]));

                else if (type == typeof(string))
                    fields[i].SetValue(data, datas[i]);

                // enum
                else
                    fields[i].SetValue(data, Enum.Parse(type, datas[i]));
            }

            catch (Exception e)
            {
                Debug.LogError($"SpreadSheet Error : {e.Message}");
            }
        }

        return (T)data; // 원래 타입 T로 캐스팅해서 반환 
    }

    List<T> GetDatas<T>(string data) // 데이터를 리스트에 모아주는 함수
    {
        List<T> returnList = new List<T>(); // 빈 리스트 
        string[] splitedData = data.Split('\n'); // 각각의 데이터들 

        foreach (string element in splitedData)
        {
            string[] datas = element.Split('\t'); // mustard 5 이런식으로 탭 기준으로 나눠줌 
            returnList.Add(GetData<T>(datas)); // 리스트에 추가해줌
        }
        return returnList;
    }

    // 여기서는 비용 계산만 하고 실제 돈은 차감하지 않습니다.
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
        Debug.Log($"현재 선택 총액: {usedMoney}");
    }

    public bool ApplyPurchase()
    {
        if (money < usedMoney)
        {
            Debug.LogWarning(" 잔액 부족! 결제 취소");

            LoadSavedDeck(); 
            return false;
        }

        money -= usedMoney;

        mineral.text = money.ToString();
        PlayerPrefs.SetInt("Gold", money);

        // 구매 성공 시 현재 덱(목록)을 저장함
        string dataToSave = string.Join(",", CheckMenu.selectedNames);
        PlayerPrefs.SetString("SavedDeck", dataToSave);

        PlayerPrefs.Save();

        Debug.Log(" 결제 성공 및 덱 저장 완료!");
        return true;
    }

}
