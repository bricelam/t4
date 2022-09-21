//
// RequiresProvidesDirectiveProcessor.cs
//
// Author:
//       Mikayla Hutchinson <m.j.hutchinson@gmail.com>
//
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
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
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.Text;

namespace Microsoft.VisualStudio.TextTemplating
{
	/// <summary>
    /// Base class for a directive processor that follows the requires, provides pattern.
    /// </summary>
    public abstract class RequiresProvidesDirectiveProcessor : DirectiveProcessor
	{
		bool isInProcessingRun;
		ITextTemplatingEngineHost host;
		readonly StringBuilder preInitBuffer = new ();
		readonly StringBuilder postInitBuffer = new ();
		readonly StringBuilder codeBuffer = new ();
		CodeDomProvider languageProvider;

		protected RequiresProvidesDirectiveProcessor ()
		{
		}

		/// <summary>
        /// Initializes the processors.
        /// </summary>
        /// <param name="host"></param>
        public override void Initialize (ITextTemplatingEngineHost host)
		{
			base.Initialize (host);
			this.host = host;
		}

		/// <summary>
        /// Method for derived classes to specify the provides parameters they will supply for each directive by putting the default name in the matching dictionary slot.
        /// </summary>
        /// <param name="directiveName"></param>
        /// <param name="providesDictionary"></param>
        protected abstract void InitializeProvidesDictionary (string directiveName, IDictionary<string, string> providesDictionary);

		/// <summary>
        /// Method for derived classes to specify the requires arguments they need for each directive by putting "<null>" in the matching dictionary slot.</null>
        /// </summary>
        /// <param name="directiveName"></param>
        /// <param name="requiresDictionary"></param>
        protected abstract void InitializeRequiresDictionary (string directiveName, IDictionary<string, string> requiresDictionary);

		protected abstract string FriendlyName { get; }

		/// <summary>
        /// Method for derived classes to contribute additively to initialization code for the TextTransformation generated class.
        /// </summary>
        /// <remarks>
        /// Additive code is useful where there are multiple directive processor instances each needing to have some instance-specific initialization.
        /// As GenerateTransformCode can add methods, matching initialization code is often required to call those methods.
        /// This code will be added after the call to the base class.
        /// </remarks>
        /// <param name="directiveName"></param>
        /// <param name="codeBuffer"></param>
        /// <param name="languageProvider"></param>
        /// <param name="requiresArguments"></param>
        /// <param name="providesArguments"></param>
        protected abstract void GeneratePostInitializationCode (string directiveName, StringBuilder codeBuffer, CodeDomProvider languageProvider,
			IDictionary<string, string> requiresArguments, IDictionary<string, string> providesArguments);

		/// <summary>
        /// Method for derived classes to contribute additively to initialization code for the TextTransformation generated class.
        /// </summary>
        /// <remarks>
        /// Additive code is useful where there are multiple directive processor instances each needing to have some instance-specific initialization.
        /// As GenerateTransformCode can add methods, matching initialization code is often required to call those methods.
        /// This code will be added before the call to the base class.
        /// </remarks>
        /// <param name="directiveName"></param>
        /// <param name="codeBuffer"></param>
        /// <param name="languageProvider"></param>
        /// <param name="requiresArguments"></param>
        /// <param name="providesArguments"></param>
        protected abstract void GeneratePreInitializationCode (string directiveName, StringBuilder codeBuffer, CodeDomProvider languageProvider,
			IDictionary<string, string> requiresArguments, IDictionary<string, string> providesArguments);

		/// <summary>
        /// Method for derived classes to generate the code they wish to add to the TextTransformation generated class.
        /// </summary>
        /// <param name="directiveName"></param>
        /// <param name="codeBuffer"></param>
        /// <param name="languageProvider"></param>
        /// <param name="requiresArguments"></param>
        /// <param name="providesArguments"></param>
        protected abstract void GenerateTransformCode (string directiveName, StringBuilder codeBuffer, CodeDomProvider languageProvider,
			IDictionary<string, string> requiresArguments, IDictionary<string, string> providesArguments);

		/// <summary>
        /// Method for derived classes to make any modifications to the dictionaries that they require
        /// </summary>
        /// <param name="directiveName"></param>
        /// <param name="requiresArguments"></param>
        /// <param name="providesArguments"></param>
        protected virtual void PostProcessArguments (string directiveName, IDictionary<string, string> requiresArguments,
			IDictionary<string, string> providesArguments)
		{
		}

		/// <summary>
        /// Gets generated class code.
        /// </summary>
        /// <returns></returns>
        public override string GetClassCodeForProcessingRun ()
		{
			AssertNotProcessing ();
			return codeBuffer.ToString ();
		}

		/// <summary>
        /// Gets list of importt.
        /// </summary>
        /// <returns></returns>
        public override string[] GetImportsForProcessingRun ()
		{
			AssertNotProcessing ();
			return null;
		}

		public override string[] GetReferencesForProcessingRun ()
		{
			AssertNotProcessing ();
			return null;
		}

		/// <summary>
        /// Get the code to contribute to the body of the initialize method of the generated
        /// template processing class as a consequence of the most recent run.
        /// This code will run after the base class' Initialize method
        /// </summary>
        /// <returns></returns>
        public override string GetPostInitializationCodeForProcessingRun ()
		{
			AssertNotProcessing ();
			return postInitBuffer.ToString ();
		}

		/// <summary>
        /// Get the code to contribute to the body of the initialize method of the generated
        /// template processing class as a consequence of the most recent run.
        /// This code will run before the base class' Initialize method
        /// </summary>
        /// <returns></returns>
        public override string GetPreInitializationCodeForProcessingRun ()
		{
			AssertNotProcessing ();
			return preInitBuffer.ToString ();
		}

		/// <summary>
        /// Starts processing run.
        /// </summary>
        /// <param name="languageProvider">Target language provider.</param>
        /// <param name="templateContents">The contents of the template being processed</param>
        /// <param name="errors">colelction to report processing errors in</param>
        public override void StartProcessingRun (CodeDomProvider languageProvider, string templateContents, CompilerErrorCollection errors)
		{
			AssertNotProcessing ();
			isInProcessingRun = true;
			base.StartProcessingRun (languageProvider, templateContents, errors);

			this.languageProvider = languageProvider;
			codeBuffer.Length = 0;
			preInitBuffer.Length = 0;
			postInitBuffer.Length = 0;
		}

		/// <summary>
        /// Finishes template processing.
        /// </summary>
        public override void FinishProcessingRun ()
		{
			isInProcessingRun = false;
		}

		void AssertNotProcessing ()
		{
			if (isInProcessingRun)
				throw new InvalidOperationException ();
		}

		//FIXME: handle escaping
		static IEnumerable<KeyValuePair<string,string>> ParseArgs (string args)
		{
			var pairs = args.Split (';');
			foreach (var p in pairs) {
				int eq = p.IndexOf ('=');
				var k = p.Substring (0, eq);
				var v = p.Substring (eq);
				yield return new KeyValuePair<string, string> (k, v);
			}
		}

		/// <summary>
        /// Processes a single directive.
        /// </summary>
        /// <param name="directiveName">Directive name.</param>
        /// <param name="arguments">Directive arguments.</param>
        public override void ProcessDirective (string directiveName, IDictionary<string, string> arguments)
		{
			if (directiveName == null)
				throw new ArgumentNullException (nameof (directiveName));
			if (arguments == null)
				throw new ArgumentNullException (nameof (arguments));

			var providesDictionary = new Dictionary<string,string> ();
			var requiresDictionary = new Dictionary<string,string> ();

			if (arguments.TryGetValue ("provides", out var provides)) {
				foreach (var arg in ParseArgs (provides)) {
					providesDictionary.Add (arg.Key, arg.Value);
				}
			}

			if (arguments.TryGetValue ("requires", out var requires)) {
				foreach (var arg in ParseArgs (requires)) {
					requiresDictionary.Add (arg.Key, arg.Value);
				}
			}

			InitializeRequiresDictionary (directiveName, requiresDictionary);
			InitializeProvidesDictionary (directiveName, providesDictionary);

			var id = ProvideUniqueId (directiveName, arguments, requiresDictionary, providesDictionary);

			foreach (var req in requiresDictionary) {
				var val = host.ResolveParameterValue (id, FriendlyName, req.Key);
				if (val != null)
					requiresDictionary[req.Key] = val;
				else if (req.Value == null)
					throw new DirectiveProcessorException ("Could not resolve required value '" + req.Key + "'");
			}

			foreach (var req in providesDictionary) {
				var val = host.ResolveParameterValue (id, FriendlyName, req.Key);
				if (val != null)
					providesDictionary[req.Key] = val;
			}

			PostProcessArguments (directiveName, requiresDictionary, providesDictionary);

			GeneratePreInitializationCode (directiveName, preInitBuffer, languageProvider, requiresDictionary, providesDictionary);
			GeneratePostInitializationCode (directiveName, postInitBuffer, languageProvider, requiresDictionary, providesDictionary);
			GenerateTransformCode (directiveName, codeBuffer, languageProvider, requiresDictionary, providesDictionary);
		}

		/// <summary>
        /// Provide a token to uniquely identify this instance of a directive processor
        /// </summary>
        /// <remarks>
        /// By default, allow an ID parameter to be used on the directive.
        /// Frequently, directive processors would choose to use one of their Provides parameters
        /// </remarks>
        /// <returns>A unique id for this directive instance</returns>
        protected virtual string ProvideUniqueId (string directiveName, IDictionary<string, string> arguments,
			IDictionary<string, string> requiresArguments, IDictionary<string, string> providesArguments)
		{
			return directiveName;
		}

		/// <summary>
        /// Gets associated text templating host.
        /// </summary>
        protected ITextTemplatingEngineHost Host {
			get { return host; }
		}
	}
}
