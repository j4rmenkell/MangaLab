using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace MangaLab.Models.Maps
{
    public class tblProfilepicsMap : EntityTypeConfiguration<tblProfilepicsModel>
    {
        public tblProfilepicsMap()
        {
            HasKey(i => i.imageID);
            ToTable("tbl_profilepics");
        }
    }
}