using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace rbkApiModules.Analysers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EndpointProducesAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "RBK001";

        private static readonly LocalizableString Title = "Missing Produces<T> on endpoint";
        private static readonly LocalizableString MessageFormat = "Endpoint for '{0} {1}' should declare .Produces<T>() to specify the return type";
        private static readonly LocalizableString Description = "Ensure that the endpoint declares Produces<T> with the actual returned type.";
        private const string Category = "Swagger";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            
            // Check if this is a MapPost, MapPut, MapGet or MapDelete call (either direct or in a chain)
            var mapHttpCall = FindMapHttpCall(invocation);
            if (mapHttpCall == null)
            {
                return;
            }

            // Verify receiver is IEndpointRouteBuilder
            var receiverType = context.SemanticModel.GetTypeInfo(mapHttpCall.Expression).Type;
            if (receiverType == null || !receiverType.AllInterfaces.Any(i =>
                i.Name == "IEndpointRouteBuilder" &&
                i.ContainingNamespace.ToDisplayString() == "Microsoft.AspNetCore.Builder"))
            {
                return;
            }

            // Find the parent method that contains this MapXXX call
            var parentMethod = invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (parentMethod?.Body == null)
            {
                return;
            }

            // Check if there's a .Produces<T>() call in the same method
            var producesFound = parentMethod.Body.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(inv =>
                {
                    if (inv.Expression is MemberAccessExpressionSyntax macc &&
                        macc.Name is GenericNameSyntax gns &&
                        gns.Identifier.Text == "Produces" &&
                        gns.TypeArgumentList.Arguments.Count == 1)
                    {
                        return true;
                    }
                    return false;
                })
                .Any();

            if (!producesFound)
            {
                // Get the route template from the MapXXX call
                var routeArg = mapHttpCall.ArgumentList.Arguments.FirstOrDefault()?.Expression;
                string routeTemplate = "unknown";
                
                if (routeArg is LiteralExpressionSyntax literal)
                {
                    routeTemplate = literal.Token.ValueText;
                }

                var diag = Diagnostic.Create(Rule, mapHttpCall.Expression.GetLocation(), routeTemplate);
                context.ReportDiagnostic(diag);
            }
        }

        private InvocationExpressionSyntax FindMapHttpCall(InvocationExpressionSyntax invocation)
        {
            // System.Diagnostics.Debugger.Launch();

            var supportedMethods = new[] { "MapPost", "MapGet", "MapPut", "MapDelete" };
            
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                supportedMethods.Contains(memberAccess.Name.Identifier.Text))
            {
                return invocation;
            }

            // Check if this is part of a method chain that starts with MapXXX
            var current = invocation;
            while (current != null)
            {
                if (current.Expression is MemberAccessExpressionSyntax macc &&
                    supportedMethods.Contains(macc.Name.Identifier.Text))
                {
                    return current;
                }

                // Move up the chain
                if (current.Parent is InvocationExpressionSyntax parentInvocation)
                {
                    current = parentInvocation;
                }
                else
                {
                    break;
                }
            }

            return null;
        }

        private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;

            // Only analyze methods that might contain endpoints
            if (methodDeclaration.Body == null)
            {
                return;
            }

            // Look for MapXXX calls in the method body
            var mapHttpCalls = methodDeclaration.Body.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(invocation => FindMapHttpCall(invocation) != null)
                .ToList();

            foreach (var mapHttpCall in mapHttpCalls)
            {
                var actualMapHttpCall = FindMapHttpCall(mapHttpCall);
                if (actualMapHttpCall == null)
                {
                    continue;
                }

                // Check if there's a .Produces<T>() call in the same method
                var producesFound = methodDeclaration.Body.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Where(inv =>
                    {
                        if (inv.Expression is MemberAccessExpressionSyntax macc &&
                            macc.Name is GenericNameSyntax gns &&
                            gns.Identifier.Text == "Produces" &&
                            gns.TypeArgumentList.Arguments.Count == 1)
                        {
                            return true;
                        }
                        return false;
                    })
                    .Any();

                if (!producesFound)
                {
                    // Get the route template from the MapXXX call
                    var routeArg = actualMapHttpCall.ArgumentList.Arguments.FirstOrDefault()?.Expression;
                    string routeTemplate = "unknown";
                    
                    if (routeArg is LiteralExpressionSyntax literal)
                    {
                        routeTemplate = literal.Token.ValueText;
                    }

                    var httpMethod = ((MemberAccessExpressionSyntax)actualMapHttpCall.Expression).Name.Identifier.Text.Replace("Map", "").ToUpper();

                    var diag = Diagnostic.Create(Rule,
                        actualMapHttpCall.Expression.GetLocation(),
                        httpMethod,
                        routeTemplate);
                    context.ReportDiagnostic(diag);
                }
            }
        }
    }
}