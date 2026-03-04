using Microsoft.Extensions.Configuration;
using Practical.HubSpot.Responses;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

var config = new ConfigurationBuilder()
    .AddUserSecrets("871c4384-4147-4331-96cd-71e635956419")
    .Build();

var accessToken = config["HubSpot:AccessToken"] ?? throw new ArgumentException("HubSpot:AccessToken is empty");
var baseUrl = "https://api.hubapi.com";

var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

var jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
};

// Get list of deals
var deals = await GetDeals(httpClient, baseUrl, jsonOptions);

// Get deal details for the first deal
if (deals != null && deals.Length > 0)
{
    var firstDealId = deals[0].Id;
    await GetDealDetails(httpClient, baseUrl, jsonOptions, firstDealId);
    
    // Update deal
    await UpdateDeal(httpClient, baseUrl, jsonOptions, firstDealId);
}

Console.WriteLine("Done!");

async Task<DealResponse[]> GetDeals(HttpClient client, string baseUrl, JsonSerializerOptions options)
{
    try
    {
        Console.WriteLine("\n=== Getting List of Deals ===");
        
        var response = await client.GetAsync($"{baseUrl}/crm/v3/objects/deals?limit=100");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var dealsListResponse = JsonSerializer.Deserialize<DealsListResponse>(content, options);
        
        if (dealsListResponse?.Results != null)
        {
            Console.WriteLine($"Found {dealsListResponse.Results.Length} deals:");
            
            foreach (var deal in dealsListResponse.Results)
            {
                Console.WriteLine($"  ID: {deal.Id}, Name: {deal.Properties?.DealName}, Stage: {deal.Properties?.DealStage}");
            }
            
            return dealsListResponse.Results;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting deals: {ex.Message}");
    }
    
    return null;
}

async Task GetDealDetails(HttpClient client, string baseUrl, JsonSerializerOptions options, string dealId)
{
    try
    {
        Console.WriteLine($"\n=== Getting Deal Details for ID: {dealId} ===");
        
        var detailResponse = await client.GetAsync(
            $"{baseUrl}/crm/v3/objects/deals/{dealId}?properties=dealname,dealstage,amount,closedate,description,dealowner");
        detailResponse.EnsureSuccessStatusCode();
        
        var detailContent = await detailResponse.Content.ReadAsStringAsync();
        var deal = JsonSerializer.Deserialize<DealResponse>(detailContent, options);
        
        Console.WriteLine($"Deal ID: {deal.Id}");
        Console.WriteLine($"  Name: {deal.Properties?.DealName}");
        Console.WriteLine($"  Stage: {deal.Properties?.DealStage}");
        Console.WriteLine($"  Amount: {deal.Properties?.Amount}");
        Console.WriteLine($"  Close Date: {deal.Properties?.CloseDate}");
        Console.WriteLine($"  Description: {deal.Properties?.Description}");
        Console.WriteLine($"  Owner: {deal.Properties?.DealOwner}");
        Console.WriteLine($"  Created: {deal.CreatedAt}");
        Console.WriteLine($"  Updated: {deal.UpdatedAt}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error getting deal details: {ex.Message}");
    }
}

async Task UpdateDeal(HttpClient client, string baseUrl, JsonSerializerOptions options, string dealId)
{
    try
    {
        Console.WriteLine($"\n=== Updating Deal ID: {dealId} ===");
        
        var updatePayload = new
        {
            properties = new
            {
                dealname = $"Updated Deal {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                description = "This deal was updated programmatically"
            }
        };
        
        var jsonContent = JsonSerializer.Serialize(updatePayload);
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
        
        var updateResponse = await client.PatchAsync($"{baseUrl}/crm/v3/objects/deals/{dealId}", content);
        updateResponse.EnsureSuccessStatusCode();
        
        var updatedContent = await updateResponse.Content.ReadAsStringAsync();
        var updatedDeal = JsonSerializer.Deserialize<DealResponse>(updatedContent, options);
        
        Console.WriteLine($"Deal {updatedDeal.Id} updated successfully");
        Console.WriteLine($"  New Name: {updatedDeal.Properties?.DealName}");
        Console.WriteLine($"  New Description: {updatedDeal.Properties?.Description}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error updating deal: {ex.Message}");
    }
}
