# SEWorldGenPlugin

This is a plugin for the game Space Engineers, which adds random star system
generation to worlds aswell as some other neat features. More info in the [wiki](https://github.com/thorwin99/SEWorldGenPlugin/wiki)

## Installation

To install the plugin, download the .dll file. Then go to your Space Engineers installation folder, by right clicking it in the Steam Library, clicking properties and then under local files, browse local files. You then want to create a new folder called 'Plugins' and copy the plugin dll into it. Now go back to Steam and into the properties of space engineers. You want to click Set launch options and add 

`-plugin "../Plugins/SEWorldGenPlugin.dll"` 

to it. With that, the game will be launched with the plugin installed. It will likely crash on first launch. For more info, look into the troubleshooting section of the installation page in the wiki.

# Troubleshooting

## Crash on startup

Some people encounter an issue on startup, that the game crashes, when this plugin is loaded. A fix for that is to right click the .dll and go into properties, and there unblock it. The field for that is at the bottom of the properties window.

## Crash due to full ram

This is probably caused by a memory leak that is present in space engineers, where it does not clean up voxel storage, that was closed or deleted. It only happens, because the plugin uses a much higher density for asteroid rings and fields by default. However, you can set the density in the configuration or in the world settings of the plugin.

## More Info

This readme only contains some information about the installation. The wiki contains more help and info. [Wiki](https://github.com/thorwin99/SEWorldGenPlugin/wiki/Installation)
