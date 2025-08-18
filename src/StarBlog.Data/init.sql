create table Category
(
    Id       INTEGER,
    Name     NVARCHAR(255),
    ParentId INTEGER not null,
    primary key (Id autoincrement)
);

create table FeaturedCategory
(
    Id           INTEGER,
    CategoryId   INTEGER not null,
    Name         NVARCHAR(255),
    Description  NVARCHAR(255),
    IconCssClass NVARCHAR(255),
    primary key (Id autoincrement)
);

create table FeaturedPhoto
(
    Id      INTEGER,
    PhotoId NVARCHAR(255),
    primary key (Id autoincrement)
);

create table FeaturedPost
(
    Id     INTEGER,
    PostId NVARCHAR(255),
    primary key (Id autoincrement)
);

create table Photo
(
    Id         NVARCHAR(255),
    Title      NVARCHAR(255),
    Location   NVARCHAR(255),
    FilePath   NVARCHAR(255),
    Height     INTEGER  not null,
    Width      INTEGER  not null,
    CreateTime DATETIME not null,
    primary key (Id)
);

create table Post
(
    Id             NVARCHAR(255),
    Title          NVARCHAR(255),
    Summary        NVARCHAR(255),
    Content        NVARCHAR(255),
    Path           NVARCHAR(255),
    CreationTime   DATETIME not null,
    LastUpdateTime DATETIME not null,
    CategoryId     INTEGER  not null,
    Categories     NVARCHAR(255),
    primary key (Id)
);

create table TopPost
(
    Id     INTEGER,
    PostId NVARCHAR(255),
    primary key (Id autoincrement)
);

create table User
(
    Id       NVARCHAR(255),
    Name     NVARCHAR(255),
    Password NVARCHAR(255),
    primary key (Id)
);


INSERT INTO FeaturedCategory (Id, CategoryId, Name, Description, IconCssClass) VALUES (1, 3, 'C# / Asp.Net Core', 'Paragraph of text beneath the heading to explain the heading. We''ll add onto it with another sentence and probably just keep going until we run out of words.', 'fa-solid fa-c');
INSERT INTO FeaturedCategory (Id, CategoryId, Name, Description, IconCssClass) VALUES (2, 8, 'Python / Django', 'Paragraph of text beneath the heading to explain the heading. We''ll add onto it with another sentence and probably just keep going until we run out of words.', 'fa-brands fa-python');
INSERT INTO FeaturedCategory (Id, CategoryId, Name, Description, IconCssClass) VALUES (3, 2, 'Flutter / Android', 'Paragraph of text beneath the heading to explain the heading. We''ll add onto it with another sentence and probably just keep going until we run out of words.', 'fa-brands fa-android');

