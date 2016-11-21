// Guids.cs
// MUST match guids.h
using System;

namespace Cqrs.Modelling.UmlProfiles
{
	static class GuidList
	{
		public const string guidvsp1PkgString = "36bac4cc-8397-4d27-aa64-2a01a2c4d059";
		public const string guidvsp1CmdSetString = "defe0f21-3b6a-48fa-a673-0fb40316fe45";

		public static readonly Guid guidvsp1CmdSet = new Guid(guidvsp1CmdSetString);
	};

	static class PkgCmdIDList
	{
		public const uint cmdidMyCommand = 0x200;
		public const uint cmdidMyTool = 0x201;

	};
}