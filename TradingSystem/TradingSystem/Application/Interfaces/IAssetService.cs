using TradingSystem.Application.DTOs;

namespace TradingSystem.Application.Interfaces;

public interface IAssetService
{
    Task<AssetResponse> CreateAssetAsync(CreateAssetRequest request);
    Task<AssetResponse?> GetAssetByIdAsync(Guid id);
    Task<IEnumerable<AssetResponse>> GetAllAssetsAsync();
}
