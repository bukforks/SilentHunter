using System.Collections.Generic;
using System.Reflection;

namespace SilentHunter.Dat.Controllers.Compiler
{
	public interface ICSharpCompiler
	{
		/// <summary>
		/// Compiles the specified <paramref name="fileNames" /> and <see name="DocFile" /> into an <see cref="Assembly" />.
		/// </summary>
		/// <param name="fileNames"></param>
		/// <param name="options"></param>
		/// <param name="loadAssembly"></param>
		/// <returns></returns>
		Assembly CompileCode(ICollection<string> fileNames, CompilerOptions options, bool loadAssembly = false);

		/// <summary>
		/// Compiles code into an <see cref="Assembly" />.
		/// </summary>
		/// <param name="code"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		Assembly CompileCode(string code, CompilerOptions options);
	}
}