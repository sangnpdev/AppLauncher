$exePath = 'D:\Program\AppLauncher\bin\Debug\net10.0-windows\AppLauncher.exe'

if (-not (Test-Path -LiteralPath $exePath)) {
    throw "Executable not found: $exePath"
}

$shortcutDirectory = Split-Path -Path $exePath -Parent
$shortcutName = '{0}.lnk' -f [System.IO.Path]::GetFileNameWithoutExtension($exePath)
$shortcutPath = Join-Path -Path $shortcutDirectory -ChildPath $shortcutName

if (Test-Path -LiteralPath $shortcutPath) {
    Remove-Item -LiteralPath $shortcutPath -Force
}

$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $exePath
$shortcut.WorkingDirectory = $shortcutDirectory
$shortcut.IconLocation = $exePath
$shortcut.Description = 'AppLauncher'
$shortcut.Save()

Write-Host "Shortcut created: $shortcutPath"
