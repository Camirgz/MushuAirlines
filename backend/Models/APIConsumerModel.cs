namespace backend.Model;

public class APIConsumerModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ApiKeyHash { get; set; }
    public bool IsActive { get; set; }
}