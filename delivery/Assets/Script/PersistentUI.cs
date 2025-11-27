using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PersistentUI : MonoBehaviour
{
    public static PersistentUI Instance;
    //public GameObject g;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            //DontDestroyOnLoad(g);
        }
        else
        {
            Destroy(gameObject); // 중복 방지
        }
    }
}
