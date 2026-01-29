using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteract : MonoBehaviour
{
    private Random_face myFace;
    private IDCardUI idCardUI;

    void Start()
    {
        myFace = GetComponent<Random_face>();
        idCardUI = FindObjectOfType<IDCardUI>();
    }

    // 마우스로 NPC를 클릭했을 때 실행 (NPC에 Collider 2D가 있어야 함)
    void OnMouseDown()
    {
        if (myFace != null && idCardUI != null)
        {
            // NPC의 얼굴 데이터를 추출해서 UI에 전달
            FaceData currentData = myFace.GetCurrentFaceData();
            idCardUI.ShowCard(currentData, "REG-7749");
        }
    }
}