param([string]$Version = '1.1.3.2', [string]$Output = 'artifacts')
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
Push-Location $repo
try {
    dotnet publish Emby.Plugin.Danmu/Emby.Plugin.Danmu.csproj -c Release -p:DisableFody=true -p:Version=$Version -p:CopyLocalLockFileAssemblies=true
    if ($LASTEXITCODE) { throw 'Build failed' }
    $toolDir = Join-Path $repo '.tools/ilrepack'
    if (-not (Test-Path $toolDir)) {
        dotnet tool install dotnet-ilrepack --version 1.0.2 --tool-path $toolDir
        if ($LASTEXITCODE) { throw 'ILRepack installation failed' }
    }
    $publish = Join-Path $repo 'Emby.Plugin.Danmu/bin/Release/netstandard2.0/publish'
    $destination = [IO.Path]::GetFullPath((Join-Path $repo $Output))
    New-Item -ItemType Directory -Path $destination -Force | Out-Null
    $packages = ((dotnet nuget locals global-packages --list) -replace '^global-packages: ', '').Trim()
    $refs = Join-Path $packages 'netstandard.library/2.0.3/build/netstandard2.0/ref'
    & (Join-Path $toolDir 'ilrepack') /internalize "/lib:$publish" "/lib:$refs" "/out:$destination/Emby.Plugin.Danmu.dll" "$publish/Emby.Plugin.Danmu.dll" "$publish/Google.Protobuf.dll"
    if ($LASTEXITCODE) { throw 'Assembly merge failed' }
    Compress-Archive -LiteralPath "$destination/Emby.Plugin.Danmu.dll" -DestinationPath "$destination/danmu_$Version.zip" -Force
} finally { Pop-Location }
