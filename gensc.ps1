$scriptDirectory = Split-Path -Path $MyInvocation.MyCommand.Path -Parent
$exePath = Join-Path -Path $scriptDirectory -ChildPath 'bin\Debug\net10.0-windows\AppLauncher.exe'
$iconPath = Join-Path -Path $scriptDirectory -ChildPath 'image\icon-app.ico'

if (-not (Test-Path -LiteralPath $exePath)) {
    throw "Executable not found: $exePath"
}

$desktopDirectory = [Environment]::GetFolderPath('DesktopDirectory')
$shortcutName = '{0}.lnk' -f [System.IO.Path]::GetFileNameWithoutExtension($exePath)
$shortcutPath = Join-Path -Path $desktopDirectory -ChildPath $shortcutName
$workingDirectory = Split-Path -Path $exePath -Parent

if (Test-Path -LiteralPath $shortcutPath) {
    Remove-Item -LiteralPath $shortcutPath -Force
}

$iconLocation = if (Test-Path -LiteralPath $iconPath) {
    '{0},0' -f $iconPath
} else {
    '{0},0' -f $exePath
}

$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $exePath
$shortcut.WorkingDirectory = $workingDirectory
$shortcut.IconLocation = $iconLocation
$shortcut.Description = 'AppLauncher'
$shortcut.Save()

Write-Host "Shortcut created on Desktop: $shortcutPath"
