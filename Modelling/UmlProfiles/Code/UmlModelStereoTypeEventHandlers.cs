using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Cqrs.Modelling.UmlProfiles.Builders;
using Microsoft.VisualStudio.ArchitectureTools.Extensibility.Uml;
using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Modeling.Diagrams;
using Microsoft.VisualStudio.Modeling.Validation;
using Microsoft.VisualStudio.Uml.AuxiliaryConstructs;
using Microsoft.VisualStudio.Uml.Classes;
using Microsoft.VisualStudio.Uml.Profiles;
using Microsoft.VisualStudio.Uml.UseCases;

// In a private assembly - used for IsElementDefinition:
using Microsoft.VisualStudio.Uml.ModelStore;

namespace Cqrs.Modelling.UmlProfiles
{
	/// <summary>
	/// This class defines a VMSDK event handler that listens for changes in the UML model.
	/// 
	/// VMSDK event handlers are called no matter what causes the change, including
	/// Undo and Redo commands. Events are therefore suitable for maintaining
	/// synchronism between elements inside the store and objects outside it.
	/// (To propagate changes between different elements inside the store,
	/// use VMSDK rules instead. See Rules.cs in this solution.)
	/// 
	/// WARNING: THIS CODE MIGHT NOT WORK with future releases of Visual Studio.
	/// It depends on the internal implementation of UML in Visual Studio 2010.
	/// 
	/// For more information about VMSDK events, see
	/// http://msdn.microsoft.com/library/bb126250.aspx
	/// 
	/// </summary>
	public partial class UmlModelStereoTypeEventHandlers
	{
		// This is the underlying DSL store, not the wrapping UML ModelStore:
		private Store store;

		#region Event handler registration.

		/// <summary>
		/// The Validation API is used to register the event handlers. 
		/// The validation framework provides a convenient way to execute code 
		/// when the model is opened. But the code does not actually perform validation, 
		/// and the user does not have to invoke validation in order to perform updates.
		/// See "Validation": http://msdn.microsoft.com/library/bb126413.aspx
		/// </summary>
		/// <param name="vcontext"></param>
		/// <param name="model"></param>
		[Export(typeof (System.Action<ValidationContext, object>))]
		[ValidationMethod(ValidationCategories.Open)]
		public void RegisterEventHandlers(ValidationContext vcontext, IModel model)
		{
			// This is the underlying DSL store, not the wrapping UML ModelStore:
			store = ((ModelElement)model).Store;

			// To register an event, you must know the name of the implementation class 
			// or relationship that you want to monitor. These classes are defined in 
			// Microsoft.VisualStudio.Modeling.Uml.dll, and you can see the class names 
			// when you watch properties in the debugger. 

			// For a list of interface types, see 
			// Model Element Types: http://msdn.microsoft.com/library/ee517353.aspx

			// WARNING: The implementation class names might be different in future releases.

			// Event handlers are methods that are added to Store.EventManagerDirectory. 
			// This is the Store of the underlying VMSDK (DSL) implementation, not the UML ModelStore. 
			// EventManagerDirectory has a fixed set of dictionaries for different types of events, 
			// such as ElementAdded and ElementDeleted. For other types of event, 
			// see "Event handlers": http://msdn.microsoft.com/library/bb126250.aspx

			// MODIFY THE FOLLOWING CODE TO REGISTER YOUR EVENT HANDLERS.


			// In this example, we register an event for the implementation
			// class that represents the application of a stereotype to a UML element:
			DomainClassInfo siClass = store.DomainDataDirectory.FindDomainClass("Microsoft.VisualStudio.Uml.Classes.StereotypeInstance");


			// This event is triggered whenever an element of the specified
			// class is created:
			store.EventManagerDirectory.ElementAdded.Add(siClass, new EventHandler<ElementAddedEventArgs>(StereotypeInstanceAdded));

			// For deletions, it is better to trigger on the deletion of the link
			// between the old instance and the owning element. 
			// That allows us to get both the deleted instance and the owner:

			DomainRelationshipInfo linkToStereotypeClass = store.DomainDataDirectory.FindDomainRelationship("Microsoft.VisualStudio.Uml.Classes.ElementHasAppliedStereotypeInstances");

			store.EventManagerDirectory.ElementDeleted.Add(linkToStereotypeClass, new EventHandler<ElementDeletedEventArgs>(StereotypeInstanceDeleted));

			// Add here handlers for other events.
			store.EventManagerDirectory.ElementPropertyChanged.Add(new EventHandler<ElementPropertyChangedEventArgs>(StereotypeInstancePropertyChanged));
		}

		#endregion

		# region Event handler definitions.

		/// <summary>
		/// Event handler called whenever a stereotype instance is linked to a UML model element.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void StereotypeInstancePropertyChanged(object sender, ElementPropertyChangedEventArgs e)
		{
			ModelBuilder builder = new CreateCommandModelBuilder();
			builder.CreateModel(store, e);

			builder = new UpdateCommandModelBuilder();
			builder.CreateModel(store, e);

			builder = new DeleteCommandModelBuilder();
			builder.CreateModel(store, e);
		}

		/// <summary>
		/// Event handler called whenever a stereotype instance is linked to a UML model element.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void StereotypeInstanceAdded(object sender, ElementAddedEventArgs e)
		{
			// Event handlers are called on any change, including those
			// caused by Undo, Redo, transaction rollback, or loading from file.
			// This makes them ideal for synchronizing changes inside the model
			// with other objects or data outside the model. 
			// However, you can ignore those cases if you want:
			if (store.InUndoRedoOrRollback || store.InSerializationTransaction) return;

			// In an event handler, you can cast the implementation class members 
			// to an appropriate UML API type such as IClass or IStereotype:
			IStereotypeInstance si = e.ModelElement as IStereotypeInstance;

			// The UML implementation uses one VMSDK model for each diagram,
			// and another for the core model. 
			// IsElementDefinition() is true if the element is in the core model.
			// Event handlers will be called for changes to any models,
			// but you should ignore changes in the diagram models:
			if (!si.IsElementDefinition()) return;

			// Event handlers are called after the completion of the transaction
			// in which the change occurred. 

			//
			// Add code here to respond to the change.
			//
			// For example, you could update an external database.


			// Only for exercising the code. Please replace with useful code:
			System.Diagnostics.Debug.WriteLine("StereotypeInstanceAdded event");
		}

		/// <summary>
		/// Called whenever a stereotype instance is deleted - well, actually, 
		/// when the link between the stereotype instance and the uml model element is deleted.
		/// Triggering on the link deletion allows us to get both ends.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void StereotypeInstanceDeleted(object sender, ElementDeletedEventArgs e)
		{
			// This is the parent type for all kinds of link:
			ElementLink elementToStereotypeLink = e.ModelElement as ElementLink;

			// Work only with the core model, not the views:
			if (!elementToStereotypeLink.IsElementDefinition()) return;

			// Get both ends of the deleted link:
			IElement owner = elementToStereotypeLink.LinkedElements[0] as IElement;
			IStereotypeInstance deletedElement = elementToStereotypeLink.LinkedElements[1] as IStereotypeInstance;


			// We're here either because an element is being deleted,
			// but it might be that its owner is also being deleted.
			// Ignore this event if the owner is being deleted:
			if ((owner as ModelElement).IsDeleting) return;

			//
			// Add code here to respond to the change.
			// 
			// For example, you could update an external database.

			// Only for exercising the code. Please replace with useful code:
			System.Diagnostics.Debug.WriteLine("StereotypeInstanceDeleted event");
		}

		#endregion
	}
}