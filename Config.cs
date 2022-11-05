using NativeLibraryLoader;

namespace VisualDesigner;

internal static class Config
{
    internal static PathInfo PathInfo;

    internal static void Setup()
    {
        PathPlatformInfo windows = new PathPlatformInfo(NativeLibraryLoader.Platform.Windows);
        windows.AddPath("libsdl2", "./lib/windows/SDL2.dll");
        windows.AddPath("libz", "./lib/windows/zlib1.dll");
        windows.AddPath("libsdl2_image", "./lib/windows/SDL2_image.dll");
        windows.AddPath("libpng", "./lib/windows/libpng16-16.dll");
        if (File.Exists("lib/windows/libjpeg-9.dll")) windows.AddPath("libjpeg", "./lib/windows/libjpeg-9.dll");
        windows.AddPath("libsdl2_ttf", "./lib/windows/SDL2_ttf.dll");
        windows.AddPath("libfreetype", "./lib/windows/libfreetype-6.dll");
        windows.AddPath("tinyfiledialogs", "./lib/windows/tinyfiledialogs64.dll");
        
        PathPlatformInfo linux = new PathPlatformInfo(NativeLibraryLoader.Platform.Linux);
        linux.AddPath("libsdl2", "./lib/linux/SDL2.so");
        linux.AddPath("libz", "./lib/linux/libz.so");
        linux.AddPath("libsdl2_image", "./lib/linux/SDL2_image.so");
        linux.AddPath("libpng", "./lib/linux/libpng16-16.so");
        if (File.Exists("lib/linux/libjpeg-9.so")) linux.AddPath("libjpeg", "./lib/linux/libjpeg-9.so");
        linux.AddPath("libsdl2_ttf", "./lib/linux/SDL2_ttf.so");
        linux.AddPath("libfreetype", "./lib/linux/libfreetype-6.so");
        linux.AddPath("tinyfiledialogs", "./lib/linux/tinyfiledialogs64.so");

        PathInfo = PathInfo.Create(windows, linux);
    }
}
