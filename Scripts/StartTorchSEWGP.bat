#place in directory which contains Torch.Server.exe

set tdir=%cd%
mkdir ./Instance/Plugins
cd ./Instance/Plugins #or C:\path\to\your\plugins
curl -L -O https://github.com/thorwin99/SEWorldGenPlugin/releases/latest/download/SEWorldGenPlugin.dll
curl -L -O https://github.com/thorwin99/SEWorldGenPlugin/releases/latest/download/0Harmony.dll
cd %tdir%
Torch.Server.exe -autostart