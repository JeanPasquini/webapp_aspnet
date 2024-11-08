using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using webAPP_ASPNET.Models;

public class ButtonAuthorize : ActionFilterAttribute
{
    private readonly IHttpClientFactory _httpClientFactory;
    public int Permission { get; set; }

    public ButtonAuthorize(IHttpClientFactory httpClientFactory, int permission)
    {
        _httpClientFactory = httpClientFactory;
        Permission = permission;
    }

    private HttpClient CreateHttpClient(ActionExecutingContext context)
    {
        var client = _httpClientFactory.CreateClient("CustomClient");
        var baseAddress = context.HttpContext.Session.GetString("BaseSelecionada");
        var authToken = context.HttpContext.Request.Cookies["AuthCookie"];

        if (!string.IsNullOrEmpty(baseAddress))
            client.BaseAddress = new Uri(baseAddress);

        if (!string.IsNullOrEmpty(authToken))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

        return client;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (LoggedUser.Department.ID != 1)
        {
            if (!await PermissionCheck(context))
            {
                context.HttpContext.Session.SetString("ErrorMessage", "You do not have permission to access this button.");
                context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }
        }
        await next();
    }

    public async Task<bool> PermissionCheck(ActionExecutingContext context)
    {
        var client = CreateHttpClient(context);
        string url = $"ButtonRelation/{LoggedUser.User.ID}/{Permission}";
        HttpResponseMessage getData = await client.GetAsync(url);

        if (getData.IsSuccessStatusCode)
        {
            string results = await getData.Content.ReadAsStringAsync();
            return !string.IsNullOrEmpty(results);
        }
        else
        {
            Console.WriteLine("Erro API");
            return false;
        }
    }
}
