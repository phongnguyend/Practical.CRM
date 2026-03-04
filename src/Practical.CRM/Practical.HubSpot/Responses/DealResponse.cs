namespace Practical.HubSpot.Responses;

public class DealResponse
{
    public string Id { get; set; }
    public DealProperties Properties { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class DealProperties
{
    public string DealName { get; set; }
    public string DealStage { get; set; }
    public string Amount { get; set; }
    public string CloseDate { get; set; }
    public string Description { get; set; }
    public string DealOwner { get; set; }
}

public class DealsListResponse
{
    public DealResponse[] Results { get; set; }
}
