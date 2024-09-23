using DbAccess.Context;
using DbAccess.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace DbAccess.Services
{
    public class DbService
    {
        private readonly LogisticCenterContext _context;

        public DbService()
        {
            _context = new LogisticCenterContext();
        }

        public string[] GetTablesName() => _context
                                                .GetType()
                                                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                                                .Select(p => p.Name.ToLower())
                                                .ToArray();
        public IQueryable<object>? GetEntriesByTableName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            switch (name.ToLower())
            {
                case "cars": return _context.Cars.AsNoTracking();
                case "cargos": return _context.Cargos.AsNoTracking();
                case "cargostransports": return _context.CargosTransports.AsNoTracking();
                case "customers": return _context.Customers.AsNoTracking();
                case "drivers": return _context.Drivers.AsNoTracking();
                case "routes": return _context.Routes.AsNoTracking();
                case "settlements": return _context.Settlements.AsNoTracking();
                case "tariffs": return _context.Tariffs.AsNoTracking();
                default: return null;
            }
        }
        public IEnumerable<Car>? FindCars(Func<Car, bool> expression) => _context.Cars.AsNoTracking().Where(expression);
        public IEnumerable<Cargo>? FindCargos(Func<Cargo, bool> expression) => _context.Cargos.AsNoTracking().Where(expression);
    }
}
