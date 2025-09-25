using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class FoodManager : MonoBehaviour
{
    private List<string> sands = new List<string>();
    private List<string> drinks = new List<string>();
    private List<string> cookies = new List<string>();

    [Header("Google Sheet CSV URL")]
    public string sheetUrl = "https://docs.google.com/spreadsheets/d/1SUvkrIiBEfRl-J2_887gtT8MJNJgfKvjd-QEr4NglY0/edit?gid=899576211#gid=899576211";

    void Start()
    {
        StartCoroutine(LoadFoodDataFromGoogle());
    }

    IEnumerator LoadFoodDataFromGoogle()
    {
        UnityWebRequest www = UnityWebRequest.Get(sheetUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("구글 시트 불러오기 실패: " + www.error);
        }
        else
        {
            string csvData = www.downloadHandler.text;
            ParseCSV(csvData);
        }
    }

    void ParseCSV(string csvData)
    {
        sands.Clear();
        drinks.Clear();
        cookies.Clear();

        string[] lines = csvData.Split('\n');

        for (int i = 1; i < lines.Length; i++) // 첫 줄은 header라서 1부터 시작
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] cols = line.Split(',');
            if (cols.Length >= 3)
            {
                sands.Add(cols[0]);
                drinks.Add(cols[1]);
                cookies.Add(cols[2]);
            }
        }

        Debug.Log($"로드 완료: 샌드 {sands.Count}, 음료 {drinks.Count}, 쿠키 {cookies.Count}");
    }

    public string GetRandomOrder()
    {
        if (sands.Count == 0 || drinks.Count == 0 || cookies.Count == 0)
            return "데이터 없음";

        string sand = sands[Random.Range(0, sands.Count)];
        string drink = drinks[Random.Range(0, drinks.Count)];
        string cookie = cookies[Random.Range(0, cookies.Count)];

        return $"샌드: {sand}, 음료: {drink}, 쿠키: {cookie}";
    }
}