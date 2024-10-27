using System.Reflection;

public static class MarseyLogger
{
	private static void Log(MarseyLogger.LogType type, string message)
	{
		MarseyLogger.Forward forward = MarseyLogger.logDelegate;
		if (forward != null)
		{
			forward(Assembly.GetExecutingAssembly().GetName(), "[" + type.ToString() + "] " + message);
		}
	}
	
	public static void Info(string message)
	{
		MarseyLogger.Log(MarseyLogger.LogType.INFO, message);
	}
	
	public static void Warn(string message)
	{
		MarseyLogger.Log(MarseyLogger.LogType.WARN, message);
	}
	
	public static void Fatal(string message)
	{
		MarseyLogger.Log(MarseyLogger.LogType.FATL, message);
	}
	
	public static void Debug(string message)
	{
		MarseyLogger.Log(MarseyLogger.LogType.DEBG, message);
	}
	
	public static MarseyLogger.Forward logDelegate;
	
	private enum LogType
	{
		INFO,
		WARN,
		FATL,
		DEBG
	}
	
	public delegate void Forward(AssemblyName asm, string message);
}
