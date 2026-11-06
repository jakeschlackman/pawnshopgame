param(
    [string]$AssetRoot = (Join-Path $PSScriptRoot '../PawnShop.Game/Content/Player/Wearables/TealWorkShirt')
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$manifest = Get-Content -Raw -LiteralPath (Join-Path $AssetRoot 'wearable.json') | ConvertFrom-Json
$baseManifest = Get-Content -Raw -LiteralPath (Join-Path $AssetRoot $manifest.baseManifest) | ConvertFrom-Json
if ($baseManifest.directions[0] -ne 'south') { throw 'Expected south as the first base atlas cell.' }
$pixelCount = 0
foreach ($entry in $manifest.downloads) {
    $image = [System.Drawing.Bitmap]::new((Join-Path $AssetRoot $entry.path))
    try {
        $expectedWidth = 48
        $expectedHeight = 64
        if ($entry.path -eq 'review-native.png') { $expectedWidth = 192; $expectedHeight = 128 }
        if ($entry.path -eq 'review-4x.png') { $expectedWidth = 768; $expectedHeight = 512 }
        if ($image.Width -ne $expectedWidth -or $image.Height -ne $expectedHeight) {
            throw "$($entry.path): unexpected dimensions $($image.Width)x$($image.Height)."
        }
    }
    finally { $image.Dispose() }
}

foreach ($sex in @('male', 'female')) {
    $images = @{}
    try {
        foreach ($layer in @('shirt', 'preview')) {
            $images[$layer] = [System.Drawing.Bitmap]::new((Join-Path $AssetRoot "$sex/$layer.png"))
        }
        $images.base = [System.Drawing.Bitmap]::new((Join-Path $AssetRoot $manifest.bases.$sex.atlas))
        $top = 64
        $bottom = -1
        $shirtPixels = 0
        $garmentTop = 23
        $garmentBottom = 41
        if ($manifest.garmentBounds -and $manifest.garmentBounds.$sex) {
            $garmentTop = $manifest.garmentBounds.$sex.top
            $garmentBottom = $manifest.garmentBounds.$sex.bottom
        }
        for ($y = 0; $y -lt 64; $y++) {
            for ($x = 0; $x -lt 48; $x++) {
                $base = $images.base.GetPixel($x, $y)
                $shirt = $images.shirt.GetPixel($x, $y)
                $preview = $images.preview.GetPixel($x, $y)
                foreach ($color in @($base, $shirt, $preview)) {
                    if ($color.A -ne 0 -and $color.A -ne 255) { throw "${sex}: partial alpha at ($x,$y)." }
                }
                if ($base.A -gt 0) { $top = [Math]::Min($top, $y); $bottom = [Math]::Max($bottom, $y) }
                if ($shirt.A -gt 0) {
                    $shirtPixels++
                    if ($y -lt $garmentTop -or $y -gt $garmentBottom -or $x -eq 0 -or $x -eq 47) {
                        throw "${sex}: garment outside the reviewed torso area at ($x,$y)."
                    }
                }
                $expected = $base
                if ($shirt.A -gt 0) { $expected = $shirt }
                if ($preview.A -ne $expected.A -or ($preview.A -gt 0 -and $preview.ToArgb() -ne $expected.ToArgb())) {
                    throw "${sex}: preview is not base plus shirt at ($x,$y)."
                }
                $pixelCount++
            }
        }
        if ($top -ne 8 -or $bottom -ne 57 -or $shirtPixels -eq 0) {
            throw "${sex}: unexpected body bounds or empty shirt layer."
        }
        if ($manifest.shoulderCoverage -and $manifest.shoulderCoverage.$sex) {
            foreach ($point in $manifest.shoulderCoverage.$sex) {
                $color = $images.shirt.GetPixel($point[0], $point[1])
                if ($color.A -ne 255 -or ($color.R -gt 64 -and $color.R -gt $color.G + 15 -and $color.R -gt $color.B + 15)) {
                    throw "${sex}: exposed or skin-colored shoulder at ($($point[0]),$($point[1]))."
                }
            }
        }
        if ($manifest.coverageRegions -and $manifest.coverageRegions.$sex) {
            foreach ($region in $manifest.coverageRegions.$sex) {
                for ($y = $region[1]; $y -le $region[3]; $y++) {
                    for ($x = $region[0]; $x -le $region[2]; $x++) {
                        if ($images.shirt.GetPixel($x, $y).A -ne 255) {
                            throw "${sex}: uncovered underlying garment at ($x,$y)."
                        }
                    }
                }
            }
        }
    }
    finally { foreach ($image in $images.Values) { $image.Dispose() } }
}

Write-Output "PASS: all $($manifest.downloads.Count) PNGs decode at their documented sizes."
Write-Output 'PASS: canonical south-facing bases retain 50-pixel height, rows 8-57.'
Write-Output "PASS: $pixelCount preview pixels equal unchanged base plus separate shirt; binary alpha and face preservation verified."
if ($manifest.shoulderCoverage) { Write-Output 'PASS: all required shoulder pixels and underlying-garment regions are covered.' }
Write-Output 'Visual approval is recorded separately in wearable.json; numeric checks do not grant it.'
