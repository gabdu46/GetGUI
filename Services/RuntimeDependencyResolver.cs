using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace GetGUI.Services;

internal static class RuntimeDependencyResolver
{
    private static readonly string DllDirectory = Path.Combine(AppContext.BaseDirectory, "dlls");

    [ModuleInitializer]
    internal static void Initialize()
    {
        if (!Directory.Exists(DllDirectory))
        {
            return;
        }

        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        if (!path.Split(Path.PathSeparator).Contains(DllDirectory, StringComparer.OrdinalIgnoreCase))
        {
            Environment.SetEnvironmentVariable("PATH", DllDirectory + Path.PathSeparator + path);
        }

        AssemblyLoadContext.Default.Resolving += ResolveManagedAssembly;
        AssemblyLoadContext.Default.ResolvingUnmanagedDll += ResolveUnmanagedDll;
    }

    private static Assembly? ResolveManagedAssembly(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        if (string.IsNullOrWhiteSpace(assemblyName.Name))
        {
            return null;
        }

        var candidate = Path.Combine(DllDirectory, assemblyName.Name + ".dll");
        return File.Exists(candidate) ? context.LoadFromAssemblyPath(candidate) : null;
    }

    private static IntPtr ResolveUnmanagedDll(Assembly assembly, string libraryName)
    {
        var fileName = libraryName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
            ? libraryName
            : libraryName + ".dll";
        var candidate = Path.Combine(DllDirectory, fileName);

        return File.Exists(candidate) ? NativeLibrary.Load(candidate) : IntPtr.Zero;
    }
}
