using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PatientService.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PatientDbContext>
    {
        public PatientDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PatientDbContext>();

            optionsBuilder.UseSqlServer("Server=LP-PG0588F8\\SQLEXPRESS;Database=HospitalPatientDb;Trusted_Connection=True;TrustServerCertificate=True;");

            return new PatientDbContext(optionsBuilder.Options);
        }

        //PatientDbContext IDesignTimeDbContextFactory<PatientDbContext>.CreateDbContext(string[] args)
        //{
        //    throw new NotImplementedException();
        //}
    }
}