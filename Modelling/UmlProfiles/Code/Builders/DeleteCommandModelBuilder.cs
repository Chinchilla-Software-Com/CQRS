using System;
using System.Linq;
using Microsoft.VisualStudio.ArchitectureTools.Extensibility.Uml;
using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Uml.Classes;

namespace Cqrs.Modelling.UmlProfiles.Builders
{
	public class DeleteCommandModelBuilder : CommandModelBuilder
	{
		public DeleteCommandModelBuilder()
			: base("Delete", false)
		{
		}
	}
}