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
    public readonly string RANGE = "A2:C";
    public readonly long SHEET_ID = 899576211;
    public List<Food_material> materials;
    public TextMeshProUGUI mineral;

    private bool dataReady = false;
    public bool DataReady => dataReady; // 외부 접근용 프로퍼티

    [System.Serializable]
    public class Food_material
    {
        public string name;
        public int cost;
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

        Debug.Log(www.downloadHandler.text);
        materials = GetDatas<Food_material>(www.downloadHandler.text);
        dataReady = true;
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
            if (string.IsNullOrWhiteSpace(element))
                continue;

            string[] datas = element.Split('\t');
            returnList.Add(GetData<T>(datas));
        }
        return returnList;
    }
}
