using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using FarmaciaNegocio.interfaces;
using FarmaciaAccDatos.Interfaces;
using FarmaciaENTITY;
using FarmaciaAccDatos.Implementacion;

namespace FarmaciaNegocio.Implementacion
{
    public class CorreoService : ICorreoService
    {
        private readonly IGenericRepository<Configuracion> _repositorio;

        public CorreoService(IGenericRepository<Configuracion> repository)
        {
            _repositorio = repository;
        }
        public async Task<bool> EnviarCorreo(string CorreoDestino, string Asunto, string Mensaje)
        {
            try
            {
                IQueryable<Configuracion> query = await _repositorio.Consultar(c => c.Recurso.Equals("Servicio_Correo"));
                Dictionary<string, string> Config = query.ToDictionary(keySelector:c => c.Propiedad,elementSelector:c => c.Valor);

                var Credenciales = new NetworkCredential(Config["correo"], Config["clave"]);
                var Correo = new MailMessage()
                {
                    From = new MailAddress(Config["correo"], Config["alias"]),
                    Subject = Asunto,
                    Body = Mensaje,
                    IsBodyHtml = true
                };

                Correo.To.Add(new MailAddress(CorreoDestino));

                var ClienteServidor = new SmtpClient()
                {
                    Host = Config["host"],
                    Port = int.Parse(Config["puerto"]),
                    Credentials = Credenciales,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    EnableSsl = true,
                };

                ClienteServidor.Send(Correo);
                return true;

            }
            catch
            {
                return false;
            }
        }
    }
}

