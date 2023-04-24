using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        public readonly IConfiguration _configuration;

        public RegistrationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("registration")]
        public string registration(Registration registration)
        {
            SqlConnection con =new SqlConnection(_configuration.GetConnectionString("EmployeeAppCon").ToString());
            SqlCommand cmd = new SqlCommand("Insert into dbo.Registration(UserName, Password, Email, IsActive) Values('" + registration.UserName + "','" + Common.CommonMethod.EncryptAES(registration.Password) + "','" + registration.Email + "','" + registration.IsActive + "')", con);
            con.Open();
            int i = cmd.ExecuteNonQuery();
            con.Close();
            if(i>0)
            {
                return "Data Inserted";
            }
            else
            {
                return "Error";
            }
            return "";
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public JsonResult login(Login login)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("EmployeeAppCon").ToString());
            SqlDataAdapter da = new SqlDataAdapter("Select * from dbo.Registration Where Email = '" + login.Email + "' AND Password = '"+ Common.CommonMethod.EncryptAES(login.Password)+"' And IsActive =1", con);
            DataTable dt = new DataTable();
            da.Fill(dt);
            if(dt.Rows.Count>0)
            {
                var token = GenerateToken();
                var data = new { state = "ok", token = token };
                return new JsonResult(data) ;
            }
            else
            {

                return new JsonResult("Error");
            }
        }
        private string GenerateToken()
        {
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:key"]));
            var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"], _configuration["Jwt:Audience"], 
                null, expires: DateTime.Now.AddMinutes(1), signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
