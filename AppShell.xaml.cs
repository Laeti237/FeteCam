namespace FeteCam;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

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
    }
}
