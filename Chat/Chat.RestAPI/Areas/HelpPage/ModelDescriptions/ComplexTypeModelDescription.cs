using System.Collections.ObjectModel;

namespace Chat.RestAPI.Areas.HelpPage.ModelDescriptions
{
	public class ComplexTypeModelDescription : ModelDescription
	{
		public ComplexTypeModelDescription()
		{
			Properties = new Collection<ParameterDescription>();
		}

		public Collection<ParameterDescription> Properties { get; private set; }
	}
}