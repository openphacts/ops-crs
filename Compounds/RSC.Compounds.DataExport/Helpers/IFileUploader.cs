using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.DataExport
{
	/// <summary>
	/// Interface for an object that can handle uploading files to a destination.
	/// All implementations must be completely thread-safe.
	/// </summary>
	public interface IFileUploader : IDisposable
	{
		/// <summary>
		/// Gets if the <see cref="IFileUploader"/> is currently busy uploading files. This will be false when the queue is empty
		/// and all active file transfers have finished.
		/// </summary>
		bool IsBusy { get; }

		/// <summary>
		/// Gets if this object has been disposed.
		/// </summary>
		bool IsDisposed { get; }

		/// <summary>
		/// Gets the number of jobs remaining. Includes both queued and in-progress jobs.
		/// </summary>
		int JobsRemaining { get; }

		/// <summary>
		/// Gets or sets if files that already exist on the destination will be skipped. Default value is true.
		/// </summary>
		bool SkipIfExists { get; set; }

		void CreateDirectoryIfNotExists(string filePath);

		/// <summary>
		/// Removes a file from the asynchronous upload queue and aborts it.
		/// </summary>
		/// <param name="remotePath">The remote path for the upload to cancel.</param>
		/// <returns>True if the job was removed; otherwise false.</returns>
		/// <exception cref="ObjectDisposedException">This object is disposed.</exception>
		bool CancelAsyncUpload(string remotePath);

		/// <summary>
		/// Deletes a directory synchronously. If the root directory is specified, then all files and folders in the root
		/// directory will be deleted, but the root directory itself will not be deleted. Otherwise, the specified directory
		/// will be deleted along with all files and folders under it.
		/// </summary>
		/// <param name="targetPath">The relative path of the directory to delete.</param>
		/// <param name="requireExists">If false, and the <paramref name="targetPath"/> does not exist, then the deletion
		/// will fail silently.</param>
		/// <exception cref="ObjectDisposedException">This object is disposed.</exception>
		void DeleteDirectory(string targetPath, bool requireExists = false);

		/// <summary>
		/// Deletes a directory asynchronously. If the root directory is specified, then all files and folders in the root
		/// directory will be deleted, but the root directory itself will not be deleted. Otherwise, the specified directory
		/// will be deleted along with all files and folders under it.
		/// </summary>
		/// <param name="targetPath">The relative path of the directory to delete.</param>
		/// <returns>True if the directory deletion task was enqueued; false if the <paramref name="targetPath"/> is already
		/// queued for deletion, or if the <paramref name="targetPath"/> is invalid.</returns>
		/// <exception cref="ObjectDisposedException">This object is disposed.</exception>
		bool DeleteDirectoryAsync(string targetPath);

		/// <summary>
		/// Tests the connection of the <see cref="IFileUploader"/> and ensures that the needed operations can be performed.
		/// The test runs synchronously.
		/// </summary>
		/// <param name="userState">An optional object that can be used. When the <see cref="TestConnectionMessage"/> event is raised,
		/// this object is passed back through the event, allowing you to differentiate between multiple connection tests.</param>
		/// <param name="error">When this method returns false, contains a string describing the error encountered during testing.</param>
		/// <returns>True if the test was successful; otherwise false.</returns>
		bool TestConnection(object userState, out string error);

		/// <summary>
		/// Enqueues a file for asynchronous uploading.
		/// </summary>
		/// <param name="sourcePath">The path to the local file to upload.</param>
		/// <param name="remotePath">The path to upload the file to on the destination.</param>
		/// <returns>True if the file was enqueued; false if either of the arguments were invalid, or the file already
		/// exists in the queue.</returns>
		/// <exception cref="ObjectDisposedException">This object is disposed.</exception>
		bool UploadAsync(string sourcePath, string remotePath);

		/// <summary>
		/// Enqueues multiple files for asynchronous uploading.
		/// </summary>
		/// <param name="files">The files to upload, where the key is the source path, and the value is the
		/// path to upload the file on the destination.</param>
		/// <exception cref="ObjectDisposedException">This object is disposed.</exception>
		void UploadAsync(IEnumerable<KeyValuePair<string, string>> files);
	}
}
