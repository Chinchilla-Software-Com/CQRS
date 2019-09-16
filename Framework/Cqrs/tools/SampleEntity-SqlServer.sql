DECLARE	@TableName VARCHAR(255);
SET		@TableName = 'MyEntity';

EXECUTE('CREATE TABLE [dbo].[' + @TableName + '](
	[Rsn] [uniqueidentifier] NOT NULL,
	[SortingOrder] [int] NOT NULL,
	[IsLogicallyDeleted] [bit] NOT NULL
	CONSTRAINT [PK_' + @TableName + '] PRIMARY KEY CLUSTERED 
	(
		[Rsn] ASC
	)
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

ALTER TABLE [dbo].[' + @TableName + '] ADD  CONSTRAINT [DF_' + @TableName + '_IsLogicallyDeleted]  DEFAULT ((0)) FOR [IsLogicallyDeleted]
');