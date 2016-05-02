using System.Linq;
using Microsoft.VisualStudio.ArchitectureTools.Extensibility.Uml;
using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Uml.Classes;
using Microsoft.VisualStudio.Uml.ModelStore;

namespace Cqrs.Modelling.UmlProfiles.Builders
{
	public abstract class ModelBuilder
	{
		public void CreateModel(Store store, ElementPropertyChangedEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("StereotypeInstancePropertyChanged event 1");

			// Event handlers are called on any change, including those
			// caused by Undo, Redo, transaction rollback, or loading from file.
			// This makes them ideal for synchronizing changes inside the model
			// with other objects or data outside the model. 
			// However, you can ignore those cases if you want:
			if (store.InUndoRedoOrRollback || store.InSerializationTransaction) return;

			// In an event handler, you can cast the implementation class members 
			// to an appropriate UML API type such as IClass or IStereotype:
			PropertyInstance propertyInstance = e.ModelElement as PropertyInstance;

			if (propertyInstance == null)
				return;

			// The UML implementation uses one VMSDK model for each diagram,
			// and another for the core model. 
			// IsElementDefinition() is true if the element is in the core model.
			// Event handlers will be called for changes to any models,
			// but you should ignore changes in the diagram models:
			if (!propertyInstance.IsElementDefinition())
				return;

			IClass modelClass = propertyInstance.StereotypeInstance.Owner as IClass;
			string classNamespace = CSharpHelper.GetNamespace(modelClass.Namespace);
			if (ShouldCreateModel(store, propertyInstance))
				CreateModel(store, propertyInstance);
			else if (ShouldDeleteModel(store, propertyInstance))
				DeleteModel(store, propertyInstance);
		}

		protected abstract bool ShouldCreateModel(Store store, PropertyInstance propertyInstance);

		protected abstract bool ShouldDeleteModel(Store store, PropertyInstance propertyInstance);

		protected abstract void CreateModel(Store store, PropertyInstance propertyInstance);

		protected abstract void DeleteModel(Store store, PropertyInstance propertyInstance);

		protected virtual IClass GetPropertyInstanceModelClass(Store store, PropertyInstance propertyInstance)
		{
			return propertyInstance.StereotypeInstance.Owner as IClass;
		}

		protected virtual ElementFactory GetMatchingElementFactory(Store store, PropertyInstance propertyInstance)
		{
			foreach (Partition partition in store.Partitions.Values)
				if (partition.ElementDirectory.ContainsElement(GetPropertyInstanceModelClass(store, propertyInstance).GetId()))
					return partition.ElementFactory;
			return null;
		}

		protected virtual IStereotypeInstance AddStereotypeInstanceIfMissingRefreshOtherwise(IElement targetElement, IElement sourceElement, string stereotypeName)
		{
			IStereotypeInstance result = targetElement.AppliedStereotypes.SingleOrDefault(stereotype => stereotype.Name == stereotypeName);
			return result ?? targetElement.ApplyStereotype(sourceElement.ApplicableStereotypes.Single(stereotype => stereotype.Name == stereotypeName));
		}

		protected virtual Property AddAttributeIfMissingRefreshOtherwise(IClass targetElement, IProperty ownedAttribute)
		{
			Property property = targetElement.OwnedAttributes.SingleOrDefault(attribute => attribute.Name == ownedAttribute.Name) as Property;
			if (property == null)
				property = targetElement.CreateAttribute() as Property;
			property.DefaultValue = ownedAttribute.DefaultValue as ValueSpecification;
			property.Description = ownedAttribute.Description;
			property.LowerValue = ownedAttribute.LowerValue as ValueSpecification;
			property.Name = ownedAttribute.Name;
			property.Type = property.Type;
			property.UpperValue = ownedAttribute.UpperValue as ValueSpecification;

			return property;
		}

		protected virtual IOperation AddOperationIfMissingRefreshOtherwise(Class elementClass, string operationName)
		{
			IOperation result = elementClass.OwnedOperations.SingleOrDefault(operation => operation.Name == operationName);
			if (result != null)
				return result;

			result = elementClass.CreateOperation();
			result.Name = operationName;
			return result;
		}
	}
}