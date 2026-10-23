using FeteCam.Models;
using FeteCam.Services;

namespace FeteCam.Views;

[QueryProperty(nameof(Token), "token")]
public partial class InvitationPage : ContentPage
{
    public string? Token { get; set; }

    private InvitationDetailDto? _detail;

    public InvitationPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (string.IsNullOrWhiteSpace(Token))
        {
            PasteCard.IsVisible = true;
            LoadingLabel.IsVisible = false;
            DetailCard.IsVisible = false;
            SubtitleLabel.Text = "Coller le lien reçu de l'organisateur";
            return;
        }

        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        PasteCard.IsVisible = false;
        DetailCard.IsVisible = false;
        ErrorCard.IsVisible = false;
        LoadingLabel.IsVisible = true;
        SubtitleLabel.Text = "Vérification du lien…";

        var detail = await ApiService.Instance.GetInvitationAsync(Token!);
        LoadingLabel.IsVisible = false;

        if (detail is null)
        {
            ErrorCard.IsVisible = true;
            ErrorLabel.Text = ApiService.Instance.LastError ?? "Ce lien n'est pas valide ou a expiré.";
            SubtitleLabel.Text = "Lien invalide";
            return;
        }

        _detail = detail;
        ShowDetail(detail);
    }

    private void ShowDetail(InvitationDetailDto detail)
    {
        DetailCard.IsVisible = true;
        SubtitleLabel.Text = detail.EventName;

        EventNameLabel.Text = detail.EventName;
        InviterLabel.Text = $"Invité(e) par {detail.InviterName} à rejoindre le comité d'organisation";
        RoleLabel.Text = detail.Role;

        if (string.IsNullOrWhiteSpace(detail.TaskTitle))
        {
            TaskRow.IsVisible = false;
        }
        else
        {
            TaskRow.IsVisible = true;
            TaskLabel.Text = detail.TaskTitle;
        }

        StatusLabel.Text = detail.Status;

        ResultLabel.IsVisible = false;
        AuthHint.IsVisible = false;
        RespondButtons.IsVisible = false;

        var pending = detail.Status == "En attente";

        if (!pending)
        {
            ResultLabel.IsVisible = true;
            ResultLabel.Text = detail.Status == "Acceptée"
                ? "Vous faites partie de ce comité d'organisation."
                : "Vous avez refusé cette invitation.";
            ResultLabel.TextColor = (Color)Application.Current!.Resources[
                detail.Status == "Acceptée" ? "Success" : "TextSecondary"];
            return;
        }

        if (!ApiService.IsAuthenticated)
        {
            AuthHint.IsVisible = true;
            return;
        }

        if (detail.CanRespond)
        {
            RespondButtons.IsVisible = true;
        }
        else
        {
            ResultLabel.IsVisible = true;
            ResultLabel.Text = "Cette invitation a déjà été traitée.";
            ResultLabel.TextColor = (Color)Application.Current!.Resources["TextSecondary"];
        }
    }

    private async void OnAcceptClicked(object sender, EventArgs e)
    {
        if (_detail is null) return;

        AcceptButton.IsEnabled = false;
        AcceptButton.Text = "Envoi…";

        var result = await ApiService.Instance.AcceptInvitationAsync(_detail.Token);
        AcceptButton.IsEnabled = true;
        AcceptButton.Text = "Accepter l'invitation";

        if (result is null)
        {
            await DisplayAlert("Erreur", ApiService.Instance.LastError ?? "Réponse impossible.", "OK");
            return;
        }

        var evt = EventService.Instance.GetByName(_detail.EventName);
        if (evt is null)
        {
            evt = new EventItem
            {
                Name = _detail.EventName,
                Status = "Confirmé",
                StatusColor = "#10B981",
                StatusTextColor = "#FFFFFF"
            };
            EventService.Instance.Events.Add(evt);
        }

        ResponsibleService.Instance.Add(
            AccountService.Instance.Name,
            _detail.Role);

        await DisplayAlert("Invitation acceptée",
            $"Vous rejoignez le comité de « {_detail.EventName} » en tant que {_detail.Role}.",
            "Super");

        await LoadAsync();
    }

    private async void OnDeclineClicked(object sender, EventArgs e)
    {
        if (_detail is null) return;

        bool confirm = await DisplayAlert("Refuser l'invitation",
            $"Voulez-vous vraiment refuser le rôle de {_detail.Role} pour « {_detail.EventName} » ?",
            "Refuser", "Annuler");
        if (!confirm) return;

        var result = await ApiService.Instance.DeclineInvitationAsync(_detail.Token);
        if (result is null)
        {
            await DisplayAlert("Erreur", ApiService.Instance.LastError ?? "Réponse impossible.", "OK");
            return;
        }

        await LoadAsync();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new LoginPage());
    }

    private async void OnOpenLinkClicked(object sender, EventArgs e)
    {
        var input = LinkEntry.Text?.Trim() ?? "";
        var token = DeepLinkService.ExtractToken(input);
        if (token is null && !string.IsNullOrWhiteSpace(input))
            token = input;

        if (token is null)
        {
            PasteErrorLabel.Text = "Ce lien n'est pas valide.";
            PasteErrorLabel.IsVisible = true;
            return;
        }

        PasteErrorLabel.IsVisible = false;
        Token = token;
        await LoadAsync();
    }

    private async void OnBackTapped(object sender, EventArgs e)
    {
        if (Navigation.NavigationStack.Count > 1)
            await Navigation.PopAsync();
    }
}
