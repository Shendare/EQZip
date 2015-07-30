EQ-Zip EverQuest Archive Manager
-----------

Current Version: 1.3

Last Updated: 7/30/2015

Github Link: https://github.com/Shendare/EQZip

To Download: https://github.com/Shendare/EQZip/releases

#Features:

* Creates, Loads and Saves .S3D, .EQG, .PFS, and .PAK EverQuest package files

* Thumbnails of all supported texture types (RGB16/24/32, DXT1/2/3/4/5, V8U8)

* Automatically converts textures to .dds with mipmaps when importing (toggle)

* Automatically converts textures to .png, .gif, .bmp, or jpg when exporting (toggle)

* Drag-and-drop files into or out of an archive and Windows Explorer, or between EQ-Zip windows!

* Cut, Copy, and Paste files between archives and Windows Explorer, or each other

* Select a texture and choose Replace... to easily browse for a new texture to swap it out with

* Or just drag and drop a new texture from Windows onto an existing one in the archive!

* Import/Export/Replace textures via: Application Menu, Toolbar Button, or Right-click context menu

* Export all files in an archive to a destination folder

* Recent menu feature to remember the last 9 archives worked with (toggle)

* .Net 3.5 compatible, but archives will compress about 30% smaller when compiled to .Net 4.5

#Screenshots:

>https://raw.githubusercontent.com/Shendare/EQZip/master/screenshots/screenshot1.png

>https://raw.githubusercontent.com/Shendare/EQZip/master/screenshots/screenshot2.png

>https://raw.githubusercontent.com/Shendare/EQZip/master/screenshots/screenshot3.png

>https://raw.githubusercontent.com/Shendare/EQZip/master/screenshots/screenshot4.png

>https://raw.githubusercontent.com/Shendare/EQZip/master/screenshots/screenshot5.png

#Disclaimer:

>EQ-Zip is not affiliated with, endorsed by, approved by, or in any way associated with Daybreak Games, the EverQuest franchise, or any of the other compression/archive based applications out there with the word "Zip" in them, who reserve all copyrights and trademarks to their properties.

#License:

>Portions of this software's code not covered by another author's or entity's copyright are released under the Creative Commons Zero (CC0) public domain license.

>To the extent possible under law, Shendare (Jon D. Jackson) has waived all copyright and related or neighboring rights to this EQ-Zip application. This work is published from: The United States.

>You may copy, modify, and distribute the work, even for commercial purposes, without asking permission.

>For more information, read the CC0 summary and full legal text here:

>https://creativecommons.org/publicdomain/zero/1.0/

#Credits:

>EverQuest game package file format determined from examination of the Delphi code in S3DSpy, by Windcatcher, without whose work this would be impossible.

>http://sourceforge.net/projects/eqemulator/files/OpenZone/S3DSpy%201.3/

>DDS texture file parsing rebuilt from code by Lorenzo Consolaro. (MIT License)

>https://code.google.com/p/kprojects/

>TGA image loading class from David Polomis. (CPOL 1.02)

>http://www.codeproject.com/Articles/31702/NET-Targa-Image-Reader

>Virtual Windows Shell File handling functionality thanks to David Anson (MIT License)

>https://dlaa.me/blog/post/9923072

>Icons made from the freeware non-commercial "Aqua Neue (Graphite)" pack.

#Release Notes:

7/30/2015 - Version 1.3

* Added feature to easily horizontally or vertically flip an image to get it to show up properly in-game if the geometry
  expects the texture to be stored in some way besides left-to-right, top-to-bottom.

* Added help descriptions to the Preferences window for choosing Import and Export auto-conversion options.

* Corrected 24/32-bit to 16-bit color scaling. NB: Integers truncate; they don't round.

* DDS Import Format - Auto

By default, importing graphics files to an EQ-Zip archive will automatically convert them to a .dds texture with mipmaps.
The default .dds format is now "Auto", which tells EQ-Zip to pick the best format for the new texture based on the graphics
file's use or absence of an alpha channel.

  * All pixels fully opaque -> RGB16 - (R5G6B5)
  * All pixels fully transparent or opaque -> ARGB16 (A1R5G5B5)
  * Some pixels partially transparent -> RGB32 (A8R8G8B8)

This yields the most compact format (without lossy compression) for any texture, while preserving good color and alpha channels.

If you do not like Auto, you can specify how many bits-per-pixel to import .dds files at (16, 24, or 32), or deactivate
auto-conversion altogether.

This completes my planned features. If you think of a new feature you want to see, or come across a bug, let me know
at Shendare at Shendare DotNet.

Enjoy customizing your EverQuest experience with EQ-Zip!

- Shendare (Jon D. Jackson)

-----------

Former Release Notes:

7/29/2015 - Version 1.2

* Noticeable improvement in quality of texture mipmaps with high quality bicubic sampling
* Added full support for 16-bit DDS textures (auto-sensing A1R5G5B5 or R5G6B5)
* Set default import format to RGB16
* Code cleanup (Settings -> Util.cs)

7/28/2015 - Version 1.1

* DXT1 Decompression - Improved Slightly
* DXT2 Decompression - Enabled
* DXT3 Decompression - Added
* DXT4 Decompression - Enabled
* DXT5 Decompression - Improved Significantly
* V8U8 Decompression - Improved Significantly

7/25/2015 - Version 1.0

* Initial Release
