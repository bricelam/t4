//
// ITextTemplatingEngineHost.cs
//
// Author:
//       Mikayla Hutchinson <m.j.hutchinson@gmail.com>
//
// Copyright (c) 2009-2010 Novell, Inc. (http://www.novell.com)
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
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.VisualStudio.TextTemplating
{
    /// <summary>
    /// (Optional) interface that DirectiveProcessors can implement if they care about the value of the HostSpecific flag when they are generating code.
    /// <remarks>
    /// Will be called immediately after Initialize
    /// </remarks>
    /// </summary>
	public interface IRecognizeHostSpecific
	{
        /// <summary>
        /// Inform the directive processor whether the run is host-specific.
        /// </summary>
        /// <remarks>
        /// Will be called after RequiresProcessingRunIsHostSpecific has been run
        /// on all directive processors to inform the processor what the final host-specifc decision is
        /// </remarks>
        /// <param name="hostSpecific"></param>
		void SetProcessingRunIsHostSpecific (bool hostSpecific);

        /// <summary>
        /// Allow a directive processor to specify that it needs the run to be host-specific.
        /// </summary>
        /// <remarks>
        /// If any directive processor in the run sets this to be true then the engine will make the entire run host-specific.
        /// </remarks>
		bool RequiresProcessingRunIsHostSpecific { get; }
	}

    /// <summary>The interface for the text template transformation engine.</summary>
	[Obsolete("Use Mono.TextTemplating.TemplatingEngine directly")]
	public interface ITextTemplatingEngine
	{
		/// <summary>Transforms the contents of a text template file to produce the generated text output.</summary>
        /// <returns>The generated text output of the text template file.</returns>
        /// <param name="content">The contents of the text template file to be transformed.</param>
        /// <param name="host">The <see cref="T:Microsoft.VisualStudio.TextTemplating.ITextTemplatingEngineHost" /> that will host this engine.</param>
        string ProcessTemplate (string content, ITextTemplatingEngineHost host);
		string PreprocessTemplate (string content, ITextTemplatingEngineHost host, string className,
			string classNamespace, out string language, out string [] references);
	}

	/// <summary>The interface for the host that transforms text templates. This is available to directive processors and can also be accessed by text templates.</summary>
	public interface ITextTemplatingEngineHost
	{
		/// <summary>Called by the Engine to ask for the value of a specified option. Return null if you do not know. </summary>
        /// <returns>Null to select the default value for this option. Otherwise, an appropriate value for the option. </returns>
        /// <param name="optionName">The name of an option. </param>
        object GetHostOption (string optionName);

		/// <summary>Acquires the text that corresponds to a request to include a partial text template file.</summary>
        /// <returns>true to indicate that the host was able to acquire the text; otherwise, false.</returns>
        /// <param name="requestFileName">The name of the partial text template file to acquire.</param>
        /// <param name="content">A <see cref="T:System.String" /> that contains the acquired text or <see cref="F:System.String.Empty" /> if the file could not be found.</param>
        /// <param name="location">A <see cref="T:System.String" /> that contains the location of the acquired text. If the host searches the registry for the location of include files or if the host searches multiple locations by default, the host can return the final path of the include file in this parameter. The host can set the <paramref name="location" /> to <see cref="F:System.String.Empty" /> if the file could not be found or if the host is not file-system based.</param>
        bool LoadIncludeText (string requestFileName, out string content, out string location);

		/// <summary>Receives a collection of errors and warnings from the transformation engine. </summary>
        /// <param name="errors">The <see cref="T:System.CodeDom.Compiler.CompilerErrorCollection" />  being passed to the host from the engine.</param>
        void LogErrors (CompilerErrorCollection errors);

#if !FEATURE_APPDOMAINS
		[Obsolete ("AppDomains are only supported on .NET Framework. This method will not be called on newer versions of .NET.")]
#endif

		/// <summary>Provides an application domain to run the generated transformation class. </summary>
        /// <returns>An <see cref="T:System.AppDomain" /> that compiles and executes the generated transformation class.</returns>
        /// <param name="content">The contents of the text template file to be processed. </param>
        AppDomain ProvideTemplatingAppDomain (string content);

		/// <summary>Allows a host to provide additional information about the location of an assembly.</summary>
        /// <returns>A <see cref="T:System.String" /> that contains the specified assembly reference or the specified assembly reference with additional information.</returns>
        /// <param name="assemblyReference">The assembly to resolve.</param>
        string ResolveAssemblyReference (string assemblyReference);

		/// <summary>Returns the type of a directive processor, given its friendly name. </summary>
        /// <returns>The <see cref="T:System.Type" /> of the directive processor.</returns>
        /// <param name="processorName">The name of the directive processor to be resolved.</param>
        Type ResolveDirectiveProcessor (string processorName);

		/// <summary>Resolves the value of a parameter for a directive processor if the parameter is not specified in the template text. </summary>
        /// <returns>A <see cref="T:System.String" /> that represents the resolved parameter value.</returns>
        /// <param name="directiveId">The ID of the directive call to which the parameter belongs. This ID disambiguates repeated calls to the same directive from the same text template.</param>
        /// <param name="processorName">The name of the directive processor to which the directive belongs.</param>
        /// <param name="parameterName">The name of the parameter to be resolved.</param>
        string ResolveParameterValue (string directiveId, string processorName, string parameterName);

		/// <summary>Allows a host to provide a complete path, given a file name or relative path.</summary>
        /// <returns>A <see cref="T:System.String" /> that contains an absolute path.</returns>
        /// <param name="path">The path to complete.</param>
        string ResolvePath (string path);

		/// <summary>Tells the host the file name extension that is expected for the generated text output. </summary>
        /// <param name="extension">The file name extension for the generated text output.</param>
        void SetFileExtension (string extension);

		/// <summary>Tells the host the encoding that is expected for the generated text output. </summary>
        /// <param name="encoding">The encoding for the generated text output.</param>
        /// <param name="fromOutputDirective">true to indicate that the user specified the encoding in the encoding parameter of the output directive.</param>
        void SetOutputEncoding (Encoding encoding, bool fromOutputDirective);

        /// <summary>Gets a list of assembly references. </summary>
        /// <returns>An <see cref="T:System.Collections.IList" /> that contains assembly names.</returns>
		IList<string> StandardAssemblyReferences { get; }

        /// <summary>Gets a list of namespaces.</summary>
        /// <returns>An <see cref="T:System.Collections.IList" /> that contains namespaces.</returns>
		IList<string> StandardImports { get; }

		/// <summary>Gets the path and file name of the text template that is being processed.</summary>
        /// <returns>A <see cref="T:System.String" /> that contains the path and file name of the text template that is being processed.</returns>
        string TemplateFile { get; }
	}

	/// <summary>Can be used to transmit information from a directive processor into a text template.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage ("Naming", "CA1710:Identifiers should have correct suffix", Justification = "API compat with Microsoft.VisualStudio.TextTemplating.dll")]
	public interface ITextTemplatingSession :
		IEquatable<ITextTemplatingSession>, IEquatable<Guid>, IDictionary<string, object>,
		ICollection<KeyValuePair<string, object>>,
		IEnumerable<KeyValuePair<string, object>>,
		IEnumerable, ISerializable
	{
		/// <summary>Identity of this session, used to compare session instances by value.</summary>
        Guid Id { get; }
	}

	/// <summary>Implemented by a text templating host, enabling callers to obtain an object denoting the current session. A session represents series of executions of text templates. The session object can be used to pass information from the host into the code of the text template.</summary>
    public interface ITextTemplatingSessionHost
	{
		/// <summary>Create a Session object that can be used to transmit information into a template. The new Session becomes the current Session.</summary>
        /// <returns>A new Session</returns>
        ITextTemplatingSession CreateSession ();

		/// <summary>The current Session.</summary>
        ITextTemplatingSession Session { get; set; }
	}

	/// <summary>Interface for a directive processor.</summary>
    public interface IDirectiveProcessor
	{
		/// <summary>Error collection for DirectiveProcessor to add errors/warnings to.</summary>
        /// <returns>Returns <see cref="T:System.CodeDom.Compiler.CompilerErrorCollection" />.</returns>
        CompilerErrorCollection Errors { get; }

		/// <summary>Allow a directive processor to specify that it needs the run to be host-specific.</summary>
        /// <returns>Returns <see cref="T:System.Boolean" />.</returns>
        bool RequiresProcessingRunIsHostSpecific { get; }

		/// <summary>Finishes a round of directive processing.</summary>
        void FinishProcessingRun ();

		/// <summary>Gets the code to contribute to the generated template processing class because of the most recent run.</summary>
        string GetClassCodeForProcessingRun ();

		/// <summary>Gets any namespaces to import because of the most recent run.</summary>
        string[] GetImportsForProcessingRun ();

		/// <summary>Gets the code to contribute to the body of the initialize method of the generated template processing class because of the most recent run. This code will run after the base class' Initialize method.</summary>
        string GetPostInitializationCodeForProcessingRun ();

		/// <summary>Gets the code to contribute to the body of the initialize method of the generated template processing class because of the most recent run. This code will run before the base class' Initialize method.</summary>
        string GetPreInitializationCodeForProcessingRun ();

		/// <summary>Gets any references to pass to the compiler because of the most recent run.</summary>
        string[] GetReferencesForProcessingRun ();

		/// <summary>Gets any custom attributes to include on the template class.</summary>
        /// <returns>A collection of custom attributes that can be null or empty.</returns>
        CodeAttributeDeclarationCollection GetTemplateClassCustomAttributes ();  //TODO

		/// <summary>Initializes the processor instance.</summary>
        void Initialize (ITextTemplatingEngineHost host);

		/// <summary>Does this DirectiveProcessor support the given directive.</summary>
        /// <returns>Returns <see cref="T:System.Boolean" />.</returns>
        bool IsDirectiveSupported (string directiveName);

		/// <summary>Processes a directive from a template file.</summary>
        void ProcessDirective (string directiveName, IDictionary<string, string> arguments);

		/// <summary>Informs the directive processor whether the run is host-specific.</summary>
        void SetProcessingRunIsHostSpecific (bool hostSpecific);

		/// <summary>Starts a round of directive processing.</summary>
        /// <param name="templateContents">The contents of the template being processed.</param>
        /// <param name="errors">The collection to report processing errors in.</param>
        void StartProcessingRun (CodeDomProvider languageProvider, string templateContents, CompilerErrorCollection errors);
	}
}
