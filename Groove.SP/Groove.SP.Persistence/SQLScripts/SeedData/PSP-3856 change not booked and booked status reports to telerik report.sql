update Reports
set 
	TelerikReportId = '<Not booked status report"s id>',
	TelerikCategoryId = '<TelerikCategoryId>',
	ReportUrl = '/telerik-report?category=CS Portal&reportkey=Not Booked Status Report&reportserverurl=<Telerik report server name>',
	TelerikCategoryName = 'CSPortal',
	SchedulingApply = 1
where Id = 2 and ReportName = 'Not Booked Status Report'


update Reports
set 
	TelerikReportId = '<Booked status report"s Id>',
	TelerikCategoryId = '<TelerikCategoryId>',
	ReportUrl = '/telerik-report?category=CS Portal&reportkey=Booked Status Report&reportserverurl=<Telerik report server name>',
	TelerikCategoryName = 'CSPortal',
	SchedulingApply = 1
where Id = 1 and ReportName = 'Booked Status Report'

