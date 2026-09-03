using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarmaciaENTITY;

namespace FarmaciaNegocio.Interfaces
{
    public interface IProducto
    {
        Task<List<Producto>> Lista();
        Task<Producto> Crear(Producto entidad,Stream imagen = null, string NombreImagen = "");
        Task<Producto> Editar(Producto entidad,Stream imagen = null);
        Task<bool> Eliminar(int idProducto);
    }
}
