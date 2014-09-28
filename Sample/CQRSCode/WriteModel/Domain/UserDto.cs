using System;
using Cqrs.Domain;

namespace CQRSCode.WriteModel.Domain
{
	public class UserDto : IDto
	{
		public string Name { get; set; }

		#region Implementation of IDto

		public Guid Id { get; set; }

		#endregion
	}
}