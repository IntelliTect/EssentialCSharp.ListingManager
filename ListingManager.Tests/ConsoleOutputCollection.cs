using Xunit;

namespace EssentialCSharp.ListingManager.Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ConsoleOutputCollection
{
    public const string Name = "Console output collection";
}
