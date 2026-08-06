using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TruckTaxPro.Data;
using Trucktaxpro.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<TruckTaxProDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity — manages users, roles, password hashing, external logins
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<TruckTaxProDbContext>()
.AddDefaultTokenProviders();

// Cookie + Google external login
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/api/account/login";
});

builder.Services.AddHttpClient<IAppEmailSender, ResendEmailSender>();

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        options.SignInScheme = IdentityConstants.ExternalScheme;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.Use(async (context, next) =>
{
    var host = context.Request.Host.Host;
    var isBlogHost = host.Equals("blog.trucktaxpro.com", StringComparison.OrdinalIgnoreCase);
    var isLocalBlogTest = host.Equals("localhost", StringComparison.OrdinalIgnoreCase) && context.Request.Query.ContainsKey("blogtest");

    if (isBlogHost || isLocalBlogTest)
    {
        var path = context.Request.Path.Value ?? "/";
        if (path == "/" || path == "")
        {
            context.Request.Path = "/Blog";
        }
        else
        {
            context.Request.Path = "/__blogpost/" + path.TrimStart('/');
        }
    }

    await next();
});

app.UseRouting();

app.UseAuthentication();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{action=Login}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllers();

app.Run();

app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{action=Login}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllers();

app.Run();