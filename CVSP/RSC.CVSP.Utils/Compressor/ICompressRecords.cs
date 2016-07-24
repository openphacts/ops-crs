using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP
{
	public interface ICompressRecords
	{
		bool Compress(IEnumerable<Record> records);
		IEnumerable<Record> Uncompress(string path);
	}
}
