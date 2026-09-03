using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Net;
using FarmaciaNegocio.Interfaces;
using FarmaciaAccDatos.Interfaces;
using FarmaciaENTITY;
using CloudinaryDotNet;
using FarmaciaNegocio.interfaces;
using Microsoft.VisualBasic;

namespace FarmaciaNegocio.Implementacion
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IGenericRepository<Usuario> _repositorio;
        private readonly Interfaces.ICloudinary _cloudinary;
        private readonly IUtilidadesService _utilidadesService;
        private readonly ICorreoService _correoService;

        public UsuarioService(IGenericRepository<Usuario> repositorio, Interfaces.ICloudinary cloudinary,
            IUtilidadesService utilidadesService, ICorreoService correoService)
        {
            _repositorio = repositorio;
            _cloudinary = cloudinary;
            _utilidadesService = utilidadesService;
            _correoService = correoService;
        }


        public async Task<List<Usuario>> Lista()
        {
            IQueryable<Usuario> query = await _repositorio.Consultar();
            return query.Include(r => r.IdRolNavigation).ToList();            
        }


        public async Task<Usuario> Crear(Usuario entidad, Stream Foto = null, string NombreFoto = "", string UrlPlantillaCorreo = "")
        {

            try
            {
                // Verificar si el correo ya existe
                Usuario usuario_existe = await _repositorio.Obtener(u => u.Correo == entidad.Correo);
                if (usuario_existe != null)
                    throw new TaskCanceledException("¡El correo ya existe!");

                // Generar clave temporal y hashearla
                string clave_generada = _utilidadesService.GenerarClave();
                entidad.Clave = _utilidadesService.ConvertirSha256(clave_generada);
                entidad.NombreFoto = NombreFoto;

                // Subir la foto si se recibe
                if (Foto != null)
                {
                    // Obtener carpeta desde configuración
                    string carpetaDestino = await _cloudinary.ObtenerCarpetaDestino("folder_usuario");
                    var respuesta_crear = await _cloudinary.SubirImagen(Foto, carpetaDestino, NombreFoto);
                    entidad.UrlFoto = respuesta_crear.SecureUrl;
                }

                // Crear el usuario en base de datos
                Usuario usuario_creado = await _repositorio.Crear(entidad);

                if (usuario_creado.IdUsuario == 0)
                    throw new TaskCanceledException("¡No se pudo crear el usuario!");

                // Enviar correo si se proporcionó una URL de plantilla
                if (!string.IsNullOrEmpty(UrlPlantillaCorreo))
                {
                    string urlPersonalizada = UrlPlantillaCorreo
                        .Replace("[correo]", usuario_creado.Correo)
                        .Replace("[clave]", clave_generada);

                    string htmlCorreo = "";

                    try
                    {
                        var request = (HttpWebRequest)WebRequest.Create(urlPersonalizada);
                        var response = (HttpWebResponse)await request.GetResponseAsync();

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using var dataStream = response.GetResponseStream();
                            using var readerStream = new StreamReader(
                                dataStream,
                                response.CharacterSet == null
                                    ? Encoding.UTF8
                                    : Encoding.GetEncoding(response.CharacterSet)
                            );
                            htmlCorreo = await readerStream.ReadToEndAsync();
                        }

                        if (!string.IsNullOrEmpty(htmlCorreo))
                            await _correoService.EnviarCorreo(usuario_creado.Correo, "Cuenta Creada", htmlCorreo);
                    }
                    catch (Exception exCorreo)
                    {
                        // Aquí puedes loguear si falla la carga de plantilla o el envío
                        Console.WriteLine("Error al enviar correo: " + exCorreo.Message);
                    }
                }

                // Traer al usuario con navegación del rol
                IQueryable<Usuario> query = await _repositorio.Consultar(u => u.IdUsuario == usuario_creado.IdUsuario);
                usuario_creado = query.Include(r => r.IdRolNavigation).FirstOrDefault();

                return usuario_creado;
            }
            catch (Exception ex)
            {
                // Loguea el error si tienes Logger
                throw new Exception("Error al crear el usuario: " + ex.Message, ex);
            }
        }


        public async Task<Usuario> Editar(Usuario entidad, Stream Foto = null, string NombreFoto = "")
        {

            Usuario usuario_existe = await _repositorio.Obtener(u => u.Correo == entidad.Correo && u.IdUsuario != entidad.IdUsuario);
            if (usuario_existe != null)
                throw new TaskCanceledException("¡El correo ya existe!");

            try
            {
                IQueryable<Usuario> queryUsuario = await _repositorio.Consultar(u => u.IdUsuario == entidad.IdUsuario);
                Usuario usuario_editar = queryUsuario.First();

                usuario_editar.Nombre = entidad.Nombre;
                usuario_editar.Correo = entidad.Correo;
                usuario_editar.Telefono = entidad.Telefono;
                usuario_editar.IdRol = entidad.IdRol;
                usuario_editar.EsActivo = entidad.EsActivo;

                if (string.IsNullOrWhiteSpace(usuario_editar.NombreFoto))
                    usuario_editar.NombreFoto = NombreFoto;

                if (Foto != null)
                {
                    string carpetaDestino = await _cloudinary.ObtenerCarpetaDestino("folder_usuario");

                    // Si ya tenía una imagen, eliminarla primero
                    if (!string.IsNullOrWhiteSpace(usuario_editar.NombreFoto))
                    {
                        await _cloudinary.EliminarImagen(carpetaDestino, usuario_editar.NombreFoto);
                    }

                    // Subir nueva imagen
                    var respuesta_editar = await _cloudinary.SubirImagen(Foto, carpetaDestino, NombreFoto);
                    usuario_editar.NombreFoto = NombreFoto;
                    usuario_editar.UrlFoto = respuesta_editar.SecureUrl;
                }

                bool respuesta = await _repositorio.Editar(usuario_editar);
                if (!respuesta)
                    throw new TaskCanceledException("¡No se pudo modificar el usuario!");

                Usuario usuario_editado = queryUsuario.Include(r => r.IdRolNavigation).First();
                return usuario_editado;
            }
            catch
            {
                throw;
            }
        }


        public async Task<bool> Eliminar(int IdUsuario)
        {
            try
            {
                Usuario Usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario == IdUsuario);
                if (Usuario_encontrado == null)
                    throw new TaskCanceledException("¡El usuario no existe!");

                string nombreFoto = Usuario_encontrado.NombreFoto;
                bool repuesta = await _repositorio.Eliminar(Usuario_encontrado);

                if (repuesta && !string.IsNullOrWhiteSpace(nombreFoto))
                {
                    string carpetaDestino = await _cloudinary.ObtenerCarpetaDestino("folder_usuario");
                    await _cloudinary.EliminarImagen(carpetaDestino, nombreFoto);
                }

                return true;
            }
            catch
            {
                throw;
            }
        }


        public async Task<Usuario> ObtenerPorCredenciales(string correo, string clave)
        {
            string clave_encriptada = _utilidadesService.ConvertirSha256(clave);

            Usuario Usuario_encontrado = await _repositorio.Obtener(u => u.Correo.Equals(correo) && u.Clave.Equals(clave_encriptada));
            return Usuario_encontrado;
        }


        public async Task<Usuario> ObtenerPorId(int IdUsuario)
        {
            IQueryable<Usuario> query =  await _repositorio.Consultar(u => u.IdUsuario == IdUsuario);

            Usuario resultado = query.Include(r => r.IdRolNavigation).FirstOrDefault();
            return resultado;
        }


        public async Task<bool> GuardarPerfil(Usuario entidad)
        {
           try
            {
                Usuario Usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario == entidad.IdUsuario);

                if (Usuario_encontrado == null)
                    throw new TaskCanceledException("¡El usuario no existe!");

                Usuario_encontrado.Correo = entidad.Correo;
                Usuario_encontrado.Telefono = entidad.Telefono;

                bool respuesta = await _repositorio.Editar(Usuario_encontrado);
                return respuesta;
            }
            catch
            {
                throw;
            }
        }


        public async Task<bool> CambiarClave(int IdUsuario, string ClaveActual, string ClaveNueva)
        {
            try
            {
                Usuario Usuario_encontrado = await _repositorio.Obtener(u => u.IdUsuario == IdUsuario);

                if (Usuario_encontrado == null)
                    throw new TaskCanceledException("¡El usuario no existe!");

                if (Usuario_encontrado.Clave != _utilidadesService.ConvertirSha256(ClaveActual))
                    throw new TaskCanceledException("¡La contraseña ingresada como actual no es correcta!");

                Usuario_encontrado.Clave = _utilidadesService.ConvertirSha256(ClaveNueva);
                bool repuesta = await _repositorio.Editar(Usuario_encontrado);

                return repuesta;

            }
            catch(Exception ex)
            {
                throw;
            }
        }


        public async Task<bool> RestablecerClave(string Correo, string UrlPlantillaCorreo)
        {
            try
            {
                Usuario Usuario_encontrado = await _repositorio.Obtener(u => u.Correo == Correo);

                if (Usuario_encontrado == null)
                    throw new TaskCanceledException("¡No se encontro ningún usuario asociado al correo!...");

                string clave_generada = _utilidadesService.GenerarClave();
                Usuario_encontrado.Clave = _utilidadesService.ConvertirSha256(clave_generada);


                UrlPlantillaCorreo = UrlPlantillaCorreo.Replace("[clave]", clave_generada);

                string htmlCorreo = "";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UrlPlantillaCorreo);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        StreamReader readerStream = null;
                        if (response.CharacterSet == null)
                            readerStream = new StreamReader(dataStream);
                        else
                            readerStream = new StreamReader(dataStream, Encoding.GetEncoding(response.CharacterSet));

                        htmlCorreo = readerStream.ReadToEnd();
                        response.Close();
                        readerStream.Close();
                    }
                }

                bool correo_enviado = false;

                if (htmlCorreo != "")
                    correo_enviado = await _correoService.EnviarCorreo(Correo, "Contraseña Restablecida", htmlCorreo);

                if(!correo_enviado)
                    throw new TaskCanceledException("¡Tenemos problemas... inténtalo más tarde!");

                bool respuesta = await _repositorio.Editar(Usuario_encontrado);
                return respuesta;
            }
            catch
            {
                throw;
            }
        }
    }
}
