using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private float startTime;

    public float ElapsedTime => Time.time - startTime; // ����� �ð� 

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
        print(ElapsedTime);
    }
}
