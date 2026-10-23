using FeteCam.Services;

namespace FeteCam;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        DeepLinkService.Init();
        DeepLinkService.LinkReceived += OnLinkReceived;

        MainPage = ApiService.IsAuthenticated
            ? new AppShell()
            : new NavigationPage(new Views.LoginPage());

        ProcessPendingInvite();
    }

    private static void OnLinkReceived(string token)
    {
        DeepLinkService.Log($"OnLinkReceived mainPage={Application.Current?.MainPage?.GetType().Name}");
        if (Application.Current?.MainPage is AppShell)
        {
            DeepLinkService.PendingToken = null;
            _ = Task.Run(async () =>
            {
                try
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                        await Shell.Current!.GoToAsync($"invitation?token={Uri.EscapeDataString(token)}"));
                    DeepLinkService.Log("GoToAsync OK");
                }
                catch (Exception ex)
                {
                    DeepLinkService.Log($"GoToAsync ERROR {ex}");
                }
            });
        }
        else
        {
            DeepLinkService.Log("OnLinkReceived: MainPage not AppShell, keep pending");
        }
    }

    private static void ProcessPendingInvite()
    {
        if (DeepLinkService.PendingToken is not string token)
        {
            DeepLinkService.Log("Cold start: no pending token");
            return;
        }

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(1500);
            if (DeepLinkService.PendingToken is not string pending
                || Application.Current?.MainPage is not AppShell)
            {
                DeepLinkService.Log($"Cold start skip: main={Application.Current?.MainPage?.GetType().Name}");
                return;
            }

            DeepLinkService.PendingToken = null;
            try
            {
                await Shell.Current!.GoToAsync($"invitation?token={Uri.EscapeDataString(pending)}");
                DeepLinkService.Log("Cold start invite OK");
            }
            catch (Exception ex)
            {
                DeepLinkService.Log($"Cold start ERROR {ex}");
            }
        });
    }

    public static async Task EnterAuthenticatedAsync()
    {
        var shell = new AppShell();
        Application.Current!.MainPage = shell;

        if (DeepLinkService.PendingToken is string token)
        {
            DeepLinkService.PendingToken = null;
            try
            {
                await shell.GoToAsync($"invitation?token={Uri.EscapeDataString(token)}");
            }
            catch { }
        }
        else
        {
            try
            {
                await shell.GoToAsync("//AccueilPage");
            }
            catch { }
        }
    }

    public static async Task EnterLoginAsync()
    {
        Application.Current!.MainPage = new NavigationPage(new Views.LoginPage());
        await Task.CompletedTask;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = base.CreateWindow(activationState);
        window.Title = "FêteCam";
        window.Width = 412;
        window.Height = 840;
        window.MinimumWidth = 360;
        window.MinimumHeight = 600;
        window.MaximumWidth = 460;
        window.MaximumHeight = 920;
        return window;
    }
}
