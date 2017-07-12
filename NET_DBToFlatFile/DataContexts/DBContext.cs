using NET_DBToFlatFile.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace NET_DBToFlatFile.DataContexts
{
    public class DBContext : DbContext
    {
        public DBContext() 
        : base("DefaultConnection")
        { }

        public DbSet<FlatFile> FlatFiles { get; set; }
        public DbSet<FlatFileWithData> FlatFilesWithData { get; set; }
        public DbSet<UploadedData> UploadedDatas { get; set; }
    }
}