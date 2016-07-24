using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

namespace ChemSpider.Formats
{
    public class FileEncoding
    {
        /// <summary>
        /// Reads a file and converts it to 1252 and returns the contents as a string.
        /// </summary>
        /// <param name="fileName">The filename of the file to be converted.</param>
        /// <returns>A string that is converted to 1252.</returns>
        public static String readFileAs1252(string fileName)
        {
            try
            {
                Encoding encoding = Encoding.Default;
                String original = String.Empty;

                //Open file using readonly reader.
                using (StreamReader sr = new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read), Encoding.Default))
                {
                    original = sr.ReadToEnd();
                    encoding = sr.CurrentEncoding;
                    sr.Close();
                }

                if (encoding == Encoding.GetEncoding(1252))
                    return original;

                byte[] encBytes = encoding.GetBytes(original);
                byte[] onetwofivetwoBytes = Encoding.Convert(encoding, Encoding.GetEncoding(1252), encBytes);
                return Encoding.GetEncoding(1252).GetString(onetwofivetwoBytes);
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }
    }
}
