using MangaLab.Models.Maps;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MangaLab.Models.Context
{
    public class MangaLabContext : DbContext
    {
        static MangaLabContext()
        {
            Database.SetInitializer<MangaLabContext>(null);
        }

        public MangaLabContext () : base("Name=db_mangalab") { }

        //Initialize per table
        public virtual DbSet<tblRegistrationsModel> tbl_registrations { get; set; }
        public virtual DbSet<tblLogsModel> tbl_logs { get; set; }
        public virtual DbSet<tblRolesModel> tbl_roles { get; set; }
        public virtual DbSet<tblProfilepicsModel> tbl_profilepics { get; set; }
        public virtual DbSet<tblFavoritesModel> tbl_favorites { get; set; }
        public virtual DbSet<tblHistoryModel> tbl_history { get; set; }






        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new tblRegistrationsMap());
            modelBuilder.Configurations.Add(new tblLogsMap());
            modelBuilder.Configurations.Add(new tblRolesMap());
            modelBuilder.Configurations.Add(new tblProfilepicsMap());
            modelBuilder.Configurations.Add(new tblFavoritesMap());
            modelBuilder.Configurations.Add(new tblHistoryMap());



        }
    }
}