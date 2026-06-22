
DECLARE	@TableName	NVARCHAR(50);
SET		@TableName	= 'Logs';

EXECUTE('
/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION

CREATE TABLE dbo.Tmp_' + @TableName + '
	(
	Id int NOT NULL IDENTITY (1, 1),
	Raised datetime NOT NULL,
	[Level] nvarchar(50) NOT NULL,
	CorrelationId uniqueidentifier NOT NULL,
	Message nvarchar(MAX) NULL,
	Container nvarchar(MAX) NULL,
	Exception nvarchar(MAX) NULL,
	Module nvarchar(255) NULL,
	Instance nvarchar(255) NULL,
	Environment nvarchar(255) NULL,
	EnvironmentInstance nvarchar(255) NULL,
	MetaData nvarchar(MAX) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]

ALTER TABLE dbo.Tmp_' + @TableName + ' SET (LOCK_ESCALATION = TABLE)

IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = ''dbo''
                 AND  TABLE_NAME = ''' + @TableName + '''))
BEGIN
	DROP TABLE dbo.' + @TableName + '
END

EXECUTE sp_rename N''dbo.Tmp_' + @TableName + ''', N''' + @TableName + ''', ''OBJECT''

ALTER TABLE dbo.' + @TableName + ' ADD CONSTRAINT
	PK_' + @TableName + ' PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


CREATE NONCLUSTERED INDEX IX_' + @TableName + '_CorrelationId ON dbo.' + @TableName + '
	(
	CorrelationId
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

 ALTER INDEX IX_' + @TableName + '_CorrelationId ON dbo.' + @TableName + ' DISABLE 

CREATE NONCLUSTERED INDEX IX_' + @TableName + '_Environment ON dbo.' + @TableName + '
	(
	Environment
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

 ALTER INDEX IX_' + @TableName + '_Environment ON dbo.' + @TableName + ' DISABLE 

CREATE NONCLUSTERED INDEX IX_' + @TableName + '_EnvironmentInstance ON dbo.' + @TableName + '
	(
	EnvironmentInstance
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

 ALTER INDEX IX_' + @TableName + '_EnvironmentInstance ON dbo.' + @TableName + ' DISABLE 

CREATE NONCLUSTERED INDEX IX_' + @TableName + '_Instance ON dbo.' + @TableName + '
	(
	Instance
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

 ALTER INDEX IX_' + @TableName + '_Instance ON dbo.' + @TableName + ' DISABLE 

CREATE NONCLUSTERED INDEX IX_' + @TableName + '_Level ON dbo.' + @TableName + '
	(
	[Level]
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

 ALTER INDEX IX_' + @TableName + '_Level ON dbo.' + @TableName + ' DISABLE 

CREATE NONCLUSTERED INDEX IX_' + @TableName + '_Module ON dbo.' + @TableName + '
	(
	Module
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

 ALTER INDEX IX_' + @TableName + '_Module ON dbo.' + @TableName + ' DISABLE 

CREATE NONCLUSTERED INDEX IX_' + @TableName + '_Raised ON dbo.' + @TableName + '
	(
	Raised
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

 ALTER INDEX IX_' + @TableName + '_Raised ON dbo.' + @TableName + ' DISABLE 

COMMIT
');