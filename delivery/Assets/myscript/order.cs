public class Order
{
    public string orderName;
    public float orderTime; 
    public bool isCompleted;

    public Order(string orderName, float time)
    {
        this.orderName = orderName;
        this.orderTime = time; 
        isCompleted = false;
    }
}
