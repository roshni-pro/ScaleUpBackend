using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Services.CompanyAPI.Persistence;
using ScaleUP.Services.CompanyDTO.Master;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ScaleUP.Services.CompanyAPI.Helpers
{

    public class AuditLogHelper
    {
        public readonly CompanyApplicationDbContext _context;
        public AuditLogHelper(CompanyApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLogDc>> GetAuditLogs(long EntityId, string EntityName, int Skip, int Take)
        {
            var query = _context.AuditLogs.Where(x => x.EntityId == EntityId && x.EntityName == EntityName && !string.IsNullOrEmpty(x.Changes));//&& x.Action == "Modified"

            var totalRecords = await query.CountAsync();
            var auditLogs = await query.Select(x => new AuditLogDc
            {
                Id = x.Id,
                UserId = x.UserId,
                EntityId = x.EntityId,
                DatabaseName = x.DatabaseName,
                EntityName = x.EntityName,
                Action = x.Action,
                Timestamp = x.Timestamp,
                Changes = x.Changes,
                TotalRecords = totalRecords
            }).OrderByDescending(x => x.Timestamp).Skip(Skip).Take(Take).ToListAsync();
            return auditLogs;
        }

        public async Task<List<AuditLogDc>> GetAuditLogsByRegex(long EntityId, string EntityName)
        {
            var auditLogs = await _context.AuditLogs.Where(x => x.EntityId == EntityId && x.EntityName == EntityName && x.Action == "Modified").Select(x => new AuditLogDc
            {
                Id = x.Id,
                UserId = x.UserId,
                EntityId = x.EntityId,
                DatabaseName = x.DatabaseName,
                EntityName = x.EntityName,
                Action = x.Action,
                Timestamp = x.Timestamp,
                Changes = x.Changes
            }).OrderByDescending(x => x.Timestamp).ToListAsync();
            try
            {
                if (auditLogs != null && auditLogs.Any())
                {
                    foreach (var auditLog in auditLogs)
                    {
                        if (!string.IsNullOrEmpty(auditLog.Changes))
                        {
                            string input = auditLog.Changes;

                            string pattern = @"(?<column>\w+): From '(?<from>[^']*)' to '(?<to>[^']*)'";

                            Regex regex = new Regex(pattern);
                            MatchCollection matches = regex.Matches(input);

                            auditLog.Fields = new List<Field>();
                            foreach (Match match in matches)
                            {
                                auditLog.Fields.Add(new Field
                                {
                                    FieldName = match.Groups["column"].Value,
                                    OldValue = match.Groups["from"].Value,
                                    NewValue = match.Groups["to"].Value
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return auditLogs;
        }
    }
}
