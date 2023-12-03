using Microsoft.EntityFrameworkCore;
using Vertragsmanagement.DomainObjects;

namespace Vertragsmanagement.DataAccess;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions options) : base(options)
    {
        
    }

    //Das ist eine Datenbanktabelle
    // public DbSet<Customer> Customer { get; set; }

    public DbSet<Vertrag> Verträge { get; set; }

    public DbSet<Adresse> Adressen { get; set; } 
    public DbSet<Kreditor> Kreditoren { get; set; }
    
    public DbSet<Debitor> Debitoren { get; set; }
    
    public DbSet<Bankverbindung> Bankverbindungen { get; set; }
    
    public DbSet<Currency> Currencys { get; set; }
}