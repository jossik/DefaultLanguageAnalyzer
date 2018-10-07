using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DefaultLanguageAnalyzer
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DefaultLanguageAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "A0001";

		private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.DefaultLanguage_Title), Resources.ResourceManager, typeof(Resources));
		private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.DefaultLanguage_MessageFormat), Resources.ResourceManager, typeof(Resources));
		private const string Category = "Naming";

		private static readonly string[] IgnoreFileEndings =
		{
			".Designer.cs"
		};

		private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterCompilationStartAction(compilationStartContext =>
			{
				var resourceFiles = compilationStartContext.Options.AdditionalFiles;

				foreach (var file in resourceFiles)
				{
					var fileText = file.GetText(compilationStartContext.CancellationToken);
					if (fileText.ToString().Contains("$this.Language"))
					{
						var fileName = Path.GetFileNameWithoutExtension(file.Path);

						compilationStartContext.RegisterSymbolAction(symbolAnalysisContext =>
						{
							var namedTypeSymbol = symbolAnalysisContext.Symbol;
							var symbolName = namedTypeSymbol.Name;

							if (symbolName == fileName)
							{
								var locations = namedTypeSymbol.Locations.Where(l => IgnoreFileEndings.All(f => !l.SourceTree.FilePath.ToLower().EndsWith(f.ToLower())));

								foreach (var location in locations)
								{
									var diagnostic = Diagnostic.Create(Rule, location, namedTypeSymbol.Name);
									symbolAnalysisContext.ReportDiagnostic(diagnostic);
								}
							}
						},
						SymbolKind.NamedType);
					}
				}
			});
		}
	}
}
