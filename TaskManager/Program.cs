using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Text.Json.Serialization;
using TaskManager;
using TaskManager.Mappings;
using TaskManager.Messages.Constants;
using TaskManager.Services;

var builder = WebApplication.CreateBuilder(args);
var authenticatedUsersPolicy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build(); 

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter(authenticatedUsersPolicy));
}).AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
.AddDataAnnotationsLocalization(options =>
{
    options.DataAnnotationLocalizerProvider = (_, factoria) => 
    factoria.Create(typeof(SharedResource));
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Add db context
builder.Services.AddDbContext<ApplicationDbContext>(options => 
options.UseSqlServer("name=DefaultConnection"));

builder.Services.AddAuthentication().AddMicrosoftAccount(options =>
{
    options.ClientId = builder.Configuration["MicrosoftClientId"]!;
    options.ClientSecret = builder.Configuration["MicrosoftSecretId"]!;
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;

}).AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
{
    options.LoginPath = "/User/Login";
    options.AccessDeniedPath = "/User/Login";
});

builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddTransient<IFileStorage, FileStorageAzure>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (dbContext.Database.IsRelational())
    {
        dbContext.Database.Migrate();
    }
}

using (var scope = app.Services.CreateScope())
{
    var localizer = scope.ServiceProvider
                            .GetRequiredService<IStringLocalizer<SupportedCultureConstants>>();

    var supportedCultures = SupportedCultureConstants
                            .GetSupportedUICulture(localizer)
                            .Select(c => new CultureInfo(c.Value))
                            .ToList();

    app.UseRequestLocalization(options =>
    {
        options.DefaultRequestCulture = new RequestCulture("es");
        options.SupportedCultures = supportedCultures;
        options.SupportedUICultures = supportedCultures;
    });
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
