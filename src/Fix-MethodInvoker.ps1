$ErrorActionPreference = 'Stop'

$sourcePath = Join-Path (Get-Location) 'MainForm.cs'
if (-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
    throw 'MainForm.cs not found. Run this script from the PrototypeUltimateTool project root.'
}

$content = [System.IO.File]::ReadAllText($sourcePath)
# Qualify only unqualified MethodInvoker references; already-qualified names remain untouched.
$pattern = '(?<![\w.])MethodInvoker\b'
$matches = [regex]::Matches($content, $pattern)

if ($matches.Count -eq 0) {
    Write-Host 'No unqualified MethodInvoker references found. The file may already be fixed.'
    exit 0
}

$updated = [regex]::Replace($content, $pattern, 'System.Windows.Forms.MethodInvoker')
$backupPath = "$sourcePath.methodinvoker.bak"
if (-not (Test-Path -LiteralPath $backupPath)) {
    Copy-Item -LiteralPath $sourcePath -Destination $backupPath
}

# Preserve UTF-8 BOM convention used by Visual Studio C# source files.
$utf8Bom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($sourcePath, $updated, $utf8Bom)
Write-Host "Qualified $($matches.Count) MethodInvoker reference(s) in MainForm.cs."
Write-Host "Backup: $backupPath"
