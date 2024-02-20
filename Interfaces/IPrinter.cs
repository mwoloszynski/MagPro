using MagPro.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace MagPro.Interfaces
{
	public interface IPrinter
	{
		List<FixedPage> CreatePage(DocumentType type);
		void AddPage(List<FixedPage> page);
		void Print();
	}
}
