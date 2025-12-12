using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

public class StageManage : MonoBehaviour
{
    public readonly string ADDRESS = "https://docs.google.com/spreadsheets/d/1SUvkrIiBEfRl-J2_887gtT8MJNJgfKvjd-QEr4NglY0";
    public readonly string RANGE = "A2:E";
    public readonly long SHEET_ID = 488429256;
    public int now_stage = 1;
    public int main_sand_cnt;
    public int cookie_cnt;
    public int drink_cnt;
    public MenuManager menu;

    Dictionary<string, int[]> stageDict = new Dictionary<string, int[]>();

    [System.Serializable]
    public class stage_manage
    {
        public string name;
        public int[] stage;

    }

    void Start()
    {
        
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
        List<stage_manage> data = GetDatas<stage_manage>(www.downloadHandler.text);

        foreach (var d in data)
        {
            Debug.Log($"name={d.name}, stage=[{d.stage[now_stage-1]}]");
            stageDict[d.name] = d.stage;
        }
        main_sand_cnt = stageDict["main"][now_stage-1];
        cookie_cnt = stageDict["cookie"][now_stage - 1];
        drink_cnt = stageDict["drink"][now_stage - 1];

        menu.OnStageDataLoaded(main_sand_cnt, cookie_cnt, drink_cnt);
        print(main_sand_cnt);
        print(cookie_cnt);
        print(drink_cnt);
    }

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

                else if (type == typeof(string))
                    fields[i].SetValue(data, datas[i]);

                else if (type == typeof(int[]))
                {
                    // 현재 필드 이후의 모든 컬럼을 int[]로 변환
                    int[] arr = new int[datas.Length - i];
                    for (int j = 0; j < arr.Length; j++)
                        arr[j] = string.IsNullOrEmpty(datas[i + j]) ? 0 : int.Parse(datas[i + j]);

                    fields[i].SetValue(data, arr);
                    break; // 배열은 마지막 필드라 이후는 필요 없음
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"SpreadSheet Error : {e.Message}");
            }
        }

        return (T)data;
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

    // 현재 스테이지에 맞게 각 행(메인메뉴,쿠키,음료 개수)에 접근

}
