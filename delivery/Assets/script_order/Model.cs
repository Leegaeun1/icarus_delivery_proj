using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MenuItemDef
{
    public string menu_id;   // 예: S01, D02, C03
    public string category;  // "Sandwich" | "Drink" | "Cookie"
    public string name;
    public int price;
    public bool is_active;
}

[Serializable]
public struct AddressParts
{
    public string zone;   // A~F
    public int complex;   // 1..5
    public int building;  // 예: 1,3,5,7,9,12,15,18,20
    public int floor;     // 1..9
    public int room;      // 1..9
    public string ToFull() => $"{zone}구역 {complex}단지 {building}건물 {floor}{room:00}호";
}

public class AddressDimensions
{
    public List<string> zones = new();
    public List<int> complexes = new();
    public List<int> buildings = new();
    public List<int> floors = new();
    public List<int> rooms = new();
}

[Serializable]
public enum MenuCategory { Sandwich, Drink, Cookie }

[Serializable]
public class LineItem
{
    public MenuCategory category;
    public string name;
    public int quantity;
}

[Serializable]
public class Order
{
    public string orderId;
    public string timestamp;
    public string address;
    public List<LineItem> items;
}
