SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Messages](
	[Rsn] [uniqueidentifier] NOT NULL,
	[IsLogicallyDeleted] [bit] NOT NULL,
	[SortingOrder] [int] NOT NULL,
	[ConversationRsn] [uniqueidentifier] NOT NULL,
	[ConversationName] [nvarchar](255) NOT NULL,
	[UserRsn] [uniqueidentifier] NOT NULL,
	[UserName] [nvarchar](255) NOT NULL,
	[Content] [text] NOT NULL,
	[DatePosted] [datetime] NOT NULL,
 CONSTRAINT [PK_Messages] PRIMARY KEY CLUSTERED 
(
	[Rsn] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

ALTER TABLE [dbo].[Messages] ADD  CONSTRAINT [DF_Messages_IsLogicallyDeleted]  DEFAULT ((0)) FOR [IsLogicallyDeleted]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Conversation messages' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Messages'
GO

CREATE NONCLUSTERED INDEX [IX_ConversationRsn_DatePosted] ON [dbo].[Messages]
(
	[ConversationRsn] ASC,
	[DatePosted] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO


