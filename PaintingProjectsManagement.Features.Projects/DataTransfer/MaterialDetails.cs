﻿using PaintingProjectsManagement.Features.Materials;
using PaintingProjectsManagement.Features.Materials.Abstractions;

namespace PaintingProjectsManagement.Features.Projects;

public class MaterialDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public MaterialUnit Unit { get; set; }
    public double PricePerUnit { get; set; }

    public static MaterialDetails FromReadOnlyMaterial(ReadOnlyMaterial material)
    {
        return new MaterialDetails
        {
            Id = material.Id,
            Name = material.Name,
            Unit = material.Unit,
            PricePerUnit = material.PricePerUnit
        };
    }
}
