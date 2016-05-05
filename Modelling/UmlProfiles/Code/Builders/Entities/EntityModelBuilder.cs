﻿using System;
using System.Linq;
using Microsoft.VisualStudio.ArchitectureTools.Extensibility.Uml;
using Microsoft.VisualStudio.Modeling;
using Microsoft.VisualStudio.Uml.Classes;

namespace Cqrs.Modelling.UmlProfiles.Builders.Entities
{
	public abstract class EntityModelBuilder : ModelBuilder
	{
		protected EntityModelBuilder(string operationModeName, bool copyAttributes = true)
		{
			CopyAttributes = copyAttributes;
			OperationModeName = operationModeName;
		}

		protected virtual string GetPropertyInstanceName()
		{
			return string.Format("Build{0}Command", OperationModeName);
		}

		protected string OperationModeName { get; private set; }

		protected bool CopyAttributes { get; private set; }

		protected override bool ShouldCreateModel(Store store, PropertyInstance propertyInstance)
		{
			return propertyInstance.Name == GetPropertyInstanceName() && propertyInstance.Value.ToLowerInvariant() == "true";
		}

		protected override bool ShouldDeleteModel(Store store, PropertyInstance propertyInstance)
		{
			return false && propertyInstance.Name == GetPropertyInstanceName() && propertyInstance.Value.ToLowerInvariant() == "false";
		}

		protected override void CreateModel(Store store, PropertyInstance propertyInstance)
		{
			IClass propertyInstanceModelClass = GetPropertyInstanceModelClass(store, propertyInstance);
			// See https://msdn.microsoft.com/en-us/library/cc512845.aspx#elements for copy/paste
			using (var transaction = store.TransactionManager.BeginTransaction())
			{
				try
				{
					var modulePackage = (Package)propertyInstanceModelClass.Package;
					var entitiesPackage = modulePackage.NestedPackages.SingleOrDefault(package => package.Name == "Entities") as Package;
					if (entitiesPackage == null)
					{
						entitiesPackage = (Package)modulePackage.CreatePackage();
						entitiesPackage.Name = "Entities";
					}

					var eventsPackage = modulePackage.NestedPackages.SingleOrDefault(package => package.Name == "Events") as Package;
					if (eventsPackage == null)
					{
						eventsPackage = (Package)modulePackage.CreatePackage();
						eventsPackage.Name = "Events";
					}

					var eventHandlersPackage = eventsPackage.NestedPackages.SingleOrDefault(package => package.Name == "Handlers") as Package;
					if (eventHandlersPackage == null)
					{
						eventHandlersPackage = (Package)eventsPackage.CreatePackage();
						eventHandlersPackage.Name = "Handlers";
					}

					string className = string.Format("{0}Entity", propertyInstanceModelClass.Name);
					var clonedClass = entitiesPackage.OwnedElements
						// This bit filters out the associations
						.Where(element => element.AppliedStereotypes.Any())
						.Cast<Class>()
						.SingleOrDefault(element => CSharpHelper.ClassifierName(element) == className);
					if (clonedClass == null)
					{
						clonedClass = (Class)entitiesPackage.CreateClass();
						clonedClass.Name = className;
					}
					AddStereotypeInstanceIfMissingRefreshOtherwise(clonedClass, propertyInstanceModelClass, "AutoGenerated");
					var entity = AddStereotypeInstanceIfMissingRefreshOtherwise(clonedClass, propertyInstanceModelClass, "Entity");


					// Copy Properties
					foreach (IProperty ownedAttribute in propertyInstanceModelClass.OwnedAttributes)
					{
						var property = AddAttributeIfMissingRefreshOtherwise(clonedClass, ownedAttribute);
					}

					var eventHandlerClass = eventHandlersPackage.OwnedElements
						// This bit filters out the associations
						.Where(element => element.AppliedStereotypes.Any())
						.Cast<Class>()
						.SingleOrDefault(element => CSharpHelper.ClassifierName(element) == string.Format("{1}{0}dEventHandler", OperationModeName, propertyInstanceModelClass.Name));

					// Create Association
					string operationName = "Handle";
					AddAssociationIfMissingRefreshOtherwise(eventHandlerClass, clonedClass, operationName);

					transaction.Commit();
				}
				catch (Exception)
				{
					transaction.Rollback();
				}
			}
		}

		protected override void DeleteModel(Store store, PropertyInstance propertyInstance)
		{
			// ElementFactory elementFactory = GetMatchingElementFactory(store, propertyInstance);
			IClass propertyInstanceModelClass = GetPropertyInstanceModelClass(store, propertyInstance);
			// See https://msdn.microsoft.com/en-us/library/cc512845.aspx#elements for copy/paste
			using (var transaction = store.TransactionManager.BeginTransaction())
			{
				try
				{
					var modulePackage = (Package)propertyInstanceModelClass.Package;
					var entitiesPackage = modulePackage.NestedPackages.SingleOrDefault(package => package.Name == "Entities") as Package;
					if (entitiesPackage == null)
						return;

					string className = string.Format("{0}{1}Entity", OperationModeName, propertyInstanceModelClass.Name);
					var clonedClass = entitiesPackage.OwnedElements
						// This bit filters out the associations
						.Where(element => element.AppliedStereotypes.Any())
						.Cast<Class>()
						.SingleOrDefault(element => CSharpHelper.ClassifierName(element) == className);
					if (clonedClass == null)
						return;
					clonedClass.Delete();

					transaction.Commit();
				}
				catch (Exception)
				{
					transaction.Rollback();
				}
			}
		}
	}
}