using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaciaNegocio.Interfaces
{
    public interface IDashBoardService
    {
        Task<int> TotalVentasUltimasSemanas();
        Task<string> TotalIngresoUltimasSemanas();
        Task<int> TotalProductos();
        Task<int> TotalCategorias();
        Task<Dictionary<string,int>> VentasUltimasSemanas();
        Task<Dictionary<string,int>> ProductosTopUltimasSemanas();
    }
}
