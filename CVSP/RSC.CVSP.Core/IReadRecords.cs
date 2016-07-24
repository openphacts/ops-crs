using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP
{
	public interface IReadRecords
	{
		IEnumerable<Record> ReadChunk(int size=1000);

		void Release();

		void Reset();
	}
}
