using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FarmaciaNegocio.Interfaces;
using FarmaciaAccDatos.Interfaces;
using FarmaciaENTITY;

namespace FarmaciaNegocio.Implementacion
{
    public class MenuService : IMenuservice
    {
        private readonly IGenericRepository<Menu> _menuRepository;
        private readonly IGenericRepository<RolMenu> _rolMenuRepository;
        private readonly IGenericRepository<Usuario> _usuarioRepository;


        public MenuService(IGenericRepository<Menu> enuRepository,
            IGenericRepository<RolMenu> rolMenuRepository, IGenericRepository<Usuario> usuarioRepository)
        {
            _menuRepository = enuRepository;
            _rolMenuRepository = rolMenuRepository;
            _usuarioRepository = usuarioRepository;
        }
        public async Task<List<Menu>> ObtenerMenus(int idUsuario)
        {
            IQueryable<Usuario> tbUsuario = await _usuarioRepository.Consultar(u => u.IdUsuario == idUsuario);
            IQueryable<RolMenu> tbRolMenu = await _rolMenuRepository.Consultar();
            IQueryable<Menu> tbMenu = await _menuRepository.Consultar();

            // asignacion de los menus
            IQueryable<Menu> MenuPadre = (from u in tbUsuario
                                          join rm in tbRolMenu on u.IdRol equals rm.IdRol
                                          join m in tbMenu on rm.IdMenu equals m.IdMenu
                                          join mpadre in tbMenu on m.IdMenuPadre equals mpadre.IdMenu
                                          select mpadre).Distinct().AsQueryable();

            // asignacion de los menus
            IQueryable<Menu> MenuHijos = (from u in tbUsuario
                                          join rm in tbRolMenu on u.IdRol equals rm.IdRol
                                          join m in tbMenu on rm.IdMenu equals m.IdMenu
                                          where m.IdMenu != m.IdMenuPadre
                                          select m).Distinct().AsQueryable();

            List<Menu> listaMenu = (from mpadre in MenuPadre
                                    select new Menu()
                                    {
                                        Descripcion = mpadre.Descripcion,
                                        Icono = mpadre.Icono,
                                        Controlador = mpadre.Controlador,
                                        PaginaAccion = mpadre.PaginaAccion,
                                        InverseIdMenuPadreNavigation = (from mhijo in MenuHijos
                                                                        where mhijo.IdMenuPadre == mpadre.IdMenu
                                                                        select mhijo).ToList()

                                    }).ToList();
            return listaMenu;
        }
    }
}
