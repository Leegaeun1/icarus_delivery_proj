using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI 연결")]
    public GameObject settingWindow;
    public Button openButton;

    public GameObject boardWindow;
    public Button cookBoardButton;

    public AudioSource audio_source;
    private float startTime;

    public float ElapsedTime => Time.time - startTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            startTime = Time.time;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 1. 이 스크립트가 활성화될 때 '씬 로드 이벤트'에 등록
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // 2. 비활성화되거나 파괴될 때 이벤트 해제
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 3. 씬이 로드될 때마다 실행되는 함수
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindAndSetupUI(); // UI 다시 찾기 실행
    }

    private void Start()
    {
        FindAndSetupUI();
    }

    private void Update()
    {
        // 마우스 클릭 소리 (오디오 소스가 있을 때만)
        if (Input.GetMouseButtonUp(0) && audio_source != null)
        {
            audio_source.Play();
        }
    }

    // UI 찾기 및 연결 통합 관리
    void FindAndSetupUI()
    {
        // 1. Setting UI 연결
        SetupUIPair(
            btnName: "setting",
            panelName: "Setting_UI",
            ref openButton,
            ref settingWindow
        );

        // 2. Board UI 연결
        SetupUIPair(
            btnName: "CookBoard",
            panelName: "Board_Panel",
            ref cookBoardButton,
            ref boardWindow
        );

        // 3. 오디오 소스 연결
        if (audio_source == null)
        {
            GameObject clickObj = GameObject.Find("click");
            if (clickObj != null) audio_source = clickObj.GetComponent<AudioSource>();
        }
    }
    // 중복 제거
    private void SetupUIPair(string btnName, string panelName, ref Button btnRef, ref GameObject panelRef)
    {
        // 1. 버튼 찾기
        if (btnRef == null)
        {
            GameObject foundObj = GameObject.Find(btnName);
            if (foundObj != null) btnRef = foundObj.GetComponent<Button>();
        }

        // 2. 패널 찾기
        if (panelRef == null)
        {
            panelRef = GameObject.Find(panelName);

            // 못 찾았는데 버튼은 있다면? -> 버튼의 부모 관계에서 검색
            if (panelRef == null && btnRef != null)
            {
                Transform targetTr = btnRef.transform.parent.Find(panelName);
                if (targetTr != null) panelRef = targetTr.gameObject;
            }
        }

        // 3. 기능 연결
        if (btnRef != null && panelRef != null)
        {
            // 패널 닫기
            panelRef.SetActive(false);

            // 지역 변수에 담아서 넘겨줌.
            GameObject targetPanel = panelRef;

            // 버튼 이벤트 연결
            btnRef.onClick.RemoveAllListeners();
            btnRef.onClick.AddListener(() => TogglePanel(targetPanel));
        }
    }

    // 범용 토글 함수
    public void TogglePanel(GameObject targetPanel)
    {
        if (targetPanel != null)
        {
            bool isActive = targetPanel.activeSelf;
            targetPanel.SetActive(!isActive);
        }
    }
}