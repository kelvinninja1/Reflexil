/* Reflexil Copyright (c) 2007-2010 Sebastien LEBRETON

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. */

#region " Imports "
using System;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Collections.Generic;
using Microsoft.VisualBasic;
#endregion

namespace Reflexil.Compilation
{
    /// <summary>
    /// .NET source code compiler
    /// </summary>
    public class Compiler : MarshalByRefObject
    {

        #region " Fields "
        private CompilerErrorCollection m_errors;
        private string m_assemblyLocation;
        #endregion

        #region " Properties "
        public CompilerErrorCollection Errors
        {
            get
            {
                return m_errors;
            }
        }

        public string AssemblyLocation
        {
            get
            {
                return m_assemblyLocation;
            }
        }
        #endregion

        #region " Methods "
        /// <summary>
        /// Lifetime initialization
        /// </summary>
        /// <returns>null for unlimited lifetime</returns>
        public override object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary>
        /// Compile source code
        /// </summary>
        /// <param name="code">full source code to compile</param>
        /// <param name="references">assembly references</param>
        /// <param name="language">target language</param>
        public void Compile(string code, string[] references, ESupportedLanguage language)
        {
            Dictionary<string, string> net35fix = new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } };
            bool use_net35fix = Array.Find(references, s => s!=null && s.ToLower().EndsWith("system.core.dll")) != null;
            CodeDomProvider provider = null;

            if (use_net35fix)
            {
                switch (language)
                {
                    case ESupportedLanguage.CSharp:
                        provider = new CSharpCodeProvider(net35fix);
                        break;
                    case ESupportedLanguage.VisualBasic:
                        provider = new VBCodeProvider(net35fix);
                        break;
                }
            }
            else
            {
                provider = CodeDomProvider.CreateProvider(language.ToString());
            }

            CompilerParameters parameters = new CompilerParameters();

            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = false;
            parameters.IncludeDebugInformation = false;
            parameters.ReferencedAssemblies.AddRange(references);

            if (language == ESupportedLanguage.CSharp)
            {
                parameters.CompilerOptions = "/unsafe";
            }

            CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);
            m_assemblyLocation = null;
            m_errors = results.Errors;

            if (!results.Errors.HasErrors)
            {
                m_assemblyLocation = results.CompiledAssembly.Location;
            }
        }

        /// <summary>
        /// Constructor.
        /// Checks that AppDomain isolation is correctly used
        /// </summary>
        public Compiler()
        {
            AppDomainHelper.CheckAppDomain();
        }
        #endregion

    }
}
