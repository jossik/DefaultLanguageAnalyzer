using System.Collections.Immutable;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DefaultLanguageAnalyzer
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DefaultLanguageCodeFixProvider)), Shared]
	public class DefaultLanguageCodeFixProvider : CodeFixProvider
	{
		private const string title = "Restore the default language";

		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create(DefaultLanguageAnalyzer.DiagnosticId); }
		}

		public sealed override FixAllProvider GetFixAllProvider()
		{
			// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
			return WellKnownFixAllProviders.BatchFixer;
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

			context.RegisterCodeFix(DeferredCodeAction.Create(title, (c, isPreview) => SetDefaultLanguageAsync(context.Document, declaration, isPreview), equivalenceKey: title), diagnostic);
		}

		private async Task<Solution> SetDefaultLanguageAsync(Document document, TypeDeclarationSyntax typeDecl, bool isPreview)
		{
			if (!isPreview)
			{
				var identifierToken = typeDecl.Identifier;
				var typeName = identifierToken.Text;

				var filePath = identifierToken.SyntaxTree.FilePath;
				var fileDirectory = Path.GetDirectoryName(filePath);

				var resxPath = Directory.GetFiles(fileDirectory, $"{typeName}.resx").FirstOrDefault();
				if (resxPath != null)
				{
					var resxLines = File.ReadAllLines(resxPath);

					var header = resxLines.TakeWhile(s => !s.Contains("$this.Language")).ToArray();
					var bottom = resxLines.Skip(header.Length).SkipWhile(s => !s.Contains("</metadata>")).Skip(1).ToArray();
					var newLines = header.Concat(bottom);

					File.WriteAllLines(resxPath, newLines);
				}
			}

			return await Task.FromResult(document.Project.Solution);
		}
	}
}
