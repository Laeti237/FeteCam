using FeteCam.Models;
using FeteCam.Services;
using Microsoft.Maui.Controls.Shapes;

namespace FeteCam.Views;

public partial class ResponsablesPage : ContentPage
{
    public ResponsablesPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await MergeServerCommitteeAsync();
        LoadResponsibles();
        await LoadSentInvitationsAsync();
    }

    private async Task MergeServerCommitteeAsync()
    {
        if (!ApiService.IsAuthenticated)
            return;

        foreach (var evt in EventService.Instance.Events.ToList())
        {
            var eventId = await ApiService.Instance.EnsureEventIdAsync(evt.Name, evt.Location, evt.Status);
            if (eventId is null)
                continue;

            var members = await ApiService.Instance.GetCommitteeAsync(eventId.Value);
            if (members is null)
                continue;

            foreach (var m in members)
            {
                if (string.IsNullOrWhiteSpace(m.Name))
                    continue;

                var local = ResponsibleService.Instance.FindByName(m.Name);
                if (local is null)
                {
                    ResponsibleService.Instance.Add(m.Name, m.Role, m.Phone);
                    local = ResponsibleService.Instance.FindByName(m.Name);
                }
                if (local is null)
                    continue;

                local.ServerMembershipId = m.Id;
                if (!string.IsNullOrWhiteSpace(m.Role))
                    local.Role = m.Role;
                if (!string.IsNullOrWhiteSpace(m.Phone))
                    local.Phone = m.Phone;
            }
        }
    }

    private void LoadResponsibles()
    {
        ResponsiblesContainer.Children.Clear();
        var list = ResponsibleService.Instance.Responsibles;

        SubtitleLabel.Text = $"Tous les événements · {list.Count} membre{(list.Count > 1 ? "s" : "")}";

        if (list.Count == 0)
        {
            MembersHeader.IsVisible = false;
            var emptyLabel = new Label
            {
                Text = "Aucun membre confirmé pour le moment.",
                TextColor = (Color)Application.Current!.Resources["TextSecondary"],
                FontSize = 13,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 12, 0, 0)
            };
            ResponsiblesContainer.Children.Add(emptyLabel);
            return;
        }

        MembersHeader.IsVisible = true;
        foreach (var item in list)
        {
            ResponsiblesContainer.Children.Add(CreateResponsibleCard(item));
        }
    }

    private Border CreateResponsibleCard(ResponsibleItem item)
    {
        var border = new Border
        {
            Style = (Style)Application.Current!.Resources["CardFrame"],
            Margin = new Thickness(0, 0, 0, 4)
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var avatar = new Border
        {
            WidthRequest = 42,
            HeightRequest = 42,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(21) },
            BackgroundColor = (Color)Application.Current!.Resources["Primary"],
            Stroke = Colors.Transparent,
            VerticalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = item.Initials,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };
        grid.Children.Add(avatar);
        Grid.SetColumn(avatar, 0);

        var infoStack = new VerticalStackLayout
        {
            Spacing = 2,
            Margin = new Thickness(12, 0, 0, 0),
            VerticalOptions = LayoutOptions.Center
        };

        infoStack.Children.Add(new Label
        {
            Text = item.Name,
            Style = (Style)Application.Current!.Resources["SectionTitle"]
        });

        string subtitle = item.Role;
        if (!string.IsNullOrWhiteSpace(item.Phone))
        {
            subtitle += $" · {item.Phone}";
        }

        infoStack.Children.Add(new Label
        {
            Text = subtitle,
            Style = (Style)Application.Current!.Resources["CaptionText"]
        });

        grid.Children.Add(infoStack);
        Grid.SetColumn(infoStack, 1);

        var deleteBtn = new Button
        {
            Text = "Supprimer",
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = (Color)Application.Current!.Resources["Error"],
            BackgroundColor = (Color)Application.Current!.Resources["ErrorLight"],
            WidthRequest = 84,
            HeightRequest = 32,
            Padding = new Thickness(6, 4),
            CornerRadius = 6,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center
        };
        deleteBtn.Clicked += async (s, e) =>
        {
            bool confirm = await DisplayAlert(
                "Supprimer un responsable",
                $"Voulez-vous retirer {item.Name} du comité d'organisation ?",
                "Supprimer",
                "Annuler");

            if (confirm)
            {
                ResponsibleService.Instance.Remove(item.Id);

                if (ApiService.IsAuthenticated)
                {
                    foreach (var evt in EventService.Instance.Events.ToList())
                    {
                        var serverEventId = await ApiService.Instance.EnsureEventIdAsync(evt.Name, evt.Location, evt.Status);
                        if (serverEventId is null)
                            continue;

                        var members = await ApiService.Instance.GetCommitteeAsync(serverEventId.Value);
                        if (members is null)
                            continue;

                        foreach (var m in members.Where(m =>
                            string.Equals(m.Name, item.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            await ApiService.Instance.DeleteCommitteeMemberAsync(serverEventId.Value, m.Id);
                        }
                    }
                }

                LoadResponsibles();
            }
        };

        grid.Children.Add(deleteBtn);
        Grid.SetColumn(deleteBtn, 2);

        border.Content = grid;
        return border;
    }

    private async Task LoadSentInvitationsAsync()
    {
        SentContainer.Children.Clear();
        SentHeader.IsVisible = false;

        if (!ApiService.IsAuthenticated)
            return;

        var sent = await ApiService.Instance.GetSentInvitationsAsync();
        if (sent is null || sent.Count == 0)
            return;

        bool merged = false;
        foreach (var inv in sent)
        {
            if (inv.Status != "Acceptée" || string.IsNullOrWhiteSpace(inv.InviteeName))
                continue;

            var name = inv.InviteeName.Trim();
            bool known = ResponsibleService.Instance.Responsibles
                .Any(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
            if (!known)
            {
                ResponsibleService.Instance.Add(name, inv.Role);
                merged = true;
            }
        }
        if (merged)
            LoadResponsibles();

        SentHeader.IsVisible = true;
        foreach (var inv in sent)
        {
            SentContainer.Children.Add(CreateInvitationCard(inv));
        }
    }

    private Border CreateInvitationCard(InvitationDto inv)
    {
        var border = new Border
        {
            Style = (Style)Application.Current!.Resources["CardFrame"],
            Margin = new Thickness(0, 0, 0, 4)
        };

        var root = new Grid { RowSpacing = 8 };
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        var row0 = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            }
        };

        var displayName = string.IsNullOrWhiteSpace(inv.InviteeName) ? "?" : inv.InviteeName;
        var initials = displayName == "?"
            ? "?"
            : (displayName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries) is { Length: >= 2 } parts
                ? (parts[0][0].ToString() + parts[1][0]).ToUpper()
                : (displayName.Length <= 2 ? displayName.ToUpper() : displayName.Substring(0, 2).ToUpper()));

        var avatar = new Border
        {
            WidthRequest = 42,
            HeightRequest = 42,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(21) },
            BackgroundColor = (Color)Application.Current!.Resources[
                inv.Status == "Acceptée" ? "Primary" : "Surface"],
            Stroke = Colors.Transparent,
            VerticalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = initials,
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = inv.Status == "Acceptée"
                    ? Colors.White
                    : (Color)Application.Current!.Resources["TextSecondary"],
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };
        row0.Children.Add(avatar);
        Grid.SetColumn(avatar, 0);

        var infoStack = new VerticalStackLayout
        {
            Spacing = 2,
            Margin = new Thickness(12, 0, 0, 0),
            VerticalOptions = LayoutOptions.Center
        };
        infoStack.Children.Add(new Label
        {
            Text = inv.InviteeName is { Length: > 0 } ? inv.InviteeName : "Invitation en attente",
            Style = (Style)Application.Current!.Resources["SectionTitle"]
        });
        infoStack.Children.Add(new Label
        {
            Text = $"{inv.Role} · {inv.EventName} · envoyée le {inv.CreatedAt.ToLocalTime():dd/MM}",
            Style = (Style)Application.Current!.Resources["CaptionText"]
        });
        row0.Children.Add(infoStack);
        Grid.SetColumn(infoStack, 1);

        var (badgeBg, badgeFg) = inv.Status switch
        {
            "Acceptée" => ("#DDF0E8", "#0B6B4F"),
            "Refusée" => ("#F9DEDC", "#B3261E"),
            _ => ("#FBEBCB", "#8A5200")
        };

        var badge = new Border
        {
            BackgroundColor = Color.FromArgb(badgeBg),
            Stroke = Colors.Transparent,
            StrokeThickness = 0,
            Padding = new Thickness(10, 5),
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(6) },
            HeightRequest = 32,
            WidthRequest = 84,
            VerticalOptions = LayoutOptions.Center,
            Content = new Label
            {
                Text = inv.Status,
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb(badgeFg),
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };
        row0.Children.Add(badge);
        Grid.SetColumn(badge, 2);

        root.Children.Add(row0);
        Grid.SetRow(row0, 0);

        var actions = new HorizontalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.End
        };

        var copyBtn = CreateActionButton("Copier");
        copyBtn.Clicked += async (s, e) =>
        {
            await Clipboard.Default.SetTextAsync(inv.DeepLink);
            bool share = await DisplayAlert(
                "Lien copié",
                $"{inv.DeepLink}\n\nLe lien a été copié. Envoyez-le à la personne invitée : elle l'ouvrira dans FêteCam pour accepter ou refuser.",
                "Partager",
                "Fermer");
            if (share)
                await ShareInvitationAsync(inv);
        };
        actions.Children.Add(copyBtn);

        var shareBtn = CreateActionButton("Partager");
        shareBtn.Clicked += async (s, e) => await ShareInvitationAsync(inv);
        actions.Children.Add(shareBtn);

        var deleteBtn = new Button
        {
            Text = "Supprimer",
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = (Color)Application.Current!.Resources["Error"],
            BackgroundColor = (Color)Application.Current!.Resources["ErrorLight"],
            WidthRequest = 84,
            HeightRequest = 32,
            Padding = new Thickness(6, 4),
            CornerRadius = 6,
            VerticalOptions = LayoutOptions.Center
        };
        deleteBtn.Clicked += async (s, e) =>
        {
            bool confirm = await DisplayAlert(
                "Supprimer l'invitation",
                $"Supprimer l'invitation « {inv.Role} » ? Le lien cessera de fonctionner.",
                "Supprimer",
                "Annuler");
            if (!confirm)
                return;

            var ok = await ApiService.Instance.DeleteInvitationAsync(inv.Id);
            if (!ok)
            {
                await DisplayAlert("Erreur", ApiService.Instance.LastError ?? "Suppression impossible.", "OK");
                return;
            }
            await LoadSentInvitationsAsync();
        };
        actions.Children.Add(deleteBtn);

        root.Children.Add(actions);
        Grid.SetRow(actions, 1);

        border.Content = root;
        return border;
    }

    private Button CreateActionButton(string text) => new()
    {
        Text = text,
        FontSize = 11,
        FontAttributes = FontAttributes.Bold,
        TextColor = (Color)Application.Current!.Resources["TextPrimary"],
        BackgroundColor = (Color)Application.Current!.Resources["Surface"],
        BorderColor = (Color)Application.Current!.Resources["Border"],
        BorderWidth = 1,
        WidthRequest = 84,
        HeightRequest = 32,
        Padding = new Thickness(6, 4),
        CornerRadius = 6,
        VerticalOptions = LayoutOptions.Center
    };

    private async Task ShareInvitationAsync(InvitationDto inv)
    {
        try
        {
            await Share.RequestAsync(new ShareTextRequest
            {
                Title = "Invitation au comité FêteCam",
                Text = $"Tu es invité(e) à rejoindre le comité « {inv.Role} » pour {inv.EventName}.\n" +
                       $"Ouvre ce lien dans FêteCam pour accepter ou refuser :\n{inv.DeepLink}"
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Partage", $"Impossible d'ouvrir le partage : {ex.Message}", "OK");
        }
    }

    private async void OnAddResponsibleClicked(object? sender, EventArgs e)
    {
        string name = await DisplayPromptAsync(
            "Inviter une personne",
            "Nom complet de la personne à inviter :",
            "Suivant",
            "Annuler",
            "Ex. Jean Kamga");

        if (string.IsNullOrWhiteSpace(name))
            return;

        string role = await DisplayPromptAsync(
            "Rôle / Mission",
            $"Quel rôle pour {name.Trim()} ?",
            "Suivant",
            "Ignorer",
            "Ex. Traiteur & Boissons",
            initialValue: "Membre du comité");

        if (string.IsNullOrWhiteSpace(role))
            role = "Membre du comité";

        if (!ApiService.IsAuthenticated)
        {
            bool goLogin = await DisplayAlert(
                "Compte requis",
                "Générer un lien d'invitation nécessite un compte FêteCam.",
                "Se connecter",
                "Annuler");
            if (goLogin)
                await Navigation.PushAsync(new LoginPage());
            return;
        }

        var evt = EventService.Instance.Current;
        var eventId = await ApiService.Instance.EnsureEventIdAsync(evt.Name, evt.Location, evt.Status);

        if (eventId is null)
        {
            bool local = await DisplayAlert(
                "Serveur injoignable",
                $"Le lien n'a pas pu être généré ({ApiService.Instance.LastError}).\nAjouter {name.Trim()} localement sans lien ?",
                "Ajouter",
                "Annuler");
            if (local)
            {
                ResponsibleService.Instance.Add(name, role);
                LoadResponsibles();
            }
            return;
        }

        var inv = await ApiService.Instance.CreateInvitationAsync(eventId.Value, role, taskTitle: role, inviteeName: name.Trim());
        if (inv is null)
        {
            bool local = await DisplayAlert(
                "Échec de la génération",
                $"{ApiService.Instance.LastError}\nAjouter {name.Trim()} localement sans lien ?",
                "Ajouter",
                "Annuler");
            if (local)
            {
                ResponsibleService.Instance.Add(name, role);
                LoadResponsibles();
            }
            return;
        }

        await Clipboard.Default.SetTextAsync(inv.DeepLink);
        bool share = await DisplayAlert(
            "Lien d'invitation copié",
            $"{inv.DeepLink}\n\nEnvoyez ce lien à {name.Trim()} : l'invitation ouvre directement FêteCam pour accepter ou refuser le rôle « {role} ».",
            "Partager",
            "Fermer");
        if (share)
            await ShareInvitationAsync(inv);

        await LoadSentInvitationsAsync();
    }

    private async void OnBackTapped(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
