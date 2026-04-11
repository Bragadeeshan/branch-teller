using BranchTeller.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BranchTeller.Core.Data;

public class BranchTellerContext : DbContext
{
    public BranchTellerContext(DbContextOptions<BranchTellerContext> options)
        : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
}