using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace MangaLab.Models.Maps
{
    public class tblLogsMap : EntityTypeConfiguration<tblLogsModel>
    {

        public tblLogsMap()
        {
            HasKey(i => i.logID);
            ToTable("tbl_logs");
        }


    }
}