using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FarmaciaDAL.DBContext;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using FarmaciaAccDatos.Interfaces;
using FarmaciaAccDatos.Implementacion;
using FarmaciaNegocio.interfaces;
using FarmaciaNegocio.Implementacion;
using FarmaciaNegocio.Interfaces;


namespace FarmaciaControl.Dependencias
{
    public static class Dependencias
    {
        public static void InyectarDependencias(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddDbContext<FarmaciaBDContext>(Options =>
            {
                Options.UseSqlServer(configuration.GetConnectionString("CadenaSql"));
            });

            services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IVentaRepository, VentaRepository>();
            services.AddScoped<ICorreoService, CorreoService>();
            services.AddScoped<ICloudinary, CloudinayServices>();
            services.AddScoped<IUtilidadesService, UtilidadesService>();
            services.AddScoped<IRolService, RolService>();
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<INegocioService, NegocioService>();
            services.AddScoped<ICategoriaService, CategoriaService>();
            services.AddScoped<IProducto, ProductoService>();
            services.AddScoped<ITipoDocumentoVentaService, TipoDocumentoVentaService>();
            services.AddScoped<IVentaService, VentaService>();
            services.AddScoped<IDashBoardService, DashBoardService>();
            services.AddScoped<IMenuservice, MenuService>();
        }
    }
}
