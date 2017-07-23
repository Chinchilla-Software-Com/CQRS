#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Configuration;
using System.Runtime.Serialization;

namespace Cqrs.Exceptions
{
	/// <summary>
	/// A <see cref="ConnectionStringSettings"/> is missing from <see cref="ConfigurationManager.ConnectionStrings"/>.
	/// </summary>
	[Serializable]
	public class MissingConnectionStringException : Exception
	{

		/// <summary>
		/// Instantiate a new instance of <see cref="MissingConnectionStringException"/>.
		/// </summary>
		/// <param name="connectionStringName">The <see cref="ConnectionStringSettings.Name"/> of the <see cref="ConnectionStringSettings"/> that is missing.</param>
		public MissingConnectionStringException(string connectionStringName)
			: base(string.Format("No connection string named '{0}' was found in the configuration file.", connectionStringName))
		{
			ConnectionStringName = connectionStringName;
		}

		/// <summary>
		/// Instantiate a new instance of <see cref="MissingConnectionStringException"/> and a reference to the inner <paramref name="exception"/> that is the cause of this <see cref="Exception"/>.
		/// </summary>
		/// <param name="connectionStringName">The <see cref="ConnectionStringSettings.Name"/> of the <see cref="ConnectionStringSettings"/> that is missing.</param>
		/// <param name="exception">The <see cref="Exception"/> that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner <see cref="Exception"/> is specified.</param>
		public MissingConnectionStringException(string connectionStringName, Exception exception)
			: base(string.Format("No connection string named '{0}' was found in the configuration file.", connectionStringName), exception)
		{
			ConnectionStringName = connectionStringName;
		}

		/// <summary>
		/// The <see cref="ConnectionStringSettings.Name"/> of the <see cref="ConnectionStringSettings"/> that is missing.
		/// </summary>
		[DataMember]
		public string ConnectionStringName { get; set; }
	}
}