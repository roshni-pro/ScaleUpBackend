﻿using Microsoft.EntityFrameworkCore;
using ScaleUP.BuildingBlocks.GRPC.Contracts.Common;
using ScaleUP.Services.LocationAPI.Persistence;
using System.Text.RegularExpressions;

namespace ScaleUP.Services.LocationAPI.Helpers
{
    public class AuditLogHelper
    {
        public readonly ApplicationDbContext _context;
        public AuditLogHelper(ApplicationDbContext context)
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
    }
}