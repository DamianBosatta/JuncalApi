using AutoMapper;
using JuncalApi.Dto.DtoRequerido;
using JuncalApi.Dto.DtoRespuesta;
using JuncalApi.Modelos;
using JuncalApi.Seguridad;
using JuncalApi.UnidadDeTrabajo;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace JuncalApi.Servicios
{
    public class ServicioUsuario : IServicioUsuario
    {
        private readonly IUnidadDeTrabajo _uow;
        public IConfiguration _configuration;
        private readonly IMapper _mapper;
        public static JuncalUsuario user = new JuncalUsuario();

        public ServicioUsuario(IConfiguration configuration,IUnidadDeTrabajo uow,IMapper mapper)
        {
            _configuration = configuration;
            _uow = uow;
            _mapper = mapper;
        }

        #region  codigo anterior
        //public dynamic InicioSesion(LoginRequerido userReq)
        //{

        //   var data = JsonConvert.DeserializeObject<dynamic>(userReq.ToString());

        //    string usuario= data.Usuario.ToString();
        //    string contraseña = data.Contraseña.ToString();

        //    JuncalUsuario usuarioLoger = _uow.RepositorioJuncalUsuario.GetAll().Where(u => u.Usuario == usuario && u.Contraseña == contraseña).FirstOrDefault();

        //    if (usuarioLoger == null)
        //    {
        //        return new
        //        {
        //            success = false,
        //            message = "Credenciales Incorrectas",
        //            result = ""

        //        };

        //    }

        //    var jwt = _configuration.GetSection("Jwt").Get<Jwt>();

        //    var claims = new[]
        //    {
        //         new Claim(JwtRegisteredClaimNames.Sub,jwt.Subject),
        //         new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
        //          new Claim(JwtRegisteredClaimNames.Iat,DateTime.UtcNow.ToString()),
        //          new Claim("id",usuarioLoger.Id.ToString()),
        //          new Claim("nombre", usuarioLoger.Nombre)
        //    };


        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));
        //    var InicioSesion = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //    var token = new JwtSecurityToken(
        //        jwt.Issuer,
        //        jwt.Audience,
        //        claims,
        //        expires:DateTime.Now.AddMinutes(60),
        //        signingCredentials:InicioSesion
        //        );

        //    return new 
        //    {
        //        success = true,
        //        message = "Inicio Sesion Exitoso",
        //        result = new JwtSecurityTokenHandler().WriteToken(token)
        //    };
        //}

        //public JuncalUsuario RegistroUsuario(UsuarioRequerido userReq)
        //{
        //    var usuario = _uow.RepositorioJuncalUsuario.GetByCondition(c => c.Dni == userReq.Dni);

        //    if(usuario is null)
        //    {
        //        JuncalUsuario usuarioNuevo = _mapper.Map<JuncalUsuario>(userReq);

        //        _uow.RepositorioJuncalUsuario.Insert(usuarioNuevo);

        //        return usuarioNuevo;

        //    }

        //    return null;

        //}
        #endregion

        public JuncalUsuario RegistroUsuario(UsuarioRequerido userReq)
        {
            CreatePasswordhHash(userReq.Contraseña, out byte[] passwordHash, out byte[] passwordSalt);
            user.Usuario=userReq.Usuario;
            user.Dni = userReq.Dni;
            user.Nombre=userReq.Nombre;
            user.Apellido = userReq.Apellido;
            user.IdRol=userReq.IdRol;
            user.Email = userReq.Email;
            //user.PasswordHash = BitConverter.ToUInt64(passwordHash, 0);       
            //user.PasswordSalt = BitConverter.ToUInt64(passwordSalt, 0);
            
            return user;

        }


        public string InicioSesion(LoginRequerido userReq)
        {
            var usuario = _uow.RepositorioJuncalUsuario.GetByCondition(u => u.Usuario == userReq.Usuario && u.Isdeleted==false);

            if (usuario.Usuario != userReq.Usuario)
            {
                return " El Usuario No Existe";
            }

            //if (!VerificarPassworHash(userReq.Contraseña, BitConverter.GetBytes(usuario.PasswordHash), BitConverter.GetBytes(usuario.PasswordSalt)))
            //{
            //    return "Password Incorrecto";

            //}

            string token = CreateToken(usuario);

            return token;



        }

        private string CreateToken(JuncalUsuario user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name,user.Usuario),
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings: Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials : creds

                ) ;
            var jwt= new JwtSecurityTokenHandler().WriteToken(token);


            return jwt;
        }

        private void CreatePasswordhHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new HMACSHA512())
            {

                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            
            
            }




        }

        private bool VerificarPassworHash(string password , byte[] passwordHash, byte[] passwordSalt)
        {


            using (var hmac = new HMACSHA512(passwordSalt))
            {

                var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computeHash.SequenceEqual(passwordHash); 

            }



        }

    }
}
