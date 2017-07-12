using LINQtoCSV;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace NET_DBToFlatFile.Models
{
    [Table("UploadedData")]
    public class UploadedData
    {
        [CsvColumn(Name="Id", FieldIndex=1)]
        public int ID { get; set; }
        [CsvColumn(Name = "Name", FieldIndex = 2)]
        public string Name { get; set; }
        [CsvColumn(Name = "Description", FieldIndex = 3)]
        public string Description { get; set; }
    }
}