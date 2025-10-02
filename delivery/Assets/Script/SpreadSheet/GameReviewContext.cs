using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameReviewContext
{
    // 데이터 타입을 List<string>에서 List<List<string>>으로 변경
    public static readonly List<List<string>> SelectedIngredients = new();
    public static readonly List<List<string>> ExcludeRequest = new();
    public static readonly List<List<string>> IncludeRequest = new();

    /// <summary>
    /// 여러 주문의 재료 정보를 한 번에 설정합니다.
    /// </summary>
    /// <param name="selected">선택된 재료 목록의 목록</param>
    /// <param name="exclude">제외 요청 재료 목록의 목록</param>
    /// <param name="include">포함 요청 재료 목록의 목록</param>
    public static void Set(
        IEnumerable<List<string>> selected,
        IEnumerable<List<string>>? exclude = null,
        IEnumerable<List<string>>? include = null)
    {
        SelectedIngredients.Clear();
        if (selected != null) SelectedIngredients.AddRange(selected);

        ExcludeRequest.Clear();
        if (exclude != null) ExcludeRequest.AddRange(exclude);

        IncludeRequest.Clear();
        if (include != null) IncludeRequest.AddRange(include);
    }

    /// <summary>
    /// 단일 주문 정보를 추가합니다. 기존 데이터에 누적됩니다.
    /// </summary>
    public static void AddOrder(
        IEnumerable<string> selected,
        IEnumerable<string>? exclude = null,
        IEnumerable<string>? include = null)
    {
        if (selected != null) SelectedIngredients.Add(selected.ToList());
        if (exclude != null) ExcludeRequest.Add(exclude.ToList());
        if (include != null) IncludeRequest.Add(include.ToList());
    }

    /// <summary>
    /// 모든 리뷰 관련 데이터를 초기화합니다.
    /// </summary>
    public static void Clear()
    {
        SelectedIngredients.Clear();
        ExcludeRequest.Clear();
        IncludeRequest.Clear();
    }
}
