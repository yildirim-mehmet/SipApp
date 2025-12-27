using Microsoft.AspNetCore.Mvc;
using Resta.MVC.Services;

public class AuthController : Controller
{
    private readonly ApiClient _api;
    public AuthController(ApiClient api) { _api = api; }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string kullaniciAdi, string sifre)
    {
        var user = await _api.PostAsync<UserVm>("Auth/login", new
        {
            kullaniciAdi,
            sifre
        });

        if (user == null)
            return View();

        HttpContext.Session.SetInt32("KULLANICI_ID", user.Id);
        return RedirectToAction("Index", "Ekran");
    }

    //create
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        string ad,
        string kullaniciAdi,
        string sifre,
        int rolId)
    {
        await _api.PostAsync<object>("Kullanici", new
        {
            ad,
            kullaniciAdi,
            sifre,
            rolId
        });

        return RedirectToAction("Index", "Ekran");
    }

}
