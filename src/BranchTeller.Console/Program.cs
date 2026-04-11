using BranchTeller.Core.Data;
using BranchTeller.Core.Services;
using Microsoft.EntityFrameworkCore;

var optionsBuilder = new DbContextOptionsBuilder<BranchTellerContext>();
optionsBuilder.UseSqlite("Data Source=BranchTellerDb.sqlite");

await using var db = new BranchTellerContext(optionsBuilder.Options);
await db.Database.EnsureCreatedAsync();

await db.Database.EnsureCreatedAsync();

// Seed a test customer and account if none exist
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
    Console.WriteLine("Test account ACC001 seeded with $1000.00 balance.");
}

var service = new AccountService(db);

Console.WriteLine("=== Branch Teller ===");
Console.Write("Account number: ");
var acct = Console.ReadLine()!;

while (true)
{
    Console.WriteLine("\n1) Deposit  2) Withdraw  3) Balance  4) Receipt  5) Exit");
    var choice = Console.ReadLine();
    if (choice == "5") break;

    if (choice is "1" or "2")
    {
        Console.Write("Amount: ");
        var amount = decimal.Parse(Console.ReadLine()!);
        if (choice == "1") await service.DepositAsync(acct, amount);
        else await service.WithdrawAsync(acct, amount);
        Console.WriteLine("Done.");
    }
    else if (choice == "3")
    {
        var account = await service.GetAccountAsync(acct);
        Console.WriteLine($"Balance: ${account?.Balance:F2}");
    }
    else if (choice == "4")
    {
        var account = await service.GetAccountAsync(acct);
        Console.WriteLine("---- RECEIPT ----");
        foreach (var t in account?.Transactions ?? [])
            Console.WriteLine($"{t.Timestamp:g}  {t.Type,-12}  ${t.Amount:F2}");
    }
}