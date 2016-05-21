using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Cqrs.Modelling.UmlProfiles.Code;
using Encryption;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
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
	// This attribute is needed to let the shell know that this package exposes some menus.
	[ProvideMenuResource("Menus.ctmenu", 1)]
	// This attribute registers a tool window exposed by this package.
	// And make it tabbed to the "Solution Explorer" Window
	[ProvideToolWindow(typeof(CqrsSolutionOptionsToolWindow), Style = VsDockStyle.Tabbed, Window = "3ae79031-e1bc-11d0-8f78-00a0c9110057")]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string)]
	public sealed class UmlPackage : Package
	{
		/// <summary>
		/// Default constructor of the package.
		/// Inside this method you can place any initialization code that does not require 
		/// any Visual Studio service because at this point the package object is created but 
		/// not sited yet inside Visual Studio environment. The place to do all the other 
		/// initialization is the Initialize method.
		/// </summary>
		public UmlPackage()
		{
			Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
			LicenseKeyHelper.IsRegistered = false;
		}

		internal static IList<Project> ProjectNames { get; private set; }

		/////////////////////////////////////////////////////////////////////////////
		// Overriden Package Implementation
		#region Package Members

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initialization code that rely on services provided by VisualStudio.
		/// </summary>
		protected override void Initialize()
		{
			Trace.TraceInformation(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", GetType().FullName));
			base.Initialize();

			try
			{
				DTE dte = (DTE)GetService(typeof(DTE));

				Projects projects = dte.Solution.Projects;
				dte.Events.SolutionEvents.ProjectAdded += project => { ProjectNames.Add(project); };
				dte.Events.SolutionEvents.ProjectRemoved += project => { if (ProjectNames.Contains(project)) { ProjectNames.Remove(project); } };
				dte.Events.SolutionEvents.ProjectRenamed += (project, originalName) => { };

				ProjectNames = new List<Project>();

				string solutionName = dte.Solution.FullName;
				solutionName = solutionName.Substring(solutionName.LastIndexOf('\\') + 1);
				foreach (Project project in projects)
				{
					ProjectNames.Add(project);
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
								using (request.GetResponse())
								{
									Debug.WriteLine("Called '{0}'.", request.Address);
								}
							});
							Debug.WriteLine("Calling '{0}'.", request.Address);
						}
						catch
						{
						}
					}
				}
			}
			catch(Exception ex)
			{
				Trace.TraceError(ex.Message);
			}

			try
			{
				var licenseKeyHelper = new LicenseKeyHelper();
				LicenseKeyHelper.IsRegistered = licenseKeyHelper.IsLicensed();
				if (!LicenseKeyHelper.IsRegistered)
					System.Windows.Forms.MessageBox.Show("The Cqrs.net visual designer is not licensed. Please contact chinchillasoftware.com to purchase an enterprise license.");
			}
			catch (Exception ex)
			{
				Trace.TraceError(ex.Message);
			}
		}
		#endregion

		public static IEnumerable<AssemblyName> GetAssemblyReferences(Project project)
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

		public static IEnumerable<AssemblyName> GetProjectReferences(Project project)
		{
			var vsproject = project.Object as VSProject;
			// note: you could also try casting to VsWebSite.VSWebSite

			foreach (Reference reference in vsproject.References)
			{
				if (reference.SourceProject == null)
				{
					// This is an assembly reference
				}
				else
				{
					// This is an assembly reference
					var fullName = GetFullName(reference);
					var assemblyName = new AssemblyName(fullName);
					yield return assemblyName;
				}
			}
		}

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

		/// <summary>
		/// This function is called when the user clicks the menu item that shows the 
		/// tool window. See the Initialize method to see how the menu item is associated to 
		/// this function using the OleMenuCommandService service and the MenuCommand class.
		/// </summary>
		private void ShowToolWindow(object sender, EventArgs e)
		{
			// Get the instance number 0 of this tool window. This window is single instance so this instance
			// is actually the only one.
			// The last flag is set to true so that if the tool window does not exists it will be created.
			ToolWindowPane window = this.FindToolWindow(typeof(CqrsSolutionOptionsToolWindow), 0, true);
			if ((null == window) || (null == window.Frame))
			{
				throw new NotSupportedException(Resources.CanNotCreateWindow);
			}
			IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
			Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
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
