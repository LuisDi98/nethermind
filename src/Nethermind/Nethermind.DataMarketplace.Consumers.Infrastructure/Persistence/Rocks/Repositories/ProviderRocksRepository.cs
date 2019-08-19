/*
 * Copyright (c) 2018 Demerzel Solutions Limited
 * This file is part of the Nethermind library.
 *
 * The Nethermind library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * The Nethermind library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nethermind.Core.Encoding;
using Nethermind.Core.Extensions;
using Nethermind.DataMarketplace.Consumers.Shared.Domain;
using Nethermind.DataMarketplace.Consumers.Shared.Repositories;
using Nethermind.Store;

namespace Nethermind.DataMarketplace.Consumers.Infrastructure.Persistence.Rocks.Repositories
{
    public class ProviderRocksRepository : IProviderRepository
    {
        private readonly IDb _database;
        private readonly IRlpDecoder<DepositDetails> _rlpDecoder;

        public ProviderRocksRepository(IDb database, IRlpDecoder<DepositDetails> rlpDecoder)
        {
            _database = database;
            _rlpDecoder = rlpDecoder;
        }

        public async Task<IReadOnlyList<DataAssetInfo>> GetDataAssetsAsync()
            => await Task.FromResult(GetAll()
                .Select(d => new DataAssetInfo(d.DataAsset.Id, d.DataAsset.Name, d.DataAsset.Description))
                .Distinct()
                .ToList());

        public async Task<IReadOnlyList<ProviderInfo>> GetProvidersAsync()
            => await Task.FromResult(GetAll()
                .Select(d => new ProviderInfo(d.DataAsset.Provider.Name, d.DataAsset.Provider.Address))
                .Distinct()
                .ToList());

        private IEnumerable<DepositDetails> GetAll()
        {
            var depositsBytes = _database.GetAll();
            if (depositsBytes.Length == 0)
            {
                yield break;
            }

            var dataAssets = new DepositDetails[depositsBytes.Length];
            for (var i = 0; i < depositsBytes.Length; i++)
            {
                yield return dataAssets[i] = Decode(depositsBytes[i]);
            }
        }

        private DepositDetails Decode(byte[] bytes)
            => bytes is null
                ? null
                : _rlpDecoder.Decode(bytes.AsRlpContext());
    }
}