using System;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;


namespace RSC.CVSP.Utils
{
    public class FTP_Related
    {
		public static bool isFTPFileExist(string FTPFilePath, string userName, string password)
		{
			try
			{
				WebClient wc = new WebClient();
				wc.Credentials = new NetworkCredential(userName, password);
				byte[] fData = wc.DownloadData(FTPFilePath);
				if (fData.Length > -1)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}


        /// <summary>
        /// get lits of files from ftp dir
        /// </summary>
        /// <returns></returns>
        public static string[] GetFileList(string ftpDir, string userName, string password)
        {
            string[] ftpFiles;
            StringBuilder result = new StringBuilder();
            WebResponse response = null;
            StreamReader reader = null;
            try
            {
                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(ftpDir);
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(userName, password);
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
                reqFTP.Proxy = null;
                reqFTP.KeepAlive = false;
                reqFTP.UsePassive = false;
                response = reqFTP.GetResponse();
                reader = new StreamReader(response.GetResponseStream());
                string line = reader.ReadLine();
                while (line != null)
                {
                    result.Append(line);
                    result.Append("\n");
                    line = reader.ReadLine();
                }
                // to remove the trailing '\n'
                result.Remove(result.ToString().LastIndexOf('\n'), 1);
                return result.ToString().Split('\n');
            }
            catch (Exception)
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (response != null)
                {
                    response.Close();
                }
                ftpFiles = null;
                return ftpFiles;
            }
        }

        public static void UploadFile2Ftp(string localFilePath, string remoteFtpDestinationFile, string userName, string password)
        {
            try{
                Console.WriteLine("Starting upload of " + @localFilePath);
                FtpWebRequest request_1 = (FtpWebRequest)WebRequest.Create(remoteFtpDestinationFile);
                request_1.UseBinary = true;
                request_1.Method = WebRequestMethods.Ftp.UploadFile;
                //request_1.KeepAlive = false;
                //request_1.Timeout = int.MaxValue;
                request_1.UsePassive = true;
                //request_1.EnableSsl = false;
                //request_1.ReadWriteTimeout = int.MaxValue;
                request_1.Credentials = new NetworkCredential(userName, password);
                request_1.ContentLength = (new FileInfo(localFilePath)).Length;
                const int bufferLength = 10240;
                byte[] buffer = new byte[bufferLength];
                using (FileStream fileStream = new FileStream(localFilePath,FileMode.Open))
                {
                    int readBytes = 0;
                    using (Stream request_Stream_1 = request_1.GetRequestStream())
                    {
                        //request_Stream_1.WriteTimeout = int.MaxValue;
                        //FileStream fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read);
                        do
                        {
                            readBytes = fileStream.Read(buffer, 0, buffer.Length);
                            request_Stream_1.Write(buffer, 0, readBytes);
                        } while (readBytes != 0);

                    }
                }
                //FtpWebResponse response_1 = (FtpWebResponse)request_1.GetResponse();
                //                Console.WriteLine("Upload of " + @localFilePath + " Complete, status {0}", response_1.StatusDescription);
                //response_1.Close();
                
                
                
            } catch(Exception ex)
            {
                Console.WriteLine("FTP upload of file " + @localFilePath + " to " + remoteFtpDestinationFile + " failed: " + Environment.NewLine + ex.Message);
                
            }
        }

        public static bool downloadFtpFiles(string remoteDirPath, string localDirPath,string userName, string password)
        {
            try
            {
                string[] files = GetFileList(remoteDirPath, userName, password);
                foreach (string file in files)
                {
                    if (file.Contains(".xml.gz") || file.Contains(".processed"))
                    {
                        string localFilePath = Path.Combine(localDirPath, Path.GetFileName(file));
                        downloadFtpFile(remoteDirPath + @"\" + file, localFilePath, userName, password);
                        
                    }
                    Thread.Sleep(1000);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
                return false;
            }
            
        }

        public static bool deleteFtpFile(string remoteFilePath, string userName, string password)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remoteFilePath);
                request.Credentials = new NetworkCredential(userName, password);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                response.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public static bool deleteFTPDirectory(string remoteDirectory, string userName, string password)
        {
            // Get the object used to communicate with the server.
            try
            {
                string[] files = GetFileList(remoteDirectory, userName, password);
                if(files!= null)
                    foreach (string file in files)
                        deleteFtpFile(remoteDirectory+@"\"+file, userName, password);

                //==============THIS IS TO DELETE FOLDER========================================
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(remoteDirectory);
                ftpRequest.Credentials = new NetworkCredential(userName, password);
                ftpRequest.Method = WebRequestMethods.Ftp.RemoveDirectory;
                ftpRequest.UsePassive = true;
                FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
                
                return true;
            } catch(WebException ex)
            {
				FtpWebResponse response = (FtpWebResponse)ex.Response;
				if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
				{
					response.Close();
					return true;
				}
				else
				{
					response.Close();
					return false;
				}  
            }
        }

        public static bool downloadFtpFile(string remoteFilePath, string localFilePath, string userName, string password)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remoteFilePath);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(userName, password);
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                using ( FileStream fileStream = File.Create(localFilePath))
                    responseStream.CopyTo(fileStream);
                response.Close();
				return true;
            }
			catch (WebException ex)
			{
				FtpWebResponse response = (FtpWebResponse)ex.Response;
				if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
				{
					response.Close();
				}
				else
				{
					response.Close();
				}
				return false;
			}
        }


        public static void UploadEmptyFile2Ftp(string remoteFtpDestinationFilePath, string userName, string password)
        {
            try
            {
                Console.WriteLine("Starting creation of " + @remoteFtpDestinationFilePath);
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remoteFtpDestinationFilePath);
                request.UseBinary = true;
                request.Method = WebRequestMethods.Ftp.UploadFile;
                //request.KeepAlive = false;
                //request.Timeout = int.MaxValue;
                //request.UsePassive = true;
                //request.EnableSsl = false;
                //request.ReadWriteTimeout = int.MaxValue;
                request.Credentials = new NetworkCredential(userName, password);
                //request_1.ContentLength = 0;
                string temp_file = Path.GetTempFileName();
                using (StreamWriter sourceStream = new StreamWriter(temp_file))
                    sourceStream.WriteLine("empty start file");

                StreamReader sourceStream_1 = new StreamReader(temp_file);
                byte[] ByteContent_1 = Encoding.ASCII.GetBytes(sourceStream_1.ReadToEnd());

                //request.ContentLength = ByteContent_1.Length;
                Stream request_Stream_1 = request.GetRequestStream();
                request_Stream_1.Write(ByteContent_1, 0, ByteContent_1.Length);
                
                //FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                //Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

                request_Stream_1.Close();
                //response.Close();
                File.Delete(temp_file);
            }
            catch (Exception ex)
            {
                Console.WriteLine("FTP upload of file " + @remoteFtpDestinationFilePath + " failed: " + ex.Message + "\n" + ex.StackTrace);
                throw;
            }
        }

        public static bool createFtpDirectory(string remoteFtpDestinationFolder, string userName, string password)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(remoteFtpDestinationFolder);
                request.Credentials = new NetworkCredential(userName, password);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.UsePassive = true;
                request.UseBinary = true;
                request.KeepAlive = false;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    return false;
                else
                    return true;
            }
            return true;

        }
    }
}
