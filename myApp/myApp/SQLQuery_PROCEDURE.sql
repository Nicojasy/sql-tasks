USE DB
GO

DROP PROCEDURE sp_Select;
GO
DROP PROCEDURE sp_AddPersonRandom;
GO
DROP PROCEDURE sp_SelectOptimized;
GO
DROP PROCEDURE sp_GetPersonsNames;
GO
DROP PROCEDURE sp_AddPerson;
GO
DROP PROCEDURE sp_GetPersons;
GO
DROP INDEX Persons.persons_idx_gender_name;
GO
CREATE INDEX persons_idx_gender_name ON Persons (Gender,Name);
GO

CREATE PROCEDURE [dbo].[sp_GetPersonsNames]
AS
	SELECT * FROM Persons
	ORDER BY Name
GO

CREATE PROCEDURE [dbo].[sp_AddPersonRandom]
    @name varchar(25),
    @birthday date,
    @gender bit
AS
    INSERT INTO Persons (Name, Birthday, Gender)
    VALUES
	(@name, @birthday, @gender)
	SELECT SCOPE_IDENTITY()
GO

CREATE PROCEDURE [dbo].[sp_AddPerson]
    @name varchar(25),
    @birthday date,
    @gender bit
AS
    INSERT INTO Persons (Name, Birthday, Gender)
    VALUES (@name, @birthday, @gender)
	SELECT SCOPE_IDENTITY()
GO

CREATE PROCEDURE [dbo].[sp_GetPersons]
AS
	SELECT * FROM Persons
GO

CREATE PROCEDURE [dbo].[sp_Select]
AS
	SELECT Name, Gender FROM Persons
	WHERE Name like 'f%' AND Gender = 0
GO

CREATE PROCEDURE [dbo].[sp_SelectOptimized]
AS
SELECT Persons.Name, Persons.Gender FROM Persons
WHERE Persons.Name LIKE 'f%' AND Persons.Gender = 0
GO