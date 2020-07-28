# This script takes our single source image and generates all the icon files in all the sizes required by the Android app.

if (-Not (Get-Module -Name Resize-Image)) {
    Import-Module -Name ".\Resize-Image.psm1"
}

$BaseDir        = "..\..\RoosterBlock.Android\Resources" | Resolve-Path
$InputFile      = Join-Path -Path $BaseDir -ChildPath "drawable\RoosterBlockIconv1point5.png"

Resize-Image -InputFile $InputFile -OutputFile (Join-Path -Path $BaseDir -ChildPath "mipmap-hdpi\icon.png")                   -Width  72 -Height  72
Resize-Image -InputFile $InputFile -OutputFile (Join-Path -Path $BaseDir -ChildPath "mipmap-hdpi\launcher_foreground.png")    -Width 162 -Height 162
                                                                                                                              
Resize-Image -InputFile $InputFile -OutputFile (Join-Path -Path $BaseDir -ChildPath "mipmap-mdpi\icon.png")                   -Width  48 -Height  48
Resize-Image -InputFile $InputFile -OutputFile (Join-Path -Path $BaseDir -ChildPath "mipmap-mdpi\launcher_foreground.png")    -Width 108 -Height 108

Resize-Image -InputFile $InputFile -OutputFile (Join-Path -Path $BaseDir -ChildPath "mipmap-xhdpi\icon.png")                  -Width  96 -Height  96
Resize-Image -InputFile $InputFile -OutputFile (Join-Path -Path $BaseDir -ChildPath "mipmap-xhdpi\launcher_foreground.png")   -Width 216 -Height 216

Resize-Image -InputFile $InputFile -OutputFile (Join-Path -Path $BaseDir -ChildPath "mipmap-xxhdpi\icon.png")                 -Width 144 -Height 144
Resize-Image -InputFile $InputFile -OutputFile (Join-Path -Path $BaseDir -ChildPath "mipmap-xxhdpi\launcher_foreground.png")  -Width 324 -Height 324

Resize-Image -InputFile $InputFile -OutputFile (Join-Path -Path $BaseDir -ChildPath "mipmap-xxxhdpi\icon.png")                -Width 192 -Height 192
Resize-Image -InputFile $InputFile -OutputFile (Join-Path -Path $BaseDir -ChildPath "mipmap-xxxhdpi\launcher_foreground.png") -Width 432 -Height 432

Remove-Module -Name Resize-Image
