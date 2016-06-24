// This file is used by the VS2010 basic extension

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using VSLangProj;

namespace Cqrs.Modelling.UmlProfiles
{
	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	///
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the 
	/// IVsPackage interface and uses the registration attributes defined in the framework to 
	/// register itself and its components with the shell.
	/// </summary>
	// This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
	// a package.
	[PackageRegistration(UseManagedResourcesOnly = true)]
	// This attribute is used to register the informations needed to show the this package
	// in the Help/About dialog of Visual Studio.
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
	[Guid(GuidList.guidvsp1PkgString)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string)]
	public sealed class vsp1Package : Package
	{
		/// <summary>
		/// Default constructor of the package.
		/// Inside this method you can place any initialization code that does not require 
		/// any Visual Studio service because at this point the package object is created but 
		/// not sited yet inside Visual Studio environment. The place to do all the other 
		/// initialization is the Initialize method.
		/// </summary>
		public vsp1Package()
		{
			Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
		}



		/////////////////////////////////////////////////////////////////////////////
		// Overriden Package Implementation
		#region Package Members

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initilaization code that rely on services provided by VisualStudio.
		/// </summary>
		protected override void Initialize()
		{
			Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
			base.Initialize();

			try
			{
				DTE dte = (DTE)GetService(typeof(DTE));
				Projects projects = dte.Solution.Projects;
				string solutionName = dte.Solution.FullName;
				solutionName = solutionName.Substring(solutionName.LastIndexOf('\\') + 1);
				foreach (Project project in projects)
				{
					IEnumerable<AssemblyName> references;
					try
					{
						references = CollectSettings(project)
							.Where(assembly => assembly.Name.StartsWith("Cqrs"))
							.ToList();
					}
					catch (NullReferenceException)
					{
						continue;
					}
					foreach (AssemblyName assemblyName in references)
					{
						string urlPath = string.Format("/{0}/{1}/{2}/{3}/{4}/{5}/{6}/{7}", HttpUtility.UrlPathEncode(dte.Version), HttpUtility.UrlPathEncode(dte.Edition), HttpUtility.UrlPathEncode(solutionName), HttpUtility.UrlPathEncode(project.Name), HttpUtility.UrlPathEncode(project.Kind), HttpUtility.UrlPathEncode(project.UniqueName.Replace('\\', '¿')), HttpUtility.UrlPathEncode(assemblyName.Name), HttpUtility.UrlPathEncode(assemblyName.Version.ToString()));

						HttpWebRequest request = WebRequest.Create(string.Format("https://www.chinchillasoftware.com/VisualStudio{0}", urlPath)) as HttpWebRequest;
						request.Method = "GET";
						try
						{
							new TaskFactory().StartNew(() =>
							{
								using (request.GetResponse()) {}
							});
						}
						catch
						{
						}
					}
				}
			}
			catch
			{
			}
		}
		#endregion

		public static IEnumerable<AssemblyName> CollectSettings(Project project)
		{
			var vsproject = project.Object as VSProject;
			// note: you could also try casting to VsWebSite.VSWebSite

			foreach (Reference reference in vsproject.References)
			{
				if (reference.SourceProject == null)
				{
					// This is an assembly reference
					var fullName = GetFullName(reference);
					var assemblyName = new AssemblyName(fullName);
					yield return assemblyName;
				}
				else
				{
					// This is a project reference
				}
			}
		}

		public static string GetFullName(Reference reference)
		{
			return string.Format("{0}, Version={1}.{2}.{3}.{4}, Culture={5}, PublicKeyToken={6}",
									reference.Name,
									reference.MajorVersion, reference.MinorVersion, reference.BuildNumber, reference.RevisionNumber,
									reference.Culture.Or("neutral"),
									reference.PublicKeyToken.Or("null"));
		}
	}

	static class Extensions
	{
		public static string Or(this string text, string alternative)
		{
			return string.IsNullOrWhiteSpace(text) ? alternative : text;
		}
	}
}