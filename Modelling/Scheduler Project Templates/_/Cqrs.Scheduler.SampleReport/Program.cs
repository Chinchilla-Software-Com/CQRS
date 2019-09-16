using System;
using $safeprojectname$.EventHandlers;

/// <summary>
/// Starts the Webjob
/// </summary>
public partial class CqrsWebJobProgram
{
	partial void GetCommandOrEventType(ref Type commandOrEventType)
	{
		commandOrEventType = typeof(SendAnEmailAtMidnight);
	}
}