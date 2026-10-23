namespace FeteCam;

public partial class AppShell : Shell
{
    private static bool _routesRegistered;

    public AppShell()
    {
        InitializeComponent();

        if (_routesRegistered)
            return;
        _routesRegistered = true;

        Routing.RegisterRoute("CreationPage", typeof(Views.CreationPage));
        Routing.RegisterRoute("FicheEvenementPage", typeof(Views.FicheEvenementPage));
        Routing.RegisterRoute("InvitesPage", typeof(Views.InvitesPage));
        Routing.RegisterRoute("ContributionsPage", typeof(Views.ContributionsPage));
        Routing.RegisterRoute("BudgetPage", typeof(Views.BudgetPage));
        Routing.RegisterRoute("TachesPage", typeof(Views.TachesPage));
        Routing.RegisterRoute("BilanPage", typeof(Views.BilanPage));
        Routing.RegisterRoute("AjouterInvitePage", typeof(Views.AjouterInvitePage));
        Routing.RegisterRoute("EnregistrerContributionPage", typeof(Views.EnregistrerContributionPage));
        Routing.RegisterRoute("AjouterDepensePage", typeof(Views.AjouterDepensePage));
        Routing.RegisterRoute("NouvelleTachePage", typeof(Views.NouvelleTachePage));
        Routing.RegisterRoute("ProfilPage", typeof(Views.ProfilPage));
        Routing.RegisterRoute("ResponsablesPage", typeof(Views.ResponsablesPage));
        Routing.RegisterRoute("NotificationsPage", typeof(Views.NotificationsPage));
        Routing.RegisterRoute("ParametresPage", typeof(Views.ParametresPage));
        Routing.RegisterRoute("AidePage", typeof(Views.AidePage));
        Routing.RegisterRoute("MonComptePage", typeof(Views.MonComptePage));
        Routing.RegisterRoute("MesEvenementsPage", typeof(Views.MesEvenementsPage));
        Routing.RegisterRoute("invitation", typeof(Views.InvitationPage));
        Routing.RegisterRoute("LoginPage", typeof(Views.LoginPage));
    }
}
