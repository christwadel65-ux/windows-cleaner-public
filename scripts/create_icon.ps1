# Script simplifié pour créer une icône basique
try {
    Add-Type -AssemblyName System.Drawing
    
    $size = 256
    $bitmap = New-Object System.Drawing.Bitmap($size, $size)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = 'AntiAlias'
    $graphics.Clear([System.Drawing.Color]::Transparent)
    
    # Cercle bleu
    $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(0, 120, 215))
    $graphics.FillEllipse($brush, 20, 20, 216, 216)
    
    # Bordure blanche
    $pen = New-Object System.Drawing.Pen([System.Drawing.Color]::White, 8)
    $graphics.DrawEllipse($pen, 20, 20, 216, 216)
    
    # Symbole de nettoyage (éclair stylisé)
    $whiteBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
    $points = @(
        [System.Drawing.Point]::new(140, 70),
        [System.Drawing.Point]::new(110, 128),
        [System.Drawing.Point]::new(130, 128),
        [System.Drawing.Point]::new(100, 186),
        [System.Drawing.Point]::new(130, 140),
        [System.Drawing.Point]::new(110, 140)
    )
    $graphics.FillPolygon($whiteBrush, $points)
    
    # Sauvegarder
    $bitmap.Save("$PSScriptRoot\app.png", [System.Drawing.Imaging.ImageFormat]::Png)
    
    # Créer des tailles multiples pour l'icône
    $icon16 = New-Object System.Drawing.Bitmap(16, 16)
    $g16 = [System.Drawing.Graphics]::FromImage($icon16)
    $g16.InterpolationMode = 'HighQualityBicubic'
    $g16.DrawImage($bitmap, 0, 0, 16, 16)
    $icon16.Save("$PSScriptRoot\app_16.png", [System.Drawing.Imaging.ImageFormat]::Png)
    
    $icon32 = New-Object System.Drawing.Bitmap(32, 32)
    $g32 = [System.Drawing.Graphics]::FromImage($icon32)
    $g32.InterpolationMode = 'HighQualityBicubic'
    $g32.DrawImage($bitmap, 0, 0, 32, 32)
    $icon32.Save("$PSScriptRoot\app_32.png", [System.Drawing.Imaging.ImageFormat]::Png)
    
    $icon48 = New-Object System.Drawing.Bitmap(48, 48)
    $g48 = [System.Drawing.Graphics]::FromImage($icon48)
    $g48.InterpolationMode = 'HighQualityBicubic'
    $g48.DrawImage($bitmap, 0, 0, 48, 48)
    $icon48.Save("$PSScriptRoot\app_48.png", [System.Drawing.Imaging.ImageFormat]::Png)
    
    Write-Host "✓ Images PNG créées avec succès!" -ForegroundColor Green
    Write-Host "`nPour créer le fichier .ico, utilisez un convertisseur en ligne:"
    Write-Host "  1. Allez sur https://convertio.co/fr/png-ico/"
    Write-Host "  2. Uploadez app.png"
    Write-Host "  3. Téléchargez app.ico"
    Write-Host "`nOu utilisez ImageMagick si installé:"
    Write-Host "  magick app.png -define icon:auto-resize=256,128,64,48,32,16 app.ico"
    
    $graphics.Dispose()
    $bitmap.Dispose()
    $g16.Dispose()
    $g32.Dispose()
    $g48.Dispose()
    $icon16.Dispose()
    $icon32.Dispose()
    $icon48.Dispose()
    $brush.Dispose()
    $whiteBrush.Dispose()
    $pen.Dispose()
}
catch {
    Write-Host "Erreur: $_" -ForegroundColor Red
}
