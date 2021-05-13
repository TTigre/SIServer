using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SIServer.Models;

namespace SIServer.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Document> Documents{get; set;}
        public DbSet<WordRepetition> WordRepetitions{get; set;}
        public DbSet<RelevanciaPalabraDocumento> RelevanciaPalabraDocumentos{get; set;}
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Document>().HasKey(x => x.DocID);

            builder.Entity<WordRepetition>().HasKey(x => new {x.DocumentID, x.Word});
            builder.Entity<WordRepetition>().HasOne(x => x.Document).WithMany(x => x.Palabras).HasForeignKey(x => x.DocumentID);

            builder.Entity<RelevanciaPalabraDocumento>().HasKey(x => new{x.DocumentID, x.Palabra});
            builder.Entity<RelevanciaPalabraDocumento>().HasOne(x => x.Documento).WithMany(x => x.Relevancias).HasForeignKey(x => x.DocumentID);
            builder.Entity<RelevanciaPalabraDocumento>().HasOne(x => x.Repetition).WithOne(x => x.Relevancia).HasForeignKey(typeof(RelevanciaPalabraDocumento),"DocumentID","Palabra");
        }
    }
}
