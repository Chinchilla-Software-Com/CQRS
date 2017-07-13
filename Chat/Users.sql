USE [Chat]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Users](
	[Rsn] [uniqueidentifier] NOT NULL,
	[IsLogicallyDeleted] [bit] NOT NULL,
	[SortingOrder] [int] NOT NULL,
	[FirstName] [nvarchar](255) NOT NULL,
	[LastName] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Rsn] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_IsLogicallyDeleted]  DEFAULT ((0)) FOR [IsLogicallyDeleted]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Users' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Users'
GO
