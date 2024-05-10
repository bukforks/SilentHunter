using System;
using System.Collections.Generic;
using System.IO;

namespace SH3Textractor
{
	public static class Files
	{
		public static List<string> RecursiveFileSearch(string directoryPath)
		{
			var fileList = new List<string>();

			try
			{
				// Get the files in the current directory
				string[] files = Directory.GetFiles(directoryPath);
				foreach (string file in files)
				{
					fileList.Add(file);
				}

				// Get the subdirectories in the current directory
				string[] subDirectories = Directory.GetDirectories(directoryPath);
				foreach (string subDirectory in subDirectories)
				{
					// Recursively search each subdirectory
					List<string> subDirectoryFiles = RecursiveFileSearch(subDirectory);
					fileList.AddRange(subDirectoryFiles);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("An error occurred while parsing files: " + e.Message);
				throw;
			}

			return fileList;
		}
	}
}