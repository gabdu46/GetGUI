# GetGUI
A WinGet marketplace with WinUI 3 to search, explore and install apps with WinGet.


GetGUI is a modern WinUI 3 interface for browsing, searching, and installing apps available in the official `winget` repository, featuring a local queue, a persistent log console, and smooth Windows 11-style navigation.

## Pre-compiled downloads

Downloads avaible here ➡️ : https://github.com/gabdu46/GetGUI/releases

## Features

- Real-time search via `winget search`
- Detailed info via `winget show`
- Direct installation or addition to a local queue
- Sequential installation of multiple apps
- Uninstallation from the detailed card
- Modern cards with name, ID, publisher, version, description, and icons if available
- Persistent console/logs tab between launches
- Option to clear logs on exit
- Import and export the queue as JSON
- Light, dark, or system mode

## Data Source

GetGUI does not use any external databases.
All information comes solely from the official `winget` commands:

- `winget search`
- `winget show`
- `winget install`
- `winget uninstall`

## Prerequisites

- Windows 10 1809 or later
- Windows 11 recommended
- `winget` installed and accessible in the PATH
- .NET 8 SDK
- Visual Studio 2022 with the **.NET Desktop Development** workload
- **Windows App SDK / WinUI 3** support 

## Project Structure

- `GetGUI/Views/SearchPage.xaml`: search page and cards
- `GetGUI/Views/DetailsPage.xaml`: application details and actions
- `GetGUI/Views/QueuePage.xaml`: queue management
- `GetGUI/Views/ConsolePage.xaml`: persistent logs
- `GetGUI/Views/SettingsPage.xaml`: theme and options
- `GetGUI/Services/WingetService.cs`: execution and parsing of Winget commands
- `GetGUI/Services/QueueService.cs`: local queue and JSON export
- `GetGUI/Services/AppLogService.cs`: persistent logging
- `GetGUI/ViewModels/`: presentation logic

## Console behavior

The console logs `winget` commands, their output, and their errors to a local file under the user profile.

Option available in settings:

- `Clear logs on exit`

## Project screenshot


<img src="https://raw.githubusercontent.com/gabdu46/GetGUI/refs/heads/main/docs/screenshot.png">


## License

MIT Licence
