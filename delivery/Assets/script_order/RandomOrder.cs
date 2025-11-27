using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RandomAddressPicker
{
    public static AddressParts Pick(CatalogRuntime rt)
    {
        string zone = rt.zones[Random.Range(0, rt.zones.Count)];
        int complex = rt.complexes[Random.Range(0, rt.complexes.Count)];
        int building = rt.buildings[Random.Range(0, rt.buildings.Count)];
        int floor = rt.floors[Random.Range(0, rt.floors.Count)];
        int room = rt.rooms[Random.Range(0, rt.rooms.Count)];
        return new AddressParts { zone = zone, complex = complex, building = building, floor = floor, room = room };
    }
}

public class RandomMenuPicker
{
    readonly List<MenuItemDef> sPool, dPool, cPool;
    public RandomMenuPicker(CatalogRuntime rt)
    {
        sPool = rt.sandwiches; dPool = rt.drinks; cPool = rt.cookies;
    }

    // "종류 수" 범위 + "수량" 범위 분리
    public List<LineItem> Pick(
        (int min, int max) kindsS, (int min, int max) kindsD, (int min, int max) kindsC,
        (int min, int max) qtyS, (int min, int max) qtyD, (int min, int max) qtyC
    )
    {
        var items = new List<LineItem>();
        AddCategory(items, MenuCategory.Sandwich, sPool, kindsS, qtyS);
        AddCategory(items, MenuCategory.Drink, dPool, kindsD, qtyD);
        AddCategory(items, MenuCategory.Cookie, cPool, kindsC, qtyC);
        return items;
    }

    void AddCategory(List<LineItem> dst, MenuCategory cat, List<MenuItemDef> pool,
        (int min, int max) kinds, (int min, int max) qtyRange)
    {
        if (pool == null || pool.Count == 0) return;
        int k = Mathf.Clamp(Random.Range(kinds.min, kinds.max + 1), 0, pool.Count);
        if (k <= 0) return;
        var chosen = new HashSet<int>();
        while (chosen.Count < k) chosen.Add(Random.Range(0, pool.Count));
        foreach (var idx in chosen)
        {
            int q = Mathf.Clamp(Random.Range(qtyRange.min, qtyRange.max + 1), 1, 99);
            dst.Add(new LineItem { category = cat, name = pool[idx].name, quantity = q });
        }
    }
}