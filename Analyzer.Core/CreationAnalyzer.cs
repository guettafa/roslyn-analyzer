using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzer.Core
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class CreationAnalyzer : DiagnosticAnalyzer
    {
        private readonly static DiagnosticDescriptor diagnosticDescriptorImmu = 
            new DiagnosticDescriptor(
                "BadWayImmutableArray",
                "Bad Way of creating immutable array",
                "Bad Way of creating immutable array",
                "Immutable arrays",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            ImmutableArray.Create(
                diagnosticDescriptorImmu
            );

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                Analyze,
                SyntaxKind.InvocationExpression);
        }

        private void Analyze(SyntaxNodeAnalysisContext context)
        {
            var node = (InvocationExpressionSyntax) context.Node;

            // Method Invocation have more than 1 args
            if (node.ArgumentList.Arguments.Count != 1) 
                return;

            // Is not a  member access expression
            if (!(node.Expression is MemberAccessExpressionSyntax addAccess) || addAccess.Name.Identifier.Text != "Add")
                return;

            if (!(addAccess.Expression is MemberAccessExpressionSyntax emptyAccess) || emptyAccess.Name.Identifier.Text != "Empty")
                return;

            if (!(emptyAccess.Expression is GenericNameSyntax immutableArray) || immutableArray.TypeArgumentList.Arguments.Count != 1 || immutableArray.Identifier.Text != "ImmutableArray")
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                diagnosticDescriptorImmu,
                node.GetLocation()));

        }
    }
}
