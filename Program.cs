using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace CSEXCompiler
{
	class Program
	{
		static async Task Main(string[] args)
		{
			if (args.Length != 2)
			{
				Console.WriteLine("Illegal Argument Inputs. Press Any Key To Exit...");
				Console.ReadKey();
				return;
			}
			var csexPath = args[0];
			var outPutPath = args[1];
			// 遍历csexLocation目录下的所有.csex文件, 迭代搜索
			await MapCSEXDirectoryToAsync(csexPath, outPutPath, () => Console.WriteLine("Compile Done."));

		}



		static async Task MapCSEXDirectoryToAsync(string csexDirectoryPath, string destinationPath, Action CallBack)
		{
            try
            {
				var fileFullNames = Directory.GetFiles(csexDirectoryPath);
				// 对csex目录下的直接文件进行访问
				foreach (var fileFullName in fileFullNames)
				{
					var fileName = GetLastName(fileFullName);
					fileName = fileName.Replace(".csex", ".cs");
					Console.WriteLine(fileName);
					await MapTextFileToAsync(fileFullName, Path.Combine(destinationPath, fileName), async str =>
					{
						return await CompileWhenStatement(str);
					});
				}

				// 对csex下的目录迭代访问
				var dirFullNames = Directory.GetDirectories(csexDirectoryPath);
				foreach (var dirFullName in dirFullNames)
				{
					var dirName = GetLastName(dirFullName);
					Console.WriteLine(dirName);
					var desDicFullName = Path.Combine(destinationPath, dirName);
					Directory.CreateDirectory(desDicFullName);
					await MapCSEXDirectoryToAsync(dirFullName, desDicFullName, () => Console.WriteLine($"Compile Directory {dirName} Done."));
				}

				CallBack();
			}
			catch(IOException)
            {
				Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error : Connot Find The csexDirectoryPath {csexDirectoryPath}");
				return;
            }
			
			

		}

		static async Task MapTextFileToAsync(string sourceFileName, string toFileName, Func<string, Task<string>> Map)
		{
			if (!File.Exists(sourceFileName)) return;
			var fileText = await File.ReadAllTextAsync(sourceFileName);
			fileText = await Map(fileText);
			await File.WriteAllTextAsync(toFileName, fileText);
			Console.WriteLine($"Source File : {sourceFileName}");
			Console.WriteLine($"To File : {toFileName}");
		}

		static async Task<string> CompileWhenStatement(string inStr)
		{
			var braceStack = new Stack<char>();
			var isSingleLineExegesis = false;
			var isMultiLineExegesis = false;
			var isWhenArea = false;
			var skipAddA_Case_ = false;
			var words = inStr.Split(' ', '\r');
			for (var i = 0; i < words.Length; i++)
			{
				var word = words[i];
				if (word.Length == 0) continue;
				if (word.StartsWith("//")) isSingleLineExegesis = true;
				if (word.StartsWith("/*")) isMultiLineExegesis = true;
				if (word.StartsWith("*/")) isMultiLineExegesis = false;
				if (word.StartsWith("\n")) isSingleLineExegesis = false;

				if (isSingleLineExegesis || isMultiLineExegesis) continue;

				if (word.StartsWith("when")) {
					isWhenArea = true;
					var afterStr = word.Substring(4);
					words[i] = $"switch{afterStr}";
					continue;
				}

				if (!isWhenArea) continue;

				
				// handle brace
				if (word.StartsWith("{"))
				{
					braceStack.Push('{');
					if (braceStack.Count > 1) words[i] = "";
					continue;
				}
				if (word.StartsWith("}"))
				{
					if (braceStack.Count == 0) throw new Exception("The curly braces do not match");
					braceStack.Pop();
					if (braceStack.Count == 0) isWhenArea = false;
					if (braceStack.Count == 1) words[i] = "break;\r\n";
					continue;
				}

				if(word.StartsWith("=>"))
				{
					words[i] = ":";
					words[i - 1] = words[i - 1].Trim();
					if(words[i - 1] != "default") words[i - 1] = $"case {words[i - 1]}";
					continue;
				}

				if(word.Length > 1)
				{
					if (word.StartsWith("else"))
					{
						words[i] = "default";
					}
					if (word.EndsWith(";") && braceStack.Count == 1) words[i] += "\nbreak;\n";
				}

				
			}
			
			return ConcatToString(words);
			
		}

		static void CopyDirectory(string directoryPath, string toPath)
		{

		}

		static string GetLastName(string fileFullName)
		{
			var dirs = fileFullName.Split("\\");
			return dirs[dirs.Length - 1];
		}

		static string ConcatToString(string[] strArray)
		{
			string result = "";
			foreach(var str in strArray)
			{
				result += str;
				result += " ";
			}
			return result;
		}

		//static string[] SplitWith(string str, params string[] separators)
		//{
		//    foreach(var sentence in str.Split(,))
		//    {

		//    }
		//}
	}
}
