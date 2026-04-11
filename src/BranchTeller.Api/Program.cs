using BranchTeller.Core.Data;
using BranchTeller.Core.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var dbPath = Environment.GetEnvironmentVariable("DB_PATH") ?? "BranchTellerDb.sqlite";

builder.Services.AddDbContext<BranchTellerContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<AccountService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BranchTellerContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/account/{number}", async (string number, AccountService svc) =>
{
    var acc = await svc.GetAccountAsync(number);
    return acc is null ? Results.NotFound() : Results.Ok(acc);
});

app.MapPost("/account/{number}/deposit", async (string number, DepositRequest req, AccountService svc) =>
{
    await svc.DepositAsync(number, req.Amount);
    return Results.Ok(new { message = "Deposit successful" });
});

app.MapPost("/account/{number}/withdraw", async (string number, WithdrawRequest req, AccountService svc) =>
{
    await svc.WithdrawAsync(number, req.Amount);
    return Results.Ok(new { message = "Withdrawal successful" });
});
app.MapPost("/seed", async (BranchTellerContext db) =>
{
    if (!db.Accounts.Any())
    {
        var customer = new BranchTeller.Core.Models.Customer
        {
            FullName = "John Smith",
            Email = "john@example.com"
        };
        db.Customers.Add(customer);
        var account = new BranchTeller.Core.Models.Account
        {
            AccountNumber = "ACC001",
            Balance = 1000.00m,
            Customer = customer
        };
        db.Accounts.Add(account);
        await db.SaveChangesAsync();
        return Results.Ok(new { message = "Seeded ACC001 with $1000.00" });
    }
    return Results.Ok(new { message = "Already seeded" });
});
app.Run();

record DepositRequest(decimal Amount);
record WithdrawRequest(decimal Amount);