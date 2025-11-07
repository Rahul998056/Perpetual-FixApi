using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Perpetuals.Fix.Shared;

public class UpstreamApiService
{
    private readonly HttpClient _httpClient;

    public UpstreamApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    public async Task<string> PostAsync(string endpoint, string jsonPayload)
    {
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);

        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }

    //public async Task<string> PostAsync(string endpoint, string jsonPayload)
    //{
    //    await Task.Delay(100);
       
    //    Console.WriteLine($"[MOCK API] Called with endpoint: {endpoint}");
    //    Console.WriteLine($"[MOCK API] Payload: {jsonPayload}");
       
    //    return """
    //        {
    //            "OrderID": "12345",
    //            "ExecID": "exec-001",
    //            "ExecType": "0",     
    //            "OrdStatus": "0",    
    //            "Symbol": "BTCUSD",
    //            "Side": "1",         
    //            "LeavesQty": "0",
    //            "CumQty": "1",
    //            "AvgPx": "42000"
    //        }
    //        """;
    //}

    public async Task<string> GetAsync(string endpoint)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(endpoint);

        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }
}
