﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SmartFleet.Core.Data;
using SmartFleet.Data;
using SmartFleet.MobileUnit.Domain.Dtos.MobileUnit;
using SmartFleet.MobileUnit.Domain.MobileUnit.Dtos;
using SmartFleet.MobileUnit.Domain.MobileUnit.Queries;
using SmartFleet.Web.Framework.DataTables;

namespace SmartFleet.MobileUnit.Domain.MobileUnit.Handlers
{
    public class MobileUnitQueriesHandler :
        IRequestHandler<GetMobileUnitsWithoutVehicleIdQuery, IEnumerable<MobileUnitSelectListDto>>,
        IRequestHandler<GetMobileUnitByIdQuery, MobileUnitDto>,
        IRequestHandler<GetMobileUnitByImeiQuery, MobileUnitDto>
    {
        private readonly IDbContextScopeFactory _dbContextScopeFactory;
        private readonly IMapper _mapper;
        private readonly DataTablesLinqQueryBulider _queryBuilder;

        public MobileUnitQueriesHandler(IDbContextScopeFactory dbContextScopeFactory, IMapper mapper, DataTablesLinqQueryBulider queryBuilder)
        {
            _dbContextScopeFactory = dbContextScopeFactory;
            _mapper = mapper;
            _queryBuilder = queryBuilder;
        }

        public async Task<IEnumerable<MobileUnitSelectListDto>> Handle(GetMobileUnitsWithoutVehicleIdQuery request, CancellationToken cancellationToken)
        {
            using (var dbFactory = _dbContextScopeFactory.Create())
            {
                var context = dbFactory.DbContexts.Get<SmartFleetObjectContext>();
                var query =  await context.Boxes
                    .Where(m=>!m.VehicleId.HasValue)
                    .Select(x=>  new {x.Id, x.Imei})
                    .ToListAsync(cancellationToken)
                    .ConfigureAwait(false);
                return query.Select(x => new MobileUnitSelectListDto  (x.Id, x.Imei));
            }
        }

        public Task<MobileUnitDto> Handle(GetMobileUnitByIdQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<MobileUnitDto> Handle(GetMobileUnitByImeiQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
