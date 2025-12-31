using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace MangaLab.Models.Maps
{
    public class tblHistoryMap : EntityTypeConfiguration<tblHistoryModel>
    {
        public tblHistoryMap() 
        { 
            HasKey(i => i.historyID);
            ToTable("tbl_history");
        }
    }
}