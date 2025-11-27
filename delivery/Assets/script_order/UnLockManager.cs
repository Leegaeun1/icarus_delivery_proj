using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnlockState
{
    public int stage = 1;
    // 메뉴 해금 “종류 수”
    public int sUnlocked = 3; // Sandwich
    public int dUnlocked = 1; // Drink
    public int cUnlocked = 1; // Cookie
    // 주소 해금: 각 차원에서 “사용 가능한 후보 개수”
    public int zonesUnlocked = 2;
    public int complexesUnlocked = 1;
    public int buildingsUnlocked = 3;
    public int floorsUnlocked = 3;
    public int roomsUnlocked = 3;
}

public class UnLockManager : MonoBehaviour
{
    public static UnLockManager I { get; private set; }
    private const string Key = "unlock_state_v2";
    public UnlockState State { get; private set; } = new UnlockState();

    [Header("메뉴 최대치 (엑셀에 맞춰 자동 캡)")]
    public int maxS = 5, maxD = 3, maxC = 3;

    [Header("주소 차원 최대치 (엑셀 길이에 맞춰 자동 캡)")]
    public int maxZones = 6;      // A..F
    public int maxComplexes = 5;  // 1..5
    public int maxBuildings = 9;  // 9개 후보
    public int maxFloors = 9;
    public int maxRooms = 9;

    [Header("증가량 (홀수=샌드 업, 짝수=음료+쿠키 업)")]
    public int incSandwich = 1;
    public int incDrink = 1;
    public int incCookie = 1;

    [Header("주소 증가량 (홀수 스테이지)")]
    public int incBuildingsOdd = 1;
    public int incFloorsOdd = 1;
    public int incRoomsOdd = 1;

    [Header("주소 증가량 (짝수 스테이지)")]
    public int incZonesEven = 1;
    public int incComplexesEven = 1;

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this; DontDestroyOnLoad(gameObject);
        Load();
    }

    public void AdvanceStage()
    {
        bool odd = (State.stage % 2 == 1);

        if (odd)
        {
            State.sUnlocked = Mathf.Min(State.sUnlocked + incSandwich, maxS);
            State.buildingsUnlocked = Mathf.Min(State.buildingsUnlocked + incBuildingsOdd, maxBuildings);
            State.floorsUnlocked = Mathf.Min(State.floorsUnlocked + incFloorsOdd, maxFloors);
            State.roomsUnlocked = Mathf.Min(State.roomsUnlocked + incRoomsOdd, maxRooms);
        }
        else
        {
            State.dUnlocked = Mathf.Min(State.dUnlocked + incDrink, maxD);
            State.cUnlocked = Mathf.Min(State.cUnlocked + incCookie, maxC);
            State.zonesUnlocked = Mathf.Min(State.zonesUnlocked + incZonesEven, maxZones);
            State.complexesUnlocked = Mathf.Min(State.complexesUnlocked + incComplexesEven, maxComplexes);
        }

        State.stage += 1;
        Save();
    }

    public void Save() { PlayerPrefs.SetString(Key, JsonUtility.ToJson(State)); }
    public void Load()
    {
        if (PlayerPrefs.HasKey(Key))
        {
            State = JsonUtility.FromJson<UnlockState>(PlayerPrefs.GetString(Key));
        }
        else
        {
            Save();
        }
    }
}