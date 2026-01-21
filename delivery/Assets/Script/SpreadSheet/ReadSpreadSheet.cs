using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class ReadSpreadSheet : MonoBehaviour
{
    public readonly string ADDRESS = "https://docs.google.com/spreadsheets/d/1SUvkrIiBEfRl-J2_887gtT8MJNJgfKvjd-QEr4NglY0";
    public readonly string RANGE = "A2:B";
    public readonly long SHEET_ID = 0;
    public List<Food_material> materials;
    public TextMeshProUGUI mineral;
    

    private bool dataReady = false;
    public bool DataReady => dataReady;//  로딩 완료 플래그


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
        // 저장된 돈 불러오기 (없으면 기본값 1000으로 시작한다고 가정)
        int savedGold = PlayerPrefs.GetInt("Gold", 1000);
        mineral.text = savedGold.ToString();

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

    public void OnIngredientToggled(string ingredientName, bool isSelected)
    {
        if (!dataReady)
        {
            Debug.LogWarning("[ReadSpreadSheet] 데이터가 아직 로드되지 않았습니다.");
            return;
        }
        if (mineral == null)
        {
            Debug.LogError("[ReadSpreadSheet] mineral(TextMeshProUGUI)이 없습니다.");
            return;
        }
        if (!int.TryParse(mineral.text, out int haveMoney))
        {
            Debug.LogError($"[ReadSpreadSheet] mineral 텍스트를 숫자로 변환할 수 없습니다: '{mineral.text}'");
            return;
        }

        // 재료 찾기 (정확히 이름이 같은 항목)
        var material = materials.Find(m => m.name == ingredientName);
        if (material == null)
        {
            Debug.LogError($"[ReadSpreadSheet] '{ingredientName}' 재료를 materials에서 찾을 수 없습니다.");
            return;
        }

        if (isSelected)
        {
            if (!CheckMenu.selectedNames.Contains(ingredientName))
            {
                CheckMenu.selectedNames.Add(ingredientName);
                haveMoney -= material.cost;

            }
                
        }
        else
        {
            CheckMenu.selectedNames.Remove(ingredientName);
            haveMoney += material.cost;
        }

        mineral.text = haveMoney.ToString();
        PlayerPrefs.SetInt("Gold", haveMoney);
        PlayerPrefs.Save();
        Debug.Log("선택된 목록: " + string.Join(", ", CheckMenu.selectedNames));
        
    }

}
