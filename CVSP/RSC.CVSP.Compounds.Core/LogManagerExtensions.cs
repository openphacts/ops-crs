using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RSC.Logging;

namespace RSC.CVSP.Compounds
{
    public static class LogManagerExtensions
    {
        public static string GetCode(this LogManager manager, string name)
        {
            var entryLog = LogManager.Logger.EntryTypes.Where(e => !string.IsNullOrEmpty(e.ShortName) && e.ShortName.Equals(name, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

            if (entryLog == null)
                throw new KeyNotFoundException("Entry log with name " + name + " cannot be found!");

            return entryLog.Code;
        }

        public static Issue GetIssue(this LogManager manager, string name)
        {
            throw new NotImplementedException();
        }
    }
}