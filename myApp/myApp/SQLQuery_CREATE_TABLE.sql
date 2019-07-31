
DROP PROCEDURE sp_AddTable;
GO

CREATE PROCEDURE [dbo].[sp_AddTable]
AS
IF EXISTS(SELECT * from INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Persons') 
BEGIN
	DROP TABLE Persons;
END  
CREATE TABLE dbo.Persons
   (ID int IDENTITY(1,1),
    Name varchar(25) NOT NULL,
    Birthday date NOT NULL,
    Gender bit NOT NULL)
GO