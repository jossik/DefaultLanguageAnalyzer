using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace DefaultLanguageAnalyzer
{
	/// <summary>
	/// Content copied from https://gist.github.com/Wolfsblvt/5bc282d1b565238058cc645bcf888db7
	/// </summary>
	public class DeferredCodeAction : CodeAction
	{
		private readonly Func<CancellationToken, bool, Task<Solution>> createChangedSolution;

		public override string Title { get; }

		public override string EquivalenceKey { get; }

		protected DeferredCodeAction(string title, Func<CancellationToken, bool, Task<Solution>> createChangedSolution, string equivalenceKey = null)
		{
			this.createChangedSolution = createChangedSolution;
			Title = title;
			EquivalenceKey = equivalenceKey;
		}

		/// <summary>
		/// Creates a <see cref="DeferredCodeAction" />.
		/// Use this factory when the change is expensive to compute and should be deferred until requested.
		/// </summary>
		/// <param name="title">Title of the <see cref="DeferredCodeAction" />.</param>
		/// <param name="createChangedSolution">Function to create the <see cref="Solution" />.</param>
		/// <param name="equivalenceKey">Optional value used to determine the equivalence of the <see cref="DeferredCodeAction" /> with other <see cref="DeferredCodeAction" />s. See <see cref="DeferredCodeAction.EquivalenceKey" />.</param>
		public static DeferredCodeAction Create(string title, Func<CancellationToken, bool, Task<Solution>> createChangedSolution, string equivalenceKey = null)
		{
			if (title == null)
			{
				throw new ArgumentNullException(nameof(title));
			}

			if (createChangedSolution == null)
			{
				throw new ArgumentNullException(nameof(createChangedSolution));
			}

			return new DeferredCodeAction(title, createChangedSolution, equivalenceKey);
		}

		protected override async Task<IEnumerable<CodeActionOperation>> ComputePreviewOperationsAsync(CancellationToken cancellationToken)
		{
			const bool isPreview = true;

			// Content copied from http://source.roslyn.io/#Microsoft.CodeAnalysis.Workspaces/CodeActions/CodeAction.cs,81b0a0866b894b0e
			var changedSolution = await GetChangedSolutionWithPreviewAsync(cancellationToken, isPreview).ConfigureAwait(false);
			if (changedSolution == null)
			{
				return null;
			}

			return new CodeActionOperation[] { new ApplyChangesOperation(changedSolution) };
		}

		protected override Task<Solution> GetChangedSolutionAsync(CancellationToken cancellationToken)
		{
			const bool isPreview = false;
			return GetChangedSolutionWithPreviewAsync(cancellationToken, isPreview);
		}

		protected virtual Task<Solution> GetChangedSolutionWithPreviewAsync(CancellationToken cancellationToken, bool isPreview)
		{
			return createChangedSolution(cancellationToken, isPreview);
		}

		protected virtual IEnumerable<FixAllScope> GetSupportedFixAllScopes()
		{
			return ImmutableArray.Create(FixAllScope.Document);
		}
	}
}
