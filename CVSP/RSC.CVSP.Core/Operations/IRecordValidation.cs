using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP
{
	public interface IRecordValidation
	{
		Record Validate(Record record);
	}
}
