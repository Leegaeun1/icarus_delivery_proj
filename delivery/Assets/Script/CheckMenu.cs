using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CheckMenu : MonoBehaviour
{
    bool isSelect= false;

    public void player_select()
    {
        if (isSelect) {
            isSelect = false;
            gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 1f);
        }
        else
        {
            isSelect = true;
            print(gameObject.name);
            gameObject.GetComponent<Image>().color = new Color(220f/255f, 200/255f, 200/255f, 1f);
        }
    }

    public void completeBtn()
    {
        this.gameObject.SetActive(false);
    }
}
