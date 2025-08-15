//using Microsoft.AspNetCore.Mvc;
//using SISTEMA_FASTSURVEY.MODEL.Models;

//[Route("api/[controller]")]
//[ApiController]
//public class TokenController : ControllerBase
//{
//    private readonly ServicesToken _servicesToken;

//    public TokenController(ServicesToken servicesToken)
//    {
//        _servicesToken = servicesToken;
//    }

//    // Gerar o token (login)
//    [HttpPost("login")]
//    public IActionResult Login([FromBody] login loginModel)
//    {
//        // Aqui você pode validar o login do usuário, como verificar o usuário e a senha
//        if (IsValidUser(loginModel.usuario, loginModel.senha))
//        {
//            var token = _servicesToken.GenerateToken(loginModel.usuario);
//            return Ok(new { Token = token });
//        }

//        return Unauthorized("Usuário ou senha inválidos.");
//    }

//    // Validar o token
//    [HttpPost("validate")]
//    public IActionResult ValidateToken([FromBody] string token)
//    {
//        var principal = _servicesToken.ValidateToken(token);
//        if (principal == null)
//        {
//            return Unauthorized("Token inválido.");
//        }

//        return Ok(new { Message = "Token válido." });
//    }

//    private bool IsValidUser(string username, string password)
//    {
//        // Implemente a lógica para validar o usuário (pode ser um banco de dados ou outro método)
//        return username == "admin" && password == "senha123"; // Exemplo fixo
//    }
//}
