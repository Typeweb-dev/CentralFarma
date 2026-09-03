using FarmaciaApp.Models.ViewModels;
using FarmaciaApp.Utilidades.Response;
using FarmaciaNegocio.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmaciaApp.Controllers
{
    [Authorize]
    public class DashBoardController : Controller
    {
        private readonly IDashBoardService _dashBoardService;

        public DashBoardController(IDashBoardService dashBoardService)
        {
            _dashBoardService = dashBoardService;
        }
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> ObtenerResumen()
        {
            GenericResponse<VMDashBoard> gResponse = new GenericResponse<VMDashBoard>();
            try
            {
                VMDashBoard vmDashBoard = new VMDashBoard();

                vmDashBoard.TotalVentas = await _dashBoardService.TotalVentasUltimasSemanas();
                vmDashBoard.TotalIngresos = await _dashBoardService.TotalIngresoUltimasSemanas();
                vmDashBoard.TotalProductos = await _dashBoardService.TotalProductos();
                vmDashBoard.TotalCategorias = await _dashBoardService.TotalCategorias();

                List<VMVentaSemana> listaVentasSemanas = new List<VMVentaSemana>();
                List<VMProductoSemana> listaProductosSemanas = new List<VMProductoSemana>();

                //Filtro VUS
                foreach (KeyValuePair<string, int> item in await _dashBoardService.VentasUltimasSemanas())
                {
                    listaVentasSemanas.Add(new VMVentaSemana()
                    {
                        Fecha = item.Key,
                        Total = item.Value
                    });
                }

                // Filtro PTUS
                foreach (KeyValuePair<string, int> item in await _dashBoardService.ProductosTopUltimasSemanas())
                {
                    listaProductosSemanas.Add(new VMProductoSemana()
                    {
                        Producto = item.Key,
                        Cantidad = item.Value
                    });
                }

                vmDashBoard.VentasUltimaSemana = listaVentasSemanas;
                vmDashBoard.ProductosTopUltimaSemana = listaProductosSemanas;

                gResponse.Estado = true;
                gResponse.Objeto = vmDashBoard;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK,gResponse);
        }
    }
}
