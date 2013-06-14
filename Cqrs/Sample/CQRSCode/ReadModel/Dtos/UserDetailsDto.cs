using System;

namespace CQRSCode.ReadModel.Dtos
{
	public class UserDetailsDto
	{
		public Guid Id;
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