using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace rbkApiModules.Analysers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EndpointProducesAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "RBK001";

        private static readonly LocalizableString Title = "Missing Produces<T> on endpoint";
        private static readonly LocalizableString MessageFormat = "Endpoint for '{0}' should declare .Produces<T>() to specify the return type";
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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

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
            
            // Check if this is a MapPost call (either direct or in a chain)
            var mapPostCall = FindMapPostCall(invocation);
            if (mapPostCall == null)
                return;

            // Verify receiver is IEndpointRouteBuilder
            var receiverType = context.SemanticModel.GetTypeInfo(mapPostCall.Expression).Type;
            if (receiverType == null || !receiverType.AllInterfaces.Any(i =>
                i.Name == "IEndpointRouteBuilder" &&
                i.ContainingNamespace.ToDisplayString() == "Microsoft.AspNetCore.Builder"))
            {
                return;
            }

            // Find the parent method that contains this MapPost call
            var parentMethod = invocation.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            if (parentMethod?.Body == null)
                return;

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
                // Get the route template from the MapPost call
                var routeArg = mapPostCall.ArgumentList.Arguments.FirstOrDefault()?.Expression;
                string routeTemplate = "unknown";
                
                if (routeArg is LiteralExpressionSyntax literal)
                {
                    routeTemplate = literal.Token.ValueText;
                }

                var diag = Diagnostic.Create(Rule,
                    mapPostCall.Expression.GetLocation(),
                    routeTemplate);
                context.ReportDiagnostic(diag);
            }
        }

        private InvocationExpressionSyntax FindMapPostCall(InvocationExpressionSyntax invocation)
        {
            // Check if this invocation is a MapPost call
            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name.Identifier.Text == "MapPost")
            {
                return invocation;
            }

            // Check if this is part of a method chain that starts with MapPost
            var current = invocation;
            while (current != null)
            {
                if (current.Expression is MemberAccessExpressionSyntax macc &&
                    macc.Name.Identifier.Text == "MapPost")
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
                return;

            // Look for MapPost calls in the method body
            var mapPostCalls = methodDeclaration.Body.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(inv => FindMapPostCall(inv) != null)
                .ToList();

            foreach (var mapPostCall in mapPostCalls)
            {
                var actualMapPostCall = FindMapPostCall(mapPostCall);
                if (actualMapPostCall == null)
                    continue;

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
                    // Get the route template from the MapPost call
                    var routeArg = actualMapPostCall.ArgumentList.Arguments.FirstOrDefault()?.Expression;
                    string routeTemplate = "unknown";
                    
                    if (routeArg is LiteralExpressionSyntax literal)
                    {
                        routeTemplate = literal.Token.ValueText;
                    }

                    var diag = Diagnostic.Create(Rule,
                        actualMapPostCall.Expression.GetLocation(),
                        routeTemplate);
                    context.ReportDiagnostic(diag);
                }
            }
        }
    }
}