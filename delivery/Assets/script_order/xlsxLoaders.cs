using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;
using ExcelDataReader;

public static class MenuCatalogXlsxLoader
{
    public static List<MenuItemDef> Load(string xlsxPath, string sheetName = "menu_catalog")
    {
        var list = new List<MenuItemDef>();
        using var stream = File.Open(xlsxPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = ExcelReaderFactory.CreateReader(stream);
        var ds = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
        });
        var t = ds.Tables.Contains(sheetName) ? ds.Tables[sheetName] : ds.Tables[0];
        foreach (DataRow r in t.Rows)
        {
            try
            {
                var m = new MenuItemDef
                {
                    menu_id = r["menu_id"]?.ToString().Trim(),
                    category = r["category"]?.ToString().Trim(),
                    name = r["name"]?.ToString().Trim(),
                    price = ParseInt(r["price"]),
                    is_active = ParseBool(r["is_active"])
                };
                if (!string.IsNullOrEmpty(m.menu_id)) list.Add(m);
            }
            catch (Exception ex) { Debug.LogWarning($"[menu row skip] {ex.Message}"); }
        }
        return list;
    }
    static int ParseInt(object o)
    {
        if (o == null) return 0; var s = o.ToString().Trim();
        if (int.TryParse(s, out var v)) return v; if (double.TryParse(s, out var d)) return (int)Math.Round(d); return 0;
    }
    static bool ParseBool(object o) { if (o == null) return false; var s = o.ToString().Trim().ToUpperInvariant(); return s == "TRUE" || s == "1" || s == "Y"; }
}

public static class AddressDimsXlsxLoader
{
    public static AddressDimensions Load(string xlsxPath, string sheetName = "address_dimensions")
    {
        var d = new AddressDimensions();
        using var stream = File.Open(xlsxPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = ExcelReaderFactory.CreateReader(stream);
        var ds = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
        });
        if (!ds.Tables.Contains(sheetName)) { Debug.LogError($"No sheet {sheetName}"); return d; }
        var t = ds.Tables[sheetName];
        foreach (DataRow r in t.Rows)
        {
            Add(r, "zone", d.zones, s => s);
            Add(r, "complex", d.complexes, s => (int)Math.Round(double.Parse(s)));
            Add(r, "building", d.buildings, s => (int)Math.Round(double.Parse(s)));
            Add(r, "floor", d.floors, s => (int)Math.Round(double.Parse(s)));
            Add(r, "room", d.rooms, s => (int)Math.Round(double.Parse(s)));
        }
        d.zones = d.zones.Distinct().ToList();
        d.complexes = d.complexes.Distinct().ToList();
        d.buildings = d.buildings.Distinct().ToList();
        d.floors = d.floors.Distinct().ToList();
        d.rooms = d.rooms.Distinct().ToList();
        return d;
    }
    static void Add<T>(DataRow r, string col, List<T> to, Func<string, T> parse)
    {
        if (!r.Table.Columns.Contains(col)) return; var cell = r[col]; if (cell == null) return;
        var s = cell.ToString().Trim(); if (string.IsNullOrEmpty(s)) return; to.Add(parse(s));
    }
}