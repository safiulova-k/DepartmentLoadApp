using System.Globalization;
using DepartmentLoadApp.Data;
using DepartmentLoadApp.Integration.CoreApi;
using DepartmentLoadApp.Integration.CoreSync;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<DepartmentLoadDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<CoreApiService>(client =>
{
    client.BaseAddress = new Uri("http://core-api:8080/api/");
});

builder.Services.AddScoped<EducationDirectionSyncService>();
builder.Services.AddScoped<LecturerStudyPostSyncService>();
builder.Services.AddScoped<LecturerDepartmentPostSyncService>();
builder.Services.AddScoped<LecturerSyncService>();
builder.Services.AddScoped<StudentGroupSyncService>();
builder.Services.AddScoped<AcademicPlanSyncService>();
builder.Services.AddScoped<AcademicPlanRecordSyncService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

var culture = new CultureInfo("ru-RU");
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = new[] { culture },
    SupportedUICultures = new[] { culture }
};

app.UseRequestLocalization(localizationOptions);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();