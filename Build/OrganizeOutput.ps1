param(
    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"

$root = [System.IO.Path]::GetFullPath($OutputPath.Trim('"'))
$languages = Join-Path $root "languages"
$dlls = Join-Path $root "dlls"

New-Item -ItemType Directory -Force -Path $languages | Out-Null
New-Item -ItemType Directory -Force -Path $dlls | Out-Null

$requiredRootLanguages = @("fr-FR", "en-us")

foreach ($directory in Get-ChildItem -Path $root -Directory) {
    if ($directory.Name -eq "languages" -or $directory.Name -eq "dlls") {
        continue
    }

    if ($directory.Name -notmatch '^[a-z]{2,3}(-[A-Za-z0-9]+)+$') {
        continue
    }

    $destination = Join-Path $languages $directory.Name
    if (Test-Path -LiteralPath $destination) {
        Remove-Item -LiteralPath $destination -Recurse -Force
    }

    if ($requiredRootLanguages -contains $directory.Name) {
        Copy-Item -LiteralPath $directory.FullName -Destination $destination -Recurse -Force
    }
    else {
        Move-Item -LiteralPath $directory.FullName -Destination $destination -Force
    }
}

$movableDllPatterns = @(
    "DirectML.dll",
    "onnxruntime*.dll",
    "Microsoft.ML.OnnxRuntime.dll",
    "System.Numerics.Tensors.dll",
    "Microsoft.Windows.AI*.dll",
    "Microsoft.Graphics.Imaging*.dll",
    "Microsoft.Web.WebView2*.dll",
    "Microsoft.Windows.Widgets*.dll",
    "Microsoft.Windows.Workloads*.dll",
    "Microsoft.Windows.AppNotifications*.dll",
    "Microsoft.Windows.PushNotifications*.dll",
    "Microsoft.Windows.BadgeNotifications*.dll"
)

foreach ($pattern in $movableDllPatterns) {
    foreach ($file in Get-ChildItem -Path $root -Filter $pattern -File -ErrorAction SilentlyContinue) {
        Move-Item -LiteralPath $file.FullName -Destination (Join-Path $dlls $file.Name) -Force
    }
}
