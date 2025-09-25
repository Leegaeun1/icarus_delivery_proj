using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRevieContext
{
    public static readonly List<string> SelectedIngredients = new(); // 플레이어가 넣은 최종 재료
    public static readonly List<string> ExcludeRequest = new();      // "빼주세요" 요청 (없으면 비워둠)
    public static readonly List<string> IncludeRequest = new();      // "꼭 넣어주세요" (없으면 비워둠)

    public static void Set(
        IEnumerable<string> selected,
        IEnumerable<string>? exclude = null,
        IEnumerable<string>? include = null)
    {
        SelectedIngredients.Clear();
        if (selected != null) SelectedIngredients.AddRange(selected);

        ExcludeRequest.Clear();
        if (exclude != null) ExcludeRequest.AddRange(exclude);

        IncludeRequest.Clear();
        if (include != null) IncludeRequest.AddRange(include);
    }

    public static void Clear()
    {
        SelectedIngredients.Clear();
        ExcludeRequest.Clear();
        IncludeRequest.Clear();
    }
}
