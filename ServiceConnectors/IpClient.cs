namespace ServiceConnectors;

public static class IpClient
{
    public static async Task<string?> GetExternalIp()
    {
        var millisecondsSinceLastRequest = DateTime.Now.Subtract(_lastUpdate).TotalMilliseconds;
        if (millisecondsSinceLastRequest < UpdateIntervalMilliseconds)
        {
            return _currentValue;
        }
        
        try
        {
            _currentValue = await Client.GetStringAsync("https://api.ipify.org");
            _lastUpdate = DateTime.Now;
        }
        catch (HttpRequestException)
        {
            return null;
        }
        
        return _currentValue;
    }
    
    private const int UpdateIntervalMilliseconds = 5 * 60 * 1000;
    
    private static readonly HttpClient Client = new();
    private static string? _currentValue;
    private static DateTime _lastUpdate = DateTime.MinValue;
}