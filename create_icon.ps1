# Script pour créer une icône pour Windows Cleaner
Add-Type -AssemblyName System.Drawing

# Créer une image 256x256
$size = 256
$bitmap = New-Object System.Drawing.Bitmap($size, $size)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias

# Fond dégradé bleu
$rect = New-Object System.Drawing.Rectangle(0, 0, $size, $size)
$brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    $rect,
    [System.Drawing.Color]::FromArgb(30, 144, 255),  # Bleu clair
    [System.Drawing.Color]::FromArgb(0, 82, 204),    # Bleu foncé
    45
)
$graphics.FillEllipse($brush, 10, 10, $size-20, $size-20)

# Bordure
$pen = New-Object System.Drawing.Pen([System.Drawing.Color]::White, 8)
$graphics.DrawEllipse($pen, 10, 10, $size-20, $size-20)

# Dessiner un symbole de balai/nettoyage
$whitePen = New-Object System.Drawing.Pen([System.Drawing.Color]::White, 12)
$whitePen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$whitePen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round

# Manche du balai
$graphics.DrawLine($whitePen, 80, 180, 150, 80)

# Brosse du balai
$brushPen = New-Object System.Drawing.Pen([System.Drawing.Color]::White, 20)
$brushPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$graphics.DrawLine($brushPen, 60, 190, 100, 200)
$graphics.DrawLine($brushPen, 70, 200, 110, 210)

# Étincelles de propreté
$sparkPen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 255, 150), 6)
$sparkPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
$sparkPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
$graphics.DrawLine($sparkPen, 160, 90, 180, 70)
$graphics.DrawLine($sparkPen, 170, 100, 190, 100)
$graphics.DrawLine($sparkPen, 150, 110, 165, 125)

# Sauvegarder en PNG d'abord
$pngPath = Join-Path $PSScriptRoot "app_temp.png"
$bitmap.Save($pngPath, [System.Drawing.Imaging.ImageFormat]::Png)

Write-Host "Image PNG créée: $pngPath"
Write-Host "Pour convertir en ICO, utilisez un outil en ligne comme:"
Write-Host "  - https://convertio.co/fr/png-ico/"
Write-Host "  - https://www.icoconverter.com/"
Write-Host "Ou installez ImageMagick: magick convert app_temp.png -define icon:auto-resize=256,128,64,48,32,16 app.ico"

# Nettoyer
$graphics.Dispose()
$bitmap.Dispose()
$brush.Dispose()
$pen.Dispose()
$whitePen.Dispose()
$brushPen.Dispose()
$sparkPen.Dispose()

Write-Host "`nPNG créé avec succès!"
