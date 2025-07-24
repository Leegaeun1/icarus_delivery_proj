using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class CardGo : MonoBehaviour
{

    private bool flipped = true;

    void Start() // Awake 대신 OnEnable 사용
    {
        transform.localRotation = Quaternion.Euler(0, 180f, 0);
        Debug.Log(gameObject.name + " 카드가 뒷면으로 초기 설정됨.");
    }

    public void Flip()
    {
        print(gameObject.name + "뒤집음");
        flipped = !flipped;
        transform.DOLocalRotate(new Vector3(0, flipped ? 180f : 0f, 0), 0.25f);
    }
}
