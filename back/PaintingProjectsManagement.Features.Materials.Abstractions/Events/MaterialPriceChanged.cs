using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Materials.Abstractions;

[EventName("Materials.MaterialPriceChanged", 1)]
public sealed record MaterialPriceChanged(Guid MaterialId, double NewPrice, DateTime ChangedAtUtc); 