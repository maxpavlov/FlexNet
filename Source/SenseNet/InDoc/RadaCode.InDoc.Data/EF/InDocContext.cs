using System.Data.Entity;
using RadaCode.InDoc.Data.DocumentNaming;

namespace RadaCode.InDoc.Data.EF
{
    public class InDocContext : DbContext
    {
        public DbSet<NamingApproach> NamingApproaches { get; set; }
    }
}
