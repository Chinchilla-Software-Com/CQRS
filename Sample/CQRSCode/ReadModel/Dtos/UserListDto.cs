using System;

namespace CQRSCode.ReadModel.Dtos
{
	public class UserListDto
	{
		public Guid Id;
		public string Name;

		public UserListDto(Guid id, string name)
		{
			Id = id;
			Name = name;
		}
	}
}