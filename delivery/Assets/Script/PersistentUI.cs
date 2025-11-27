using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PersistentUI : MonoBehaviour
{
    public static PersistentUI Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // 중복 방지
        }
    }
}
