using System.Globalization;
using DepartmentLoadApp.Data;
using DepartmentLoadApp.Integration.CoreApi;
using DepartmentLoadApp.Integration.CoreSync;
using DepartmentLoadApp.Integration.CoreSync.Interfaces;
using DepartmentLoadApp.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<DepartmentLoadDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<CalculationImportService>();
builder.Services.AddScoped<WorkloadDistributionService>();
builder.Services.AddScoped<IndividualPlanService>();

builder.Services.AddHttpClient<CoreApiService>(client =>
{
    client.BaseAddress = new Uri("http://core-api:8080/api/");
});

builder.Services.AddScoped<IEducationDirectionSyncService, EducationDirectionSyncService>();
builder.Services.AddScoped<ILecturerStudyPostSyncService, LecturerStudyPostSyncService>();
builder.Services.AddScoped<ILecturerDepartmentPostSyncService, LecturerDepartmentPostSyncService>();
builder.Services.AddScoped<ILecturerSyncService, LecturerSyncService>();
builder.Services.AddScoped<IStudentGroupSyncService, StudentGroupSyncService>();
builder.Services.AddScoped<IAcademicPlanSyncService, AcademicPlanSyncService>();
builder.Services.AddScoped<IAcademicPlanRecordSyncService, AcademicPlanRecordSyncService>();


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