using UnityEngine;
using UnityEngine.UI; // UI 컴포넌트를 사용하기 위해 필요

public class IsClicked : MonoBehaviour
{
    private Animator anim;
    private bool isPressed = false; // 현재 눌린 상태인지 저장하는 변수

    /*void Start()
    {
        // 같은 오브젝트에 있는 Animator 컴포넌트를 찾아 연결
        anim = GetComponent<Animator>();

        // 같은 오브젝트에 있는 Button 컴포넌트를 찾아 클릭 이벤트에 함수 연결
        // 이렇게 하면 Inspector에서 일일이 연결할 필요가 없습니다.
        GetComponent<Button>().onClick.AddListener(ToggleState);
    }*/

    // 버튼이 클릭될 때마다 실행될 함수
   /* public void ToggleState()
    {
        // 상태를 반전시킴 (true -> false, false -> true)
        isPressed = !isPressed;

        // Animator의 'IsPressed' 파라미터에 현재 상태를 전달
        anim.SetBool("IsPressed", isPressed);
    }*/
}