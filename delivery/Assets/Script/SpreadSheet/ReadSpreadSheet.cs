using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class ReadSpreadSheet : MonoBehaviour
{
    [Header("Google Sheet Settings")]
    // readonly를 제거하여 인스펙터에서 수정이 가능합니다.
    public string ADDRESS = "https://docs.google.com/spreadsheets/d/1SUvkrIiBEfRl-J2_887gtT8MJNJgfKvjd-QEr4NglY0";
    public string RANGE = "A2:B";
    public long SHEET_ID = 0; // 인스펙터에서 이 값을 변경하여 다른 시트 탭을 읽을 수 있습니다.

    [Header("UI & Data")]
    public List<Food_material> materials;
    public TextMeshProUGUI mineral;

    private bool dataReady = false;
    public bool DataReady => dataReady; // 외부에서 로딩 상태를 확인할 수 있는 프로퍼티

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

        // 저장된 돈 불러오기
        int savedGold = PlayerPrefs.GetInt("Gold", 1000);
        mineral.text = savedGold.ToString();

        // 인스펙터에 입력된 SHEET_ID를 사용하여 데이터 로드 시작
        StartCoroutine(LoadData());
    }

    // 주소 생성 시 현재 클래스에 설정된 변수값들을 조합합니다.
    public string GetTSVAddress()
    {
        return $"{ADDRESS}/export?format=tsv&range={RANGE}&gid={SHEET_ID}";
    }

    private IEnumerator LoadData()
    {
        // 최신화된 주소를 가져옵니다.
        string fullURL = GetTSVAddress();
        Debug.Log($"[ReadSpreadSheet] 요청 주소: {fullURL}");

        UnityWebRequest www = UnityWebRequest.Get(fullURL);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(www.downloadHandler.text);
            materials = GetDatas<Food_material>(www.downloadHandler.text);
            dataReady = true; // 데이터 준비 완료
        }
        else
        {
            Debug.LogError($"[ReadSpreadSheet] 데이터 로드 실패: {www.error}");
        }
    }

    // TSV 한 행을 객체로 변환하는 리플렉션 로직
    T GetData<T>(string[] datas)
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

    // 전체 데이터를 리스트로 변환
    List<T> GetDatas<T>(string data)
    {
        List<T> returnList = new List<T>();
        string[] splitedData = data.Split('\n');

        foreach (string element in splitedData)
        {
            if (string.IsNullOrWhiteSpace(element)) continue;
            string[] datas = element.Split('\t');
            returnList.Add(GetData<T>(datas));
        }
        return returnList;
    }

    // 재료 선택 시 돈 계산 및 저장 로직
    public void OnIngredientToggled(string ingredientName, bool isSelected)
    {
        if (!dataReady) return;
        if (mineral == null) return;

        if (!int.TryParse(mineral.text, out int haveMoney)) return;

        var material = materials.Find(m => m.name == ingredientName);
        if (material == null) return;

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
    }
}