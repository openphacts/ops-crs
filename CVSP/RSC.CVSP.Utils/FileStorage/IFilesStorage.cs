using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP
{
	/// <summary>
	/// General interface for uploading and hosting files in CVSP
	/// </summary>
	public interface IFileStorage
	{
		/// <summary>
		/// Upload new file to deposition
		/// </summary>
		/// <param name="guid">Deposition GUID</param>
		/// <param name="filename">File name</param>
		/// <param name="stream">Binary file's content</param>
		/// <returns>Full path to the storage where the file been uploaded</returns>
		string UploadFile(Guid guid, string filename, Stream stream);

		/// <summary>
		/// Return list of files uploaded to the deposition
		/// </summary>
		/// <param name="guid">Deposition's guid</param>
		/// <returns>List of file names</returns>
		IEnumerable<string> GetFiles(Guid guid);

		/// <summary>
		/// Return file stream
		/// </summary>
		/// <param name="guid">Deposition GUID</param>
		/// <param name="filename">File name</param>
		/// <returns></returns>
		Stream GetFile(Guid guid, string filename);

		/// <summary>
		/// Clean all files uploaded to the deposition
		/// </summary>
		/// <param name="guid">Deposition's guid</param>
		void CleanFiles(Guid guid);
	}
}
