using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagPro.Interfaces
{
	/// <summary>
	/// Interfejs nawigacji obsługujący cofanie się do poprzedniego widoku.
	/// </summary>
	public interface INavigation
	{
		bool CanGoBack();
		void GoBack();
	}
}
