using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnitV3;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BudgetLens.ArchitectureTests;

/// <summary>
/// Enforces the hexagonal dependency rules from CLAUDE.md. Project references already stop the
/// inner layers from seeing the outer ones; these tests also catch transitive access
/// (e.g. Mcp using Domain types through Application) that the compiler allows.
/// </summary>
public class LayerDependencyTests
{
    private static readonly System.Reflection.Assembly DomainAssembly = typeof(Domain.AssemblyMarker).Assembly;
    private static readonly System.Reflection.Assembly ApplicationAssembly = typeof(Application.AssemblyMarker).Assembly;
    private static readonly System.Reflection.Assembly InfrastructureAssembly = typeof(Infrastructure.AssemblyMarker).Assembly;
    private static readonly System.Reflection.Assembly McpAssembly = typeof(Mcp.AssemblyMarker).Assembly;
    private static readonly System.Reflection.Assembly ApiAssembly = typeof(Program).Assembly;

    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(DomainAssembly, ApplicationAssembly, InfrastructureAssembly, McpAssembly, ApiAssembly)
        .Build();

    private static readonly IObjectProvider<IType> DomainLayer =
        Types().That().ResideInAssembly(DomainAssembly).As("Domain");

    private static readonly IObjectProvider<IType> ApplicationLayer =
        Types().That().ResideInAssembly(ApplicationAssembly).As("Application");

    private static readonly IObjectProvider<IType> InfrastructureLayer =
        Types().That().ResideInAssembly(InfrastructureAssembly).As("Infrastructure");

    private static readonly IObjectProvider<IType> McpLayer =
        Types().That().ResideInAssembly(McpAssembly).As("Mcp");

    private static readonly IObjectProvider<IType> ApiLayer =
        Types().That().ResideInAssembly(ApiAssembly).As("Api");

    [Fact]
    public void Domain_depends_on_no_other_layer() =>
        Types().That().Are(DomainLayer)
            .Should().NotDependOnAny(ApplicationLayer)
            .AndShould().NotDependOnAny(InfrastructureLayer)
            .AndShould().NotDependOnAny(McpLayer)
            .AndShould().NotDependOnAny(ApiLayer)
            .Check(Architecture);

    [Fact]
    public void Application_depends_only_on_Domain() =>
        Types().That().Are(ApplicationLayer)
            .Should().NotDependOnAny(InfrastructureLayer)
            .AndShould().NotDependOnAny(McpLayer)
            .AndShould().NotDependOnAny(ApiLayer)
            .Check(Architecture);

    [Fact]
    public void Infrastructure_does_not_depend_on_driving_adapters() =>
        Types().That().Are(InfrastructureLayer)
            .Should().NotDependOnAny(McpLayer)
            .AndShould().NotDependOnAny(ApiLayer)
            .Check(Architecture);

    [Fact]
    public void Mcp_uses_Application_only() =>
        Types().That().Are(McpLayer)
            .Should().NotDependOnAny(DomainLayer)
            .AndShould().NotDependOnAny(InfrastructureLayer)
            .AndShould().NotDependOnAny(ApiLayer)
            .Because("MCP tools call Application use cases and return their DTOs")
            .Check(Architecture);

    [Fact]
    public void Api_does_not_use_Domain_types() =>
        Types().That().Are(ApiLayer)
            .Should().NotDependOnAny(DomainLayer)
            .Because("gRPC services call Application use cases and map their DTOs to Protobuf messages")
            .Check(Architecture);

    [Fact]
    public void Only_the_composition_root_wires_Infrastructure() =>
        Types().That().Are(ApiLayer)
            // Program (top-level statements) and the compiler-generated types nested in it.
            .And().DoNotHaveFullNameMatching(@"^Program(\+.*)?$")
            .Should().NotDependOnAny(InfrastructureLayer)
            .Because("Infrastructure is wired exclusively in the Api composition root")
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
}
