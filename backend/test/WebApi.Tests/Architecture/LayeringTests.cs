// Verifies architecture boundaries that should remain true across backend changes.
using System.Reflection;

namespace WebApi.Tests.Architecture;

public sealed class LayeringTests
{
    private static readonly Assembly WebApiAssembly = typeof(Program).Assembly;

    [Fact]
    public void Domain_types_do_not_depend_on_infrastructure_or_presentation()
    {
        var forbiddenNamespaces = new[]
        {
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore",
            "WebApi.Infrastructure",
            "WebApi.Presentation"
        };

        var domainTypes = WebApiAssembly.GetTypes()
            .Where(type => type.Namespace?.StartsWith("WebApi.Domain", StringComparison.Ordinal) == true);

        var violations = domainTypes
            .SelectMany(type => GetReferencedTypes(type).Select(reference => new { Type = type, Reference = reference }))
            .Where(item => forbiddenNamespaces.Any(ns =>
                item.Reference.Namespace?.StartsWith(ns, StringComparison.Ordinal) == true))
            .Select(item => $"{item.Type.FullName} -> {item.Reference.FullName}")
            .ToArray();

        Assert.Empty(violations);
    }

    private static IEnumerable<Type> GetReferencedTypes(Type type)
    {
        return type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
            .SelectMany(GetMemberReferencedTypes)
            .Where(reference => reference is not null)!;
    }

    private static IEnumerable<Type?> GetMemberReferencedTypes(MemberInfo member)
    {
        return member switch
        {
            FieldInfo field => [field.FieldType],
            PropertyInfo property => [property.PropertyType],
            MethodInfo method => method.GetParameters().Select(parameter => parameter.ParameterType)
                .Append(method.ReturnType),
            ConstructorInfo constructor => constructor.GetParameters().Select(parameter => parameter.ParameterType),
            _ => []
        };
    }
}