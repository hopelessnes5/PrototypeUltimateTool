param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$solution = Join-Path $projectRoot 'PrototypeUltimateTool.sln'
$outputDir = Join-Path $projectRoot "bin\$Configuration\net48"

$msbuild = Get-Command 'msbuild.exe' -ErrorAction SilentlyContinue
if (-not $msbuild) {
    $msbuild = Get-Command 'msbuild' -ErrorAction SilentlyContinue
}
if (-not $msbuild) {
    throw 'MSBuild не найден. Запустите этот скрипт из Developer PowerShell for Visual Studio с установленным .NET Framework 4.8 Developer Pack.'
}

& $msbuild.Source $solution /t:Rebuild /m "/p:Configuration=$Configuration" '/p:Platform=Any CPU'
if ($LASTEXITCODE -ne 0) {
    throw "Сборка завершилась с кодом $LASTEXITCODE."
}

$exe = Join-Path $outputDir 'PrototypeUltimateTool.exe'
if (-not (Test-Path $exe)) {
    throw "EXE не найден по ожидаемому пути: $exe"
}

$dlls = @(Get-ChildItem -Path $outputDir -Filter '*.dll' -File -ErrorAction SilentlyContinue)
if ($dlls.Count -gt 0) {
    $names = ($dlls | ForEach-Object { $_.Name }) -join ', '
    throw "В выходной папке остались внешние DLL: $names"
}

Write-Host "Готово: $exe"
Write-Host 'Проверка пройдена: рядом с EXE нет DLL-файлов.'
