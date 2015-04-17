using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace Cqrs.Sql.DataStores
{
	public abstract class LinqToSqlDataContext : DataContext
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Data.Linq.DataContext"/> class by referencing a file source.
		/// </summary>
		/// <param name="fileOrServerOrConnection">This argument can be any one of the following:The name of a file where a SQL Server Express database resides.The name of a server where a database is present. In this case the provider uses the default database for a user.A complete connection string. LINQ to SQL just passes the string to the provider without modification.</param>
		protected LinqToSqlDataContext(string fileOrServerOrConnection) : base(fileOrServerOrConnection)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Data.Linq.DataContext"/> class by referencing a file source and a mapping source.
		/// </summary>
		/// <param name="fileOrServerOrConnection">This argument can be any one of the following:The name of a file where a SQL Server Express database resides.The name of a server where a database is present. In this case the provider uses the default database for a user.A complete connection string. LINQ to SQL just passes the string to the provider without modification.</param><param name="mapping">The <see cref="T:System.Data.Linq.Mapping.MappingSource"/>.</param>
		protected LinqToSqlDataContext(string fileOrServerOrConnection, MappingSource mapping) : base(fileOrServerOrConnection, mapping)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Data.Linq.DataContext"/> class by referencing the connection used by the .NET Framework.
		/// </summary>
		/// <param name="connection">The connection used by the .NET Framework.</param>
		protected LinqToSqlDataContext(IDbConnection connection) : base(connection)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.Data.Linq.DataContext"/> class by referencing a connection and a mapping source.
		/// </summary>
		/// <param name="connection">The connection used by the .NET Framework.</param><param name="mapping">The <see cref="T:System.Data.Linq.Mapping.MappingSource"/>.</param>
		protected LinqToSqlDataContext(IDbConnection connection, MappingSource mapping) : base(connection, mapping)
		{
		}
	}
}