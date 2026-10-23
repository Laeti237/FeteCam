using FeteCam.Services;

namespace FeteCam.Views;

public partial class LoginPage : ContentPage
{
    private bool _registerMode;

    public LoginPage()
    {
        InitializeComponent();
        ServerLabel.Text = $"Serveur : {ApiService.BaseUrl}";
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BackLabel.IsVisible = Navigation.NavigationStack.Count > 1;
    }

    private void SetMode(bool register)
    {
        _registerMode = register;
        RegisterFields.IsVisible = register;
        LoginTab.BackgroundColor = (Color)(register
            ? Application.Current!.Resources["Surface"]
            : Application.Current!.Resources["Primary"]);
        LoginTabLabel.TextColor = (Color)(register
            ? Application.Current!.Resources["TextSecondary"]
            : Colors.White);
        RegisterTab.BackgroundColor = (Color)(register
            ? Application.Current!.Resources["Primary"]
            : Application.Current!.Resources["Surface"]);
        RegisterTabLabel.TextColor = (Color)(register
            ? Colors.White
            : Application.Current!.Resources["TextSecondary"]);
        FormTitleLabel.Text = register ? "Créez votre compte" : "Heureux de vous revoir";
        PrimaryButton.Text = register ? "Créer mon compte" : "Se connecter";
        ErrorLabel.IsVisible = false;
    }

    private void OnLoginTabTapped(object sender, EventArgs e) => SetMode(false);

    private void OnRegisterTabTapped(object sender, EventArgs e) => SetMode(true);

    private async void OnPrimaryClicked(object sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        var email = EmailEntry.Text?.Trim() ?? "";
        var password = PasswordEntry.Text ?? "";

        if (!email.Contains('@'))
        {
            ShowError("Adresse e-mail invalide.");
            return;
        }
        if (password.Length < 6)
        {
            ShowError("Le mot de passe doit contenir au moins 6 caractères.");
            return;
        }

        PrimaryButton.IsEnabled = false;
        PrimaryButton.Text = _registerMode ? "Création…" : "Connexion…";

        UserDto? user;
        if (_registerMode)
        {
            var name = NameEntry.Text?.Trim() ?? "";
            if (name.Length < 2)
            {
                ShowError("Entrez votre nom complet.");
                ResetPrimary();
                return;
            }
            user = await ApiService.Instance.RegisterAsync(email, password, name,
                PhoneEntry.Text?.Trim() ?? "", CityEntry.Text?.Trim() ?? "");
        }
        else
        {
            user = await ApiService.Instance.LoginAsync(email, password);
        }

        if (user is null)
        {
            ShowError(ApiService.Instance.LastError ?? "Connexion impossible.");
            ResetPrimary();
            return;
        }

        ResetPrimary();
        await LeaveAsync();
    }

    private async Task LeaveAsync()
    {
        if (Navigation.NavigationStack.Count > 1)
            await Navigation.PopAsync();
        else
            await App.EnterAuthenticatedAsync();
    }

    private void ResetPrimary()
    {
        PrimaryButton.IsEnabled = true;
        PrimaryButton.Text = _registerMode ? "Créer mon compte" : "Se connecter";
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private async void OnInviteLinkTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new InvitationPage());
    }

    private async void OnOfflineTapped(object sender, EventArgs e)
    {
        await LeaveAsync();
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        if (Navigation.NavigationStack.Count > 1)
            await Navigation.PopAsync();
    }
}
