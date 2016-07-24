using System;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using com.ggasoftware.indigo;

using System.Collections.Generic;

namespace RSC.CVSP.Utils
{
	public class FileOperations
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="splitChunkSize"></param>
		/// <param name="inputGzippedFilePath"></param>
		/// <param name="outputDirectoryPath"></param>
		/// <param name="dataDomain"></param>
		/// <param name="startingChunkOrdinal"></param>
		/// <returns>last chunk id</returns>
		public static int splitFileIntoChunks(int splitChunkSize, string inputGzippedFilePath, string outputDirectoryPath, DataDomain dataDomain, int startingChunkOrdinal = 0)
		{
			if (!File.Exists(inputGzippedFilePath))
				return startingChunkOrdinal;
			if (dataDomain != DataDomain.Substances && dataDomain != DataDomain.Reactions)
				return startingChunkOrdinal;
			if (!Directory.Exists(outputDirectoryPath))
				Directory.CreateDirectory(outputDirectoryPath);


			StringBuilder sdf_string = new StringBuilder();
			using (FileStream fileStream = File.OpenRead(inputGzippedFilePath))
			using (GZipStream gunzip = new GZipStream(fileStream, CompressionMode.Decompress))
			{
				FileStream fileOut = null;
				GZipStream gzOut = null;
				if (dataDomain == DataDomain.Substances)
				{

					//int numOfChunks = startingChunkOrdinal;
					using (ChemSpider.Molecules.SdfReader reader = new ChemSpider.Molecules.SdfReader(gunzip))
					{
						int chunkSize = 0;

						foreach (ChemSpider.Molecules.SdfRecord sdf_rec in reader.Records)
						{
							chunkSize++;

							if (chunkSize <= splitChunkSize)
								sdf_string.Append(sdf_rec.ToString());
							else
							{
								fileOut = File.OpenWrite(Path.Combine(outputDirectoryPath, startingChunkOrdinal.ToString() + ".txt.gz"));
								gzOut = new GZipStream(fileOut, CompressionMode.Compress);
								byte[] sdf = ASCIIEncoding.ASCII.GetBytes(sdf_string.ToString());
								gzOut.Write(sdf, 0, sdf.Length);
								//File.WriteAllText(Path.Combine(chunkDir.FullName, numOfChunks.ToString() + ".txt"), sdf_string.ToString(), Encoding.ASCII);

								File.WriteAllText(Path.Combine(outputDirectoryPath, startingChunkOrdinal.ToString() + ".prepared"), "", Encoding.ASCII);

								startingChunkOrdinal++;
								sdf_string.Clear();
								sdf_string.Append(sdf_rec.ToString());
								chunkSize = 1;
								gzOut.Close();
								fileOut.Close();

							}
						}
						if (sdf_string.Length != 0)
						{
							fileOut = File.OpenWrite(Path.Combine(outputDirectoryPath, startingChunkOrdinal.ToString() + ".txt.gz"));
							gzOut = new GZipStream(fileOut, CompressionMode.Compress);
							byte[] sdf = ASCIIEncoding.ASCII.GetBytes(sdf_string.ToString());
							gzOut.Write(sdf, 0, sdf.Length);
							gzOut.Close();
							fileOut.Close();
							//File.WriteAllText(Path.Combine(chunkDir.FullName, numOfChunks.ToString() + ".txt"), sdf_string.ToString(), Encoding.ASCII);
							File.WriteAllText(Path.Combine(outputDirectoryPath, startingChunkOrdinal.ToString() + ".prepared"), "", Encoding.ASCII);
						}

					}

				}
				else if (dataDomain == DataDomain.Reactions)
				{
					Indigo indigo = new Indigo();
					indigo.setOption("ignore-stereochemistry-errors", "true");
					indigo.setOption("unique-dearomatization", "false");
					indigo.setOption("ignore-noncritical-query-features", "true");
					indigo.setOption("timeout", "600000");
					string tempRdfFile = Path.GetTempFileName();
					using (Stream file = File.OpenWrite(tempRdfFile))
						gunzip.CopyTo(file);

					int chunkSize = 0;
					int numOfChunks = 0;
					IndigoObject saver = indigo.createFileSaver(Path.Combine(outputDirectoryPath, numOfChunks.ToString() + ".txt"), "rdf");


					foreach (IndigoObject item in indigo.iterateRDFile(tempRdfFile))
					{
						chunkSize++;

						if (chunkSize < splitChunkSize)
							//sdf_string.Append(item.ToString());
							saver.append(item);
						else
						{
							File.WriteAllText(Path.Combine(outputDirectoryPath, numOfChunks.ToString() + ".prepared"), "", Encoding.ASCII);
							saver.append(item);
							saver.close();

							fileOut = File.OpenWrite(Path.Combine(outputDirectoryPath, numOfChunks.ToString() + ".txt.gz"));
							gzOut = new GZipStream(fileOut, CompressionMode.Compress);
							byte[] text = System.IO.File.ReadAllBytes(Path.Combine(outputDirectoryPath, numOfChunks.ToString() + ".txt"));

							gzOut.Write(text, 0, text.Length);
							gzOut.Close();
							fileOut.Close();
							DeleteFileSilently(Path.Combine(outputDirectoryPath, numOfChunks.ToString() + ".txt"));


							chunkSize = 0;
							numOfChunks++;
							saver.close();
							saver = indigo.createFileSaver(Path.Combine(outputDirectoryPath, numOfChunks.ToString() + ".txt"), "rdf");
						}
					}
					if (chunkSize > 0)
					{
						saver.close();
						fileOut = File.OpenWrite(Path.Combine(outputDirectoryPath, numOfChunks.ToString() + ".txt.gz"));
						gzOut = new GZipStream(fileOut, CompressionMode.Compress);
						byte[] text = System.IO.File.ReadAllBytes(Path.Combine(outputDirectoryPath, numOfChunks.ToString() + ".txt"));

						gzOut.Write(text, 0, text.Length);
						gzOut.Close();
						fileOut.Close();
						DeleteFileSilently(Path.Combine(outputDirectoryPath, numOfChunks.ToString() + ".txt"));

						File.WriteAllText(Path.Combine(outputDirectoryPath, numOfChunks.ToString() + ".prepared"), "", Encoding.ASCII);
					}
					DeleteFileSilently(tempRdfFile);
				}
			}
			return startingChunkOrdinal;
		}

		public static void extractSdfTags(string inputGzippedFilePath, int parseNumberOfRecords, out List<string> SDTags)
		{
			SDTags = new List<string>();
			int recordCount = 0;
			using (FileStream fileSTream = File.OpenRead(inputGzippedFilePath))
			using (GZipStream gunzip = new GZipStream(fileSTream, CompressionMode.Decompress))
			using (ChemSpider.Molecules.SdfReader reader = new ChemSpider.Molecules.SdfReader(gunzip))
			{
				List<ChemSpider.Molecules.SdfRecord> records = reader.Records.ToList();
				foreach (ChemSpider.Molecules.SdfRecord record in records)
				{
					recordCount++;
					if (record.Properties != null)
					{
						foreach(KeyValuePair<string,List<string>> property in record.Properties)
						{
							if(!SDTags.Contains(property.Key))
								SDTags.Add(property.Key);
						}
					}
					if (recordCount > parseNumberOfRecords)
						break;
				}
			}

		}
		public static int countNumberOfRecords(string inputGzippedFilePath, DataDomain dataDomain, out int numOfRecords)
		{
			numOfRecords = 0;
			if (!File.Exists(inputGzippedFilePath) || dataDomain == DataDomain.Unidentified)
				return 0;

			using (FileStream fileSTream = File.OpenRead(inputGzippedFilePath))
			{
				using (GZipStream gunzip = new GZipStream(fileSTream, CompressionMode.Decompress))
				{
					if (dataDomain == DataDomain.Substances)
						using (ChemSpider.Molecules.SdfReader reader = new ChemSpider.Molecules.SdfReader(gunzip))
						{
							List<ChemSpider.Molecules.SdfRecord> records = reader.Records.ToList();
							numOfRecords = records.Count;
						}
					else if (dataDomain == DataDomain.Reactions)
					{
						//can't get stream to iterator so creating temp file
						string tempFile = Path.GetTempFileName();

						//string tempFile = Path.GetTempFileName();
						using (Stream file = File.OpenWrite(tempFile))
							gunzip.CopyTo(file);

						Indigo i = new Indigo();
						foreach (IndigoObject item in i.iterateRDFile(tempFile))
							numOfRecords++;


						DeleteFileSilently(tempFile);
					}
					else if (dataDomain == DataDomain.Spectra || dataDomain == DataDomain.Crystals)
					{
						//can't get stream to iterator so creating temp file
						string tempFile = Path.GetTempFileName();

						//string tempFile = Path.GetTempFileName();
						using (Stream file = File.OpenWrite(tempFile))
							gunzip.CopyTo(file);

						//read file and split by "$$$$"
						using (StreamReader sr = new StreamReader(tempFile))
						{
							string fileContentString = sr.ReadToEnd();
							string[] stringSeparators = new string[] { "$$$$||||$$$$" + Environment.NewLine };

							string[] result = fileContentString.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
							numOfRecords = result.Length;
							numOfRecords = (from p in result where !String.IsNullOrEmpty(p.Replace("\r\n", "").Replace("\n", "").Replace("\r", "").Trim()) select p).Count();
						}
						DeleteFileSilently(tempFile);
					}
				}
			}

			return numOfRecords;
		}


		public static void DeleteFileSilently(string path)
		{
			try
			{
				if (File.Exists(path)) File.Delete(path);
			}
			catch (Exception)
			{
				// ignored
			}
		}
	}
}
