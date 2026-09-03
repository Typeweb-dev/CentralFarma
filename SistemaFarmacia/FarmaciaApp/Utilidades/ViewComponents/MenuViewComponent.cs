using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FarmaciaApp.Models.ViewModels;
using FarmaciaNegocio.Interfaces;

namespace FarmaciaApp.Utilidades.ViewComponents
{

    public class MenuViewComponent : ViewComponent
    {
        private readonly IMenuservice _menuService;
        private readonly IMapper _mapper;

        public MenuViewComponent(IMenuservice menuService, IMapper mapper)
        {
            _menuService = menuService;
            _mapper = mapper;
        }


        public async Task<IViewComponentResult> InvokeAsync()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            List<VMMenu> listaMenus;

            if(claimUser.Identity.IsAuthenticated)
            {
                string idUsuario = claimUser.Claims
               .Where(c => c.Type == ClaimTypes.NameIdentifier)
               .Select(c => c.Value).SingleOrDefault();

                listaMenus = _mapper.Map<List<VMMenu>>(await _menuService.ObtenerMenus(int.Parse(idUsuario)));
            }
            else
            {
                listaMenus = new List<VMMenu>();
            }

            return View(listaMenus);
        }
    }
}
