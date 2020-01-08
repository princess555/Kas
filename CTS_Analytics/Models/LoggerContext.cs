using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace CTS_Analytics.Models
{
    public class LoggerContext:DbContext
    {
        public LoggerContext() : base("DefaultConnection")
        {
        }

        public DbSet<ExceptionDetail> ExceptionDetails { get; set; }
    }
}