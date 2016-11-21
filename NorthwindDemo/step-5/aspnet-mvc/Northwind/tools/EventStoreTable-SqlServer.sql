/****** Object:  Table [dbo].[EventStore]    Script Date: 7-Apr-2016 2:21:45 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EventStore](
	[EventId] [uniqueidentifier] NOT NULL,
	[Data] [text] NOT NULL,
	[EventType] [nvarchar](MAX) NOT NULL,
	[AggregateId] [nvarchar](445) NOT NULL,
	[Version] [bigint] NOT NULL,
	[Timestamp] [datetime] NOT NULL,
 CONSTRAINT [PK_EventStore] PRIMARY KEY CLUSTERED 
(
	[EventId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
 CONSTRAINT [UIX_AggregateId_Version] UNIQUE NONCLUSTERED 
(
	[AggregateId] ASC,
	[Version] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

GO


