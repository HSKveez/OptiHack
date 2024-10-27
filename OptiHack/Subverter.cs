using HarmonyLib;

public static class SubverterPatch
{
    public static string Name = "OptiHack";
    public static string Description = "See more than they want";
    public static Harmony Harm = new("com.hsk.eye");
}