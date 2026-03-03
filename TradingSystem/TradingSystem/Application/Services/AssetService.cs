using FluentValidation;
using TradingSystem.Application.DTOs;
using TradingSystem.Application.Interfaces;
using TradingSystem.Application.Mappers;
using TradingSystem.Domain.Entities;
using TradingSystem.Domain.Interfaces;

namespace TradingSystem.Application.Services;

public class AssetService : IAssetService
{
    private readonly IAssetRepository _assetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateAssetRequest> _validator;

    public AssetService(
        IAssetRepository assetRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateAssetRequest> validator)
    {
        _assetRepository = assetRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<AssetResponse> CreateAssetAsync(CreateAssetRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Check for duplicate symbol
        var existing = await _assetRepository.GetBySymbolAsync(request.Symbol);
        if (existing != null)
        {
            throw new InvalidOperationException($"Asset with symbol '{request.Symbol}' already exists.");
        }

        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            Symbol = request.Symbol.ToUpperInvariant(),
            Name = request.Name,
            CreatedAt = DateTime.UtcNow
        };

        await _assetRepository.AddAsync(asset);
        await _unitOfWork.SaveChangesAsync();

        return asset.ToResponse();
    }

    public async Task<AssetResponse?> GetAssetByIdAsync(Guid id)
    {
        var asset = await _assetRepository.GetByIdAsync(id);
        return asset?.ToResponse();
    }

    public async Task<IEnumerable<AssetResponse>> GetAllAssetsAsync()
    {
        var assets = await _assetRepository.GetAllAsync();
        return assets.Select(a => a.ToResponse());
    }
}
