using FeteCam.Services;

namespace FeteCam.Views;

public partial class MonComptePage : ContentPage
{
    public MonComptePage()
    {
        InitializeComponent();

        var acc = AccountService.Instance;
        NameEntry.Text = acc.Name;
        PhoneEntry.Text = acc.Phone;
        CityEntry.Text = acc.City;
        EmailEntry.Text = acc.Email;
        RefreshPreview();
    }

    private void RefreshPreview()
    {
        var acc = AccountService.Instance;
        string name = string.IsNullOrWhiteSpace(NameEntry.Text) ? acc.Name : NameEntry.Text;
        string phone = string.IsNullOrWhiteSpace(PhoneEntry.Text) ? acc.Phone : PhoneEntry.Text;
        string city = string.IsNullOrWhiteSpace(CityEntry.Text) ? acc.City : CityEntry.Text;

        PreviewNameLabel.Text = name;
        PreviewMetaLabel.Text = $"{phone} · {city}";

        if (string.IsNullOrWhiteSpace(name))
        {
            InitialsLabel.Text = "?";
        }
        else
        {
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            InitialsLabel.Text = parts.Length >= 2
                ? (parts[0][0].ToString() + parts[1][0].ToString()).ToUpper()
                : (name.Length <= 2 ? name.ToUpper() : name.Substring(0, 2).ToUpper());
        }
    }

    private void OnFieldChanged(object? sender, TextChangedEventArgs e)
    {
        RefreshPreview();
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        string name = NameEntry.Text?.Trim() ?? string.Empty;
        string phone = PhoneEntry.Text?.Trim() ?? string.Empty;
        string city = CityEntry.Text?.Trim() ?? string.Empty;
        string email = EmailEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
        {
            await DisplayAlert("Mon compte", "Le nom complet est obligatoire.", "OK");
            return;
        }

        if (!string.IsNullOrWhiteSpace(email) && !email.Contains('@'))
        {
            await DisplayAlert("Mon compte", "L'adresse e-mail n'est pas valide.", "OK");
            return;
        }

        AccountService.Instance.Update(name, phone, city, email);
        await DisplayAlert("Mon compte", "Vos informations ont été mises à jour.", "OK");
        await Navigation.PopAsync();
    }
}
