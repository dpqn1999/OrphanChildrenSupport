USE OrphanChildrenSupport
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PersonalProfile](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AccountName] [nvarchar](max) NOT NULL,
	[FullName] [nvarchar](max) NULL,
	[Gender] [bit] NOT NULL,
	[BirthDay] [datetime2](7) NULL,
	[Address] [nvarchar](max) NULL,
	[Mobile] [nvarchar](max) NULL,
	[Email] [nvarchar](max) NULL,
	[CreatedBy] [nvarchar](max) NULL,
	[CreatedTime] [datetime2](7) NULL,
	[LastModified] [datetime2](7) NULL,
	[ModifiedBy] [nvarchar](max) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_PersonalProfile] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

SET IDENTITY_INSERT [dbo].[PersonalProfile] ON 
INSERT [dbo].[PersonalProfile] ([Id], [AccountName], [FullName], [Gender], [BirthDay], [Address], [Mobile], [Email], [CreatedBy], [CreatedTime], [LastModified], [ModifiedBy], [IsDeleted]) VALUES (1,  N'tht0310', N'Tran Hoang Tien', 1, NULL, N'', N'0373831808', N'thtien0310@gmail.com', N'', CAST(N'2021-07-13T02:12:06.4666667' AS DateTime2), NULL, NULL, 0)
SET IDENTITY_INSERT [dbo].[PersonalProfile] OFF
GO