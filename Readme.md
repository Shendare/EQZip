EQ-Zip EverQuest Archive Manager
-----------

Current Version: 1.1

Last Updated: 7/28/2015

Github Link: https://github.com/Shendare/EQZip

#Features:

* Creates, Loads and Saves .S3D, .EQG, .PFS, and .PAK EverQuest package files

* Thumbnails of all supported texture types (RGB24, RGB32, DXT1/2/3/4/5, V8U8)

* Automatically converts textures to .dds uncompressed when importing (toggle)

* Automatically converts textures to .png, .dds, .gif, .bmp, or jpg when exporting (toggle)

* Drag-and-drop files into or out of an archive and Windows Explorer, or between EQ-Zip windows!

* Cut, Copy, and Paste files between archives and Windows Explorer, or each other

* Select a texture and choose Replace... to easily browse for a new texture to swap it out with

* Or just drag and drop a new texture from Windows onto an existing one in the archive!

* Import/Export/Replace textures via: Application Menu, Toolbar Button, or Right-click context menu

* Export all files in an archive to a destination folder

* Recent menu feature to remember the last 9 archives worked with (toggle)

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

#Known Issues and Planned Updates:

* DDS conversion does not support compression. This is intended. DXT compression is very lossy ( see http://www.fsdeveloper.com/wiki/index.php?title=DXT_compression_explained ), and graphics cards have plenty of video memory for uncompressed textures these days.

* Support for reading and writing 16-bit uncompressed DDS textures is planned, as it would cut image sizes in half with much lower loss of definition than DXT compression.

* A feature will be added to vertically flip a texture, as some in-game geometry expects a texture to be bottom-up, and it can be hard to tell until you see it in-game.

* It's a little strange that there is a separate EQArchive class and PFSFormat class to handle EQ package files. When I began the project, I was under the mistaken impression that S3D and EQG files used different file formats for storing their contents, so I had a PFSArchives.cs and an EQGArchives.cs both tied to EQArchive.cs. When I learned that both used the same format, I removed EQGArchives.cs, but because the remaining two classes are working fine together, I have not merged them at this point.

#Release Notes:

7/28/2015 - Version 1.1

* DXT1 Decompression - Improved Slightly
* DXT2 Decompression - Enabled
* DXT3 Decompression - Added
* DXT4 Decompression - Enabled
* DXT5 Decompression - Improved Significantly
* V8U8 Decompression - Improved Significantly

7/25/2015 - Version 1.0

* Initial Release
