using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.EntityFramework
{
	public interface IBeforeInsert
	{
		void OnBeforeInsert();
	}

	public interface IBeforeUpdate
	{
		void OnBeforeUpdate();
	}

	public interface IBeforeDelete
	{
		void OnBeforeDelete();
	}
}
