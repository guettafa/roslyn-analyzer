using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using Analyzer.Core;
using System;
using System.Linq;
using System.Reflection;

namespace Analyzer.Test
{
    public class Tests
    {
        public Solution solution;
        public ProjectId projectId;

        #region Setup & TearDown

        [SetUp]
        public void Setup()
        {
            AdhocWorkspace workspace = new AdhocWorkspace();

            projectId = ProjectId.CreateNewId();

            solution = workspace.CurrentSolution
               .AddProject(projectId, "MyTestProject", "MyTestProject", LanguageNames.CSharp);
        }

        #endregion

        #region Tests

        [Test]
        public async Task When_PublicClassPropertyWrongNaming_HasDiagnostic()
        {
            const string code = @"
                internal class TestClass
                {
                    public string myProperty { get; set; }

                    public static void Main(string[] args) {}
                }
            ";

            var diagnostics = await GetDiagnostics(code);
            var diagnostic = diagnostics[0];
            var locationSpanDiagnostic = diagnostic.Location.GetLineSpan();

            Assert.Multiple(() =>
            {
                Assert.That(diagnostics.Length, Is.EqualTo(1));
                Assert.That(diagnostic.Id, Is.EqualTo("NMCVPUBP"));
                Assert.That(diagnostic.Descriptor.DefaultSeverity, Is.EqualTo(DiagnosticSeverity.Warning));
            });
        }

        #endregion

        #region Utility for Diagnostics 

        private async Task<ImmutableArray<Diagnostic>> GetDiagnostics(string code)
        {
            solution = solution.AddDocument(DocumentId.CreateNewId(projectId), "File.cs", code);

            var project = solution.GetProject(projectId)
                .AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddMetadataReferences(GetAllReferencesNeededForType(typeof(ImmutableArray)));

            var compilation = await project.GetCompilationAsync();

            var compilationWithAnalyzers = compilation.WithAnalyzers(
                ImmutableArray.Create<DiagnosticAnalyzer>(
                    new ConventionAnalyzer())
                );

            return await compilationWithAnalyzers.GetAllDiagnosticsAsync();
        }

        #endregion

        #region Utility for references

        private static MetadataReference[] GetAllReferencesNeededForType(Type type)
        {
            var files = GetAllAssemblyFilesNeededForType(type);

            return files.Select(x => MetadataReference.CreateFromFile(x)).Cast<MetadataReference>().ToArray();
        }

        private static ImmutableArray<string> GetAllAssemblyFilesNeededForType(Type type)
        {
            return type.Assembly.GetReferencedAssemblies()
                .Select(x => Assembly.Load(x.FullName))
                .Append(type.Assembly)
                .Select(x => x.Location)
                .ToImmutableArray();
        }

        #endregion
    }
}