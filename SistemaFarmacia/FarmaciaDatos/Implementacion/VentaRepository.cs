using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FarmaciaDAL.DBContext;
using FarmaciaENTITY;
using FarmaciaAccDatos.Interfaces;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace FarmaciaAccDatos.Implementacion
{
    public class VentaRepository : GenericRepository<Venta>, IVentaRepository
    {
        private readonly FarmaciaBDContext _dbContext;

        public VentaRepository(FarmaciaBDContext dbContext): base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Venta> Registar(Venta entidad)
        {
            Venta VentaGenerada = new Venta();
            using (var Transaction = _dbContext.Database.BeginTransaction())
            {
                try
                {
                    foreach (DetalleVenta dv in entidad.DetalleVenta)
                    {
                        Producto producto_encontrado = _dbContext.Productos.Where(p => p.IdProducto == dv.IdProducto).First();

                        producto_encontrado.Stock = producto_encontrado.Stock - dv.Cantidad;
                        _dbContext.Productos.Update(producto_encontrado);
                    }
                    await _dbContext.SaveChangesAsync();

                    NumeroCorrelativo correlativo = _dbContext.NumeroCorrelativos.FirstOrDefault(n => n.Gestion == "Venta");

                    if (correlativo == null)
                    {
                        correlativo = new NumeroCorrelativo
                        {
                            Gestion = "Venta",
                            UltimoNumero = 1,
                            CantidadDigitos = 6,
                            FechaActualizacion = DateTime.Now
                        };
                        _dbContext.NumeroCorrelativos.Add(correlativo);
                        await _dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        correlativo.UltimoNumero += 1;
                        correlativo.FechaActualizacion = DateTime.Now;
                        _dbContext.NumeroCorrelativos.Update(correlativo);
                        await _dbContext.SaveChangesAsync();
                    }

                    string ceros = string.Concat(Enumerable.Repeat("0", correlativo.CantidadDigitos.Value));
                    string numeroVenta = ceros + correlativo.UltimoNumero.ToString();
                    numeroVenta = numeroVenta.Substring(numeroVenta.Length - correlativo.CantidadDigitos.Value, correlativo.CantidadDigitos.Value);

                    entidad.NumeroVenta = numeroVenta;

                    await _dbContext.Venta.AddAsync(entidad);
                    await _dbContext.SaveChangesAsync();

                    VentaGenerada = entidad;
                    Transaction.Commit();
                        
                }
                catch(Exception ex)
                {
                    Transaction.Rollback();
                    throw ex;
                }
            }
            return VentaGenerada;
        }

        public async Task<List<DetalleVenta>> Reporte(DateTime FechaInicio, DateTime FechaFin)
        {
            List<DetalleVenta> ListaResumen = await _dbContext.DetalleVenta
                .Include(v => v.IdVentaNavigation)
                .ThenInclude(u => u.IdUsuarioNavigation)
                .Include(v => v.IdVentaNavigation)
                .ThenInclude(Idv => Idv.IdTipoDocumentoVentaNavigation)
                .Where(dv => dv.IdVentaNavigation.FechaRegistro.Value.Date >= FechaInicio.Date &&
                dv.IdVentaNavigation.FechaRegistro.Value.Date <= FechaFin.Date).ToListAsync();
            return ListaResumen;

        }
    }
}
