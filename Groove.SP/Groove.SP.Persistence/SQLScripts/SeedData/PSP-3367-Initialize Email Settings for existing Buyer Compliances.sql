BEGIN TRAN;

CREATE TABLE #EmailSetting
(
	EmailType INT
)
declare @current datetime2(7) = getdate()

insert into #EmailSetting (EmailType) values (10) --Booking Import via API
insert into #EmailSetting (EmailType) values (20) --Booking Imported Failure
insert into #EmailSetting (EmailType) values (30) --Booking Imported Successfully
insert into #EmailSetting (EmailType) values (40) --Booking Approval
insert into #EmailSetting (EmailType) values (50) --Booking Rejected
insert into #EmailSetting (EmailType) values (60) --Booking Approved
insert into #EmailSetting (EmailType) values (70) --Booking Confirmed
insert into #EmailSetting (EmailType) values (80) --Booking Cargo Received

INSERT INTO EmailSettings
(
	[BuyerComplianceId],
	[EmailType],
	[CreatedBy],
	[CreatedDate]
)
SELECT
	bc.Id,
	es.EmailType,
	'System',
	@current
FROM BuyerCompliances bc left join #EmailSetting es ON 1 = 1
drop table #EmailSetting

COMMIT TRAN;