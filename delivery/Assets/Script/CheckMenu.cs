using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CheckMenu : MonoBehaviour
{
    Image image;
    private void Awake()
    {
        image = GetComponent<Image>();
    }
    bool isSelect= false;
    public GameObject selectMenu;

    public void player_select()
    {
        if (isSelect) {
            isSelect = false;
            image.color = new Color32(255, 255, 255, 255);
        }
        else
        {
            isSelect = true;
            print(gameObject.name);
            image.color = new Color32(220, 200, 200, 255);
        }
    }

    public void completeBtn()
    {
        this.gameObject.SetActive(false);
        selectMenu.gameObject.SetActive(true);
    }
}
