IF OBJECT_ID('dbo.Sys_Notifications','U') IS NULL
CREATE TABLE Sys_Notifications (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    UserId          INT             NOT NULL,
    Title           NVARCHAR(255)   NOT NULL,
    Message         NVARCHAR(1000)  NULL,
    Type            VARCHAR(50)     NOT NULL,     -- WorkflowSubmit / WorkflowApprove / WorkflowReject / WorkflowPublish / System
    EntityType      VARCHAR(100)    NULL,         -- SubmissionWorkflow / VillageDraft ...
    EntityId        VARCHAR(50)     NULL,
    LinkUrl         VARCHAR(500)    NULL,         -- deep link FE, VD /submissions/123
    IsRead          BIT             NOT NULL DEFAULT 0,
    ReadAt          DATETIME        NULL,
    CreatedAt       DATETIME        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Sys_Notifications PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Notification_User FOREIGN KEY (UserId) REFERENCES Sys_Users(Id)
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Notification_User_Read')
    CREATE NONCLUSTERED INDEX IX_Notification_User_Read 
    ON Sys_Notifications(UserId, IsRead, CreatedAt DESC) 
    INCLUDE (Type, Title, EntityType, EntityId);
GO
