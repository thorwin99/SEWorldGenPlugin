# SEWorldGenPlugin

This is a plugin for the game Space Engineers, which adds random star system
generation to worlds aswell as some other neat features. More info in the [wiki](https://github.com/thorwin99/SEWorldGenPlugin/wiki)

## Installation

For installation instructions, please refer to the wiki [here](https://github.com/thorwin99/SEWorldGenPlugin/wiki/Installation)

## Important info for servers

This plugin is not really optimised for server usage. The planetary generation function will always work, however the custom asteroid generation
can cause problems, if you have high player counts on that server. It is possible to run the plugin on those servers, however it will be very performance
intensive, since it uses a slight hack to get asteroids to generate. You can see the details of the issue with large player counts and servers in the issues
[#109](https://github.com/thorwin99/SEWorldGenPlugin/issues/109) and [#80](https://github.com/thorwin99/SEWorldGenPlugin/issues/80). You can always disable the asteroid generation and use the vanilla one, if you only want to generate the planets and gps for them.

# Troubleshooting

## Crash on startup

Some people encounter an issue on startup, that the game crashes, when this plugin is loaded. A fix for that is to right click the .dll and go into properties, and there unblock it. The field for that is at the bottom of the properties window.

## Crash due to full ram

This is probably caused by a memory leak that is present in space engineers, where it does not clean up voxel storage, that was closed or deleted. It only happens, because the plugin uses a much higher density for asteroid rings and fields by default. However, you can set the density in the configuration or in the world settings of the plugin.

## More Info

This readme only contains some information about the installation. The wiki contains more help and info. [Wiki](https://github.com/thorwin99/SEWorldGenPlugin/wiki/Installation)
