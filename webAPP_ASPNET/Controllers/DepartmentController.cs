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
    public class DepartmentController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DepartmentController(IHttpClientFactory httpClientFactory)
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

            string url = "Department";
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
            string url = $"Department/{id}";
            HttpResponseMessage getData = await client.GetAsync(url);

            if (getData.IsSuccessStatusCode)
            {
                var jsonResponse = await getData.Content.ReadAsStringAsync();
                var department = JsonConvert.DeserializeObject<Department>(jsonResponse);

                return View(department);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while fetching user data.");
            }
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Put(Department departmentModel)
        {
            var client = CreateHttpClient();
            var jsonContent = new StringContent(JsonConvert.SerializeObject(departmentModel), System.Text.Encoding.UTF8, "application/json");
            string url = $"Department/{departmentModel.ID}";
            HttpResponseMessage putDataDepartment = await client.PutAsync(url, jsonContent);

            if (putDataDepartment.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(departmentModel);
        }

        [HttpPost]
        public async Task<ActionResult> Post(Department departmentModel)
        {
            var client = CreateHttpClient();
            var jsonContentUser = new StringContent(JsonConvert.SerializeObject(departmentModel), System.Text.Encoding.UTF8, "application/json");
            string url = $"Department";
            HttpResponseMessage postDataDepartment = await client.PostAsync(url, jsonContentUser);

            if (postDataDepartment.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(departmentModel);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            var client = CreateHttpClient();
            string url = $"Department/{id}";
            HttpResponseMessage deleteData = await client.DeleteAsync(url);

            if (deleteData.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
    }
}
