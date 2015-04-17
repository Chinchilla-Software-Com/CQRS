using System;
using Cqrs.Entities;

namespace CQRSCode.ReadModel.Dtos
{
	public class UserDetailsDto : Entity
	{
		public Guid Id
		{
			get { return Rsn; }
			set { Rsn = value; }
		}
		public string Name;
		public int Version;

		public UserDetailsDto(Guid id, string name, int version)
		{
			Id = id;
			Name = name;
			Version = version;
		}
	}
}