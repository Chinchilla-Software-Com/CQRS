USE [Chat]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Credentials](
	[Rsn] [uniqueidentifier] NOT NULL,
	[IsLogicallyDeleted] [bit] NOT NULL,
	[SortingOrder] [int] NOT NULL,
	[UserRsn] [uniqueidentifier] NOT NULL,
	[Hash] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_Credentials] PRIMARY KEY CLUSTERED 
(
	[Rsn] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Credentials] ADD  CONSTRAINT [DF_Credentials_IsLogicallyDeleted]  DEFAULT ((0)) FOR [IsLogicallyDeleted]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'User Credentials' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Credentials'
GO

CREATE NONCLUSTERED INDEX [IX_IsLogicallyDeleted_Hash] ON [dbo].[Credentials]
(
	[IsLogicallyDeleted] ASC,
	[Hash] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

CREATE NONCLUSTERED INDEX [IX_UserRsn] ON [dbo].[Credentials]
(
	[UserRsn] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

