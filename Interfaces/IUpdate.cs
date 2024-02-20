using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagPro.Interfaces
{
    /// <summary>
    /// Interfejs do aktualizowania danych
    /// Używany przez większość klas do aktualizacji
    /// danych interfejsu graficznego pobranych z bazy danych
    /// </summary>
    public interface IUpdate
    {
        void Refresh();
    }
}
