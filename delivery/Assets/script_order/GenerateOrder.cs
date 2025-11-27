using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.Networking;

public class GenerateOrder : MonoBehaviour
{
    public string xlsxFileName = "menu_and_address.xlsx";

    void Start()
    {
        // Start는 void 그대로 두고, 코루틴 실행
        StartCoroutine(LoadAndBuildOnce());
    }

    private IEnumerator LoadAndBuildOnce()
    {
        string path = Path.Combine(Application.streamingAssetsPath, xlsxFileName);

#if UNITY_ANDROID
        using (var req = UnityWebRequest.Get(path))
        {
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(req.error);
                yield break; // 실패하면 코루틴 종료
            }

            var dst = Path.Combine(Application.persistentDataPath, xlsxFileName);
            File.WriteAllBytes(dst, req.downloadHandler.data);
            path = dst; // 안드로이드 환경에서는 퍼시스턴트 경로 사용
        }
#endif

        // 1) 메뉴 로드 (is_active가 TRUE인 메뉴만)
        var menus = MenuCatalogXlsxLoader
                        .Load(path, "menu_catalog")
                        .Where(m => m.is_active)
                        .ToList();

        // 2) 주소 로드
        var dims = AddressDimsXlsxLoader.Load(path, "address_dimensions");

        // 3) 해금 상태 적용
        var runtime = CatalogRuntime.Build(menus, dims, UnLockManager.I.State);

        // 4) 주문 생성
        var addr = RandomAddressPicker.Pick(runtime);
        var picker = new RandomMenuPicker(runtime);

        // 샌드위치/음료/쿠키 등의 개수 범위
        var items = picker.Pick(
            (1, 2), // 샌드위치 최소1~2개
            (0, 1), // 음료 최소0~1개
            (0, 2), // 쿠키 최소0~2개
            (1, 2), // 추가 범주
            (1, 2), // 추가 범주
            (1, 3)  // 추가 범주
        );

        var order = new Order
        {
            orderId = System.Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
            timestamp = System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
            address = addr.ToFull(),
            items = items
        };


        Debug.Log($"[주문]\n" +
                  $"ID: {order.orderId}\n" +
                  $"주소: {order.address}\n");

        yield break; // 코루틴 종료
    }
}