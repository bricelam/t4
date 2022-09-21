//
// TextTransformation.cs
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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Microsoft.VisualStudio.TextTemplating
{
	/// <summary>
    /// Base class for generated text transformations
    /// </summary>
    /// <remarks>
    /// Any class specified in an inherits directive must match this class in a duck-typing style.
    /// Note that this class therefore specifies an implicit contract with the transformation object.
    /// The object doesn't have to derive from any specific type or interface, but it must have
    /// a) A void Initialize() method.
    /// b) A string TransformText() method
    /// c) An Errors property that's duck-compatible with CompilerErrorCollection
    /// d) A GeneratonEnvironment property that's duck-compatible with StringBuilder.
    /// e) A void Write(string) method
    /// Using any further features of T4 such as expression blocks will require the class to have further methods, such as ToStringHelper, but
    /// those will produce regular compiler errors at transform time that the base class author can address.
    /// These few methods together form a subset of the TextTransformation default base class' API.
    /// If you change this pseudo-contract to add more requirements, you should consider this a breaking change.
    /// It's OK, however, to change the contract to have fewer requirements.
    /// </remarks>
    public abstract class TextTransformation : IDisposable
	{
		Stack<int> indents;
		string currentIndent = string.Empty;
		CompilerErrorCollection errors;
		StringBuilder builder;
		bool endsWithNewline;

		public TextTransformation ()
		{
		}

		/// <summary>
        /// Initialize the templating class
        /// </summary>
        /// <remarks>
        /// Derived classes are allowed to return errors from initialization
        /// </remarks>
        public virtual void Initialize ()
		{
		}

		/// <summary>
        /// Generate the output text of the transformation
        /// </summary>
        /// <returns></returns>
        public abstract string TransformText ();

		/// <summary>
        /// Current transformation session
        /// </summary>
        public virtual IDictionary<string, object> Session { get; set; }

		#region Errors

		/// <summary>
        /// Raise an error
        /// </summary>
        /// <param name="message"></param>
        public void Error (string message)
		{
			Errors.Add (new CompilerError ("", 0, 0, "", message));
		}

		/// <summary>
        /// Raise a warning
        /// </summary>
        /// <param name="message"></param>
        public void Warning (string message)
		{
			Errors.Add (new CompilerError ("", 0, 0, "", message) { IsWarning = true });
		}

		/// <summary>
        /// The error collection for the generation process
        /// </summary>
        protected internal CompilerErrorCollection Errors {
			get {
				if (errors == null)
					errors = new CompilerErrorCollection ();
				return errors;
			}
		}

		Stack<int> Indents {
			get {
				if (indents == null)
					indents = new Stack<int> ();
				return indents;
			}
		}

		#endregion

		#region Indents

		/// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        /// <returns>The removed indent string</returns>
        public string PopIndent ()
		{
			if (Indents.Count == 0)
				return "";
			int lastPos = currentIndent.Length - Indents.Pop ();
			string last = currentIndent.Substring (lastPos);
			currentIndent = currentIndent.Substring (0, lastPos);
			return last;
		}

		/// <summary>
        /// Increase the indent
        /// </summary>
        /// <param name="indent">indent string</param>
        public void PushIndent (string indent)
		{
			if (indent == null)
				throw new ArgumentNullException (nameof (indent));
			Indents.Push (indent.Length);
			currentIndent += indent;
		}

		/// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent ()
		{
			currentIndent = string.Empty;
			Indents.Clear ();
		}

		/// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent {
			get { return currentIndent; }
		}

		#endregion

		#region Writing

		/// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected StringBuilder GenerationEnvironment {
			get {
				if (builder == null)
					builder = new StringBuilder ();
				return builder;
			}
			set {
				builder = value;
			}
		}

		/// <summary>
        /// Write text directly into the generated output
        /// </summary>
        /// <param name="textToAppend"></param>
        public void Write (string textToAppend)
		{
			if (string.IsNullOrEmpty (textToAppend))
				return;

			if ((GenerationEnvironment.Length == 0 || endsWithNewline) && CurrentIndent.Length > 0) {
				GenerationEnvironment.Append (CurrentIndent);
			}
			endsWithNewline = false;

			char last = textToAppend[textToAppend.Length-1];
			if (last == '\n' || last == '\r') {
				endsWithNewline = true;
			}

			if (CurrentIndent.Length == 0) {
				GenerationEnvironment.Append (textToAppend);
				return;
			}

			//insert CurrentIndent after every newline (\n, \r, \r\n)
			//but if there's one at the end of the string, ignore it, it'll be handled next time thanks to endsWithNewline
			int lastNewline = 0;
			for (int i = 0; i < textToAppend.Length - 1; i++) {
				char c = textToAppend[i];
				if (c == '\r') {
					if (textToAppend[i + 1] == '\n') {
						i++;
						if (i == textToAppend.Length - 1)
							break;
					}
				} else if (c != '\n') {
					continue;
				}
				i++;
				int len = i - lastNewline;
				if (len > 0) {
					GenerationEnvironment.Append (textToAppend, lastNewline, i - lastNewline);
				}
				GenerationEnvironment.Append (CurrentIndent);
				lastNewline = i;
			}
			if (lastNewline > 0)
				GenerationEnvironment.Append (textToAppend, lastNewline, textToAppend.Length - lastNewline);
			else
				GenerationEnvironment.Append (textToAppend);
		}

		/// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void Write (string format, params object[] args)
		{
			Write (string.Format (CultureInfo.CurrentCulture, format, args));
		}

		/// <summary>
        /// Write text directly into the generated output
        /// </summary>
        /// <param name="textToAppend"></param>
        public void WriteLine (string textToAppend)
		{
			Write (textToAppend);
			GenerationEnvironment.AppendLine ();
			endsWithNewline = true;
		}

		/// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void WriteLine (string format, params object[] args)
		{
			WriteLine (string.Format (CultureInfo.CurrentCulture, format, args));
		}

		#endregion

		#region Dispose

		/// <summary>
        /// Disposes the state of this object.
        /// </summary>
        public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		/// <summary>
        /// Dispose implementation.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose (bool disposing)
		{
		}

		/// <summary>
        /// Finaizlier.
        /// </summary>
        ~TextTransformation ()
		{
			Dispose (false);
		}

		#endregion

	}
}
