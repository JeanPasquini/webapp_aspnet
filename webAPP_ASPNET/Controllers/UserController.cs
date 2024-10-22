using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using NuGet.Common;
using System.Data;
using System.Diagnostics;
using System.Net.Http.Headers;
using webAPP_ASPNET.Models;
using YourNamespace.Filters;

namespace webAPP_ASPNET.Controllers
{
    [TokenAuthorize]
    [DepartmentSecurityFilter]
    public class UserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public UserController(IHttpClientFactory httpClientFactory)
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
                dt.Columns.Add("PASSWORD", typeof(string));
                dt.Columns.Add("FULLNAME", typeof(string));
                dt.Columns.Add("EMAIL", typeof(string));
                dt.Columns.Add("DEPARTMENTNAME", typeof(string));

                foreach (var user in users)
                {
                    var id = user.user.id;
                    var username = user.user.username;
                    var password = user.user.password;
                    var fullname = user.user.fullname;
                    var email = user.user.email;
                    var department = user.departmentRelation != null ? user.department.departmentname : "None";

                    dt.Rows.Add(id, username, password, fullname, email, department);
                }
            }
            else
            {
                Console.WriteLine("Erro API");
            }
            ViewData.Model = dt;

            return View();
        }

        public async Task ListDepartment()
        {
            var client = CreateHttpClient();
            string url = "Department";
            HttpResponseMessage getData = await client.GetAsync(url);

            if (getData.IsSuccessStatusCode)
            {
                var result = await getData.Content.ReadAsStringAsync();
                var departments = JsonConvert.DeserializeObject<List<Department>>(result);

                ViewBag.Departments = new SelectList(departments, "ID", "DEPARTMENTNAME");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Não foi possível carregar os departamentos.");
            }
        }

        public async Task<IActionResult> Create()
        {
            await ListDepartment();
            return View(); 
        }

        public async Task<ActionResult> Edit(int id)
        {
            var client = CreateHttpClient();
            string url = $"User/Department/{id}";
            HttpResponseMessage getData = await client.GetAsync(url);

            if (getData.IsSuccessStatusCode)
            {
                var jsonResponse = await getData.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<UserWithDepartment>(jsonResponse);

                await ListDepartment();

                ViewBag.RelationId = user.DepartmentRelation != null ? user.DepartmentRelation?.ID : null;
                ViewBag.SelectedDepartmentId = user.Department != null ? user.Department?.ID : null;
                ViewBag.SelectedDepartmentName = user.Department != null ? user.Department?.DEPARTMENTNAME : null;

                return View(user);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while fetching user data.");
            }
            return View();
        }




        [HttpPost]
        public async Task<ActionResult> Put(UserWithDepartment userModel)
        {
            var client = CreateHttpClient();
            var jsonContent = new StringContent(JsonConvert.SerializeObject(userModel.User), System.Text.Encoding.UTF8, "application/json");
            string url = $"User/{userModel.User.ID}";
            string url2 = $"User/Department/{userModel.User.ID}";
            HttpResponseMessage putDataUser = await client.PutAsync(url, jsonContent);
            HttpResponseMessage getData = await client.GetAsync(url2);

            if (putDataUser.IsSuccessStatusCode && getData.IsSuccessStatusCode)
            {
                var jsonResponse = await getData.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<UserWithDepartment>(jsonResponse);

                if (user.Department != null)
                {
                    DepartmentRelation departmentRelation = new DepartmentRelation
                    {
                        IDDEPARTMENT = userModel.DepartmentRelation.IDDEPARTMENT,
                        IDUSER = userModel.User.ID
                    };
                    var jsonContentDepartment = new StringContent(JsonConvert.SerializeObject(departmentRelation), System.Text.Encoding.UTF8, "application/json");

                    string urlDepartment = $"DepartmentRelation/{userModel.DepartmentRelation.ID}";
                    HttpResponseMessage putDataDepartment = await client.PutAsync(urlDepartment, jsonContentDepartment);

                    if (putDataDepartment.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    DepartmentRelation departmentRelation = new DepartmentRelation
                    {
                        IDDEPARTMENT = userModel.DepartmentRelation.IDDEPARTMENT,
                        IDUSER = userModel.User.ID
                    };
                    var jsonContentDepartment = new StringContent(JsonConvert.SerializeObject(departmentRelation), System.Text.Encoding.UTF8, "application/json");

                    string urlDepartment = $"DepartmentRelation";
                    HttpResponseMessage postDataDepartment = await client.PostAsync(urlDepartment, jsonContentDepartment);

                    if (postDataDepartment.IsSuccessStatusCode)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
            }
            return View(userModel);
        }

        [HttpPost]
        public async Task<ActionResult> Post(UserWithDepartment userModel)
        {
            var client = CreateHttpClient();
            var jsonContentUser = new StringContent(JsonConvert.SerializeObject(userModel.User), System.Text.Encoding.UTF8, "application/json");
            string url = $"User";
            HttpResponseMessage postDataUser = await client.PostAsync(url, jsonContentUser);

            if (postDataUser.IsSuccessStatusCode)
            {
                var jsonResponse = await postDataUser.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(jsonResponse);
                DepartmentRelation departmentRelation = new DepartmentRelation
                {
                    IDDEPARTMENT = userModel.Department.ID,
                    IDUSER = user.ID
                };
                var jsonContentDepartment = new StringContent(JsonConvert.SerializeObject(departmentRelation), System.Text.Encoding.UTF8, "application/json");

                string urlDepartment = $"DepartmentRelation";
                HttpResponseMessage postDataDepartment = await client.PostAsync(urlDepartment, jsonContentDepartment);

                if (postDataDepartment.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(userModel);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            var client = CreateHttpClient();
            string url = $"User/{id}";
            HttpResponseMessage deleteData = await client.DeleteAsync(url);

            if (deleteData.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
    }
}
