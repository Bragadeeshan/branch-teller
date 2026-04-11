namespace BranchTeller.Core.Models;

public class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}