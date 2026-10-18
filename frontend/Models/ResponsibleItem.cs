namespace FeteCam.Models;

public class ResponsibleItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = "Membre du comité";
    public string Phone { get; set; } = string.Empty;
    public int TaskCount { get; set; } = 0;
    public string Initials => string.IsNullOrWhiteSpace(Name) ? "?" : (Name.Length <= 2 ? Name.ToUpper() : Name.Substring(0, 2).ToUpper());
}
