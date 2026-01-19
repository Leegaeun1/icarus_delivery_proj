using System.Collections.Generic;

// 주문 정보를 담는 그릇
[System.Serializable]
public class Order_check
{
    // 입력 데이터
    public List<string> SelectedIngredients = new();
    public List<string> ExcludeRequest = new();
    public List<string> IncludeRequest = new();

    public string RequestedSpecial = "";
    public string ProvidedSpecial = "";

    public List<string> RequestedSides = new();
    public List<string> ProvidedSides = new();

    public bool IsDeliverySuccess = true;

    // 출력 데이터 (판정 결과)
    public bool isSuccess;
    public string failReason;
}

// 주문이 성공인지 실패인지 판단
public static class OrderChecker
{
    public static void Check(Order_check order)
    {
        // 1. 배달 사고 
        if (!order.IsDeliverySuccess)
        {
            SetFail(order, "음식을 주문했는데 오질 않네요.. 배달원이 딴 데로 갔나 봐요.");
            return;
        }

        // 2. 스페셜 메뉴 체크
        // (1) 요청했는데 안 왔거나 다른 게 온 경우
        if (!string.IsNullOrEmpty(order.RequestedSpecial))
        {
            if (order.RequestedSpecial != order.ProvidedSpecial)
            {
                if (string.IsNullOrEmpty(order.ProvidedSpecial))
                    SetFail(order, $"제 {order.RequestedSpecial} 어디 갔나요? 이게 메인인데...");
                else
                    SetFail(order, $"{order.RequestedSpecial} 시켰는데 {order.ProvidedSpecial}가 왔어요.");
                return;
            }
        }
        // (2) 요청 안 했는데 온 경우
        else if (!string.IsNullOrEmpty(order.ProvidedSpecial))
        {
            SetFail(order, $"{order.ProvidedSpecial} 시킨 적 없는데요...");
            return;
        }

        // 3. 사이드 메뉴 체크
        
        // (1) 요청 안 한 게 왔는지
        foreach (var provSide in order.ProvidedSides)
        {
            if (!order.RequestedSides.Contains(provSide))
            {
                SetFail(order, $"{provSide} 주문 안 했는데 왔어요. 확인 좀 해주세요.");
                return;
            }
        }
        // (2) 요청한 게 안 왔는지
        foreach (var reqSide in order.RequestedSides)
        {
            if (!order.ProvidedSides.Contains(reqSide))
            {
                SetFail(order, $"{reqSide}도 같이 시켰는데 안 왔어요. 실망입니다.");
                return;
            }
        }
        

        // 4. 기본 재료 제외 요청
        foreach (var excluded in order.ExcludeRequest)
        {
            if (order.SelectedIngredients.Contains(excluded))
            {
                SetFail(order, $"{excluded} 빼달라고 메모 남겼는데 들어있네요..");
                return;
            }
        }

        // 5. 기본 재료 추가 요청
        foreach (var included in order.IncludeRequest)
        {
            if (!order.SelectedIngredients.Contains(included))
            {
                SetFail(order, $"{included} 추가해달라고 했는데 빠졌어요.");
                return;
            }
        }

        // 모든 관문 통과
        order.isSuccess = true;
        order.failReason = "";
    }

    // 내부용 헬퍼 함수
    private static void SetFail(Order_check order, string reason)
    {
        order.isSuccess = false;
        order.failReason = reason;
    }
}