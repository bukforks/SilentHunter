using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Particles;
using SH3Textractor;
using SilentHunter.FileFormats.Dat;
using SilentHunter.FileFormats.DependencyInjection;

namespace Sh3Xtractor
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			IServiceCollection svcCollection = new ServiceCollection();
			svcCollection.AddSilentHunterParsers(configurer =>
				configurer.Controllers.FromAssembly(typeof(ParticleGenerator).Assembly));
			ServiceProvider svcProvider = svcCollection.BuildServiceProvider();

			string fPath = args[0];

			List<string> fileList = Files.RecursiveFileSearch(fPath);
			fileList = fileList.Where(s =>
					s.EndsWith(".dat") || s.EndsWith(".sim") || s.EndsWith(".zon") || s.EndsWith(".val")
				 || s.EndsWith(".cam")
				 || s.EndsWith(".dsd"))
				.ToList();

			Console.WriteLine("Files found: " + fileList.Count);

			var serializeSettings = new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				PreserveReferencesHandling = PreserveReferencesHandling.Objects,
				ContractResolver = Serializer.ShouldSerializeContractResolver.Instance,
				Formatting = Formatting.Indented
			};

			Action<string, JsonSerializer> doer = (string file, JsonSerializer serializer) =>
			{
				Console.WriteLine("Processing: " + file);
				DatFile datFile = svcProvider.GetRequiredService<DatFile>();
				try
				{
					datFile.LoadAsync(file).Wait();
				}
				catch (Exception e)
				{
					Console.WriteLine("Error loading file: " + e.Message);
				}

				string newFile = file + ".sh3xtractor.json";
				using (StreamWriter sw = new StreamWriter(newFile))
				{
					serializer.Serialize(sw, datFile);
					Console.WriteLine("Saved to: " + newFile);
				}

				datFile.Dispose();
			};

			var options = new ParallelOptions
			{
				MaxDegreeOfParallelism = Environment.ProcessorCount == 1 ? 1 : Environment.ProcessorCount - 1
			};

			var localSerializer =
				new ThreadLocal<JsonSerializer>(() => JsonSerializer.Create(serializeSettings));

			Parallel.ForEach(fileList,
				options,
				(string f) =>
				{
					doer(f, localSerializer.Value);
				});
		}
	}
}