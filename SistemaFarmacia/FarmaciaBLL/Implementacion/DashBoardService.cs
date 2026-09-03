using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FarmaciaNegocio.Interfaces;
using FarmaciaAccDatos.Interfaces;
using FarmaciaENTITY;
using System.Globalization;

namespace FarmaciaNegocio.Implementacion
{
    public class DashBoardService : IDashBoardService
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly IGenericRepository<DetalleVenta> _detalleRepository;
        private readonly IGenericRepository<Categoria> _categoriaRepository;
        private readonly IGenericRepository<Producto> _productoRepository;
        private DateTime FechaInicio = DateTime.Now;



        public DashBoardService(IVentaRepository ventaRepository, IGenericRepository<DetalleVenta> detalleRepository,
            IGenericRepository<Categoria> categoriaRepository, IGenericRepository<Producto> productoRepository)
        {
            _ventaRepository = ventaRepository;
            _detalleRepository = detalleRepository;
            _categoriaRepository = categoriaRepository;
            _productoRepository = productoRepository;

            FechaInicio = FechaInicio.AddDays(-7);
        }
        public async Task<int> TotalVentasUltimasSemanas()
        {
            try
            {
                IQueryable<Venta> query = await _ventaRepository.Consultar(v => v.FechaRegistro.Value.Date >= FechaInicio.Date);
                int total = query.Count();
                return total;
            }
            catch
            {
                throw;
            }
        }


        public async Task<string> TotalIngresoUltimasSemanas()
        {
            try
            {
                IQueryable<Venta> query = await _ventaRepository.Consultar(v => v.FechaRegistro.Value.Date >= FechaInicio.Date);

                decimal resultado = query
                    .Select(v => v.Total)
                    .Sum(v => v.Value);

                return Convert.ToString(resultado, new CultureInfo("es-NI"));
            }
            catch
            {
                throw;
            }
        }


        public async Task<int> TotalProductos()
        {
            try
            {
                IQueryable<Producto> query = await _productoRepository.Consultar();
                int total = query.Count();

                return total;
            }
            catch
            {
                throw;
            }
        }
        public async Task<int> TotalCategorias()
        {
            try
            {
                IQueryable<Categoria> query = await _categoriaRepository.Consultar();
                int total = query.Count();

                return total;
            }
            catch
            {
                throw;
            }
        }


        public async Task<Dictionary<string, int>> VentasUltimasSemanas()
        {
            try
            {
                IQueryable<Venta> query = await _ventaRepository.Consultar( v => v.FechaRegistro.Value.Date >= FechaInicio.Date);

                Dictionary<string, int> resultado = query
                    .GroupBy(v => v.FechaRegistro.Value.Date).OrderByDescending(g => g.Key)
                    .Select(dv => new { Fecha = dv.Key.ToString("dd/MM/yyyy"), Total = dv.Count() })
                    .ToDictionary(keySelector: r => r.Fecha, elementSelector: r => r.Total);

                return resultado;
            }
            catch
            {
                throw;
            }
        }


        public async Task<Dictionary<string, int>> ProductosTopUltimasSemanas()
        {
            try
            {
                IQueryable<DetalleVenta> query = await _detalleRepository.Consultar();

                Dictionary<string, int> resultado = query
                    .Include(v => v.IdVentaNavigation)
                    .Where(dv =>dv.IdVentaNavigation.FechaRegistro.Value.Date >= FechaInicio.Date)
                    .GroupBy(dv => dv.DescripcionProducto).OrderByDescending(g => g.Count())
                    .Select(dv => new { Producto = dv.Key, Total = dv.Count()}).Take(4)
                    .ToDictionary(keySelector: r => r.Producto, elementSelector: r => r.Total);

                return resultado;
            }
            catch
            {
                throw;
            }
        }
    }
}
