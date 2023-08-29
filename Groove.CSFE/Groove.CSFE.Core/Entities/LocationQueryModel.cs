using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.CSFE.Core.Entities
{
    public class LocationQueryModel
    {
        public long Id { set; get; }
        public long CountryId { set; get; }
        public string CountryName { set; get; }
        public string Name { set; get; }
        public string LocationDescription { set; get; }
        public string EdiSonPortCode { set; get; }
    }
}
