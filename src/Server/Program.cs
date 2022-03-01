using FootyTipping.Server.Authorization;
using FootyTipping.Server.Data;
using FootyTipping.Server.Helpers;
using FootyTipping.Server.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configure DbContext(s) for EF core using SQL Server
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Configure AutoMapper with all profiles from this assembly
builder.Services.AddAutoMapper(typeof(Program));

// Configure strongly typed appsettings object
builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection("SecuritySettings"));

// Configure services for DI container
builder.Services.AddScoped<IHashingUtilities, HashingUtilities>();
builder.Services.AddScoped<IJwtUtilities, JwtUtilities>();
builder.Services.AddScoped<IUserService, UserService>();


var app = builder.Build();

// Migrate any database changes on startup (including initial db creation)
using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    dataContext.Database.Migrate();
}

// Use custom middleware
app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseMiddleware<JwtMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();