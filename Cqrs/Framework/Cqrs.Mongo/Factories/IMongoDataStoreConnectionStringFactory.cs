namespace Cqrs.Mongo.Factories
{
	public interface IMongoDataStoreConnectionStringFactory
	{
		string GetMongoConnectionString();

		string GetMongoDatabaseName();
	}
}