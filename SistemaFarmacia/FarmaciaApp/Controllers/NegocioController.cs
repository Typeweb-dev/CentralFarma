using AutoMapper;
using FarmaciaApp.Models.ViewModels;
using FarmaciaApp.Utilidades.Response;
using FarmaciaENTITY;
using FarmaciaNegocio.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft;
using Newtonsoft.Json;

namespace FarmaciaApp.Controllers
{
    [Authorize]
    public class NegocioController : Controller
    {
        private readonly IMapper _mapper;
        private readonly INegocioService _negocioServce;

        public NegocioController(IMapper mapper, INegocioService negocioServce)
        {
            _mapper = mapper;
            _negocioServce = negocioServce;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Obtener()
        {
            GenericResponse<VMNegocio> gResponse = new GenericResponse<VMNegocio>();

            try
            {
                VMNegocio vmNegocio = _mapper.Map<VMNegocio>(await _negocioServce.Obtener());

                gResponse.Estado = true;
                gResponse.Objeto = vmNegocio;
            }
            catch(Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarCambios([FromForm]IFormFile logo, [FromForm]string modelo)
        {
            GenericResponse<VMNegocio> gResponse = new GenericResponse<VMNegocio>();

            try
            {
                if (string.IsNullOrWhiteSpace(modelo))
                    throw new Exception("Datos del negocio no recibidos");

                VMNegocio vmNegocio = JsonConvert.DeserializeObject<VMNegocio>(modelo);

                string nombreLogo = "";
                Stream logoStream = null;

                if (logo != null)
                {
                    
                    if (logo.Length > 2 * 1024 * 1024)
                        throw new Exception("El archivo no debe superar los 2MB.");

                    
                    string extension = Path.GetExtension(logo.FileName).ToLower();
                    string[] extensionesPermitidas = [".jpg", ".jpeg", ".png", ".webp"];

                    if (!extensionesPermitidas.Contains(extension))
                        throw new Exception("Formato de imagen no válido. Solo se permiten JPG, PNG, JPEG y WEBP.");

                    
                    string nombre_en_codigo = Guid.NewGuid().ToString("N");
                    nombreLogo = string.Concat(nombre_en_codigo, extension);
                    logoStream = logo.OpenReadStream();
                }

                Negocio negocio_editado = await _negocioServce.GuardarCambios(_mapper.Map<Negocio>(vmNegocio), logoStream, nombreLogo);

                vmNegocio = _mapper.Map<VMNegocio>(negocio_editado);

                gResponse.Estado = true;
                gResponse.Objeto = vmNegocio;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }
    }
}
