﻿//
// RegexContextHandler.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2015 Xamarin Inc. (http://xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using MonoDevelop.Ide.CodeCompletion;
using MonoDevelop.Ide.TypeSystem;
using Microsoft.CodeAnalysis.Text;
using ICSharpCode.NRefactory6.CSharp.Analysis;

namespace ICSharpCode.NRefactory6.CSharp.Completion
{	
	class RegexContextHandler : CompletionContextHandler
	{
		public override bool IsTriggerCharacter (Microsoft.CodeAnalysis.Text.SourceText text, int position)
		{
			var ch = text [position];
			return ch == '\\';
		}

		protected async override Task<IEnumerable<CompletionData>> GetItemsWorkerAsync (CompletionResult completionResult, CompletionEngine engine, CompletionContext completionContext, CompletionTriggerInfo info, SyntaxContext ctx, CancellationToken cancellationToken)
		{
			var document = completionContext.Document;
			var position = completionContext.Position;
			var semanticModel = ctx.SemanticModel;

			if (ctx.TargetToken.Parent != null && ctx.TargetToken.Parent.Parent != null && 
			    ctx.TargetToken.Parent.Parent.IsKind(SyntaxKind.Argument)) {
				var argument = ctx.TargetToken.Parent.Parent as ArgumentSyntax;

				var symbolInfo = semanticModel.GetSymbolInfo (ctx.TargetToken.Parent.Parent.Parent.Parent);
				if (symbolInfo.Symbol == null)
					return Enumerable.Empty<CompletionData> ();
				
				if (SemanticHighlightingVisitor<int>.IsRegexMatchMethod (symbolInfo)) {
					if (((ArgumentListSyntax)argument.Parent).Arguments[1] != argument)
						return Enumerable.Empty<CompletionData> ();
					completionResult.AutoSelect = false;
					return GetFormatCompletionData(engine, argument.Expression.ToString ()[0] == '@');
				}
				if (SemanticHighlightingVisitor<int>.IsRegexConstructor (symbolInfo)) {
					if (((ArgumentListSyntax)argument.Parent).Arguments[0] != argument)
						return Enumerable.Empty<CompletionData> ();
					completionResult.AutoSelect = false;
					return GetFormatCompletionData(engine, argument.Expression.ToString ()[0] == '@');
				}

			}

			return Enumerable.Empty<CompletionData> ();
		}

		IEnumerable<CompletionData> GetFormatCompletionData (CompletionEngine engine, bool isVerbatimString)
		{
			yield return engine.Factory.CreateFormatItemCompletionData(this, (isVerbatimString ? "" : "\\") +"d", "Digit character", null);
			yield return engine.Factory.CreateFormatItemCompletionData(this, (isVerbatimString ? "" : "\\") +"D", "Non-digit character", null);

			yield return engine.Factory.CreateFormatItemCompletionData(this, (isVerbatimString ? "" : "\\") +"b", "Word boundary", null);
			yield return engine.Factory.CreateFormatItemCompletionData(this, (isVerbatimString ? "" : "\\") +"B", "Non-word boundary", null);

			yield return engine.Factory.CreateFormatItemCompletionData(this, (isVerbatimString ? "" : "\\") +"w", "Word character", null);
			yield return engine.Factory.CreateFormatItemCompletionData(this, (isVerbatimString ? "" : "\\") +"W", "Non-word character", null);

			yield return engine.Factory.CreateFormatItemCompletionData(this, (isVerbatimString ? "" : "\\") +"s", "White-space character", null);
			yield return engine.Factory.CreateFormatItemCompletionData(this, (isVerbatimString ? "" : "\\") +"S", "Non-white-space character", null);

			yield return engine.Factory.CreateFormatItemCompletionData(this, (isVerbatimString ? "" : "\\") +"A", "Start boundary", null);
			yield return engine.Factory.CreateFormatItemCompletionData(this, (isVerbatimString ? "" : "\\") +"Z", "End boundary", null);

			yield return engine.Factory.CreateFormatItemCompletionData(this, (isVerbatimString ? "" : "\\") +"k<name>", "Named backreference", null);
			yield return engine.Factory.CreateFormatItemCompletionData(this, (isVerbatimString ? "" : "\\") +"P{name}", "Negative unicode category or unicode block", null);
			yield return engine.Factory.CreateFormatItemCompletionData(this, (isVerbatimString ? "" : "\\") +"p{name}", "Unicode category or unicode block", null);
		}
	}
}