param(
    [string]$AssetRoot = (Join-Path $PSScriptRoot '../PawnShop.Game/Content/Player/Bases')
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$manifest = Get-Content -Raw -LiteralPath (Join-Path $AssetRoot 'base-manifest.json') | ConvertFrom-Json
$width = $manifest.canvas.width
$height = $manifest.canvas.height
$poses = 0
foreach ($sheet in $manifest.sheets) {
    $image = [System.Drawing.Bitmap]::new((Join-Path $AssetRoot $sheet.file))
    try {
        if ($image.Width -ne $width * $manifest.directions.Count -or $image.Height -ne $height) {
            throw "$($sheet.file): unexpected atlas dimensions."
        }
        for ($frame = 0; $frame -lt $manifest.directions.Count; $frame++) {
            $minX = $width; $maxX = -1; $minY = $height; $maxY = -1
            for ($y = 0; $y -lt $height; $y++) {
                for ($x = 0; $x -lt $width; $x++) {
                    $color = $image.GetPixel($frame * $width + $x, $y)
                    if ($color.A -ne 0 -and $color.A -ne 255) { throw "$($sheet.file): partial alpha in frame $frame." }
                    if ($color.A -eq 0) { continue }
                    if ($x -eq 0 -or $x -eq $width - 1 -or $y -eq 0 -or $y -eq $height - 1) {
                        throw "$($sheet.file): frame $frame touches its border."
                    }
                    $minX = [Math]::Min($minX, $x); $maxX = [Math]::Max($maxX, $x)
                    $minY = [Math]::Min($minY, $y); $maxY = [Math]::Max($maxY, $y)
                }
            }
            if ($minY -ne $manifest.baseBounds.top -or $maxY -ne $manifest.baseBounds.bottom -or
                $maxY - $minY + 1 -ne $manifest.visibleBodyHeight.target -or $maxY + 1 -ne $manifest.pivot.y -or
                [Math]::Abs(($minX + $maxX) / 2.0 - ($manifest.pivot.x - 0.5)) -gt 0.5) {
                throw "$($sheet.file): frame $frame violates the height, centering or ground-pivot contract."
            }
            $poses++
        }
    }
    finally { $image.Dispose() }
}
foreach ($file in $manifest.files) {
    $image = [System.Drawing.Bitmap]::new((Join-Path $AssetRoot $file))
    $image.Dispose()
}
Write-Output "PASS: $poses canonical base poses have 48x64 cells, 50-pixel body height, clear borders, binary alpha and pivot (24,58)."
Write-Output "PASS: all $($manifest.files.Count) base/source PNGs decode. Visual acceptance remains pose-specific."
