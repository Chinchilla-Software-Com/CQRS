SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ConversationSummary](
	[Rsn] [uniqueidentifier] NOT NULL,
	[IsLogicallyDeleted] [bit] NOT NULL,
	[SortingOrder] [int] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[MessageCount] [int] NOT NULL,
	[LastUpdatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_ConversationSummary] PRIMARY KEY CLUSTERED 
(
	[Rsn] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[ConversationSummary] ADD  CONSTRAINT [DF_ConversationSummary_IsLogicallyDeleted]  DEFAULT ((0)) FOR [IsLogicallyDeleted]
GO

ALTER TABLE [dbo].[ConversationSummary] ADD  CONSTRAINT [DF_ConversationSummary_MessageCount]  DEFAULT ((0)) FOR [MessageCount]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'A summary of conversations' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ConversationSummary'
GO


