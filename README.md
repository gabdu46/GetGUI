# GetGUI
A WinGet marketplace with WinUI 3 to search, explore and install apps with WinGet.
<img src="https://raw.githubusercontent.com/gabdu46/GetGUI/refs/heads/main/docs/getgui%20(2).png">

GetGUI is a modern WinUI 3 interface for browsing, searching, and installing apps available in the official `winget` repository, featuring a local queue, a persistent log console, and smooth Windows 11-style navigation.

## Pre-compiled downloads

Downloads available here ➡️ : https://github.com/gabdu46/GetGUI/releases

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


## Star History

<a href="https://www.star-history.com/?repos=gabdu46%2FGetGUI&type=date&legend=top-left">
 <picture>
   <source media="(prefers-color-scheme: dark)" srcset="https://api.star-history.com/chart?repos=gabdu46/GetGUI&type=date&theme=dark&legend=top-left" />
   <source media="(prefers-color-scheme: light)" srcset="https://api.star-history.com/chart?repos=gabdu46/GetGUI&type=date&legend=top-left" />
   <img alt="Star History Chart" src="https://api.star-history.com/chart?repos=gabdu46/GetGUI&type=date&legend=top-left" />
 </picture>
</a>

## License

MIT License

Copyright (c) 2026 gabdu46

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
