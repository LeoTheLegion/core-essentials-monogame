using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace CoreEssentials.Tests.Documentation
{
    public class XmlDocumentationTests
    {
        [Fact]
        public void XmlDocumentationFile_ShouldExist()
        {
            // Arrange
            var assembly = typeof(CoreEssentials.Assets.Asset).Assembly;
            var assemblyPath = Path.GetDirectoryName(assembly.Location);
            var xmlFileName = $"{assembly.GetName().Name}.xml";
            var xmlFilePath = Path.Combine(assemblyPath, xmlFileName);

            // Act & Assert
            Assert.True(File.Exists(xmlFilePath), $"XML documentation file '{xmlFilePath}' does not exist");
        }

        [Fact]
        public void Asset_Class_ShouldHaveXmlDocumentation()
        {
            // This test checks if our example Asset class has XML documentation
            // We're testing this because we've added documentation to this class specifically
            var type = typeof(CoreEssentials.Assets.Asset);
            
            // Get XML documentation file
            var assembly = type.Assembly;
            var assemblyPath = Path.GetDirectoryName(assembly.Location);
            var xmlFileName = $"{assembly.GetName().Name}.xml";
            var xmlFilePath = Path.Combine(assemblyPath, xmlFileName);
            
            Assert.True(File.Exists(xmlFilePath), "XML documentation file not found");
            
            // Read the XML documentation
            string xmlContent = File.ReadAllText(xmlFilePath);
            
            // Check if the Asset class has documentation
            Assert.Contains($"<member name=\"T:{type.FullName}\">", xmlContent);
            
            // Check if the Load method has documentation
            var loadMethod = type.GetMethod("Load");
            Assert.NotNull(loadMethod);
            Assert.Contains($"<member name=\"M:{type.FullName}.Load(CoreEssentials.Assets.IContentManager)\">", xmlContent);
        }
    }
}
