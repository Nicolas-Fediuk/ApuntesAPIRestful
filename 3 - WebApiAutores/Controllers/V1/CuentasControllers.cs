using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.AccessControl;
using System.Security.Claims;
using System.Text;
using WebApiAutores.DTOs;
using WebApiAutores.Servicios;

namespace WebApiAutores.Controllers.V1
{
    [ApiController]
    [Route("api/v1/cuentas")]
    public class CuentasControllers : ControllerBase
    {
        private readonly ApplicationDbContex contex;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly HashService hashService;
        private readonly IDataProtector dataProtection;

        public CuentasControllers(ApplicationDbContex applicationDbContex,
            UserManager<IdentityUser> userManager,
            IConfiguration configuration,
            SignInManager<IdentityUser> signInManager,
            IDataProtectionProvider dataProtectionProvider,
            HashService hashService)
        {
            contex = applicationDbContex;
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
            this.hashService = hashService;
            //se crea la llave de proposito que es parte de la encriptacion
            dataProtection = dataProtectionProvider.CreateProtector("3F2504E0-4F89-11D3-9A0C-0305E82C3301");
        }

        //Esta es la mejor forma de guardar contraseñas
        [HttpGet("hash/{textoPlano}")]
        [AllowAnonymous]
        public ActionResult RealizarHash(string textoPlano)
        {
            var resultado1 = hashService.Hash(textoPlano);
            var resultado2 = hashService.Hash(textoPlano);

            return Ok(new
            {
                textoPlano,
                Hash1 = resultado1,
                Hash2 = resultado2,

            });
        }

        //Para encryptar y desencryptar datos
        [HttpGet("encriptar")]
        public ActionResult Encryptar()
        {
            var textoPlano = "Nicolas Fediuk";
            var textoCifrado = dataProtection.Protect(textoPlano);
            var textoDesencriptado = dataProtection.Unprotect(textoCifrado);

            return Ok(new
            {
                textoPlano,
                textoCifrado,
                textoDesencriptado
            });
        }



        //Para encryptar y desencryptar datos por un limite tiempo
        [HttpGet("encriptarPorTiempo")]
        [AllowAnonymous]
        public ActionResult EncryptarPorTiempo()
        {
            var protectoPorTimepoLimitado = dataProtection.ToTimeLimitedDataProtector();

            var textoPlano = "Nicolas Fediuk";
            var textoCifrado = protectoPorTimepoLimitado.Protect(textoPlano, lifetime: TimeSpan.FromSeconds(5));
            Thread.Sleep(TimeSpan.FromSeconds(6));
            var textoDesencriptado = protectoPorTimepoLimitado.Unprotect(textoCifrado);

            return Ok(new
            {
                textoPlano,
                textoCifrado,
                textoDesencriptado
            });
        }

        [HttpPost("registrar", Name = "crearUsuarioV1")] //api/cuentas/registrar

        //Para crear el token
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Registrar(CredencialesUsuariosDTO credencialesUsuarioDTO)
        {
            var usuario = new IdentityUser { UserName = credencialesUsuarioDTO.Email, Email = credencialesUsuarioDTO.Email };

            var resultado = await userManager.CreateAsync(usuario, credencialesUsuarioDTO.Password);

            if (resultado.Succeeded)
            {
                return await CrearToken(credencialesUsuarioDTO);
            }
            else
            {
                return BadRequest(resultado.Errors);
            }
        }

        [HttpPost("login", Name = "loginV1")]
        //isPersistent: si usamos cookie de autenticacion
        //lockoutOnFailure: si es usuario se tiene que bloquear si los intentos no son satisfactorio  
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Login(CredencialesUsuariosDTO credencialesUsuariosDTO)
        {
            var resultado = await signInManager.PasswordSignInAsync(credencialesUsuariosDTO.Email, credencialesUsuariosDTO.Password, isPersistent: false, lockoutOnFailure: false);

            if (resultado.Succeeded)
            {
                return await CrearToken(credencialesUsuariosDTO);
            }
            else
            {
                return BadRequest("Login incorrecto");
            }

        }

        //construye un nuevo token
        [HttpGet("RenovarToken", Name = "renovarTokenV1")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Renovar()
        {
            var emailClaim = HttpContext.User.Claims.Where(claim => claim.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;
            var credencialesUsuario = new CredencialesUsuariosDTO()
            {
                Email = email
            };

            return await CrearToken(credencialesUsuario);
        }


        private async Task<RespuestaAutenticacionDTO> CrearToken(CredencialesUsuariosDTO credencialesUsuariosDTO)
        {
            //es una coleccion de llave valor, donde guardamos informacion tanto para nosotros como para el usuarios
            //no se guarda informacion sensibel 
            var claims = new List<Claim>()
            {
                new Claim("email", credencialesUsuariosDTO.Email)
            };

            //Para traerme todos los claims del usuaurio que estan en base
            var usuario = await userManager.FindByEmailAsync(credencialesUsuariosDTO.Email);
            var claimsDB = await userManager.GetClaimsAsync(usuario);

            claims.AddRange(claimsDB);

            //armamos el token
            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"]));
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            //Expiracion del token
            var expiracion = DateTime.UtcNow.AddYears(1);

            //Token armado
            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiracion, signingCredentials: creds);

            return new RespuestaAutenticacionDTO()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Experacion = expiracion
            };
        }

        [HttpPost("HacerAdmin", Name = "hacerAdminV1")]
        //asi agregamos el Claim de esAdmin a un usuario
        public async Task<ActionResult> HacerAdmin(EditarAdminDTO editarAdminDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);
            await userManager.AddClaimAsync(usuario, new Claim("esAdmin", "1"));
            return NoContent();
        }

        [HttpPost("RemoverAdmin", Name = "eliminarAdminV1")]
        //asi sacamos el Claim de esAdmin a un usuario
        public async Task<ActionResult> RemoverAdmin(EditarAdminDTO editarAdminDTO)
        {
            var usuario = await userManager.FindByEmailAsync(editarAdminDTO.Email);
            await userManager.RemoveClaimAsync(usuario, new Claim("esAdmin", "1"));
            return NoContent();
        }
    }
}
