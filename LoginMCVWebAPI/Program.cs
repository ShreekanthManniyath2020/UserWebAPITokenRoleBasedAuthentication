using LoginMCVWebAPI.Middleware;
using LoginMCVWebAPI.Services;
using LoginMCVWebAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// Configure API client
builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddScoped<IProductApiClient, ProductApiClient>();

// Configure HttpClient for Product API
builder.Services.AddHttpClient<IProductApiClient, ProductApiClient>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    client.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"]!);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Authentication
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
        options.SlidingExpiration = true;
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication(); 
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}") // "{controller=Home}/{action=Index}/{id?}"
    .WithStaticAssets();

// Use custom extensions
app.UseTokenRefresh();

app.Run();
