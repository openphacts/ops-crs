using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Reflection;

namespace RSC.Compounds.DataExport
{
	public class ZipUtils
	{
		public static byte[] zip_sdf(string sdf)
		{
			MemoryStream ms = new MemoryStream();
			GZipStream zs = new GZipStream(ms, CompressionMode.Compress);
			ASCIIEncoding enc = new ASCIIEncoding();
			byte[] mol = enc.GetBytes(sdf);
			zs.Write(mol, 0, mol.Length);
			zs.Close();

			return ms.ToArray();
		}

		// Gzip/Ungzip

		public static byte[] gzip(string text, Encoding enc)
		{
			return text == null ? null : gzip(enc.GetBytes(text));
		}

		public static byte[] gzip(byte[] bytes)
		{
			if (bytes == null)
				return null;

			MemoryStream ms = new MemoryStream();
			GZipStream zs = new GZipStream(ms, CompressionMode.Compress);
			zs.Write(bytes, 0, bytes.Length);
			zs.Close();

			return ms.GetBuffer();
		}

		public static string ungzip(byte[] bytes, Encoding enc)
		{
			return bytes == null ? null : enc.GetString(ungzip(bytes));
		}

		public static byte[] ungzip(byte[] buffer)
		{
			if (buffer == null)
				return null;

			GZipStream zs = new GZipStream(new MemoryStream(buffer), CompressionMode.Decompress);
			MemoryStream ms = new MemoryStream();
			BinaryReader reader = new BinaryReader(zs);
			BinaryWriter writer = new BinaryWriter(ms);

			do
			{
				byte[] buffer2 = reader.ReadBytes(1024);
				writer.Write(buffer2);
				if (buffer2.Length != 1024)
					break;
			}
			while (true);

			ms.Flush();
			return ms.ToArray();
		}

		// overwrites output file if exists!
		public static void ungzip_file(string input, string output)
		{
			using (GZipStream zs = new GZipStream(new FileStream(input, FileMode.Open), CompressionMode.Decompress, false))
			{
				using (BinaryReader reader = new BinaryReader(zs))
				{
					using (FileStream os = new FileStream(output, FileMode.OpenOrCreate))
					{
						do
						{
							byte[] buffer2 = reader.ReadBytes(1024);
							os.Write(buffer2, 0, buffer2.Length);
							if (buffer2.Length != 1024)
								break;
						}
						while (true);
					}
				}
			}
		}

		/// <summary>
		/// Compresses a given file using Gzip and optionally deletes the source file.
		/// </summary>
		/// <param name="fileToCompress">FileInfo object containing the file to be compressed.</param>
		/// <param name="delete_source_file">Boolean indicating whether to delete the source file.</param>
		public static FileInfo Compress(FileInfo fileToCompress, bool delete_source_file = true)
		{
			using (FileStream originalFileStream = fileToCompress.OpenRead())
			{
				if ((File.GetAttributes(fileToCompress.FullName) & FileAttributes.Hidden) != FileAttributes.Hidden & fileToCompress.Extension != ".gz")
				{
					using (FileStream compressedFileStream = File.Create(fileToCompress.FullName + ".gz"))
					using ( GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress) ) {
						originalFileStream.CopyTo(compressionStream);
					}
				}
			}
			if (delete_source_file)
				fileToCompress.Delete();

			//return the zipped FileInfo
			return new FileInfo(fileToCompress.FullName + ".gz");
		}

		public static void zip_dir(string dir, string zip_file)
		{
			//  get all filess from all subfolders...
			string[] filenames = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
			using (ZipOutputStream s = new ZipOutputStream(File.Create(zip_file)))
			{
				s.SetLevel(9);
				byte[] buffer = new byte[64 * 1024];
				foreach (string file in filenames)
				{
					FileInfo fi = new FileInfo(file);
					//  need to save directory hierarchy...
					ZipEntry e = new ZipEntry(file.Replace(dir, ""));
					e.DateTime = fi.LastWriteTime;
					e.Size = fi.Length;
					s.PutNextEntry(e);

					using (FileStream fs = File.OpenRead(file))
					{
						int sourceBytes;
						do
						{
							sourceBytes = fs.Read(buffer, 0, buffer.Length);
							s.Write(buffer, 0, sourceBytes);
						}
						while (sourceBytes > 0);
					}
				}

				s.Finish();
				s.Close();
			}
		}

		public static ArrayList unzip(string dir_to, string zip_file)
		{
			return unzip(dir_to, zip_file, false);
		}

		public static ArrayList unzip(string dir_to, string zip_file, bool ignoreIOErrors)
		{
			ArrayList result = new ArrayList();
			using (FileStream fin = new FileStream(zip_file, FileMode.Open))
			{
				using (ZipInputStream zin = new ZipInputStream(fin))
				{
					ZipEntry ze = null;
					while ((ze = zin.GetNextEntry()) != null)
					{
						string name;
						if (ze.Name.StartsWith("\\"))
							name = ze.Name.Substring(1);
						else
							name = ze.Name;
						string path = Path.Combine(dir_to, name);
						Directory.CreateDirectory(Path.GetDirectoryName(path));
						try
						{
							using (FileStream fout = new FileStream(path, FileMode.CreateNew))
							{
								byte[] buffer = new byte[64 * 1024];
								int rd = 0;
								while ((rd = zin.Read(buffer, 0, buffer.Length)) > 0)
								{
									fout.Write(buffer, 0, rd);
								}
								result.Add(ze.Name);
							}
						}
						catch (IOException)
						{
							if (!ignoreIOErrors)
								throw;
							result.Add(ze.Name);
						}
						finally
						{
							zin.CloseEntry();
						}
					}
				}
			}
			return result;
		}

		private const int ZIP_BUFFER = 8 * 1024;

		public static IEnumerable<string> UnzipFiles(string file)
		{
			using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
				return UnzipFiles(fs, null, null);
		}

		public static IEnumerable<string> UnzipFiles(byte[] zipBuffer)
		{
			using (var ms = new MemoryStream(zipBuffer))
				return UnzipFiles(ms, null, null);
		}

		public static IEnumerable<string> UnzipFiles(Stream zipStream)
		{
			return UnzipFiles(zipStream, null, null);
		}

		public static IEnumerable<string> UnzipFiles(string file, string filter)
		{
			using (Stream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
				return UnzipFiles(fs, null, filter);
		}

		public static IEnumerable<string> UnzipFiles(byte[] zipBuffer, string filter)
		{
			using (var ms = new MemoryStream(zipBuffer))
				return UnzipFiles(ms, null, filter);
		}

		public static IEnumerable<string> UnzipFiles(Stream zipStream, string filter)
		{
			return UnzipFiles(zipStream, null, filter);
		}

		public static IEnumerable<string> UnzipFiles(string file, string path, string filter)
		{
			using (Stream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
				return UnzipFiles(fs, path, filter);
		}

		public static IEnumerable<string> UnzipFiles(byte[] zipBuffer, string path, string filter)
		{
			using (var ms = new MemoryStream(zipBuffer))
				return UnzipFiles(ms, path, filter);
		}

		public static IEnumerable<string> UnzipFiles(Stream zipStream, string path, string filter)
		{
			List<string> filesList = new List<string>();

			Regex regex = null;
			if (!String.IsNullOrEmpty(filter))
				regex = new Regex(filter, RegexOptions.IgnoreCase);

			string tempPath = String.IsNullOrEmpty(path) ?
				Path.Combine(Path.GetTempPath(), Path.Combine(Assembly.GetExecutingAssembly().GetType().GUID.ToString(), "zip-" + new Random((int)DateTime.Now.Ticks).Next().ToString())) :
				path;

			using (ZipInputStream s = new ZipInputStream(zipStream))
			{
				ZipEntry entry;
				while ((entry = s.GetNextEntry()) != null)
				{
					if (!entry.IsFile)
						continue;
					if (regex != null && !regex.IsMatch(entry.Name))
						continue;

					string entryFile = Path.GetFileName(entry.Name);
					string entryDirectory = Path.GetDirectoryName(entry.Name);
					string tmpDirectory = Path.Combine(tempPath, entryDirectory);
					if (!Directory.Exists(tmpDirectory))
						Directory.CreateDirectory(tmpDirectory);

					string tmpFilePath = Path.Combine(tmpDirectory, entryFile);
					using (FileStream streamWriter = File.Create(tmpFilePath))
					{
						byte[] data = new byte[ZIP_BUFFER];
						while (true)
						{
							int size = s.Read(data, 0, data.Length);
							if (size <= 0)
								break;

							streamWriter.Write(data, 0, size);
						}
					}

					filesList.Add(tmpFilePath);
				}
			}

			return filesList;
		}

		public static void UnzipStream(Stream zipStream, Action<string, Stream> entryAction)
		{
			using (ZipFile zf = new ZipFile(zipStream))
			{
				foreach (ZipEntry zipEntry in zf)
				{
					if (!zipEntry.IsFile)
						continue;

					using (Stream zs = zf.GetInputStream(zipEntry))
						entryAction(zipEntry.Name, zs);
				}
			}
		}
	}
}
