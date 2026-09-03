using FarmaciaNegocio.Cloudinary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaciaNegocio.Interfaces
{
    public interface ICloudinary
    {
        Task<CloudinaryResponse> SubirImagen(Stream StreamArchivo, string CarpetaDestino, string NombreArchivo);
        Task<bool> EliminarImagen(string CarpetaDestino, string NombreArchivo);
        Task<string> ObtenerCarpetaDestino(string clave);
    }
}
