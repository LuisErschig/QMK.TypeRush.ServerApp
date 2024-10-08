namespace QMK.TypeRush.ServerApp.Shared;

public partial class NavMenu
{
    private bool collapseNavMenu = true;

    private string? NavMenuCssClass => this.collapseNavMenu ? "collapse" : null;

    private void ToggleNavMenu()
    {
        this.collapseNavMenu = !this.collapseNavMenu;
    }
}
