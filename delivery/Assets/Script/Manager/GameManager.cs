using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;
    private float startTime;
    public GameObject setting_ui;
    private bool isclick=false;

    public AudioSource audio_source;

    public float ElapsedTime => Time.time - startTime; // 경과된 시간 

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

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Mouse0)) // 마우스 클릭시 소리
        {
            audio_source.Play();
        }
    }

    public void openUi() // 설정 열기
    {
        if (!isclick)
        {
            isclick = true;
        }
        else
        {
            isclick = false;
        }
        setting_ui.SetActive(isclick);
    }
}
