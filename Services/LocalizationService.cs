using System.Globalization;

namespace GetGUI.Services;

public sealed class LocalizationService
{
    public static LocalizationService Current { get; } = new();

    private bool IsFrench => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.Equals("fr", StringComparison.OrdinalIgnoreCase);

    private string Pick(string fr, string en) => IsFrench ? fr : en;

    public string NavSearch => Pick("Recherche", "Search");
    public string NavQueue => Pick("File", "Queue");
    public string NavConsole => Pick("Console", "Console");
    public string NavSettings => Pick("Parametres", "Settings");

    public string SearchPlaceholder => Pick("Rechercher une application winget", "Search for a winget app");
    public string PopularOnWinget => Pick("Populaires sur winget", "Popular on winget");
    public string Details => Pick("Details", "Details");
    public string AddToQueue => Pick("Ajouter a la file", "Add to queue");
    public string InstallNow => Pick("Installer maintenant", "Install now");

    public string ConsoleTitle => Pick("Console", "Console");
    public string Refresh => Pick("Actualiser", "Refresh");
    public string Clear => Pick("Effacer", "Clear");
    public string ConsoleInfo => Pick(
        "Les logs sont conserves entre les lancements, sauf si l'option de vidage a la fermeture est activee.",
        "Logs are kept between launches unless clear-on-exit is enabled.");

    public string Back => Pick("Retour", "Back");
    public string Install => Pick("Installer", "Install");
    public string Uninstall => Pick("Desinstaller", "Uninstall");
    public string InstallOperation => Pick("Installation", "Install");
    public string UninstallOperation => Pick("Desinstallation", "Uninstall");
    public string MoveUp => Pick("Monter", "Move up");
    public string MoveDown => Pick("Descendre", "Move down");
    public string Retry => Pick("Reessayer", "Retry");
    public string Remove => Pick("Supprimer", "Remove");
    public string Publisher => Pick("Editeur", "Publisher");
    public string License => Pick("Licence", "License");
    public string Homepage => Pick("Page web", "Homepage");
    public string Description => Pick("Description", "Description");
    public string Logs => Pick("Logs", "Logs");

    public string QueueTitle => Pick("File d'installation", "Install queue");
    public string InstallAll => Pick("Installer tout", "Install all");
    public string Import => Pick("Importer", "Import");
    public string Export => Pick("Exporter", "Export");
    public string QueueLogsPlaceholder => Pick(
        "Les logs winget apparaitront ici pendant les installations.",
        "winget logs will appear here during installations.");
    public string EmptyQueue => Pick("La file est vide.", "The queue is empty.");
    public string AddAppsFromSearch => Pick("Ajoutez des applications depuis la recherche.", "Add apps from search.");

    public string SettingsTitle => Pick("Parametres", "Settings");
    public string PreferencesStoredLocally => Pick("Les preferences sont stockees localement.", "Preferences are stored locally.");
    public string Theme => Pick("Theme", "Theme");
    public string SystemTheme => Pick("Systeme", "System");
    public string LightTheme => Pick("Clair", "Light");
    public string DarkTheme => Pick("Sombre", "Dark");
    public string SilentInstall => Pick("Installation silencieuse", "Silent install");
    public string SilentInstallOff => Pick("winget install sans --silent", "winget install without --silent");
    public string SilentInstallOn => Pick("winget install --silent", "winget install --silent");
    public string KeepLogs => Pick("Conserver les logs", "Keep logs");
    public string KeepLogsOff => Pick("Masquer les logs apres operations", "Hide logs after operations");
    public string KeepLogsOn => Pick("Afficher les logs winget", "Show winget logs");
    public string ClearLogsOnExit => Pick("Vider les logs a la fermeture", "Clear logs on exit");
    public string ClearLogsOnExitOff => Pick("Conserver la console entre les lancements", "Keep the console between launches");
    public string ClearLogsOnExitOn => Pick("Effacer la console quand GetGUI se ferme", "Clear the console when GetGUI closes");
    public string DataSourceTitle => Pick("Source des donnees", "Data source");
    public string DataSourceMessage => Pick(
        "GetGUI interroge uniquement winget search et winget show. Aucune base applicative externe n'est utilisee.",
        "GetGUI only queries winget search and winget show. No external app database is used.");

    public string SearchStartMessage => Pick(
        "Saisissez un nom d'application, un editeur ou un ID winget.",
        "Enter an app name, publisher, or winget ID.");
    public string PopularRefreshMessage => Pick("Mise a jour rapide des details populaires...", "Refreshing popular app details...");
    public string MinimumSearchLength => Pick("Entrez au moins 2 caracteres pour lancer la recherche winget.", "Enter at least 2 characters to search winget.");
    public string SearchInProgress => Pick("Recherche winget en cours...", "Searching winget...");
    public string NoResults => Pick("Aucun resultat winget pour cette recherche.", "No winget results for this search.");
    public string SearchResults(int count) => Pick($"{count} resultat(s) depuis la source winget.", $"{count} result(s) from the winget source.");
    public string AddedToQueue(string name) => Pick($"{name} ajoute a la file.", $"{name} added to the queue.");
    public string AlreadyInQueue(string name) => Pick($"{name} est deja dans la file.", $"{name} is already in the queue.");
    public string Installing(string name) => Pick($"Installation de {name}...", $"Installing {name}...");
    public string Installed(string name) => Pick($"{name} installe.", $"{name} installed.");
    public string InstallFailed(string name) => Pick($"Echec de l'installation de {name}.", $"Failed to install {name}.");
    public string QueueLocalMessage => Pick("La file est locale et peut etre exportee en JSON.", "The queue is local and can be exported as JSON.");
    public string QueueNeedsItems => Pick("Ajoutez d'abord des applications a la file.", "Add apps to the queue first.");
    public string QueueInstalling => Pick("Installation sequentielle en cours...", "Sequential install in progress...");
    public string QueueComplete => Pick("Traitement de la file termine.", "Queue processing complete.");
    public string QueueExported(string path) => Pick($"File exportee vers {path}.", $"Queue exported to {path}.");
    public string QueueImported(string path) => Pick($"File importee depuis {path}.", $"Queue imported from {path}.");
    public string InvalidWingetApp => Pick("Application winget invalide.", "Invalid winget app.");
    public string LoadingDetails => Pick("Chargement des details winget...", "Loading winget details...");
    public string DetailsLoaded => Pick("Details charges depuis winget.", "Details loaded from winget.");
    public string OperationInProgress(string label, string name) => Pick($"{label} de {name}...", $"{label} {name}...");
    public string OperationDone(string label) => Pick($"{label} terminee.", $"{label} complete.");
    public string OperationFailed(string label) => Pick($"{label} en echec.", $"{label} failed.");
    public string PublisherUnavailable => Pick("Editeur indisponible", "Publisher unavailable");
    public string DescriptionAvailableInDetails => Pick(
        "Description disponible dans la fiche detaillee winget.",
        "Description available in the detailed winget view.");
    public string StatusPending => Pick("En attente", "Pending");
    public string StatusInstalling => Pick("Installation", "Installing");
    public string StatusSuccess => Pick("Installee", "Installed");
    public string StatusFailed => Pick("Echec", "Failed");
    public string StatusUnknown => Pick("Inconnu", "Unknown");
    public string ClearLogsTitle => Pick("Effacer les logs ?", "Clear logs?");
    public string ClearLogsContent => Pick("Cette action vide la console locale de GetGUI.", "This clears GetGUI's local console.");
    public string UninstallDialogTitle => Pick("Desinstaller cette application ?", "Uninstall this app?");
    public string UninstallDialogPrimary => Pick("Desinstaller", "Uninstall");
    public string Cancel => Pick("Annuler", "Cancel");
    public string AppStarted => Pick("GetGUI demarre.", "GetGUI started.");
    public string AppClosed => Pick("GetGUI ferme.", "GetGUI closed.");
}
