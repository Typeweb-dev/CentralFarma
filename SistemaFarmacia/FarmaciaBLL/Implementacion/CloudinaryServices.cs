using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FarmaciaAccDatos.Implementacion;
using FarmaciaAccDatos.Interfaces;
using FarmaciaENTITY;
using FarmaciaNegocio.Cloudinary;
using FarmaciaNegocio.interfaces;
using FarmaciaNegocio.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ICloudinary = FarmaciaNegocio.Interfaces.ICloudinary;



namespace FarmaciaNegocio.Implementacion
{
    public class CloudinayServices : ICloudinary
    {
        private readonly IGenericRepository<Configuracion> _repositorio;

        public CloudinayServices(IGenericRepository<Configuracion> repositorio)
        {
            _repositorio = repositorio;
        }

        private async Task<CloudinaryDotNet.Cloudinary> ObtenerCliente()
        {
            var configQuery = await _repositorio.Consultar(c => c.Recurso == "Cloudinary");
            var configList = configQuery.ToList();

            string cloudName = configList.FirstOrDefault(x => x.Propiedad == "cloud_name")?.Valor;
            string apiKey = configList.FirstOrDefault(x => x.Propiedad == "api_key")?.Valor;
            string apiSecret = configList.FirstOrDefault(x => x.Propiedad == "api_secret")?.Valor;

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                throw new Exception("Faltan configuraciones de Cloudinary.");
            }

            var cuenta = new Account(cloudName, apiKey, apiSecret);
            return new CloudinaryDotNet.Cloudinary(cuenta);
        }

        public async Task<CloudinaryResponse> SubirImagen(Stream StreamArchivo, string CarpetaDestino, string NombreArchivo)
        {
            var cloudinary = await ObtenerCliente();

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(NombreArchivo, StreamArchivo),
                Folder = CarpetaDestino, // ✅ Aquí se crea la carpeta correctamente
                PublicId = Path.GetFileNameWithoutExtension(NombreArchivo),
                Overwrite = true
            };

            var uploadResult = await cloudinary.UploadAsync(uploadParams);

            if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return new CloudinaryResponse
                {
                    PublicId = uploadResult.PublicId,
                    SecureUrl = uploadResult.SecureUrl.ToString()
                };
            }
            else
            {
                throw new Exception($"Error al subir imagen: {uploadResult.Error?.Message}");
            }
        }

        public async Task<bool> EliminarImagen(string CarpetaDestino, string NombreArchivo)
        {

            var cloudinary = await ObtenerCliente();
            var publicId = $"{CarpetaDestino}/{NombreArchivo}";

            var deletionParams = new DeletionParams(publicId);
            var result = await cloudinary.DestroyAsync(deletionParams);

            return result.Result == "ok" || result.Result == "not found";
        }

        public async Task<string> ObtenerCarpetaDestino(string clave)
        {
            var configQuery = await _repositorio.Consultar(c => c.Recurso == "Cloudinary" && c.Propiedad == clave);
            var config = configQuery.FirstOrDefault();

            if (config == null)
                throw new Exception($"No se encontró la configuración para '{clave}'");

            return $"CentralFarma/{config.Valor}";
        }
    }
}
