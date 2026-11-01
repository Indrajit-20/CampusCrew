# Update-SourceCode.ps1
# Automates updating CampusCrew_Source_Code.txt with all current source code files.

$ErrorActionPreference = "Stop"
$repoDir = $PSScriptRoot
if (-not $repoDir) { $repoDir = Get-Location }

$outputFile = Join-Path $repoDir "CampusCrew_Source_Code.txt"
$allowedExtensions = @('.cs', '.cshtml', '.css', '.js', '.json', '.csproj', '.md')
$excludeDirs = @('.git', '.vs', 'bin', 'obj')
$excludeFiles = @('CampusCrew_Source_Code.txt', 'Update-SourceCode.ps1', 'Update-SourceCode.bat')

Write-Host "Scanning files in $repoDir..." -ForegroundColor Cyan

$files = Get-ChildItem -Path $repoDir -Recurse -File | Where-Object {
    $item = $_
    $ext = $item.Extension.ToLowerInvariant()
    $name = $item.Name

    # Check extension
    if ($allowedExtensions -notcontains $ext) { return $false }
    # Skip sourcemaps
    if ($name.EndsWith(".map")) { return $false }
    # Skip excluded files
    if ($excludeFiles -contains $name) { return $false }
    if ($name.EndsWith(".user")) { return $false }

    # Check directories to skip
    $relativePath = $item.FullName.Substring($repoDir.Length)
    $parts = $relativePath.Split([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    foreach ($part in $parts) {
        if ($excludeDirs -contains $part) { return $false }
    }

    return $true
} | Sort-Object FullName

Write-Host "Found $($files.Count) files. Writing to $outputFile..." -ForegroundColor Cyan

$utf8WithBom = New-Object System.Text.UTF8Encoding($true)
$writer = New-Object System.IO.StreamWriter($outputFile, $false, $utf8WithBom)

try {
    $writer.WriteLine("PROJECT: CampusCrew`r`n")

    foreach ($file in $files) {
        $rel = $file.FullName.Substring($repoDir.Length)
        if (-not $rel.StartsWith("\")) {
            $rel = "\" + $rel
        }

        $writer.WriteLine("========================================")
        $writer.WriteLine("FILE: $rel")
        $writer.WriteLine("========================================")

        $content = [System.IO.File]::ReadAllText($file.FullName)
        $writer.WriteLine($content)
        $writer.WriteLine("`r`n")
    }
}
finally {
    $writer.Dispose()
}

Write-Host "Done! Successfully updated $outputFile ($($files.Count) files)." -ForegroundColor Green

