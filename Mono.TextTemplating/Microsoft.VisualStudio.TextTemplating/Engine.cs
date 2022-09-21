//
// Engine.cs
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
using Mono.TextTemplating;

namespace Microsoft.VisualStudio.TextTemplating
{
    /// <summary>
    /// Text templating engine
    /// </summary>
	[Obsolete ("Use Mono.TextTemplating.TemplatingEngine directly")]
	public class Engine : ITextTemplatingEngine
	{
		TemplatingEngine engine = new TemplatingEngine ();

        /// <summary>
        /// Processes a template
        /// </summary>
        /// <param name="content">The contents of the template file to be processed</param>
        /// <param name="host">The ITextTemplatingEngineHost that will host this engine</param>
        /// <returns>The output from the processed template</returns>
		public string ProcessTemplate (string content, ITextTemplatingEngineHost host)
		{
			return engine.ProcessTemplate (content, host);
		}

        /// <summary>
        /// Process the contents of a templated file running inline code to produce a class that represents the template.
        /// </summary>
        /// <param name="content">The content of the templated file</param>
        /// <param name="host">The hosting environment using this engine</param>
        /// <param name="className">The name of the class to produce</param>
        /// <param name="classNamespace">The namespace of the class to produce</param>
        /// <param name="language">The language that the template's control code was written in</param>
        /// <param name="references">The set of references required by the template</param>
        /// <returns></returns>
		public string PreprocessTemplate (string content, ITextTemplatingEngineHost host, string className,
			string classNamespace, out string language, out string[] references)
		{
			return engine.PreprocessTemplate (content, host, className, classNamespace, out language, out references);
		}

        /// <summary>
        /// CacheAssemblies option string
        /// </summary>
		public const string CacheAssembliesOptionString = "CacheAssemblies";
	}
}
