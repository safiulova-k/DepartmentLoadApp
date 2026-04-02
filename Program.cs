using DepartmentLoadApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using DepartmentLoadApp.Integration.PortalMock;
using DepartmentLoadApp.Integration.PracticeMock;
using DepartmentLoadApp.Integration.GiaMock;
using DepartmentLoadApp.Integration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<DepartmentLoadDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAcademicPlanImportService, JsonAcademicPlanImportService>();
builder.Services.AddScoped<IPracticeWorkloadImportService, JsonPracticeWorkloadImportService>();
builder.Services.AddScoped<IGiaWorkloadImportService, JsonGiaWorkloadImportService>();
builder.Services.AddScoped<ILecturerImportService, JsonLecturerImportService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var culture = new CultureInfo("ru-RU");

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = new[] { culture },
    SupportedUICultures = new[] { culture }
};

app.UseRequestLocalization(localizationOptions);

app.Run();