using BranchTeller.Core.Data;
using BranchTeller.Core.Models;
using BranchTeller.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace BranchTeller.WinForms;

public class TellerForm : Form
{
    private readonly BranchTellerContext _db;
    private readonly AccountService _service;
    private Account? _currentAccount;

    private TextBox txtAccountNumber = new();
    private TextBox txtAmount = new();
    private Label lblNameValue = new();
    private Label lblBalanceValue = new();
    private Label lblStatus = new();
    private ListBox lstTransactions = new();
    private Button btnLoad = new();
    private Button btnDeposit = new();
    private Button btnWithdraw = new();
    private Button btnReceipt = new();

    public TellerForm()
    {
        BuildUI();

        var options = new DbContextOptionsBuilder<BranchTellerContext>()
            .UseSqlite("Data Source=BranchTellerDb.sqlite")
            .Options;

        _db = new BranchTellerContext(options);
        _db.Database.EnsureCreated();
        _service = new AccountService(_db);

        SeedIfEmpty();
    }

    private void BuildUI()
    {
        Text = "Branch Teller";
        Size = new Size(620, 600);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.White;
        Font = new Font("Segoe UI", 10);

        // Account number row
        var lblAccount = new Label { Text = "Account Number", Location = new Point(20, 20), Size = new Size(130, 25), TextAlign = ContentAlignment.MiddleLeft };
        txtAccountNumber = new TextBox { Location = new Point(160, 20), Size = new Size(200, 25) };
        btnLoad = new Button { Text = "Load Account", Location = new Point(370, 18), Size = new Size(130, 30), BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnLoad.Click += btnLoad_Click;

        // Customer info
        var lblName = new Label { Text = "Customer", Location = new Point(20, 65), Size = new Size(130, 25), TextAlign = ContentAlignment.MiddleLeft };
        lblNameValue = new Label { Location = new Point(160, 65), Size = new Size(300, 25), ForeColor = Color.DarkSlateGray };

        var lblBal = new Label { Text = "Balance", Location = new Point(20, 95), Size = new Size(130, 25), TextAlign = ContentAlignment.MiddleLeft };
        lblBalanceValue = new Label { Text = "$0.00", Location = new Point(160, 95), Size = new Size(200, 25), ForeColor = Color.DarkGreen, Font = new Font("Segoe UI", 12, FontStyle.Bold) };

        // Divider
        var divider = new Panel { Location = new Point(20, 130), Size = new Size(560, 2), BackColor = Color.LightGray };

        // Amount row
        var lblAmount = new Label { Text = "Amount ($)", Location = new Point(20, 145), Size = new Size(130, 25), TextAlign = ContentAlignment.MiddleLeft };
        txtAmount = new TextBox { Location = new Point(160, 145), Size = new Size(150, 25) };

        // Action buttons
        btnDeposit = new Button { Text = "Deposit", Location = new Point(20, 185), Size = new Size(130, 38), BackColor = Color.SeaGreen, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnDeposit.Click += btnDeposit_Click;

        btnWithdraw = new Button { Text = "Withdraw", Location = new Point(165, 185), Size = new Size(130, 38), BackColor = Color.Tomato, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnWithdraw.Click += btnWithdraw_Click;

        btnReceipt = new Button { Text = "Print Receipt", Location = new Point(310, 185), Size = new Size(130, 38), BackColor = Color.SlateGray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        btnReceipt.Click += btnReceipt_Click;

        // Divider
        var divider2 = new Panel { Location = new Point(20, 235), Size = new Size(560, 2), BackColor = Color.LightGray };

        // Transaction history
        var lblHistory = new Label { Text = "Transaction History", Location = new Point(20, 245), Size = new Size(200, 25), Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        lstTransactions = new ListBox { Location = new Point(20, 275), Size = new Size(560, 220), Font = new Font("Courier New", 9), BorderStyle = BorderStyle.FixedSingle };

        // Status bar
        lblStatus = new Label { Text = "Ready", Location = new Point(20, 510), Size = new Size(560, 25), ForeColor = Color.Gray };

        Controls.AddRange(new Control[]
        {
            lblAccount, txtAccountNumber, btnLoad,
            lblName, lblNameValue,
            lblBal, lblBalanceValue,
            divider,
            lblAmount, txtAmount,
            btnDeposit, btnWithdraw, btnReceipt,
            divider2,
            lblHistory, lstTransactions,
            lblStatus
        });
    }

    private void SeedIfEmpty()
    {
        if (!_db.Accounts.Any())
        {
            var customer = new Customer { FullName = "John Smith", Email = "john@example.com" };
            _db.Customers.Add(customer);
            _db.Accounts.Add(new Account
            {
                AccountNumber = "ACC001",
                Balance = 1000.00m,
                Customer = customer
            });
            _db.SaveChanges();
        }
    }

    private async void btnLoad_Click(object? sender, EventArgs e)
    {
        var number = txtAccountNumber.Text.Trim();
        if (string.IsNullOrEmpty(number))
        {
            SetStatus("Please enter an account number.", Color.Red);
            return;
        }

        _currentAccount = await _service.GetAccountAsync(number);

        if (_currentAccount is null)
        {
            SetStatus("Account not found.", Color.Red);
            lblNameValue.Text = string.Empty;
            lblBalanceValue.Text = "$0.00";
            lstTransactions.Items.Clear();
            return;
        }

        RefreshAccountDisplay();
        SetStatus("Account loaded successfully.", Color.Green);
    }

    private async void btnDeposit_Click(object? sender, EventArgs e)
    {
        if (!ValidateInput(out decimal amount)) return;

        try
        {
            await _service.DepositAsync(_currentAccount!.AccountNumber, amount);
            _currentAccount = await _service.GetAccountAsync(_currentAccount.AccountNumber);
            RefreshAccountDisplay();
            SetStatus($"Deposited ${amount:F2} successfully.", Color.Green);
            txtAmount.Clear();
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}", Color.Red);
        }
    }

    private async void btnWithdraw_Click(object? sender, EventArgs e)
    {
        if (!ValidateInput(out decimal amount)) return;

        try
        {
            await _service.WithdrawAsync(_currentAccount!.AccountNumber, amount);
            _currentAccount = await _service.GetAccountAsync(_currentAccount.AccountNumber);
            RefreshAccountDisplay();
            SetStatus($"Withdrew ${amount:F2} successfully.", Color.Green);
            txtAmount.Clear();
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}", Color.Red);
        }
    }

    private void btnReceipt_Click(object? sender, EventArgs e)
    {
        if (_currentAccount is null)
        {
            SetStatus("Load an account first.", Color.Red);
            return;
        }

        var receipt = new System.Text.StringBuilder();
        receipt.AppendLine("========== RECEIPT ==========");
        receipt.AppendLine($"Account : {_currentAccount.AccountNumber}");
        receipt.AppendLine($"Customer: {_currentAccount.Customer.FullName}");
        receipt.AppendLine($"Balance : ${_currentAccount.Balance:F2}");
        receipt.AppendLine("-----------------------------");
        foreach (var t in _currentAccount.Transactions.OrderByDescending(t => t.Timestamp))
            receipt.AppendLine($"{t.Timestamp:g}  {t.Type,-12}  ${t.Amount:F2}");
        receipt.AppendLine("=============================");

        MessageBox.Show(receipt.ToString(), "Receipt", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void RefreshAccountDisplay()
    {
        if (_currentAccount is null) return;

        lblNameValue.Text = _currentAccount.Customer.FullName;
        lblBalanceValue.Text = $"${_currentAccount.Balance:F2}";

        lstTransactions.Items.Clear();
        foreach (var t in _currentAccount.Transactions.OrderByDescending(t => t.Timestamp))
            lstTransactions.Items.Add($"{t.Timestamp:g}  {t.Type,-12}  ${t.Amount:F2}");
    }

    private bool ValidateInput(out decimal amount)
    {
        amount = 0;

        if (_currentAccount is null)
        {
            SetStatus("Load an account first.", Color.Red);
            return false;
        }

        if (!decimal.TryParse(txtAmount.Text, out amount) || amount <= 0)
        {
            SetStatus("Please enter a valid amount.", Color.Red);
            return false;
        }

        return true;
    }

    private void SetStatus(string message, Color color)
    {
        lblStatus.Text = message;
        lblStatus.ForeColor = color;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _db.Dispose();
        base.OnFormClosing(e);
    }
}