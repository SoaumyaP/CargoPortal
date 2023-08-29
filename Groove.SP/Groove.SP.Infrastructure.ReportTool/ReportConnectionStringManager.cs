using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Telerik.Reporting;
using Telerik.Reporting.XmlSerialization;

namespace Groove.SP.Infrastructure.ReportTool
{
    public class ReportConnectionStringManager
    {
        static string reportServerAddress = "http://report.csglobal-ltl.com:83/";
        public readonly string reportFolder = @"Reports\Main";
        readonly string mainEnv = "Main";
        readonly string env;
        readonly string connectionString;
        readonly HttpClient httpClient;
        List<ReportModel> reports;
        List<CategoryModel> categories;

        public ReportConnectionStringManager(string connectionString, HttpClient httpClient, string env, List<ReportModel> reports, List<CategoryModel> categories)
        {
            this.env = env;
            this.connectionString = connectionString;
            this.httpClient = httpClient;
            this.reports = reports;
            this.categories = categories;
        }

        public void UpdateConnectionString(string reportFile)
        {
            var outputFile = reportFile.Replace(mainEnv, env);
            var dir = Path.GetDirectoryName(outputFile);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var reportSource = new UriReportSource { Uri = reportFile };

            UpdateReportSource(reportSource, outputFile);
        }

        public ReportSource UpdateReportSource(ReportSource sourceReportSource, string outputFile = "")
        {
            if (sourceReportSource is UriReportSource)
            {
                var uriReportSource = (UriReportSource)sourceReportSource;
                // unpackage TRDP report
                // http://docs.telerik.com/reporting/report-packaging-trdp#unpackaging
                // var reportInstance = UnpackageReport(uriReportSource);
                // or deserialize TRDX report(legacy format)
                // http://docs.telerik.com/reporting/programmatic-xml-serialization#deserialize-report-definition-from-xml-file
                var reportInstance = DeserializeReport(uriReportSource);

                ValidateReportSource(uriReportSource.Uri);
                this.SetConnectionString(reportInstance);
                return CreateInstanceReportSource(reportInstance, uriReportSource, outputFile);
            }

            if (sourceReportSource is XmlReportSource)
            {
                var xml = (XmlReportSource)sourceReportSource;
                ValidateReportSource(xml.Xml);
                var reportInstance = this.DeserializeReport(xml);
                this.SetConnectionString(reportInstance);
                return CreateInstanceReportSource(reportInstance, xml, outputFile);
            }

            if (sourceReportSource is InstanceReportSource)
            {
                var instanceReportSource = (InstanceReportSource)sourceReportSource;
                this.SetConnectionString((ReportItemBase)instanceReportSource.ReportDocument);
                return instanceReportSource;
            }

            if (sourceReportSource is TypeReportSource)
            {
                var typeReportSource = (TypeReportSource)sourceReportSource;
                var typeName = typeReportSource.TypeName;
                ValidateReportSource(typeName);
                var reportType = Type.GetType(typeName);
                var reportInstance = (Report)Activator.CreateInstance(reportType);
                this.SetConnectionString((ReportItemBase)reportInstance);
                return CreateInstanceReportSource(reportInstance, typeReportSource, outputFile);
            }

            throw new NotImplementedException("Handler for the used ReportSource type is not implemented.");
        }

        ReportSource CreateInstanceReportSource(IReportDocument report, ReportSource originalReportSource, string outputFile = "")
        {
            if (!string.IsNullOrEmpty(outputFile))
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(outputFile))
                {
                    ReportXmlSerializer xmlSerializer =
                        new ReportXmlSerializer();

                    xmlSerializer.Serialize(xmlWriter, report);
                }

                Upload(outputFile, env);
            }

            var instanceReportSource = new InstanceReportSource { ReportDocument = report };
            instanceReportSource.Parameters.AddRange(originalReportSource.Parameters);
            return instanceReportSource;
        }

        void ValidateReportSource(string value)
        {
            if (value.Trim().StartsWith("="))
            {
                throw new InvalidOperationException("Expressions for ReportSource are not supported when changing the connection string dynamically");
            }
        }

        void Upload(string outputFile, string env)
        {
            try
            {
                var bytes = File.ReadAllBytes(outputFile);

                var name = Path.GetFileNameWithoutExtension(outputFile);
                var category = categories.FirstOrDefault(c => c.Name == env);
                var categoryId = string.Empty;
                if (category != null)
                {
                    categoryId = category.Id;
                    var report = reports.FirstOrDefault(r => r.Name == name && r.CategoryId == categoryId);
                    if (report != null)
                    {
                        var deleteResponse = httpClient.DeleteAsync($"{reportServerAddress}api/reportserver/reports/{report.Id}").Result;
                        if (!deleteResponse.IsSuccessStatusCode)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    var postCategoryResponse = httpClient.PostAsync($"{reportServerAddress}api/reportserver/categories", new StringContent(JsonConvert.SerializeObject(new CategoryModel() { Name = env }), System.Text.Encoding.UTF8, "application/json")).Result;
                    if (!postCategoryResponse.IsSuccessStatusCode)
                    {
                        return;
                    }
                    else
                    {
                        categoryId = postCategoryResponse.Content.ReadAsStringAsync().Result;
                    }
                }

                var rq = new
                {
                    Name = name,
                    Description = $"{name} - {env}",
                    CategoryId = categoryId,
                    ReportFile = new
                    {
                        FileName = Path.GetFileName(outputFile),
                        Buffer = Convert.ToBase64String(bytes)
                    }
                };

                var response = httpClient.PostAsync($"{reportServerAddress}api/reportserver/reports", new StringContent(JsonConvert.SerializeObject(rq), System.Text.Encoding.UTF8, "application/json")).Result;
                if (!response.IsSuccessStatusCode)
                {
                    return;
                }
                else
                {
                    Console.WriteLine($"Uploaded '{rq.Description}' report!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed! Exception: {e.Message}");
            }
        }

        private async Task<string> GetCategoryIdAsync(string categoryName)
        {
            var response = await httpClient.GetAsync(Path.Combine($"{reportServerAddress}api/reportserver/", "Categories"));
            response.EnsureSuccessStatusCode();

            var jsonResult = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<dynamic>>(jsonResult);
            var category = result.Single(x => string.Equals((string)x.Name, categoryName, StringComparison.OrdinalIgnoreCase));

            return category.Id;
        }

        Report UnpackageReport(UriReportSource uriReportSource)
        {
            var reportPackager = new ReportPackager();
            using (var sourceStream = System.IO.File.OpenRead(uriReportSource.Uri))
            {
                var report = (Report)reportPackager.UnpackageDocument(sourceStream);
                return report;
            }
        }

        Report DeserializeReport(UriReportSource uriReportSource)
        {
            var settings = new System.Xml.XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            using (var xmlReader = System.Xml.XmlReader.Create(uriReportSource.Uri, settings))
            {
                var xmlSerializer = new Telerik.Reporting.XmlSerialization.ReportXmlSerializer();
                var report = (Telerik.Reporting.Report)xmlSerializer.Deserialize(xmlReader);
                return report;
            }
        }

        Report DeserializeReport(XmlReportSource xmlReportSource)
        {
            var settings = new System.Xml.XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            var textReader = new System.IO.StringReader(xmlReportSource.Xml);
            using (var xmlReader = System.Xml.XmlReader.Create(textReader, settings))
            {
                var xmlSerializer = new Telerik.Reporting.XmlSerialization.ReportXmlSerializer();
                var report = (Telerik.Reporting.Report)xmlSerializer.Deserialize(xmlReader);
                return report;
            }
        }

        void SetConnectionString(ReportItemBase reportItemBase)
        {
            if (reportItemBase.Items.Count < 1)
                return;

            if (reportItemBase is Report)
            {
                var report = (Report)reportItemBase;

                if (report.DataSource is SqlDataSource)
                {
                    var sqlDataSource = (SqlDataSource)report.DataSource;
                    sqlDataSource.ConnectionString = connectionString;
                }
                foreach (var parameter in report.ReportParameters)
                {
                    if (parameter.AvailableValues.DataSource is SqlDataSource)
                    {
                        var sqlDataSource = (SqlDataSource)parameter.AvailableValues.DataSource;
                        sqlDataSource.ConnectionString = connectionString;
                    }
                }
            }

            foreach (var item in reportItemBase.Items)
            {
                // Since Telerik reporting server only allow 1 level of category,
                // so all reports and subreports are upload to same env category.
                // Change category of subreport to current env category.
                if (item is SubReport subReport)
                {
                    if (subReport.ReportSource is UriReportSource uriReportSource)
                    {
                        uriReportSource.Uri = env + "/" + uriReportSource.Uri;
                    }
                }

                //recursively set the connection string to the items from the Items collection
                SetConnectionString(item);

                //set the drillthrough report connection strings
                var drillThroughAction = item.Action as NavigateToReportAction;
                if (null != drillThroughAction)
                {
                    var updatedReportInstance = this.UpdateReportSource(drillThroughAction.ReportSource);
                    drillThroughAction.ReportSource = updatedReportInstance;
                }

                //Covers all data items(Crosstab, Table, List, Graph, Map and Chart)
                if (item is DataItem)
                {
                    var dataItem = (DataItem)item;
                    if (dataItem.DataSource is SqlDataSource)
                    {
                        var sqlDataSource = (SqlDataSource)dataItem.DataSource;
                        sqlDataSource.ConnectionString = connectionString;
                        continue;
                    }
                }

            }
        }
    }
}
