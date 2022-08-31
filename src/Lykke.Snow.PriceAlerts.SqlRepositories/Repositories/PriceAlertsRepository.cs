using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Lykke.Common.MsSql;
using Lykke.Snow.Common.Model;
using Lykke.Snow.PriceAlerts.Domain.Models;
using Lykke.Snow.PriceAlerts.Domain.Repositories;
using Lykke.Snow.PriceAlerts.SqlRepositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Snow.PriceAlerts.SqlRepositories.Repositories
{
    public class PriceAlertsRepository : IPriceAlertsRepository
    {
        private readonly MsSqlContextFactory<PriceAlertsDbContext> _contextFactory;
        private readonly IMapper _mapper;

        private const string DoesNotExistExceptionMessage =
            "Database operation expected to affect 1 row(s) but actually affected 0 row(s).";

        public PriceAlertsRepository(MsSqlContextFactory<PriceAlertsDbContext> contextFactory,
            IMapper mapper)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
        }

        public async Task<Result<PriceAlert, PriceAlertErrorCodes>> GetByIdAsync(string id)
        {
            await using var context = _contextFactory.CreateDataContext();
            var entity = await context.PriceAlerts.FindAsync(id);

            if (entity == null)
                return new Result<PriceAlert, PriceAlertErrorCodes>(PriceAlertErrorCodes.DoesNotExist);

            return new Result<PriceAlert, PriceAlertErrorCodes>(_mapper.Map<PriceAlert>(entity));
        }

        public async Task<Result<PriceAlertErrorCodes>> InsertAsync(PriceAlert priceAlert)
        {
            await using var context = _contextFactory.CreateDataContext();
            var entity = _mapper.Map<PriceAlertEntity>(priceAlert);

            await context.PriceAlerts.AddAsync(entity);

            try
            {
                await context.SaveChangesAsync();
                return new Result<PriceAlertErrorCodes>();
            }
            catch (DbUpdateException e)
            {
                if (e.ValueAlreadyExistsException())
                {
                    return new Result<PriceAlertErrorCodes>(PriceAlertErrorCodes.AlreadyExists);
                }

                throw;
            }
        }

        public async Task<Result<PriceAlertErrorCodes>> UpdateAsync(PriceAlert priceAlert)
        {
            await using var context = _contextFactory.CreateDataContext();
            var entity = _mapper.Map<PriceAlertEntity>(priceAlert);

            context.Update(entity);

            try
            {
                await context.SaveChangesAsync();
                return new Result<PriceAlertErrorCodes>();
            }
            catch (DbUpdateConcurrencyException e)
            {
                if (e.Message.Contains(DoesNotExistExceptionMessage))
                    return new Result<PriceAlertErrorCodes>(PriceAlertErrorCodes.DoesNotExist);

                throw;
            }
        }

        public async Task<PaginatedResponse<PriceAlert>> GetByPageAsync(string accountId, string productId,
            List<AlertStatus> statuses, int skip, int take)
        {
            await using var context = _contextFactory.CreateDataContext();
            var query = context.PriceAlerts.AsQueryable();

            query = query.Where(x => x.AccountId == accountId);
            if (!string.IsNullOrEmpty(productId))
            {
                query = query.Where(x => x.ProductId == productId);
            }

            if (statuses != null && statuses.Count > 0)
            {
                query = query.Where(x => statuses.Contains(x.Status));
            }

            var total = await query.CountAsync();

            if (take == 0)
            {
                return new PaginatedResponse<PriceAlert>(new List<PriceAlert>(), skip, 0, total);
            }

            var entities = await query
                .OrderBy(u => u.ProductId)
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            var contents = _mapper.Map<List<PriceAlert>>(entities);
            return new PaginatedResponse<PriceAlert>(contents, skip, contents.Count, total);
        }

        public async Task<IEnumerable<PriceAlert>> GetAllActiveAlerts()
        {
            await using var context = _contextFactory.CreateDataContext();
            var query = context.PriceAlerts.AsQueryable();

            var entities = await query
                .Where(x => x.Status == AlertStatus.Active)
                .ToListAsync();
            
            return _mapper.Map<List<PriceAlert>>(entities);
        }
    }
}