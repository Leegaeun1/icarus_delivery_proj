using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CatalogRuntime
{
    public List<MenuItemDef> sandwiches;
    public List<MenuItemDef> drinks;
    public List<MenuItemDef> cookies;

    public List<string> zones;
    public List<int> complexes, buildings, floors, rooms;

    public static CatalogRuntime Build(List<MenuItemDef> allMenus, AddressDimensions dims, UnlockState st)
    {
        // (1) 메뉴: is_active=TRUE → menu_id 정렬 → 앞에서 N개
        var sAll = allMenus.Where(m => m.is_active && m.category == "Sandwich").OrderBy(m => m.menu_id).ToList();
        var dAll = allMenus.Where(m => m.is_active && m.category == "Drink").OrderBy(m => m.menu_id).ToList();
        var cAll = allMenus.Where(m => m.is_active && m.category == "Cookie").OrderBy(m => m.menu_id).ToList();

        var s = sAll.Take(Mathf.Min(st.sUnlocked, sAll.Count)).ToList();
        var d = dAll.Take(Mathf.Min(st.dUnlocked, dAll.Count)).ToList();
        var c = cAll.Take(Mathf.Min(st.cUnlocked, cAll.Count)).ToList();

        // (2) 주소: 각 차원 정렬 → 앞에서 N개
        var z = dims.zones.OrderBy(x => x).Take(Mathf.Min(st.zonesUnlocked, dims.zones.Count)).ToList();
        var cx = dims.complexes.OrderBy(x => x).Take(Mathf.Min(st.complexesUnlocked, dims.complexes.Count)).ToList();
        var b = dims.buildings.OrderBy(x => x).Take(Mathf.Min(st.buildingsUnlocked, dims.buildings.Count)).ToList();
        var f = dims.floors.OrderBy(x => x).Take(Mathf.Min(st.floorsUnlocked, dims.floors.Count)).ToList();
        var r = dims.rooms.OrderBy(x => x).Take(Mathf.Min(st.roomsUnlocked, dims.rooms.Count)).ToList();

        return new CatalogRuntime
        {
            sandwiches = s,
            drinks = d,
            cookies = c,
            zones = z,
            complexes = cx,
            buildings = b,
            floors = f,
            rooms = r
        };
    }
}