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

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = "Missing Produces<T> on endpoint";
        private static readonly LocalizableString MessageFormat = "Endpoint for '{0}' returns '{1}' but does not declare .Produces<{1}>()";
        private static readonly LocalizableString Description = "Ensure that the endpoint declares Produces<T> with the actual returned type.";
        private const string Category = "Swagger";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticId,
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess))
                return;

            // Only consider .MapPost calls
            if (memberAccess.Name.Identifier.Text != "MapPost")
                return;

            // Verify receiver is IEndpointRouteBuilder
            var receiverType = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;
            if (receiverType == null || !receiverType.AllInterfaces.Any(i =>
                i.Name == "IEndpointRouteBuilder" &&
                i.ContainingNamespace.ToDisplayString() == "Microsoft.AspNetCore.Builder"))
            {
                return;
            }

            // Find the lambda passed to MapPost
            var lambda = invocation.ArgumentList.Arguments
                .Select(arg => arg.Expression)
                .OfType<LambdaExpressionSyntax>()
                .FirstOrDefault();
            if (lambda == null)
                return;

            // Extract block body
            var block = lambda.Body as BlockSyntax;
            if (block == null)
                return;

            // Find 'var result = await dispatcher.SendAsync(...);'
            var resultDecl = block.Statements
                .OfType<LocalDeclarationStatementSyntax>()
                .Select(ld => new
                {
                    Statement = ld,
                    Variable = ld.Declaration.Variables.FirstOrDefault()
                })
                .Where(x => x.Variable?.Identifier.Text == "result")
                .Select(x => new
                {
                    x.Statement,
                    SendInvocation = (x.Variable.Initializer?.Value as AwaitExpressionSyntax)?.Expression as InvocationExpressionSyntax
                })
                .Where(x => x.SendInvocation != null &&
                            x.SendInvocation.Expression is MemberAccessExpressionSyntax ma &&
                            ma.Name.Identifier.Text == "SendAsync")
                .FirstOrDefault();

            if (resultDecl == null)
                return;

            var sendInvocation = resultDecl.SendInvocation;
            // Resolve request type argument
            var requestArg = sendInvocation.ArgumentList.Arguments.FirstOrDefault()?.Expression;
            if (requestArg == null)
                return;
            var requestType = context.SemanticModel.GetTypeInfo(requestArg).Type;
            if (requestType == null)
                return;

            // Find handler implementing ICommandHandler<requestType>
            var handlerInterface = context.Compilation.GetTypeByMetadataName("rbkApiModules.Commons.Core.ICommandHandler`1");
            if (handlerInterface == null)
                return;

            var handlerTypes = context.Compilation.GlobalNamespace.GetNamespaceMembers()
                .SelectMany(ns => ns.GetTypeMembers())
                .Where(t => t.AllInterfaces.Any(i =>
                    i.OriginalDefinition.Equals(handlerInterface, SymbolEqualityComparer.Default) &&
                    SymbolEqualityComparer.Default.Equals(i.TypeArguments[0], requestType)))
                .ToList();
            if (handlerTypes.Count != 1)
                return;

            var handlerType = handlerTypes[0];
            var handleMethod = handlerType.GetMembers()
                .OfType<IMethodSymbol>()
                .FirstOrDefault(m => m.Name == "HandleAsync"
                                     && m.Parameters.Length == 2);
            if (handleMethod == null)
                return;

            // Get the syntax of HandleAsync to inspect the return statement
            var syntaxRef = handleMethod.DeclaringSyntaxReferences.FirstOrDefault();
            if (syntaxRef == null)
                return;

            var methodSyntax = syntaxRef.GetSyntax(context.CancellationToken) as MethodDeclarationSyntax;
            if (methodSyntax?.Body == null)
                return;

            var handlerModel = context.Compilation.GetSemanticModel(methodSyntax.SyntaxTree);
            var returnStmt = methodSyntax.Body.Statements
                .OfType<ReturnStatementSyntax>()
                .FirstOrDefault();
            if (returnStmt == null)
                return;

            var returnInv = returnStmt.Expression as InvocationExpressionSyntax;
            if (returnInv == null)
                return;

            var returnAccess = returnInv.Expression as MemberAccessExpressionSyntax;
            if (returnAccess == null || returnAccess.Name.Identifier.Text != "Success")
                return;

            // The argument to CommandResponse.Success(...) is the object we want
            var dataExpr = returnInv.ArgumentList.Arguments.FirstOrDefault()?.Expression;
            if (dataExpr == null)
                return;
            var dataType = handlerModel.GetTypeInfo(dataExpr).Type;
            if (dataType == null)
                return;

            // Check for a matching .Produces<T>() in the full chain
            var stmt = invocation.Ancestors().OfType<ExpressionStatementSyntax>().FirstOrDefault();
            if (stmt == null)
                return;

            var producesFound = stmt.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(inv =>
                {
                    if (inv.Expression is MemberAccessExpressionSyntax macc &&
                        macc.Name is GenericNameSyntax gns &&
                        gns.Identifier.Text == "Produces" &&
                        gns.TypeArgumentList.Arguments.Count == 1)
                    {
                        var ms = context.SemanticModel.GetSymbolInfo(inv).Symbol as IMethodSymbol;
                        return ms != null && SymbolEqualityComparer.Default.Equals(ms.TypeArguments[0], dataType);
                    }
                    return false;
                })
                .Any();

            if (!producesFound)
            {
                var actualTypeName = dataType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
                var requestName = requestType.Name;
                var properties = ImmutableDictionary<string, string>.Empty.Add("ActualType", actualTypeName);
                var diag = Diagnostic.Create(Rule,
                    memberAccess.Name.GetLocation(),
                    properties,
                    requestName,
                    actualTypeName);
                context.ReportDiagnostic(diag);
            }
        }
    }
}