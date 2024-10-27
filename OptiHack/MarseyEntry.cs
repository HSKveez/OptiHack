using System.Reflection;
using HarmonyLib;

public static class MarseyEntry
{
    public static void Entry()
    {
        Harmony.DEBUG = true;
        MarseyLogger.Info("Entry for patching started.");
        bool flag = !MarseyEntry.TryGetAssembly("Content.Client");
        if (!flag)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            SubverterPatch.Harm.PatchAll(executingAssembly);
        }
    }
    
    private static bool TryGetAssembly(string assembly)
    {
        for (int i = 0; i < 50; i++)
        {
            bool flag = MarseyEntry.FindAssembly(assembly) != null;
            if (flag)
            {
                return true;
            }
            Thread.Sleep(200);
        }
        return false;
    }
    
    private static Assembly FindAssembly(string assemblyName)
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        return assemblies.FirstOrDefault(delegate(Assembly assembly)
        {
            string fullName = assembly.FullName;
            return fullName != null && fullName.Contains(assemblyName);
        });
    }
}