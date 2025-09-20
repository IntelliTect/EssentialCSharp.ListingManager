using Xunit;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace EssentialCSharp.ListingManager.Tests;

public class GlobalToolTests
{
    [Fact]
    public void ProjectFile_IsConfiguredAsGlobalTool()
    {
        // Arrange
        string projectDirectory = GetProjectDirectory();
        // Navigate up from test bin directory to find the project file
        var testDir = new DirectoryInfo(projectDirectory);
        var solutionDir = testDir.Parent?.Parent?.Parent?.Parent; // Go up from bin/Debug/net8.0/
        string projectFilePath = Path.Combine(solutionDir?.FullName ?? throw new InvalidOperationException("Could not find solution directory"), "ListingManager", "EssentialCSharp.ListingManager.csproj");
        
        // Act & Assert
        Assert.True(File.Exists(projectFilePath), $"Project file not found at: {projectFilePath}");
        
        XDocument projectDoc = XDocument.Load(projectFilePath);
        
        var packAsToolElement = projectDoc.Descendants("PackAsTool").FirstOrDefault();
        Assert.NotNull(packAsToolElement);
        Assert.Equal("true", packAsToolElement.Value, ignoreCase: true);
        
        var toolCommandNameElement = projectDoc.Descendants("ToolCommandName").FirstOrDefault();
        Assert.NotNull(toolCommandNameElement);
        Assert.Equal("ListingManager", toolCommandNameElement.Value);
        
        var packageIdElement = projectDoc.Descendants("PackageId").FirstOrDefault();
        Assert.NotNull(packageIdElement);
        Assert.Equal("IntelliTect.EssentialCSharp.ListingManager", packageIdElement.Value);
        
        var outputTypeElement = projectDoc.Descendants("OutputType").FirstOrDefault();
        Assert.NotNull(outputTypeElement);
        Assert.Equal("Exe", outputTypeElement.Value);
    }

    [Fact]
    public void Assembly_HasCorrectEntryPoint()
    {
        // Arrange
        var assembly = typeof(Program).Assembly;
        
        // Act & Assert
        var entryPoint = assembly.EntryPoint;
        Assert.NotNull(entryPoint);
        // In modern C#, the entry point may be synthesized as "<Main>$" or "<Main>" for top-level programs
        Assert.True(entryPoint.Name == "Main" || entryPoint.Name == "<Main>$" || entryPoint.Name == "<Main>", 
            $"Expected entry point name to be 'Main', '<Main>$', or '<Main>', but was '{entryPoint.Name}'");
        Assert.Equal(typeof(Program), entryPoint.DeclaringType);
    }

    private static string GetProjectDirectory()
    {
        // Get the directory where the test assembly is located
        string assemblyLocation = Assembly.GetExecutingAssembly().Location;
        return Path.GetDirectoryName(assemblyLocation) ?? throw new InvalidOperationException("Could not determine project directory");
    }
}