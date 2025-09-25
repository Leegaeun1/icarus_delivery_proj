using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRevieContext
{
    public static readonly List<string> SelectedIngredients = new();

    // 손님 요청: 빼주세요 / 꼭 넣어주세요 (없으면 비워둠)
    public static readonly List<string> ExcludeRequest = new();
    public static readonly List<string> IncludeRequest = new();

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
