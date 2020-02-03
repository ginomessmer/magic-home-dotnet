using System;
using System.Linq;
using Xunit;

namespace MagicHome.Tests
{
    public class LightUnitTests
    {
        [Fact]
        public void Apply_Checksum_ShouldBeValid()
        {
            // Arrange
            var bytes = new byte[] {1, 2, 3};

            // Act
            var newBytes = Light.CalculateChecksum(bytes);

            // Assert
            Assert.Equal(6, newBytes.Last());
        }
    }
}
