using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.MSIdentity.Shared;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Newtonsoft.Json;
using System.Data;
using System.Net.Http.Headers;
using webAPP_ASPNET.Models;
using YourNamespace.Filters;

namespace webAPP_ASPNET.Controllers
{
    [TokenAuthorize]
    [DepartmentSecurityFilter]
    public class ButtonController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ButtonController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateHttpClient()
        {
            var client = _httpClientFactory.CreateClient("CustomClient");
            client.BaseAddress = new Uri(HttpContext.Session.GetString("BaseSelecionada"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Request.Cookies["AuthCookie"]);
            return client;
        }

        public async Task<ActionResult> Index()
        {
            DataTable? dt = new DataTable();
            var client = CreateHttpClient();

            string url = "Button";
            HttpResponseMessage getData = await client.GetAsync(url);

            if (getData.IsSuccessStatusCode)
            {
                string results = getData.Content.ReadAsStringAsync().Result;
                dt = JsonConvert.DeserializeObject<DataTable>(results);
            }
            else
            {
                Console.WriteLine("Erro API");
            }
            ViewData.Model = dt;

            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        public async Task<ActionResult> Edit(int id)
        {
            var client = CreateHttpClient();

            string url = $"Button/{id}";
            HttpResponseMessage getData = await client.GetAsync(url);

            if (getData.IsSuccessStatusCode)
            {
                var result = getData.Content.ReadAsStringAsync().Result;
                var jsonResults = JsonConvert.DeserializeObject<Button>(result);

                return View(jsonResults);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while fetching user data.");
            }

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Post(Button button)
        {
            var client = CreateHttpClient();
            var jsonContent = new StringContent(JsonConvert.SerializeObject(button), System.Text.Encoding.UTF8, "application/json");

            string url = "Button";
            HttpResponseMessage postData = await client.PostAsync(url, jsonContent);

            if (postData.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));

            }
            return View(button);
        }

        [HttpPost]
        public async Task<ActionResult> Put(Button button)
        {
            var client = CreateHttpClient();
            var jsonContent = new StringContent(JsonConvert.SerializeObject(button), System.Text.Encoding.UTF8, "application/json");

            string url = $"Button/{button.ID}";
            HttpResponseMessage putData = await client.PutAsync(url, jsonContent);

            if (putData.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(button);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            var client = CreateHttpClient();

            string url = $"Button/{id}";
            HttpResponseMessage deleteData = await client.DeleteAsync(url);
            if (deleteData.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

    }

    [TokenAuthorize]
    [DepartmentSecurityFilter]
    public class ButtonRelationController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ButtonRelationController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateHttpClient()
        {
            var client = _httpClientFactory.CreateClient("CustomClient");
            client.BaseAddress = new Uri(HttpContext.Session.GetString("BaseSelecionada"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Request.Cookies["AuthCookie"]);
            return client;
        }

        public async Task<ActionResult> Index()
        {
            DataTable dt = new DataTable();
            var client = CreateHttpClient();

            string url = "User";
            HttpResponseMessage getData = await client.GetAsync(url);

            if (getData.IsSuccessStatusCode)
            {
                string results = await getData.Content.ReadAsStringAsync();

                var users = JsonConvert.DeserializeObject<List<dynamic>>(results);

                dt.Columns.Add("ID", typeof(string));
                dt.Columns.Add("USERNAME", typeof(string));
                dt.Columns.Add("FULLNAME", typeof(string));
                dt.Columns.Add("DEPARTMENTNAME", typeof(string));

                foreach (var user in users)
                {
                    var id = user.user.id;
                    var username = user.user.username;
                    var fullname = user.user.fullname;
                    var department = user.departmentRelation != null ? user.department.departmentname : "None";

                    dt.Rows.Add(id, username, fullname, department);
                }
            }
            else
            {
                Console.WriteLine("Erro API");
            }
            ViewData.Model = dt;

            return View();
        }

        public async Task<ActionResult> ListRelation(int id)
        {
            DataTable dt = new DataTable();
            var client = CreateHttpClient();

            string url = $"ButtonRelation/Permissions/{id}";
            string urlUser = $"User/{id}";
            HttpResponseMessage getData = await client.GetAsync(url);
            HttpResponseMessage getDataUser = await client.GetAsync(urlUser);

            if (getData.IsSuccessStatusCode)
            {
                string results = await getData.Content.ReadAsStringAsync();
                var userWithButtons = JsonConvert.DeserializeObject<List<UserWithButton>>(results);

                string resultsUser= await getDataUser.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(resultsUser);

                ViewBag.IdUser = user.ID;
                ViewBag.Username = user.USERNAME;

                dt.Columns.Add("ID", typeof(string));
                dt.Columns.Add("BUTTONNAME", typeof(string));
                dt.Columns.Add("DESCRIPTION", typeof(string));

                if (userWithButtons != null)
                {
                    foreach (var button in userWithButtons)
                    {
                        var ids = button.ButtonRelation.ID;
                        var buttonname = button.Button.BUTTONNAME;
                        var description = button.Button.DESCRIPTION;

                        dt.Rows.Add(ids, buttonname, description);
                    }
                    
                }

            }
            else
            {
                Console.WriteLine("Erro API");
            }
            
            ViewData.Model = dt;

            return View();
        }

        public async Task<IActionResult> Create(int IdUser)
        {
            await User(IdUser);
            await ListButtons();
            return View();
        }

        public async Task ListButtons()
        {
            var client = CreateHttpClient();
            string url = "Button";
            HttpResponseMessage getData = await client.GetAsync(url);

            if (getData.IsSuccessStatusCode)
            {
                var result = await getData.Content.ReadAsStringAsync();
                var buttons = JsonConvert.DeserializeObject<List<Button>>(result);

                ViewBag.Buttons = new SelectList(buttons, "ID", "BUTTONNAME");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Não foi possível carregar os botões.");
            }
        }

        public async Task User(int IdUser)
        {
            var client = CreateHttpClient();
            string url = $"User/{IdUser}";
            HttpResponseMessage getData = await client.GetAsync(url);

            if (getData.IsSuccessStatusCode)
            {
                var result = await getData.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(result);

                ViewBag.Username = user.USERNAME;
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Não foi possível carregar os botões.");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post(ButtonRelation buttonModel)
        {
            var client = CreateHttpClient();
            var jsonContent = new StringContent(JsonConvert.SerializeObject(buttonModel), System.Text.Encoding.UTF8, "application/json");
            string url = $"ButtonRelation";
            HttpResponseMessage postDataButtonRelation = await client.PostAsync(url, jsonContent);

            if (postDataButtonRelation.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(buttonModel);
        }

        [HttpPost]
        public async Task<ActionResult> Put(Button button)
        {
            var client = CreateHttpClient();
            var jsonContent = new StringContent(JsonConvert.SerializeObject(button), System.Text.Encoding.UTF8, "application/json");

            string url = $"Button/{button.ID}";
            HttpResponseMessage putData = await client.PutAsync(url, jsonContent);

            if (putData.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(button);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            var client = CreateHttpClient();

            string url = $"ButtonRelation/{id}";
            HttpResponseMessage deleteData = await client.DeleteAsync(url);
            if (deleteData.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return View();
        }


    }
}
