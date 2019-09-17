IF NOT EXISTS (
  SELECT
    *
  FROM
    [dbo].[AspNetUsers]
  WHERE
    [UserName] = N'testuser@headspring.com'
) BEGIN INSERT [dbo].[AspNetUsers] (
  [Id],
  [AccessFailedCount],
  [ConcurrencyStamp],
  [Email],
  [EmailConfirmed],
  [LockoutEnabled],
  [LockoutEnd],
  [NormalizedEmail],
  [NormalizedUserName],
  [PasswordHash],
  [PhoneNumber],
  [PhoneNumberConfirmed],
  [SecurityStamp],
  [TwoFactorEnabled],
  [UserName]
)
VALUES
  (
    N'2b159f1d-40cf-4445-9f36-437738d6c81f',
    0,
    N'97f20006-a99f-479a-9cef-414cacdbae9d',
    N'testuser@headspring.com',
    1,
    1,
    NULL,
    N'TESTUSER@HEADSPRING.COM',
    N'TESTUSER@HEADSPRING.COM',
    N'AQAAAAEAACcQAAAAEKBVSUS75H8QD7OwCu5RmTsjk/GBgocVdIVZXFGXDHMkhDrIT5Mequ6SqvfNEBp9ew==',
    N'+10001234567',
    1,
    N'D2UGVYRADKOKUV4RGRIDYF3JOXYZMFTW',
    0,
    N'testuser@headspring.com'
  ) END
