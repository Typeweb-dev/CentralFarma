using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FarmaciaAccDatos.Interfaces;
using FarmaciaENTITY;
using FarmaciaNegocio.Interfaces;

namespace FarmaciaNegocio.Implementacion
{
    public class ProductoService : IProducto
    {
        private readonly IGenericRepository<Producto> _repositorio;
        private readonly ICloudinary _cloudinary;
        private readonly IGenericRepository<Configuracion> _repositorioConfiguracion;

        public ProductoService(IGenericRepository<Producto> repositorio, ICloudinary cloudinary, 
          IGenericRepository<Configuracion> repositorioConfiguracion)
        {
            _repositorio = repositorio;
            _cloudinary = cloudinary;
            _repositorioConfiguracion = repositorioConfiguracion; 
        }


        public async Task<List<Producto>> Lista()
        {
            IQueryable<Producto> query = await _repositorio.Consultar();
            return query.Include(c => c.IdCategoriaNavigation).ToList();
        }

        public async Task<Producto> Crear(Producto entidad, Stream imagen = null, string NombreImagen = "")
        {
            try
            {
                var producto_existe = await _repositorio.Obtener(p => p.CodigoBarra == entidad.CodigoBarra);
                if (producto_existe != null)
                    throw new TaskCanceledException("El código de barra ya existe");

               
                var configuraciones = await _repositorioConfiguracion.Consultar(c => c.Recurso == "Cloudinary");
                string carpetaProducto = configuraciones.FirstOrDefault(c => c.Propiedad == "folder_producto")?.Valor
                                         ?? "CentralFarma/IMAGENES_PRODUCTO";

                
                entidad.NombreImagen = NombreImagen;

                
                if (imagen != null)
                {
                    var resultado = await _cloudinary.SubirImagen(imagen, carpetaProducto, NombreImagen);
                    entidad.UrlImagen = resultado.SecureUrl;
                }

                
                var producto_creado = await _repositorio.Crear(entidad);
                if (producto_creado.IdProducto == 0)
                    throw new TaskCanceledException("No se pudo crear el producto");

                
                IQueryable<Producto> query = await _repositorio.Consultar(p => p.IdProducto == producto_creado.IdProducto);
                producto_creado = query.Include(c => c.IdCategoriaNavigation).FirstOrDefault();

                return producto_creado;
            }
            catch (Exception ex)
            {
                
                throw new Exception("Error al crear el producto: " + ex.Message, ex);
            }
        }

        public async Task<Producto> Editar(Producto entidad, Stream imagen = null)
        {
            try
            {
              
                var producto_existe = await _repositorio.Obtener(p => p.CodigoBarra == entidad.CodigoBarra
                                                                    && p.IdProducto != entidad.IdProducto);
                if (producto_existe != null)
                    throw new TaskCanceledException("El código de barra ya existe");

                
                IQueryable<Producto> queryProducto = await _repositorio.Consultar(p => p.IdProducto == entidad.IdProducto);
                Producto producto_editar = queryProducto.FirstOrDefault();

                if (producto_editar == null)
                    throw new TaskCanceledException("No se encontró el producto");

                
                producto_editar.CodigoBarra = entidad.CodigoBarra;
                producto_editar.Marca = entidad.Marca;
                producto_editar.Descripcion = entidad.Descripcion;
                producto_editar.IdCategoria = entidad.IdCategoria;
                producto_editar.Stock = entidad.Stock;
                producto_editar.Precio = entidad.Precio;
                producto_editar.EsActivo = entidad.EsActivo;

                
                if (imagen != null)
                {
                    string carpetaDestino = await _cloudinary.ObtenerCarpetaDestino("folder_producto");

               
                    if (!string.IsNullOrEmpty(producto_editar.NombreImagen))
                    {
                        await _cloudinary.EliminarImagen(carpetaDestino, producto_editar.NombreImagen);
                    }

                    
                    string nuevoNombreImagen = Guid.NewGuid().ToString();
                    var resultado = await _cloudinary.SubirImagen(imagen, carpetaDestino, nuevoNombreImagen);

                    producto_editar.NombreImagen = nuevoNombreImagen;
                    producto_editar.UrlImagen = resultado.SecureUrl;
                }

               
                bool respuesta = await _repositorio.Editar(producto_editar);

                if (!respuesta)
                    throw new TaskCanceledException("No se pudo modificar el producto");

                
                Producto producto_editado = queryProducto.Include(c => c.IdCategoriaNavigation).FirstOrDefault();

                return producto_editado;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al editar el producto: " + ex.Message, ex);
            }
        }

        public async Task<bool> Eliminar(int idProducto)
        {
            try
            {
               
                Producto producto_encontrado = await _repositorio.Obtener(p => p.IdProducto == idProducto);

                if (producto_encontrado == null)
                    throw new TaskCanceledException("El producto no existe...");

                string nombreImagen = producto_encontrado.NombreImagen;

                
                bool respuesta = await _repositorio.Eliminar(producto_encontrado);

                if (!respuesta)
                    throw new TaskCanceledException("No se pudo eliminar el producto.");

               
                if (!string.IsNullOrWhiteSpace(nombreImagen))
                {
                    string carpeta = await _cloudinary.ObtenerCarpetaDestino("folder_producto");
                    await _cloudinary.EliminarImagen(carpeta, nombreImagen);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el producto: " + ex.Message, ex);
            }
        }
    }
}
