using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using FabricMQ.Broker.Ciphers;
using FabricMQ.Broker.Identity.Models;
using FabricMQ.Broker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace FabricMQ.Broker.Controllers
{
    [Route("api/[controller]/[action]")]
    public class TokenAuthController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenAuthController> _logger;

        public TokenAuthController(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IPasswordHasher<User> pwdHasher,
            IConfiguration config,
            ILogger<TokenAuthController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _passwordHasher = pwdHasher;
            _configuration = config;
            _logger = logger;
        }


        [HttpPost]
        [ValidateModel]
        public async Task<ResponseMessage<AuthenticateResultModel>> Authenticate([FromBody] AuthenticateModel model)
        {
            var response = new ResponseMessage<AuthenticateResultModel>();

            try
            {
                var user = await _userManager.FindByNameAsync(model.UserName);

                if (user != null)
                {
                    if (_passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Success)
                    {
                        var userClaims = await _userManager.GetClaimsAsync(user);

                        var claims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(JwtRegisteredClaimNames.GivenName, user.UserName),
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        }.Union(userClaims).ToList();

                        var roles = await _userManager.GetRolesAsync(user);

                        // adds role name to token, so you can use  [Authorize(Roles = "Admin")] attribute
                        claims.AddRange(roles.Select(role => new Claim(ClaimsIdentity.DefaultRoleClaimType, role)));

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var now = DateTime.UtcNow;
                        // put this in configuration
                        var tokenDuration = TimeSpan.FromMinutes(15);

                        var token = new JwtSecurityToken(
                            issuer: _configuration["Tokens:Site"],
                            audience: _configuration["Tokens:Site"],
                            claims: claims,
                            notBefore: now,
                            expires: now.Add(tokenDuration),
                            signingCredentials: creds);

                        response.Result = new AuthenticateResultModel();
                        response.Result.UserId = user.Id;
                        response.Result.AccessToken = new JwtSecurityTokenHandler().WriteToken(token);
                        response.Result.EncryptedAccessToken = StringCipher.Instance.Encrypt(response.Result.AccessToken);
                        response.Result.ExpireIn = token.ValidTo;
                        response.Result.ExpireInSeconds = (int)tokenDuration.TotalSeconds;
                        response.Success = true;

                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                response.Error = ex.Message;
            }

            return response;
        }
    }
}