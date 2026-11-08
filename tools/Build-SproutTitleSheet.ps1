$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = Join-Path $PSScriptRoot '../PawnShop.Game/Content/TitleScreen'
$sheet = New-Object System.Drawing.Bitmap 272,1520
$original = [System.Drawing.Bitmap]::FromFile((Join-Path $root 'sprout-title.png'))
function Copy-Frame($image, $sourceX, $sourceY, $width, $height, $index) {
    $left = ($index % 4) * 68 + (68 - $width) / 2
    $top = [Math]::Floor($index / 4) * 80 + (80 - $height) / 2
    for ($y = 0; $y -lt $height; $y++) {
        for ($x = 0; $x -lt $width; $x++) {
            $sheet.SetPixel($left + $x, $top + $y, $image.GetPixel($sourceX + $x, $sourceY + $y))
        }
    }
}
try {
    $clips = @(
        @{ Name='floating-point-right'; First=0; Count=13; Width=68; Height=76 },
        @{ Name='point-to-fast-flight'; First=32; Count=13; Width=68; Height=76 },
        @{ Name='menu-fast-jet-flight'; First=45; Count=17; Width=60; Height=68 },
        @{ Name='fast-flight-to-point'; First=62; Count=13; Width=68; Height=80 }
    )
    foreach ($clip in $clips) {
        for ($i = 0; $i -lt $clip.Count; $i++) {
            $frame = [System.Drawing.Bitmap]::FromFile((Join-Path $root "Sources/sprout-$($clip.Name)/frame_$i.png"))
            try {
                if ($frame.Width -ne $clip.Width -or $frame.Height -ne $clip.Height) { throw "Unexpected dimensions: $($clip.Name)/$i" }
                Copy-Frame $frame 0 0 $frame.Width $frame.Height ($clip.First + $i)
            } finally { $frame.Dispose() }
        }
    }
    for ($i = 16; $i -lt 32; $i++) {
        Copy-Frame $original (($i % 4) * 48) ([Math]::Floor($i / 4) * 48) 48 48 $i
    }
    $sheet.Save((Join-Path $root 'sprout-title-menu.png'), [System.Drawing.Imaging.ImageFormat]::Png)
} finally { $sheet.Dispose(); $original.Dispose() }
