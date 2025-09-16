using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using ExcelDataReader;

public static class MenuCatalogXlsxLoader
{
    public static List<MenuItemDef> Load(string xlsxPathOrGoogleUrl, string sheetName = "menu_catalog")
    {
        var table = SheetTableLoader.LoadTable(xlsxPathOrGoogleUrl, sheetName);
        var list = new List<MenuItemDef>();

        if (table == null)
        {
            Debug.LogError($"[MenuCatalog] Failed to load sheet '{sheetName}' from '{xlsxPathOrGoogleUrl}'");
            return list;
        }

        foreach (DataRow r in table.Rows)
        {
            try
            {
                var m = new MenuItemDef
                {
                    menu_id = r.GetStr("menu_id"),
                    category = r.GetStr("category"),
                    name = r.GetStr("name"),
                    price = ParseInt(r["price"]),
                    is_active = ParseBool(r["is_active"])
                };
                if (!string.IsNullOrEmpty(m.menu_id)) list.Add(m);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[menu row skip] {ex.Message}");
            }
        }
        return list;
    }

    static int ParseInt(object o)
    {
        if (o == null) return 0;
        var s = o.ToString().Trim();
        if (int.TryParse(s, out var v)) return v;
        if (double.TryParse(s, out var d)) return (int)Math.Round(d);
        return 0;
    }

    static bool ParseBool(object o)
    {
        if (o == null) return false;
        var s = o.ToString().Trim().ToUpperInvariant();
        return s == "TRUE" || s == "1" || s == "Y";
    }
}

public static class AddressDimsXlsxLoader
{
    public static AddressDimensions Load(string xlsxPathOrGoogleUrl, string sheetName = "address_dimensions")
    {
        var dims = new AddressDimensions();

        var table = SheetTableLoader.LoadTable(xlsxPathOrGoogleUrl, sheetName);
        if (table == null)
        {
            Debug.LogError($"[AddressDims] No sheet {sheetName}");
            return dims;
        }

        foreach (DataRow r in table.Rows)
        {
            Add(r, "zone", dims.zones, s => s);
            Add(r, "complex", dims.complexes, s => RoundInt(s));
            Add(r, "building", dims.buildings, s => RoundInt(s));
            Add(r, "floor", dims.floors, s => RoundInt(s));
            Add(r, "room", dims.rooms, s => RoundInt(s));
        }

        dims.zones = dims.zones.Distinct().ToList();
        dims.complexes = dims.complexes.Distinct().ToList();
        dims.buildings = dims.buildings.Distinct().ToList();
        dims.floors = dims.floors.Distinct().ToList();
        dims.rooms = dims.rooms.Distinct().ToList();

        return dims;
    }

    static int RoundInt(string s)
    {
        if (double.TryParse(s, out var d)) return (int)Math.Round(d);
        if (int.TryParse(s, out var v)) return v;
        return 0;
    }

    static void Add<T>(DataRow r, string col, List<T> to, Func<string, T> parse)
    {
        if (!r.Table.Columns.Contains(col)) return;
        var cell = r[col];
        if (cell == null) return;
        var s = cell.ToString().Trim();
        if (string.IsNullOrEmpty(s)) return;
        to.Add(parse(s));
    }
}

/* -------------------- 공용 로더/유틸 -------------------- */

static class SheetTableLoader
{
    public static DataTable LoadTable(string pathOrUrl, string sheetName)
    {
        if (GoogleSheetUtil.IsGoogleSheetUrl(pathOrUrl))
        {
            try
            {
                var csv = GoogleSheetUtil.DownloadCsvBlocking(pathOrUrl, sheetName);
                if (string.IsNullOrEmpty(csv))
                {
                    Debug.LogError($"[GoogleSheet] empty CSV for sheet '{sheetName}'");
                    return null;
                }
                return CsvToDataTable(csv);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GoogleSheet] download/parse failed: {ex.Message}");
                return null;
            }
        }
        else
        {
            try
            {
                using var stream = File.Open(pathOrUrl, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = ExcelReaderFactory.CreateReader(stream);
                var ds = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
                });
                return ds.Tables.Contains(sheetName) ? ds.Tables[sheetName] : ds.Tables[0];
            }
            catch (Exception ex)
            {
                Debug.LogError($"[XLSX] read failed: {ex.Message}");
                return null;
            }
        }
    }

    // 간단한 RFC4180 CSV -> DataTable 변환
    static DataTable CsvToDataTable(string csv)
    {
        var dt = new DataTable();
        using var sr = new StringReader(csv);
        string line;
        List<string> headers = null;

        while ((line = sr.ReadLine()) != null)
        {
            var fields = CsvParseLine(line);
            if (headers == null)
            {
                headers = fields;
                foreach (var h in headers)
                    dt.Columns.Add(string.IsNullOrEmpty(h) ? "col" + dt.Columns.Count : h.Trim());
            }
            else
            {
                var row = dt.NewRow();
                for (int i = 0; i < dt.Columns.Count; i++)
                    row[i] = i < fields.Count ? fields[i] : "";
                dt.Rows.Add(row);
            }
        }
        return dt;
    }

    // 따옴표/콤마 대응 파서
    static List<string> CsvParseLine(string line)
    {
        var res = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    // 이스케이프된 "" 처리
                    if (i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                    else inQuotes = false;
                }
                else sb.Append(c);
            }
            else
            {
                if (c == ',') { res.Add(sb.ToString()); sb.Length = 0; }
                else if (c == '"') inQuotes = true;
                else sb.Append(c);
            }
        }
        res.Add(sb.ToString());
        // 앞뒤 공백 제거
        for (int i = 0; i < res.Count; i++) res[i] = res[i]?.Trim() ?? "";
        return res;
    }
}

static class GoogleSheetUtil
{
    // docs.google.com/spreadsheets/d/{DOCID}/...
    public static bool IsGoogleSheetUrl(string s) =>
        !string.IsNullOrEmpty(s) && s.Contains("docs.google.com/spreadsheets/d/");

    public static string DownloadCsvBlocking(string urlOrDocId, string sheetName)
    {
        string docId = ExtractDocId(urlOrDocId);
        if (string.IsNullOrEmpty(docId))
            throw new Exception("Invalid Google Sheet URL or Doc ID.");

        string url = $"https://docs.google.com/spreadsheets/d/{docId}/gviz/tq?tqx=out:csv&sheet={Uri.EscapeDataString(sheetName)}";

        using var req = UnityWebRequest.Get(url);
#if UNITY_2020_2_OR_NEWER
        var op = req.SendWebRequest();
#else
        var op = req.SendWebRequest();
#endif
        // 동기 대기 (주의: 메인스레드 블로킹 가능)
        while (!op.isDone) { }

#if UNITY_2020_2_OR_NEWER
        if (req.result != UnityWebRequest.Result.Success)
#else
        if (req.isNetworkError || req.isHttpError)
#endif
            throw new Exception($"HTTP {req.responseCode}: {req.error}");

        // 구글이 시트명을 못 찾으면 HTML이 떨어질 수 있으니 간단 체크
        var text = req.downloadHandler.text;
        if (text.StartsWith("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase))
            throw new Exception("Received HTML instead of CSV. Check sharing permission and sheet name.");

        return text;
    }

    static string ExtractDocId(string urlOrId)
    {
        if (urlOrId.StartsWith("http"))
        {
            // .../d/{DOCID}/...
            var mark = "/d/";
            var idx = urlOrId.IndexOf(mark, StringComparison.Ordinal);
            if (idx < 0) return null;
            var s = idx + mark.Length;
            var e = urlOrId.IndexOf('/', s);
            return e > s ? urlOrId.Substring(s, e - s) : urlOrId.Substring(s);
        }
        // 순수 DOCID 만 준 경우
        return urlOrId;
    }
}

/* -------------------- 작은 헬퍼 -------------------- */
static class DataRowExt
{
    public static string GetStr(this DataRow r, string col)
    {
        if (!r.Table.Columns.Contains(col)) return "";
        var o = r[col];
        return o == null ? "" : o.ToString().Trim();
    }
}
