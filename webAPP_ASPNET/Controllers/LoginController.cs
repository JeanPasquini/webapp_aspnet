using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.MSIdentity.Shared;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using webAPP_ASPNET.Models;

namespace webAPP_ASPNET.Controllers
{
    public class LoginController : Controller
    {
        private readonly HttpClient _httpClient;

        public LoginController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var loginModel = new { Username = username, Password = password };
            var jsonContent = new StringContent(JsonConvert.SerializeObject(loginModel), Encoding.UTF8, "application/json");
            HttpContext.Session.SetString("BaseSelecionada", Data.AppContext.ApiBaseURL);

            string baseUrl = HttpContext.Session.GetString("BaseSelecionada");
            if (string.IsNullOrEmpty(baseUrl))
            {
                ModelState.AddModelError(string.Empty, "A URL base não foi configurada.");
                return View("Index");
            }

            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await _httpClient.PostAsync("User/login", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                var tokenObject = JsonConvert.DeserializeObject<dynamic>(responseContent);
                string token = tokenObject?.token;

                if (!string.IsNullOrEmpty(token))
                {
                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTimeOffset.Now.AddMinutes(30),
                        HttpOnly = true,
                        Secure = true
                    };

                    Response.Cookies.Append("AuthCookie", token, cookieOptions);
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Request.Cookies["AuthCookie"]);
                    HttpContext.Session.SetString("Token", token);

                    HttpResponseMessage response2 = await _httpClient.GetAsync($"User/Information/{username}/{password}");
                    if (response2.IsSuccessStatusCode)
                    {
                        var responseContent2 = await response2.Content.ReadAsStringAsync();
                        var user = JsonConvert.DeserializeObject<UserWithDepartment>(responseContent2);
                        LoggedUser.User.ID = user.User.ID;
                        LoggedUser.User.FULLNAME = user.User.FULLNAME;
                        LoggedUser.User.USERNAME = user.User.USERNAME;
                        LoggedUser.User.EMAIL = user.User.EMAIL;
                        LoggedUser.Department.ID = user.Department.ID;
                        LoggedUser.Department.DEPARTMENTNAME = user.Department.DEPARTMENTNAME;
                    }

                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "Nome de usuário ou senha inválidos.");
            return View("Index");
        }


        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthCookie");
            return RedirectToAction("Index", "Login");
        }
    }
}
