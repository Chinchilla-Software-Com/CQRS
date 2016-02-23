using System;
using System.Diagnostics;
using cdmdotnet.Logging;
using Cqrs.Ninject.Configuration;
using MyCompany.MyProject.Domain.Common.Services;
using MyCompany.MyProject.Domain.Host.Configuration;
using MyCompany.MyProject.Domain.Meetings.Services;
using MyCompany.MyProject.Domain.Notifications.Services;
using Ninject.Modules;

namespace MyCompany.MyProject.Domain.Host
{
	public abstract class Host<THostModule>
			where THostModule : NinjectModule, new()
	{
		public virtual void Configure()
		{
			Trace.TraceInformation("Starting configurations...");
			new DomainConfiguration<THostModule>().Start();
			Trace.TraceInformation("Setting service point configurations...");
			new ServicePointManagerConfiguration().Start();

			Trace.TraceInformation("Data contracts configuring...");

			// Move to a Configuration class, just like above
			AnnouncementServiceGetEffectiveAnnouncementsResolver.RegisterDataContracts();
			AnnouncementServiceGetUnreadEffectiveAnnouncementsResolver.RegisterDataContracts();
			AnnouncementServiceReadAnnouncementResolver.RegisterDataContracts();
			AnnouncementServiceCreateAnnouncementParametersResolver.RegisterDataContracts();
			AnnouncementServiceUpdateAnnouncementParametersResolver.RegisterDataContracts();
			AnnouncementServiceDeleteAnnouncementParametersResolver.RegisterDataContracts();

			MeetingInstanceServiceAttendeeViewAgendaResolver.RegisterDataContracts();
			MeetingInstanceServiceAttachFileToCommentResolver.RegisterDataContracts();
			MeetingInstanceServiceAttachFileToCommentReplyResolver.RegisterDataContracts();
			MeetingInstanceServiceGetAllFileAttachmentsByMeetingInstanceRsnResolver.RegisterDataContracts();

			CurrencyServiceGetAllCurrenciesResolver.RegisterDataContracts();

			Trace.TraceInformation("Data contracts configured.");
		}

		public virtual void Start()
		{
			LogApplicationStarted();

			Run();

			LogApplicationStopped();
		}

		protected abstract void Run();

		protected virtual ILogger GetLogger()
		{
			return NinjectDependencyResolver.Current.Resolve<ILogger>();
		}

		protected virtual void LogApplicationStarted()
		{
			try
			{
				ILogger logger = GetLogger();

				if (logger != null)
				{
					NinjectDependencyResolver.Current.Resolve<ICorrelationIdHelper>().SetCorrelationId(Guid.Empty);
					logger.LogInfo("Application started.", "LogApplicationStarted");
				}
			}
			catch { }
		}

		protected virtual void LogApplicationStopped()
		{
			try
			{
				ILogger logger = GetLogger();

				if (logger != null)
				{
					NinjectDependencyResolver.Current.Resolve<ICorrelationIdHelper>().SetCorrelationId(Guid.Empty);
					logger.LogInfo("Application stopped.", "LogApplicationStopped");
				}
			}
			catch { }
		}
	}
}