namespace FeteCam.Services;

public class AccountService
{
    private static AccountService? _instance;
    public static AccountService Instance => _instance ??= new AccountService();

    public string Name { get; set; } = "Laeticia Ngono";
    public string Phone { get; set; } = "+237 671 22 33 44";
    public string City { get; set; } = "Douala";
    public string Email { get; set; } = "laeticia.ngono@fetecam.cm";

    private AccountService()
    {
    }

    public string Initials
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Name))
                return "?";
            var parts = Name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return (parts[0][0].ToString() + parts[1][0].ToString()).ToUpper();
            return Name.Length <= 2 ? Name.ToUpper() : Name.Substring(0, 2).ToUpper();
        }
    }

    public void Update(string name, string phone, string city, string email)
    {
        Name = name;
        Phone = phone;
        City = city;
        Email = email;
    }
}
