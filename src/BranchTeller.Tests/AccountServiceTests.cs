using BranchTeller.Core.Models;
using BranchTeller.Core.Services;
using BranchTeller.Tests.Helpers;

namespace BranchTeller.Tests.Services;

public class AccountServiceTests
{
    private async Task<(BranchTeller.Core.Data.BranchTellerContext db, AccountService service)> SetupAsync()
    {
        var db = TestDbContextFactory.Create();
        var service = new AccountService(db);

        var customer = new Customer { FullName = "John Smith", Email = "john@example.com" };
        db.Customers.Add(customer);

        var account = new Account
        {
            AccountNumber = "ACC001",
            Balance = 1000.00m,
            Customer = customer
        };
        db.Accounts.Add(account);
        await db.SaveChangesAsync();

        return (db, service);
    }

    [Fact]
    public async Task Deposit_IncreasesBalance()
    {
        var (_, service) = await SetupAsync();

        await service.DepositAsync("ACC001", 500.00m);

        var account = await service.GetAccountAsync("ACC001");
        Assert.Equal(1500.00m, account!.Balance);
    }

    [Fact]
    public async Task Withdraw_DecreasesBalance()
    {
        var (_, service) = await SetupAsync();

        await service.WithdrawAsync("ACC001", 300.00m);

        var account = await service.GetAccountAsync("ACC001");
        Assert.Equal(700.00m, account!.Balance);
    }

    [Fact]
    public async Task Withdraw_InsufficientFunds_ThrowsException()
    {
        var (_, service) = await SetupAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.WithdrawAsync("ACC001", 9999.00m)
        );
    }

    [Fact]
    public async Task Deposit_CreatesTransactionRecord()
    {
        var (_, service) = await SetupAsync();

        await service.DepositAsync("ACC001", 200.00m);

        var account = await service.GetAccountAsync("ACC001");
        Assert.Single(account!.Transactions);
        Assert.Equal(TransactionType.Deposit, account.Transactions.First().Type);
        Assert.Equal(200.00m, account.Transactions.First().Amount);
    }

    [Fact]
    public async Task Withdraw_CreatesTransactionRecord()
    {
        var (_, service) = await SetupAsync();

        await service.WithdrawAsync("ACC001", 100.00m);

        var account = await service.GetAccountAsync("ACC001");
        Assert.Single(account!.Transactions);
        Assert.Equal(TransactionType.Withdrawal, account.Transactions.First().Type);
        Assert.Equal(100.00m, account.Transactions.First().Amount);
    }

    [Fact]
    public async Task GetAccount_InvalidNumber_ReturnsNull()
    {
        var (_, service) = await SetupAsync();

        var account = await service.GetAccountAsync("INVALID");

        Assert.Null(account);
    }

    [Fact]
    public async Task Deposit_InvalidAccount_ThrowsException()
    {
        var (_, service) = await SetupAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.DepositAsync("INVALID", 100.00m)
        );
    }
}