using Xunit;

namespace CoreEssentials.Tests.TestInfrastructure
{
    /// <summary>
    /// Collection definition for all MonoGame-related tests.
    /// This allows all tests to share a single MonoGameTestFixture instance.
    /// </summary>
    [CollectionDefinition("MonoGame Tests")]
    public class MonoGameTestCollection : ICollectionFixture<MonoGameTestFixture>
    {
        // This class acts as a marker and doesn't need any implementation.
        // The ICollectionFixture<MonoGameTestFixture> interface tells xUnit to
        // create a single instance of MonoGameTestFixture for all tests
        // marked with [Collection("MonoGame Tests")]
    }
}