using PaintingProjectsManagement.Features.Materials;

namespace PaintingProjectsManagment.Database;

public partial class DatabaseSeed
{

    internal const string MaterialVallejoVarnish = "Vallejo varnish";
    internal const string MaterialArmyPainterVarnish = "The Army Painter varnish";
    internal const string MaterialGswVarnish = "Green Stuff World varnish";
    internal const string MaterialTamiyaTS80 = "Tamiya matt varnish TS-80";
    internal const string MaterialBiltemaMatteVarnish = "Biltema matt varnish";
    internal const string MaterialBiltemaGlossVarnish = "Biltema gloss varnish";

    internal const string MaterialGswPrimer = "Green Stuff World primer";
    internal const string MaterialVallejoPrimer = "Vallejo primer";

    internal const string MaterialCitadelContrast = "Citadel contrast paints";
    internal const string MaterialGswAcrylic = "Green Stuff World acrylic paints";
    internal const string MaterialGswChrome = "Green Stuff World chrome paint";
    internal const string MaterialAkChrome = "AK chrome paint";
    internal const string MaterialTamiyaClear = "Tamiya clear paints";
    internal const string MaterialVallejoAcrylic = "Vallejo acrylic paints";
    internal const string MaterialAtomAcrylic = "ATOM acrylic paints";
    internal const string MaterialAmmoAcrylic = "AMMO acrylic paints";
    internal const string MaterialAmmoTransparent = "AMMO transparator";
    internal const string MaterialAmmoDrybrush = "AMMO drybrush paints";
    internal const string MaterialAmmoCandy = "AMMO candy paints";
    internal const string MaterialArmyPainterAcrylic = "The Army Painter acrylic paints";
    internal const string MaterialVallejoGlaze = "Vallejo medium glaze";
    internal const string MaterialAmmoTransparator = "AMMO Transparator";

    internal const string MaterialSunluWaterWashableStandardResin = "SUNLU Water Washable Standard resin";
    internal const string MaterialSunluAbsResin = "SUNLU Water Washable ABS-like resin";
    internal const string MaterialJayoAbsResin = "JAYO ABS-Like resin";
    internal const string MaterialAnycubricClearResin = "ANYCUBIC Clear resin";

    internal const string MaterialSuperGlue1g = "Super Glue 1g";
    internal const string MaterialMilliput = "Milliput";
    internal const string MaterialNailWoodSwab = "Nail wood swab";
    internal const string MaterialPaintMixingCupCorrugated = "Paint mixing cup (corrugated)";
    internal const string MaterialPaintMixingCupNonCorrugated = "Paint mixing cup (non-corrugated)";
    internal const string MaterialDspiaeSandingDisk = "DSPIAE sanding disk";
    internal const string MaterialDisposableBrush = "Disposable brush";
    internal const string MaterialArmyPainterSnow = "The Army Painter snow";

    internal const string MaterialMagnet2x2 = "2x2 magnet";
    internal const string MaterialMagnet3x2 = "3x2 magnet";
    internal const string MaterialMagnet3x3 = "3x3 magnet";
    internal const string MaterialMagnet4x3 = "4x3 magnet";
    internal const string MaterialMagnet5x1 = "5x1 magnet";
    internal const string MaterialMagnet5x2 = "5x2 magnet";
    internal const string MaterialMagnet5x3 = "5x3 magnet";
    internal const string MaterialMagnet6x3 = "6x3 magnet";
    internal const string MaterialMagnet6x4 = "6x4 magnet";
    internal const string MaterialMagnet8x3 = "8x3 magnet";
    internal const string MaterialMagnet8x4 = "8x4 magnet";
    internal const string MaterialMagnet10x5 = "10x5 magnet";
    internal const string MaterialMaskingTapeGeneric2mm = "Generic masking tape 2mm";
    internal const string MaterialMaskingTapeGeneric3mm = "Generic masking tape 3mm";
    internal const string MaterialMaskingTapeGeneric5mm = "Generic masking tape 5mm";
    internal const string MaterialMaskingTapeGeneric10mm = "Generic masking tape 10mm";
    internal const string MaterialMaskingTapeGeneric25mm = "Generic masking tape 25mm";
    internal const string MaterialMaskingTapeTamiyaFlexible2mm = "Tamiya flexible masking tape 2mm";
    internal const string MaterialMaskingTapeTamiya10mm = "Tamiya masking tape 10mm";
    internal const string MaterialMaskingTapeTamiya20mm = "Tamiya masking tape 20mm";
    internal const string MaterialMaskingTape3m25mm = "3M masking tape 25mm";
    internal const string MaterialVallejoLiquidMask = "Vallejo liquid mask";
    internal const string MaterialMaskingPutty = "Masking putty";
    internal const string MaterialModellingPutty = "Modelling putty";
    internal const string MaterialGlooves = "Glooves";

    private void MaterialsSeed(DatabaseContext context, IServiceProvider provider)
    {
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialVallejoVarnish, MaterialCategory.Varnishes, new Quantity(200, PackageContentUnit.Mililiter), new Money(80, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialArmyPainterVarnish, MaterialCategory.Varnishes, new Quantity(100, PackageContentUnit.Mililiter), new Money(50, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialGswVarnish, MaterialCategory.Varnishes, new Quantity(60, PackageContentUnit.Mililiter), new Money(7.5, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialTamiyaTS80, MaterialCategory.Varnishes, new Quantity(100, PackageContentUnit.Mililiter), new Money(89, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialBiltemaMatteVarnish, MaterialCategory.Varnishes, new Quantity(400, PackageContentUnit.Mililiter), new Money(49, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialBiltemaGlossVarnish, MaterialCategory.Varnishes, new Quantity(400, PackageContentUnit.Mililiter), new Money(49, "DKK")));

        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialGswPrimer, MaterialCategory.Primers, new Quantity(60, PackageContentUnit.Mililiter), new Money(7.5, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialVallejoPrimer, MaterialCategory.Primers, new Quantity(200, PackageContentUnit.Mililiter), new Money(15, "EUR")));

        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialCitadelContrast, MaterialCategory.Paints, new Quantity(18, PackageContentUnit.Mililiter), new Money(58, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialGswAcrylic, MaterialCategory.Paints, new Quantity(17, PackageContentUnit.Mililiter), new Money(3, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialGswChrome, MaterialCategory.Paints, new Quantity(17, PackageContentUnit.Mililiter), new Money(4.6, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialAkChrome, MaterialCategory.Paints, new Quantity(60, PackageContentUnit.Mililiter), new Money(17, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialTamiyaClear, MaterialCategory.Paints, new Quantity(10, PackageContentUnit.Mililiter), new Money(19, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialVallejoAcrylic, MaterialCategory.Paints, new Quantity(17, PackageContentUnit.Mililiter), new Money(19, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialAtomAcrylic, MaterialCategory.Paints, new Quantity(20, PackageContentUnit.Mililiter), new Money(3, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialAmmoAcrylic, MaterialCategory.Paints, new Quantity(17, PackageContentUnit.Mililiter), new Money(2.5, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialAmmoTransparent, MaterialCategory.Paints, new Quantity(60, PackageContentUnit.Mililiter), new Money(5, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialAmmoDrybrush, MaterialCategory.Paints, new Quantity(40, PackageContentUnit.Mililiter), new Money(4, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialAmmoCandy, MaterialCategory.Paints, new Quantity(30, PackageContentUnit.Mililiter), new Money(7.5, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialArmyPainterAcrylic, MaterialCategory.Paints, new Quantity(17, PackageContentUnit.Mililiter), new Money(19, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialVallejoGlaze, MaterialCategory.Paints, new Quantity(60, PackageContentUnit.Mililiter), new Money(8, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialAmmoTransparator, MaterialCategory.Paints, new Quantity(60, PackageContentUnit.Mililiter), new Money(5, "EUR")));

        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialSunluWaterWashableStandardResin, MaterialCategory.Resins, new Quantity(1000, PackageContentUnit.Gram), new Money(1082 / 6, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialSunluAbsResin, MaterialCategory.Resins, new Quantity(1000, PackageContentUnit.Gram), new Money(1082 / 6, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialJayoAbsResin, MaterialCategory.Resins, new Quantity(1000, PackageContentUnit.Gram), new Money((188 + 55) / 15, "USD"))); // 55 from alcohol wash
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialAnycubricClearResin, MaterialCategory.Resins, new Quantity(1000, PackageContentUnit.Gram), new Money(75/5, "EUR")));

        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialSuperGlue1g, MaterialCategory.Others, new Quantity(50, PackageContentUnit.Each), new Money(8, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMilliput, MaterialCategory.Others, new Quantity(45, PackageContentUnit.Gram), new Money(9.5, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialNailWoodSwab, MaterialCategory.Others, new Quantity(500, PackageContentUnit.Each), new Money(6, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialPaintMixingCupCorrugated, MaterialCategory.Others, new Quantity(150, PackageContentUnit.Each), new Money(3.75, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialPaintMixingCupNonCorrugated, MaterialCategory.Others, new Quantity(200, PackageContentUnit.Each), new Money(8.5, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialDspiaeSandingDisk, MaterialCategory.Others, new Quantity(480, PackageContentUnit.Each), new Money(6.3, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialDisposableBrush, MaterialCategory.Others, new Quantity(100, PackageContentUnit.Each), new Money(7, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialArmyPainterSnow, MaterialCategory.Others, new Quantity(150, PackageContentUnit.Gram), new Money(40, "DKK")));

        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMagnet2x2, MaterialCategory.Magnets, new Quantity(120, PackageContentUnit.Each), new Money(10, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMagnet3x2, MaterialCategory.Magnets, new Quantity(200, PackageContentUnit.Each), new Money(9.5, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMagnet3x3, MaterialCategory.Magnets, new Quantity(200, PackageContentUnit.Each), new Money(4.5, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMagnet4x3, MaterialCategory.Magnets, new Quantity(100, PackageContentUnit.Each), new Money(4.5, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMagnet5x1, MaterialCategory.Magnets, new Quantity(200, PackageContentUnit.Each), new Money(8.5, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMagnet5x2, MaterialCategory.Magnets, new Quantity(100, PackageContentUnit.Each), new Money(7.5, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMagnet5x3, MaterialCategory.Magnets, new Quantity(120, PackageContentUnit.Each), new Money(9.5, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMagnet6x3, MaterialCategory.Magnets, new Quantity(108, PackageContentUnit.Each), new Money(15, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMagnet6x4, MaterialCategory.Magnets, new Quantity(200, PackageContentUnit.Each), new Money(11, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMagnet8x3, MaterialCategory.Magnets, new Quantity(108, PackageContentUnit.Each), new Money(15, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMagnet8x4, MaterialCategory.Magnets, new Quantity(100, PackageContentUnit.Each), new Money(10, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMagnet10x5, MaterialCategory.Magnets, new Quantity(50, PackageContentUnit.Each), new Money(8.5, "USD")));

        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMaskingTapeGeneric2mm, MaterialCategory.Masking, new Quantity(50, PackageContentUnit.Meter), new Money(2, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMaskingTapeGeneric3mm, MaterialCategory.Masking, new Quantity(50, PackageContentUnit.Meter), new Money(2, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMaskingTapeGeneric5mm, MaterialCategory.Masking, new Quantity(50, PackageContentUnit.Meter), new Money(2, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMaskingTapeGeneric10mm, MaterialCategory.Masking, new Quantity(50, PackageContentUnit.Meter), new Money(2, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMaskingTapeGeneric25mm, MaterialCategory.Masking, new Quantity(15, PackageContentUnit.Meter), new Money(2, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMaskingTapeTamiyaFlexible2mm, MaterialCategory.Masking, new Quantity(20, PackageContentUnit.Meter), new Money(7.5, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMaskingTapeTamiya10mm, MaterialCategory.Masking, new Quantity(18, PackageContentUnit.Meter), new Money(24, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMaskingTapeTamiya20mm, MaterialCategory.Masking, new Quantity(18, PackageContentUnit.Meter), new Money(32, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMaskingTape3m25mm, MaterialCategory.Masking, new Quantity(41, PackageContentUnit.Meter), new Money(4.1, "EUR")));
        context.SaveChanges();
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialVallejoLiquidMask, MaterialCategory.Masking, new Quantity(85, PackageContentUnit.Mililiter), new Money(11, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialMaskingPutty, MaterialCategory.Masking, new Quantity(80, PackageContentUnit.Gram), new Money(120, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialModellingPutty, MaterialCategory.Others, new Quantity(100, PackageContentUnit.Gram), new Money(120, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", MaterialGlooves, MaterialCategory.Others, new Quantity(100, PackageContentUnit.Each), new Money(12, "EUR")));

        context.SaveChanges();
    }
}