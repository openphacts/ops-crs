using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.DataExport
{
	/// <summary>
	/// Delegate for handling download events from the <see cref="IFileUploader"/>.
	/// </summary>
	/// <param name="sender">The <see cref="IFileUploader"/> that the event came from.</param>
	/// <param name="localFile">The local file for the job that finished.</param>
	/// <param name="remoteFile">The remote file for the job that finished.</param>
	public delegate void FileUploaderDownloadEventHandler(IFileUploader sender, string localFile, string remoteFile);

	/// <summary>
	/// Delegate for handling upload error events from the <see cref="IFileUploader"/>.
	/// </summary>
	/// <param name="sender">The <see cref="IFileUploader"/> that the event came from.</param>
	/// <param name="localFile">The local file for the job related to the error.</param>
	/// <param name="remoteFile">The remote file for the job related to the error.</param>
	/// <param name="error">A string containing the error message.</param>
	/// <param name="attemptCount">The number of times this particular job has been attempted. This value is incremented every
	/// time the job is attempted, even if it fails for a different reason.
	/// Once this value reaches 255, it will no longer increment.</param>
	public delegate void FileUploaderDownloadErrorEventHandler(
		IFileUploader sender, string localFile, string remoteFile, string error, byte attemptCount);


	/// <summary>
	/// Interface for an object that can handle uploading files to a destination.
	/// All implementations must be completely thread-safe.
	/// </summary>
	public interface IFileDownloader : IDisposable
	{
		/// <summary>
		/// Notifies listeners when an asynchronous download has been completed.
		/// </summary>
		event FileUploaderDownloadEventHandler DownloadComplete;

		/// <summary>
		/// Notifies listeners when there has been an error related to one of the asynchronous download jobs.
		/// The job in question will still be re-attempted by default.
		/// </summary>
		event FileUploaderDownloadErrorEventHandler DownloadError;

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
		/// Removes a file from the asynchronous download queue and aborts it.
		/// </summary>
		/// <param name="localPath">The fully qualified local path of the download to cancel.</param>
		/// <returns>True if the job was removed; otherwise false.</returns>
		/// <exception cref="ObjectDisposedException">This object is disposed.</exception>
		bool CancelAsyncDownload(string localPath);

		/// <summary>
		/// Synchronously downloads a remote file and returns the contents of the downloaded file as an array of bytes.
		/// </summary>
		/// <param name="remoteFile">The remote file to download.</param>
		/// <param name="requireExists">If false, and the remote file does not exist, a null will be returned instead.</param>
		/// <returns>The downloaded file's contents.</returns>
		/// <exception cref="ObjectDisposedException">This object is disposed.</exception>
		byte[] Download(string remoteFile, bool requireExists = false);

		/// <summary>
		/// Synchronously downloads a remote file and returns the contents of the downloaded file as a string.
		/// </summary>
		/// <param name="remoteFile">The remote file to download.</param>
		/// <param name="requireExists">If false, and the remote file does not exist, a null will be returned instead.</param>
		/// <returns>The downloaded file's contents.</returns>
		/// <exception cref="ObjectDisposedException">This object is disposed.</exception>
		string DownloadAsString(string remoteFile, bool requireExists = false);

		/// <summary>
		/// Enqueues a file for asynchronous downloading.
		/// </summary>
		/// <param name="remotePath">The path to the file to download on the destination.</param>
		/// <param name="sourcePath">The fully qualified path to download the file to.</param>
		/// <returns>True if the file was enqueued; false if either of the arguments were invalid, or the file already
		/// exists in the queue.</returns>
		/// <exception cref="ObjectDisposedException">This object is disposed.</exception>
		bool DownloadAsync(string remotePath, string sourcePath);

		/// <summary>
		/// Enqueues multiple files for asynchronous downloading.
		/// </summary>
		/// <param name="files">The files to download, where the key is the remote file path, and the value is the
		/// fully qualified local path to download the file to.</param>
		/// <exception cref="ObjectDisposedException">This object is disposed.</exception>
		void DownloadAsync(IEnumerable<KeyValuePair<string, string>> files);

	}
}
