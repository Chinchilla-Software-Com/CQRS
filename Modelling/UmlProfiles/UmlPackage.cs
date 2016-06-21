using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using Cqrs.Modelling.UmlProfiles.Code;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
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
	[ProvideSolutionProperties(CqrsNetSettingsKey)]
	public sealed class UmlPackage : Package, IVsPersistSolutionProps
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

		internal static IVsPersistSolutionProps Solution { get; private set; }

		internal static bool IsDirty { get; set; }

		internal static CqrsSettings CqrsSettings { get; set; }

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
				ProjectNames = new List<Project>();

				DTE dte = (DTE)GetService(typeof(DTE));

				dte.Events.SolutionEvents.ProjectAdded += project => { ProjectNames.Add(project); };
				dte.Events.SolutionEvents.ProjectRemoved += project => { if (ProjectNames.Contains(project)) { ProjectNames.Remove(project); } };
				dte.Events.SolutionEvents.ProjectRenamed += (project, originalName) => { };

				dte.Events.SolutionEvents.Opened += () => { PopulateProjectNames(dte); };

				dte.Events.SolutionEvents.BeforeClosing += ProjectNames.Clear;

				PopulateProjectNames(dte);
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

			try
			{

				// Add our command handlers for menu (commands must exist in the .vsct file)
				OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
				if (null != mcs)
				{
					// Create the command for the menu item.
					CommandID menuCommandID = new CommandID(GuidList.guidvsp1CmdSet, (int)PkgCmdIDList.cmdidMyCommand);
					MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
					mcs.AddCommand(menuItem);
					// Create the command for the tool window
					CommandID toolwndCommandID = new CommandID(GuidList.guidvsp1CmdSet, (int)PkgCmdIDList.cmdidMyTool);
					MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
					mcs.AddCommand(menuToolWin);
				}
			}
			catch (Exception ex)
			{
				Trace.TraceError(ex.Message);
			}
		}

		private static void PopulateProjectNames(DTE dte)
		{
			Solution = (IVsPersistSolutionProps)GetGlobalService(typeof(IVsPersistSolutionProps));

			Projects projects = dte.Solution.Projects;

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
					string urlPath = string.Format("/{0}/{1}/{2}/{3}/{4}/{5}/{6}/{7}", HttpUtility.UrlPathEncode(dte.Version),
						HttpUtility.UrlPathEncode(dte.Edition), HttpUtility.UrlPathEncode(solutionName),
						HttpUtility.UrlPathEncode(project.Name), HttpUtility.UrlPathEncode(project.Kind),
						HttpUtility.UrlPathEncode(project.UniqueName.Replace('\\', '¿')), HttpUtility.UrlPathEncode(assemblyName.Name),
						HttpUtility.UrlPathEncode(assemblyName.Version.ToString()));

					HttpWebRequest request =
						WebRequest.Create(string.Format("https://www.chinchillasoftware.com/VisualStudio{0}", urlPath)) as HttpWebRequest;
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

		/// <summary>
		/// This function is the callback used to execute a command when the a menu item is clicked.
		/// See the Initialize method to see how the menu item is associated to this function using
		/// the OleMenuCommandService service and the MenuCommand class.
		/// </summary>
		private void MenuItemCallback(object sender, EventArgs e)
		{
			// Show a Message Box to prove we were here
			IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
			Guid clsid = Guid.Empty;
			int result;
			Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
					   0,
					   ref clsid,
					   "VSPackage",
					   string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.ToString()),
					   string.Empty,
					   0,
					   OLEMSGBUTTON.OLEMSGBUTTON_OK,
					   OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
					   OLEMSGICON.OLEMSGICON_INFO,
					   0,        // false
					   out result));
		}

		private const string CqrsNetSettingsKey = "CqrsNetSettings";

		public int SaveUserOptions(IVsSolutionPersistence pPersistence)
		{
			try
			{
				pPersistence.SavePackageUserOpts(this, CqrsNetSettingsKey);

				return VSConstants.S_OK;
			}
			finally
			{
				Marshal.ReleaseComObject(pPersistence); // See Package.cs from MPF for reason
			}
		}

		public int LoadUserOptions(IVsSolutionPersistence pPersistence, uint grfLoadOpts)
		{
			try
			{
				pPersistence.LoadPackageUserOpts(this, CqrsNetSettingsKey);
			}
			finally
			{
				Marshal.ReleaseComObject(pPersistence);
			}

			return VSConstants.S_OK;
		}

		public int WriteUserOptions(IStream pOptionsStream, string pszKey)
		{
			try
			{
				using (ComStreamWrapper wrapper = new ComStreamWrapper(pOptionsStream))
				{
					switch (pszKey)
					{
						case CqrsNetSettingsKey:
							SerializeEnlistData(wrapper, true);
							break;
					}
				}

				return VSConstants.S_OK;
			}
			finally
			{
				Marshal.ReleaseComObject(pOptionsStream); // See Package.cs from MPF for reason
			}
		}

		public int ReadUserOptions(IStream pOptionsStream, string pszKey)
		{
			try
			{
				using (ComStreamWrapper wrapper = new ComStreamWrapper(pOptionsStream, true))
				{
					switch (pszKey)
					{
						case CqrsNetSettingsKey:
							SerializeEnlistData(wrapper, false);
							break;
					}
				}
				return VSConstants.S_OK;
			}
			finally
			{
				Marshal.ReleaseComObject(pOptionsStream);
			}
		}

		public int QuerySaveSolutionProps(IVsHierarchy pHierarchy, VSQUERYSAVESLNPROPS[] pqsspSave)
		{
			// This function is called by the IDE to determine if something needs to be saved in the solution.
			// If the package returns that it has dirty properties, the shell will callback on SaveSolutionProps

			if (pHierarchy == null)
			{
				VSQUERYSAVESLNPROPS result = VSQUERYSAVESLNPROPS.QSP_HasNoProps;

				if (CqrsSettings != null)
				{
					if (IsDirty)
						result = VSQUERYSAVESLNPROPS.QSP_HasDirtyProps;
					else
						result = VSQUERYSAVESLNPROPS.QSP_HasNoDirtyProps;
				}
				pqsspSave[0] = result;
			}

			return VSConstants.S_OK;
		}

		public int SaveSolutionProps(IVsHierarchy pHierarchy, IVsSolutionPersistence pPersistence)
		{
			// This function gets called by the shell after QuerySaveSolutionProps returned QSP_HasDirtyProps

			// The package will pass in the key under which it wants to save its properties, 
			// and the IDE will call back on WriteSolutionProps

			// The properties will be saved in the Pre-Load section
			// When the solution will be reopened, the IDE will call our package to load them back before the projects in the solution are actually open
			// This could help if the source control package needs to persist information like projects translation tables, that should be read from the suo file
			// and should be available by the time projects are opened and the shell start calling IVsSccEnlistmentPathTranslation functions.
			if (pHierarchy == null) // Only save the property on the solution itself
			{
				// SavePackageSolutionProps will call WriteSolutionProps with the specified key

				if (CqrsSettings != null)
					pPersistence.SavePackageSolutionProps(1 /* TRUE */, null, this, CqrsNetSettingsKey);

				// Once we saved our props, the solution is not dirty anymore
				IsDirty = false;
			}

			return VSConstants.S_OK;
		}

		public int WriteSolutionProps(IVsHierarchy pHierarchy, string pszKey, IPropertyBag pPropBag)
		{
			if (pHierarchy != null)
				return VSConstants.S_OK; // Not sent by our code!

			if (pPropBag == null)
				return VSConstants.E_POINTER;

			// This method is called from the VS implementation after a request from SaveSolutionProps

			using (IPropertyMap map = GetMap(pPropBag))
			{
				switch (pszKey)
				{
					case CqrsNetSettingsKey:
						map.SetRawValue(CqrsNetSettingsKey, new JavaScriptSerializer().Serialize(CqrsSettings));
						map.Flush();
						break;
				}
			}

			return VSConstants.S_OK;
		}

		public int ReadSolutionProps(IVsHierarchy pHierarchy, string pszProjectName, string pszProjectMk, string pszKey, int fPreLoad, IPropertyBag pPropBag)
		{
			if (pHierarchy != null)
				return VSConstants.S_OK;

			using (IPropertyMap map = GetMap(pPropBag))
			{
				bool preload = (fPreLoad != 0);

				switch (pszKey)
				{
					case CqrsNetSettingsKey:
						if (preload)
						{
							string value;
							bool register = map.TryGetValue(CqrsNetSettingsKey, out value);

							if (!register || string.IsNullOrEmpty(value))
								register = false;

							if (register)
								CqrsSettings = new JavaScriptSerializer().Deserialize<CqrsSettings>(value);
							
						}
						break;
				}
			}

			return VSConstants.S_OK;
		}

		public int OnProjectLoadFailure(IVsHierarchy pStubHierarchy, string pszProjectName, string pszProjectMk, string pszKey)
		{
			return VSConstants.S_OK;
		}

		public IPropertyMap GetMap(IPropertyBag propertyBag)
		{
			return new PropertyBag(propertyBag);
		}

		/// <summary>
		/// Serializes the enlist data.
		/// </summary>
		/// <param name="store">The store.</param>
		/// <param name="writeData">if set to <c>true</c> [write data].</param>
		private void SerializeEnlistData(Stream store, bool writeData)
		{
			if (writeData)
				WriteEnlistData(store);
			else
				ReadEnlistUserData(store);
		}

		const int EnlistSerializerVersion = 1;
		EnlistBase enlist = null;

		private void ReadEnlistUserData(Stream store)
		{
			if (store.Length < 2 * sizeof(int))
				return;

			using (BinaryReader br = new BinaryReader(store))
			{
				int version = br.ReadInt32(); // The enlist version used to write the data
				int requiredVersion = br.ReadInt32(); // All enlist versions older then this should ignore all data

				if ((requiredVersion > EnlistSerializerVersion) || version < requiredVersion)
					return; // Older versions (we) should ignore this data

				int count = br.ReadInt32(); // 1

				int stringCount = br.ReadInt32();

				List<string> values = new List<string>();

				for (int j = 0; j < stringCount; j++)
					values.Add(br.ReadString());

				enlist = new EnlistBase();
				enlist.LoadUserData(values);
			}
		}

		private void WriteEnlistData(Stream store)
		{
			if (enlist == null)
				return;

			using (BinaryWriter bw = new BinaryWriter(store))
			{
				bw.Write((int)1); // Minimum version required to read these settings; update if incompatible
				bw.Write((int)EnlistSerializerVersion); // Writer version

				bw.Write(1);
				string[] values = enlist.GetUserData();

				bw.Write(values.Length);

				foreach (string s in values)
					bw.Write(s);
			}
		}
	}

	static class Extensions
	{
		public static string Or(this string text, string alternative)
		{
			return string.IsNullOrWhiteSpace(text) ? alternative : text;
		}
	}

	internal class CqrsSettings
	{
		public string ModellingProjectName { get; set; }

		public string PublicEventProjectName { get; set; }

		public string PrivateEventProjectName { get; set; }
	}

	class EnlistBase
	{
		string[] _userData;

		public virtual void LoadUserData(List<string> values)
		{
			_userData = values.ToArray();
		}

		internal string[] GetUserData()
		{
			return (string[])(_userData ?? new string[0]).Clone();
		}

		internal bool ShouldSerialize()
		{
			return true;
		}
	}
}
