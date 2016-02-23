using Cqrs.Domain;
using MyCompany.MyProject.Domain.Authentication.Entities;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Cqrs.Services;

namespace MyCompany.MyProject.Domain.Authentication.Services
{

	/// <summary>
	/// A <see cref="DataContractResolver"/> for using <see cref="IUserService.GetAll"/> via WCF
	/// </summary>
	public partial class UserServiceGetAllResolver
	{
	}


	/// <summary>
	/// A <see cref="DataContractResolver"/> for using <see cref="IUserService.GetByRsn"/> via WCF
	/// </summary>
	public partial class UserServiceGetByRsnResolver
	{
	}

}
