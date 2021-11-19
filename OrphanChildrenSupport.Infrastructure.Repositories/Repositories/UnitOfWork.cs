using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using OrphanChildrenSupport.Infrastructure.Data;
using OrphanChildrenSupport.Services.Models;
using Microsoft.Data.SqlClient;

namespace OrphanChildrenSupport.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly OrphanChildrenSupportDbContext _context;
        private readonly ILogger _logger;
        private string _connectionString;

        #region DBSet Repotiory
        private IAsyncRepository<PersonalProfile> _personalProfileRepository;

        public IAsyncRepository<PersonalProfile> PersonalProfileRepository => _personalProfileRepository ?? (_personalProfileRepository = new EfRepository<PersonalProfile>(_context));
        #endregion

        public UnitOfWork(string connectionString)
        {
            _connectionString = connectionString;
            var optionsBuilder = new DbContextOptionsBuilder<OrphanChildrenSupportDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            _context = new OrphanChildrenSupportDbContext(optionsBuilder.Options);
            var loggerFactory = new LoggerFactory();
            _logger = loggerFactory.CreateLogger<UnitOfWork>();
        }

        public async Task SaveChanges()
        {
            const string loggerHeader = "UnitOfWork";
            using (var dbContextTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _context.SaveChangesAsync();
                    await dbContextTransaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{loggerHeader} have error: {ex.Message}");
                    await dbContextTransaction.RollbackAsync();
                    await SaveErrorLog(ex);
                    throw;
                }
            }
        }

        public async Task SaveErrorLog(Exception ex)
        {
            try
            {
                int code = 0;
                var w32ex = ex as Win32Exception;
                if (w32ex == null)
                {
                    w32ex = ex.InnerException as Win32Exception;
                }
                if (w32ex != null)
                {
                    code = w32ex.ErrorCode;
                }

                var ErrorLog = new ErrorLog
                {
                    Time = DateTime.UtcNow,
                    Number = code.ToString(),
                    Name = ex.GetType().Name,
                    Description = ex.Message,
                    IsDeleted = false
                };

                var commandText = @"INSERT ErrorLog (Name, Number, Time, Description, IsDeleted)
                                    VALUES (@Name, @Number, @Time, @Description, I@sDeleted)";

                SqlParameter[] parameters = new[]{
                    new SqlParameter("@Name", ErrorLog.Name),
                    new SqlParameter("@Number", ErrorLog.Number),
                    new SqlParameter("@Time", ErrorLog.Time),
                    new SqlParameter("@Description", ErrorLog.Description),
                    new SqlParameter("@IsDeleted", ErrorLog.IsDeleted),
                };
                await _context.Database.ExecuteSqlRawAsync(commandText, parameters);
            }
            catch { }
        }

        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }

        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr.IsNull(column.ColumnName) ? null : dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }

        #region IDisposable Support  
        private bool _disposedValue = false;

        protected virtual async Task Dispose(bool disposing)
        {
            if (_disposedValue) return;

            if (disposing)
            {
                await _context.DisposeAsync();
            }

            _disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true).Wait();
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
