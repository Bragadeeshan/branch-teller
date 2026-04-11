using BranchTeller.Core.Data;
using BranchTeller.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BranchTeller.Core.Services;

public class AccountService
{
    private readonly BranchTellerContext _db;

    public AccountService(BranchTellerContext db) => _db = db;

    public async Task<Account?> GetAccountAsync(string accountNumber) =>
        await _db.Accounts.Include(a => a.Transactions)
                          .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

    public async Task DepositAsync(string accountNumber, decimal amount)
    {
        var account = await GetAccountAsync(accountNumber)
            ?? throw new InvalidOperationException("Account not found");
        account.Balance += amount;
        _db.Transactions.Add(new Transaction
        {
            AccountId = account.Id, Type = TransactionType.Deposit, Amount = amount
        });
        await _db.SaveChangesAsync();
    }

    public async Task WithdrawAsync(string accountNumber, decimal amount)
    {
        var account = await GetAccountAsync(accountNumber)
            ?? throw new InvalidOperationException("Account not found");
        if (account.Balance < amount)
            throw new InvalidOperationException("Insufficient funds");
        account.Balance -= amount;
        _db.Transactions.Add(new Transaction
        {
            AccountId = account.Id, Type = TransactionType.Withdrawal, Amount = amount
        });
        await _db.SaveChangesAsync();
    }
}