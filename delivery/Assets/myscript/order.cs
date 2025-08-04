[System.Serializable]
public class Order
{
    public string orderName;
    public float orderTime;
    public bool isCompleted;

    public Order(string name, float time)
    {
        orderName = name;
        orderTime = time;
        isCompleted = false;
    }
}