using Moq;

namespace ServiceConnectors.Tests;

[TestClass]
public sealed class IpClientTests
{
    private Mock<IHttpStringClient> _mockHttpClient;
    private Mock<INowProvider> _mockNowProvider;
    private IpClient _ipClient;

    private const int InvalidateIntervalSeconds = 5 * 60;

    [TestInitialize]
    public void Setup()
    {
        _mockHttpClient = new Mock<IHttpStringClient>();
        _mockNowProvider = new Mock<INowProvider>();
        _ipClient = new IpClient(_mockHttpClient.Object, _mockNowProvider.Object);
    }

    [TestMethod]
    public async Task GetExternalIp_ShouldReturnIp_WhenCalledFirstTime()
    {
        // Arrange
        const string expectedIp = "192.168.1.1";
        _mockHttpClient.Setup(client => client.GetStringAsync(It.IsAny<string>())).ReturnsAsync(expectedIp);
        _mockNowProvider.Setup(time => time.Now).Returns(DateTime.Now);

        // Act
        var result = await _ipClient.GetExternalIp();

        // Assert
        Assert.AreEqual(expectedIp, result);
    }

    [TestMethod]
    public async Task GetExternalIp_ShouldReturnCachedIp_WhenCalledWithinInvalidateInterval()
    {
        // Arrange
        const string cachedIp = "192.168.1.1";
        var initialTime = DateTime.Now;
        _mockNowProvider.SetupSequence(time => time.Now)
            .Returns(initialTime)
            .Returns(initialTime.AddSeconds(InvalidateIntervalSeconds / 2.0
                ));
        _mockHttpClient.Setup(client => client.GetStringAsync(It.IsAny<string>())).ReturnsAsync(cachedIp);

        await _ipClient.GetExternalIp(); // First call to update the IP

        // Act
        var result = await _ipClient.GetExternalIp();

        // Assert
        Assert.AreEqual(cachedIp, result);
    }

    [TestMethod]
    public async Task GetExternalIp_ShouldReturnNull_WhenHttpRequestExceptionThrown()
    {
        // Arrange
        _mockHttpClient.Setup(client => client.GetStringAsync(It.IsAny<string>())).ThrowsAsync(new HttpRequestException());
        _mockNowProvider.Setup(time => time.Now).Returns(DateTime.Now);

        // Act
        var result = await _ipClient.GetExternalIp();

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetExternalIp_ShouldUpdateIp_WhenCalledAfterInvalidateInterval()
    {
        // Arrange
        const string initialIp = "192.168.1.1";
        const string updatedIp = "10.0.0.1";
        var initialTime = DateTime.Now;
        _mockNowProvider.SetupSequence(time => time.Now)
            .Returns(initialTime)
            .Returns(initialTime.AddSeconds(InvalidateIntervalSeconds + 1));
        _mockHttpClient.SetupSequence(client => client.GetStringAsync(It.IsAny<string>()))
            .ReturnsAsync(initialIp)
            .ReturnsAsync(updatedIp);

        await _ipClient.GetExternalIp(); // First call to update the IP

        // Act
        var result = await _ipClient.GetExternalIp();

        // Assert
        Assert.AreEqual(updatedIp, result);
    }
}