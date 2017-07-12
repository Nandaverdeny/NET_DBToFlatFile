using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace NET_DBToFlatFile.Models
{
    [Table("FlatFileWithData")]
    public class FlatFileWithData
    {
        [Key]
        public int FileId { get; set; }
        [StringLength(255)]
        public string FileName { get; set; }
        [StringLength(100)]
        public string ContentType { get; set; }
        public byte[] Content { get; set; }
        public string FilePath { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
    }
}