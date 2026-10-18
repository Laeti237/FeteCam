namespace FeteCam;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new AppShell();
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
