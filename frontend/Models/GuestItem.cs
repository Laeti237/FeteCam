namespace FeteCam.Models;

public class GuestItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int PersonCount { get; set; } = 1;
    public string Status { get; set; } = "En attente";
    public string StatusColor { get; set; } = "#FBEBCB";
    public string Initials => string.IsNullOrWhiteSpace(Name) ? "?" : (Name.Length <= 2 ? Name.ToUpper() : Name.Substring(0, 2).ToUpper());
    public string StatusTextColor { get; set; } = "#FFFFFF";
}
