namespace Groove.SP.Infrastructure.ReportTool
{
    public class ReportModel
    {
        public string Id { get; set; }
        public string CategoryId { get; set; }
        public string Name { get; set; }
    }

    public class CategoryModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
