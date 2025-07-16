using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class CardGo : MonoBehaviour
{

    private bool flipped = true;

    void Start() // Awake ��� OnEnable ���
    {
        transform.localRotation = Quaternion.Euler(0, 180f, 0);
        Debug.Log(gameObject.name + " ī�尡 �޸����� �ʱ� ������.");
    }

    public void Flip()
    {
        print(gameObject.name + "������");
        flipped = !flipped;
        transform.DOLocalRotate(new Vector3(0, flipped ? 180f : 0f, 0), 0.25f);
    }
}
