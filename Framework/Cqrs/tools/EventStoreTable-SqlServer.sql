DECLARE	@TableName	NVARCHAR(50);
SET		@TableName	= 'EventStore';
-- If you are going to use snapshots, run this script again replacing 'EventStore' above with 'Snapshots'

EXECUTE ('
CREATE TABLE [dbo].[' + @TableName + '](
	[EventId] [uniqueidentifier] NOT NULL,
	[Data] [text] NOT NULL,
	[EventType] [nvarchar](MAX) NOT NULL,
	[AggregateId] [nvarchar](445) NOT NULL,
	[AggregateRsn] [uniqueidentifier] NOT NULL,
	[Version] [bigint] NOT NULL,
	[Timestamp] [datetime] NOT NULL,
	[CorrelationId] [uniqueidentifier] NOT NULL,
	CONSTRAINT [PK_' + @TableName + '] PRIMARY KEY NONCLUSTERED 
	(
		[EventId] ASC
	) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON),
	CONSTRAINT [UIX_' + @TableName + '_AggregateId_Version] UNIQUE NONCLUSTERED 
	(
		[AggregateId] ASC,
		[Version] DESC
	) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)

CREATE PARTITION FUNCTION [PF_' + @TableName + '_AggregateRsn] (uniqueidentifier)
AS RANGE RIGHT
FOR VALUES
(
   N''00000000-0000-0000-0000-0D0000000000'', 
   N''00000000-0000-0000-0000-1A0000000000'', 
   N''00000000-0000-0000-0000-270000000000'', 
   N''00000000-0000-0000-0000-340000000000'', 
   N''00000000-0000-0000-0000-410000000000'', 
   N''00000000-0000-0000-0000-4E0000000000'', 
   N''00000000-0000-0000-0000-5B0000000000'', 
   N''00000000-0000-0000-0000-680000000000'', 
   N''00000000-0000-0000-0000-750000000000'', 
   N''00000000-0000-0000-0000-820000000000'', 
   N''00000000-0000-0000-0000-8F0000000000'', 
   N''00000000-0000-0000-0000-9C0000000000'', 
   N''00000000-0000-0000-0000-A90000000000'', 
   N''00000000-0000-0000-0000-B60000000000'', 
   N''00000000-0000-0000-0000-C30000000000'', 
   N''00000000-0000-0000-0000-D00000000000'', 
   N''00000000-0000-0000-0000-DD0000000000'', 
   N''00000000-0000-0000-0000-EA0000000000'', 
   N''00000000-0000-0000-0000-F70000000000'' 
) ;

CREATE PARTITION SCHEME [PS_' + @TableName + '_AggregateRsn]
AS PARTITION [PF_' + @TableName + '_AggregateRsn] ALL TO ([PRIMARY]) 

CREATE CLUSTERED INDEX [IX_' + @TableName + '_AggregateRsn] ON [dbo].[' + @TableName + ']
(
	[AggregateRsn] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
ON [PS_' + @TableName + '_AggregateRsn](AggregateRsn)

CREATE NONCLUSTERED INDEX [IX_' + @TableName + '_EventId] ON [dbo].[' + @TableName + ']
(
	[EventId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

CREATE NONCLUSTERED INDEX [IX_' + @TableName + '_CorrelationId] ON [dbo].[' + @TableName + ']
(
	[CorrelationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

CREATE NONCLUSTERED INDEX [IX_' + @TableName + '_Timestamp] ON [dbo].[' + @TableName + ']
(
	[Timestamp] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

CREATE NONCLUSTERED INDEX [IX_' + @TableName + '_Timestamp_EventId_CorrelationId] ON [dbo].[' + @TableName + ']
(
	[Timestamp] DESC,
	[EventId] ASC,
	[CorrelationId] ASC
)
INCLUDE ([EventType]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF)

CREATE STATISTICS [ST_' + @TableName + '_CorrelationId_Timestamp] ON [dbo].[' + @TableName + ']([CorrelationId], [Timestamp])

CREATE STATISTICS [ST_' + @TableName + '_EventId_CorrelationId_Timestamp] ON [dbo].[' + @TableName + ']([EventId], [CorrelationId], [Timestamp])

SELECT ps.name,pf.name,boundary_id,value
, ps.*
FROM sys.partition_schemes ps
INNER JOIN sys.partition_functions pf ON pf.function_id=ps.function_id
INNER JOIN sys.partition_range_values prf ON pf.function_id=prf.function_id

SELECT o.name objectname,i.name indexname, partition_id, partition_number, [rows]
FROM sys.partitions p
INNER JOIN sys.objects o ON o.object_id=p.object_id
INNER JOIN sys.indexes i ON i.object_id=p.object_id and p.index_id=i.index_id
WHERE o.name = ''' + @TableName + '''
')