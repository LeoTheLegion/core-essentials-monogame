# Generates a small button-plate sprite used by the themed-button demo in GuiAnchorDemo.xml.
# Produces a 160x56 PNG with a vertical gradient fill and a thin border, sized to read as a
# clickable plate. Run from the repo root:  ./scripts/make-button-plate.ps1
param(
    [string]$Out = "CoreEssentials.Playground/Content/Sprites/ButtonPlate.png"
)

Add-Type -AssemblyName System.Drawing

$W = 160
$H = 56
$img = [System.Drawing.Bitmap]::new($W, $H)
$g = [System.Drawing.Graphics]::FromImage($img)
$g.SmoothingMode = 'AntiAlias'
$g.InterpolationMode = 'HighQualityBicubic'

# Vertical gradient fill (top slightly lighter than bottom) to give the plate a subtle bevel.
$topColor = [System.Drawing.Color]::FromArgb(255, 70, 130, 200)
$bottomColor = [System.Drawing.Color]::FromArgb(255, 40, 90, 160)
$rect = [System.Drawing.Rectangle]::new(1, 1, $W - 2, $H - 2)
$brush = [System.Drawing.Drawing2D.LinearGradientBrush]::new($rect, $topColor, $bottomColor, [float]90.0)
$g.FillRectangle($brush, $rect)

# Thin inner border to make the plate edge read against any backdrop.
$borderPen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(255, 180, 210, 255), 2)
$g.DrawRectangle($borderPen, 0, 0, $W - 1, $H - 1)

$g.Dispose()
$img.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png)
$img.Dispose()

Write-Host "Wrote $Out ($W x $H)"
