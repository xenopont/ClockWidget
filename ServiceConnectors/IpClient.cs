namespace ServiceConnectors;

public interface IHttpStringClient
{
    Task<string> GetStringAsync(string uri);
}

public interface INowProvider
{
    DateTime Now { get; }
}

public class IpClient(IHttpStringClient? httpStringClient = null, INowProvider? nowProvider = null)
{
    private const int InvalidateIntervalSeconds = 5 * 60;

    private static string? _currentValue;
    private static DateTime _lastUpdate = DateTime.MinValue;

    private readonly IHttpStringClient _httpClient = httpStringClient ?? new DefaultHttpClient();
    private readonly INowProvider _nowProvider = nowProvider ?? new DefaultNowProvider();

    public async Task<string?> GetExternalIp()
    {
        var updatingMoment = _nowProvider.Now;
        var secondsSinceLastRequest = updatingMoment.Subtract(_lastUpdate).TotalSeconds;
        if (secondsSinceLastRequest < InvalidateIntervalSeconds) return _currentValue;

        try
        {
            _currentValue = await _httpClient.GetStringAsync("https://api.ipify.org");
            _lastUpdate = updatingMoment;
        }
        catch (HttpRequestException)
        {
            return null;
        }

        return _currentValue;
    }

    private class DefaultHttpClient : IHttpStringClient
    {
        private readonly HttpClient _httpClient = new();

        public Task<string> GetStringAsync(string uri)
        {
            return _httpClient.GetStringAsync(uri);
        }
    }

    private class DefaultNowProvider : INowProvider
    {
        public DateTime Now => DateTime.Now;
    }
}