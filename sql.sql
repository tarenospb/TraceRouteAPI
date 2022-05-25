CREATE DATABASE TraceRouteApi
GO
USE TraceRouteApi
GO
CREATE TABLE [StationsList](
	[regionTitle] nvarchar(128) NULL,
	[settlementTitle] nvarchar(128) NULL,
	[stationCode] nvarchar(128) NULL,
	[stationName] nvarchar(128) NULL,
	[transportType] nvarchar(128) NULL,
	[stationLongitude] nvarchar(25) NULL,
	[stationLatitude] nvarchar(25) NULL,
) ON [PRIMARY]
GO
CREATE CLUSTERED INDEX [stationCode] ON [StationsList]([stationCode]) ON [PRIMARY]
GO
