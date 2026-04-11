using BranchTeller.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace BranchTeller.Tests.Helpers;

public static class TestDbContextFactory
{
    public static BranchTellerContext Create()
    {
        var options = new DbContextOptionsBuilder<BranchTellerContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new BranchTellerContext(options);
    }
}