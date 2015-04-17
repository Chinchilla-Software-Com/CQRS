using System;
using Cqrs.Entities;

namespace CQRSCode.ReadModel.Dtos
{
	public class UserListDto : Entity
	{
		public Guid Id
		{
			get { return Rsn; }
			set { Rsn = value; }
		}
		public string Name;

		public UserListDto(Guid id, string name)
		{
			Id = id;
			Name = name;
		}
	}
}