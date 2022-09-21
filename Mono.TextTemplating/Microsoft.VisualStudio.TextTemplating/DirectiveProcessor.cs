//
// DirectiveProcessor.cs
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
using System.CodeDom;

namespace Microsoft.VisualStudio.TextTemplating
{
    /// <summary>
    /// Base class for a concrete DirectiveProcessor
    /// </summary>
    /// <remarks>
    /// A singleton instance of any of these classes that is
    /// required will be held by the Engine.
    /// This class implements a state machine with
    /// the Get... methods only valid after a Start...Finish pair.
    /// </remarks>
	public abstract class DirectiveProcessor : IDirectiveProcessor
	{
		CompilerErrorCollection errors;

		protected DirectiveProcessor ()
		{
		}

        /// <summary>
        /// Initialize the processor instance
        /// </summary>
        /// <param name="host"></param>
		public virtual void Initialize (ITextTemplatingEngineHost host)
		{
			if (host == null)
				throw new ArgumentNullException (nameof (host));
		}

        /// <summary>
        /// Begin a round of directive processing
        /// </summary>
        /// <param name="languageProvider"></param>
        /// <param name="templateContents">the contents of the template being processed</param>\
        /// <param name="errors">collection to report processing errors in</param>
		public virtual void StartProcessingRun (CodeDomProvider languageProvider, string templateContents, CompilerErrorCollection errors)
		{
			if (languageProvider == null)
				throw new ArgumentNullException (nameof (languageProvider));
			this.errors = errors;
		}

        /// <summary>
        /// Finish a round of directive processing
        /// </summary>
		public abstract void FinishProcessingRun ();

		/// <summary>
        /// Get the code to contribute to the generated
        /// template processing class as a consequence of the most recent run.
        /// </summary>
        /// <returns>The code that this DirectiveProcessor contributes to the generated TextTemplating class</returns>
        public abstract string GetClassCodeForProcessingRun ();

		/// <summary>
        /// Get any namespaces to import as a consequence of
        /// the most recent run.
        /// </summary>
        /// <returns></returns>
        public abstract string[] GetImportsForProcessingRun ();

        /// <summary>
        /// Get the code to contribute to the body of the initialize method of the generated
        /// template processing class as a consequence of the most recent run.
        /// This code will run after the base class' Initialize method
        /// </summary>
        /// <returns></returns>
		public abstract string GetPostInitializationCodeForProcessingRun ();

        /// <summary>
        /// Get the code to contribute to the body of the initialize method of the generated
        /// template processing class as a consequence of the most recent run.
        /// This code will run before the base class' Initialize method
        /// </summary>
        /// <returns></returns>
		public abstract string GetPreInitializationCodeForProcessingRun ();

        /// <summary>
        /// Get any references to pass to the compiler
        /// as a consequence of the most recent run.
        /// </summary>
        /// <returns></returns>
		public abstract string[] GetReferencesForProcessingRun ();

        /// <summary>
        /// Does this DirectiveProcessor support the given directive
        /// </summary>
        /// <remarks>
        /// This call is not connected to the state machine
        /// </remarks>
        /// <param name="directiveName"></param>
        /// <returns></returns>
		public abstract bool IsDirectiveSupported (string directiveName);

        /// <summary>
        /// Process a directive from a template file
        /// </summary>
		public abstract void ProcessDirective (string directiveName, IDictionary<string, string> arguments);

        /// <summary>
        /// Get any custom attributes to place on the template class.
        /// </summary>
        /// <returns>A collection of custom attributes that can be null or empty.</returns>
        /// <remarks>
        /// The default implementation is to produce no attributes.
        /// </remarks>
		public virtual CodeAttributeDeclarationCollection GetTemplateClassCustomAttributes ()
		{
			return null;
		}

		CompilerErrorCollection IDirectiveProcessor.Errors { get { return errors; } }

		void IDirectiveProcessor.SetProcessingRunIsHostSpecific (bool hostSpecific)
		{
		}

		bool IDirectiveProcessor.RequiresProcessingRunIsHostSpecific {
			get { return false; }
		}
	}
}
