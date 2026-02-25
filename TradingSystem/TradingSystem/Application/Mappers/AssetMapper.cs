using TradingSystem.Application.DTOs;
using TradingSystem.Domain.Entities;

namespace TradingSystem.Application.Mappers;

public static class AssetMapper
{
    public static AssetResponse ToResponse(this Asset asset)
    {
        return new AssetResponse(
            Id: asset.Id,
            Symbol: asset.Symbol,
            Name: asset.Name,
            CreatedAt: asset.CreatedAt
        );
    }
}
