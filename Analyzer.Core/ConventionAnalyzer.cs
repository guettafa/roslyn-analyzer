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
    public class ConventionAnalyzer : DiagnosticAnalyzer
    {
        #region Diagnostic Descriptors Related

        private readonly static DiagnosticDescriptor descClassPrivMember = 
            new DiagnosticDescriptor("NMCVPRVM", "Private Class Members should start with an underscore '_' Ex : _symbol", "Wrong name for symbol", "Naming Convention", DiagnosticSeverity.Warning, isEnabledByDefault: true);

        private readonly static DiagnosticDescriptor descClassPublicProperty =
            new DiagnosticDescriptor("NMCVPUBP", "Public Class Property symbol should be in 'PascalCase' Ex : MyProperty", "This property name does not satisfy the convention", "Naming Convention", DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            ImmutableArray.Create(
                descClassPrivMember,
                descClassPublicProperty
            );

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzePrivateMembers, SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeAction(AnalyzePublicProperties, SyntaxKind.PropertyDeclaration);
        }

        #endregion

        #region Analyzers

        private void AnalyzePrivateMembers(SyntaxNodeAnalysisContext context)
        {
            var node = (InvocationExpressionSyntax) context.Node;

            // Method Invocation have more than 1 args
            if (node.ArgumentList.Arguments.Count != 1) 
                return;

            // Is not a member access expression
            if (!(node.Expression is MemberAccessExpressionSyntax addAccess) || addAccess.Name.Identifier.Text != "Add")
                return;

            if (!(addAccess.Expression is MemberAccessExpressionSyntax emptyAccess) || emptyAccess.Name.Identifier.Text != "Empty")
                return;

            if (!(emptyAccess.Expression is GenericNameSyntax immutableArray) || immutableArray.TypeArgumentList.Arguments.Count != 1 || immutableArray.Identifier.Text != "ImmutableArray")
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                descClassPrivMember,
                node.GetLocation()));
        }

        /// <summary>
        /// Analyze Class Properties Naming to check if they 
        /// follow the convention - Naming is PascalCase
        /// </summary>
        /// <param name="context"></param>
        private void AnalyzePublicProperties(SyntaxNodeAnalysisContext context)
        {
            var property = (PropertyDeclarationSyntax) context.Node;

            // if Class Property is not Public
            if (!property.Modifiers.Any(SyntaxKind.PublicKeyword))
                return;

            // if Class Property start with a Upper and next char is lower
            if (char.IsUpper(property.Identifier.Text[0]) && char.IsLower(property.Identifier.Text[1]))
                return;

            context.ReportDiagnostic(Diagnostic.Create(
                descClassPublicProperty,
                property.GetLocation()));

        }

        #endregion
    }
}
