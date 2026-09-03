using FarmaciaENTITY;
using FarmaciaNegocio.Interfaces;
using FarmaciaAccDatos.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmaciaNegocio.Implementacion
{
    public class NegocioService : INegocioService
    {
        private readonly IGenericRepository<Negocio> _repositorio;
        private readonly ICloudinary _cloudinaryService;

        public NegocioService(IGenericRepository<Negocio> repositorio, ICloudinary cloudinary)
        {
            _cloudinaryService = cloudinary;
            _repositorio = repositorio;
        }

        public async Task<Negocio> Obtener()
        {
            try
            {
                Negocio negocio_encontrado = await _repositorio.Obtener(n => n.IdNegocio == 1);
                return negocio_encontrado;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Negocio> GuardarCambios(Negocio entidad, Stream Logo = null, string NombreLogo = "")
        {
            try
            {
                // Obtener el único registro de negocio (ID = 1)
                Negocio negocio_encontrado = await _repositorio.Obtener(n => n.IdNegocio == 1);

                if (negocio_encontrado == null)
                    throw new TaskCanceledException("¡No se encontró el negocio!");

                // Actualizar campos básicos
                negocio_encontrado.NumeroDocumento = entidad.NumeroDocumento;
                negocio_encontrado.Nombre = entidad.Nombre;
                negocio_encontrado.Correo = entidad.Correo;
                negocio_encontrado.Direccion = entidad.Direccion;
                negocio_encontrado.Telefono = entidad.Telefono;
                negocio_encontrado.PorcentajeImpuesto = entidad.PorcentajeImpuesto;
                negocio_encontrado.SimboloMoneda = entidad.SimboloMoneda;

                // Si no hay logo previo, asignar el nombre
                if (string.IsNullOrWhiteSpace(negocio_encontrado.NombreLogo) && !string.IsNullOrWhiteSpace(NombreLogo))
                {
                    negocio_encontrado.NombreLogo = NombreLogo;
                }

                // Subir nuevo logo si se proporciona
                if (Logo != null)
                {
                    string carpetaDestino = await _cloudinaryService.ObtenerCarpetaDestino("folder_logo");

                    // Eliminar logo anterior (si existía)
                    if (!string.IsNullOrWhiteSpace(negocio_encontrado.NombreLogo))
                    {
                        await _cloudinaryService.EliminarImagen(carpetaDestino, negocio_encontrado.NombreLogo);
                    }

                    // Subir nuevo logo
                    var resultado = await _cloudinaryService.SubirImagen(Logo, carpetaDestino, NombreLogo);

                    // Asignar nueva info de logo
                    negocio_encontrado.NombreLogo = NombreLogo;
                    negocio_encontrado.UrlLogo = resultado.SecureUrl;
                }

                // Guardar cambios
                bool editado = await _repositorio.Editar(negocio_encontrado);

                if (!editado)
                    throw new Exception("No se pudieron guardar los cambios del negocio.");

                return negocio_encontrado;
            }
            catch (Exception ex)
            {
                // Aquí puedes registrar logs si tienes un ILogger
                throw new Exception("Error al guardar cambios del negocio: " + ex.Message, ex);
            }
        }
    }
}
