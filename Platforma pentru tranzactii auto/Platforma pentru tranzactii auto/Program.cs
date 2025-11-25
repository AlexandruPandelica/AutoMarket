using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Platforma_pentru_tranzactii_auto.Models;  
using Platforma_pentru_tranzactii_auto.Areas.Identity.Data;
//using Platforma_pentru_tranzactii_auto.Areas.Identity.Data;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// === 1. DB CONTEXT (PRIMUL!) ===
builder.Services.AddDbContext<PlatformaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PlatformaDb")));

// === 2. IDENTITY + ROLES ===
builder.Services.AddDefaultIdentity<Utilizator>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<PlatformaDbContext>();

builder.Services.AddTransient<IEmailSender, EmailSender>();

// === 3. RAZOR + MVC ===
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Creăm rolurile la pornire
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

    string[] roles = { "Client", "Administrator" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole<int>(role));
    }
}

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
app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();