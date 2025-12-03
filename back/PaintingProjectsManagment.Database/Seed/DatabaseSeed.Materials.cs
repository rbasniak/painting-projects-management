using PaintingProjectsManagement.Features.Materials;

namespace PaintingProjectsManagment.Database;

public partial class DatabaseSeed
{

    private string _materialVallejoVarnish = "Vallejo varnish";
    private string _materialArmyPainterVarnish = "The Army Painter varnish";
    private string _materialGswVarnish = "Green Stuff World varnish";
    private string _materialTamiyaTS80 = "Tamiya matt varnish TS-80";
    private string _materialBiltemaMatteVarnish = "Biltema matt varnish";
    private string _materialBiltemaGlossVarnish = "Biltema gloss varnish";

    private string _materialGswPrimer = "Green Stuff World primer";
    private string _materialVallejoPrimer = "Vallejo primer";

    private string _materialCitadelContrast = "Citadel contrast paints";
    private string _materialGswAcrylic = "Green Stuff World acrylic paints";
    private string _materialGswChrome = "Green Stuff World chrome paint";
    private string _materialAkChrome = "AK chrome paint";
    private string _materialTamiyaClear = "Tamiya clear paints";
    private string _materialVallejoAcrylic = "Vallejo acrylic paints";
    private string _materialAtomAcrylic = "ATOM acrylic paints";
    private string _materialAmmoAcrylic = "AMMO acrylic paints";
    private string _materialAmmoTransparent = "AMMO transparator";
    private string _materialAmmoDrybrush = "AMMO drybrush paints";
    private string _materialAmmoCandy = "AMMO candy paints";
    private string _materialArmyPainterAcrylic = "The Army Painter acrylic paints";
    private string _materialVallejoGlaze = "Vallejo medium glaze";
    private string _materialAmmoTransparator = "AMMO Transparator";

    private string _materialSunluWaterWashableStandardResin = "SUNLU Water Washable Standard resin";
    private string _materialSunluAbsResin = "SUNLU Water Washable ABS-like resin";
    private string _materialJayoAbsResin = "JAYO ABS-Like resin";
    private string _materialAnycubricClearResin = "ANYCUBIC Clear resin";

    private string _materialSuperGlue1g = "Super Glue 1g";
    private string _materialMilliput = "Milliput";
    private string _materialNailWoodSwab = "Nail wood swab";
    private string _materialPaintMixingCupCorrugated = "Paint mixing cup (corrugated)";
    private string _materialPaintMixingCupNonCorrugated = "Paint mixing cup (non-corrugated)";
    private string _materialDspiaeSandingDisk = "DSPIAE sanding disk";
    private string _materialDisposableBrush = "Disposable brush";
    private string _materialArmyPainterSnow = "The Army Painter snow";

    private string _materialMagnet2x2 = "2x2 magnet";
    private string _materialMagnet3x2 = "3x2 magnet";
    private string _materialMagnet3x3 = "3x3 magnet";
    private string _materialMagnet4x3 = "4x3 magnet";
    private string _materialMagnet5x1 = "5x1 magnet";
    private string _materialMagnet5x2 = "5x2 magnet";
    private string _materialMagnet5x3 = "5x3 magnet";
    private string _materialMagnet6x3 = "6x3 magnet";
    private string _materialMagnet6x4 = "6x4 magnet";
    private string _materialMagnet8x3 = "8x3 magnet";
    private string _materialMagnet8x4 = "8x4 magnet";
    private string _materialMagnet10x5 = "10x5 magnet";
    private string _materialMaskingTapeGeneric2mm = "Generic masking tape 2mm";
    private string _materialMaskingTapeGeneric3mm = "Generic masking tape 3mm";
    private string _materialMaskingTapeGeneric5mm = "Generic masking tape 5mm";
    private string _materialMaskingTapeGeneric10mm = "Generic masking tape 10mm";
    private string _materialMaskingTapeGeneric25mm = "Generic masking tape 25mm";
    private string _materialMaskingTapeTamiyaFlexible2mm = "Tamiya flexible masking tape 2mm";
    private string _materialMaskingTapeTamiya10mm = "Tamiya masking tape 10mm";
    private string _materialMaskingTapeTamiya20mm = "Tamiya masking tape 20mm";
    private string _materialMaskingTape3m25mm = "3M masking tape 25mm";
    private string _materialVallejoLiquidMask = "Vallejo liquid mask";
    private string _materialMaskingPutty = "Masking putty";
    private string _materialModellingPutty = "Modelling putty";
    private string _materialGlooves = "Glooves";

    private void MaterialsSeed(DatabaseContext context, IServiceProvider provider)
    {
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialVallejoVarnish, MaterialCategory.Varnishes, new Quantity(200, PackageContentUnit.Mililiter), new Money(80, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialArmyPainterVarnish, MaterialCategory.Varnishes, new Quantity(100, PackageContentUnit.Mililiter), new Money(50, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialGswVarnish, MaterialCategory.Varnishes, new Quantity(60, PackageContentUnit.Mililiter), new Money(7.5, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialTamiyaTS80, MaterialCategory.Varnishes, new Quantity(100, PackageContentUnit.Mililiter), new Money(89, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialBiltemaMatteVarnish, MaterialCategory.Varnishes, new Quantity(400, PackageContentUnit.Mililiter), new Money(49, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialBiltemaGlossVarnish, MaterialCategory.Varnishes, new Quantity(400, PackageContentUnit.Mililiter), new Money(49, "DKK")));

        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialGswPrimer, MaterialCategory.Primers, new Quantity(60, PackageContentUnit.Mililiter), new Money(7.5, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialVallejoPrimer, MaterialCategory.Primers, new Quantity(200, PackageContentUnit.Mililiter), new Money(15, "EUR")));

        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialCitadelContrast, MaterialCategory.Paints, new Quantity(18, PackageContentUnit.Mililiter), new Money(58, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialGswAcrylic, MaterialCategory.Paints, new Quantity(17, PackageContentUnit.Mililiter), new Money(3, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialGswChrome, MaterialCategory.Paints, new Quantity(17, PackageContentUnit.Mililiter), new Money(4.6, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialAkChrome, MaterialCategory.Paints, new Quantity(60, PackageContentUnit.Mililiter), new Money(17, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialTamiyaClear, MaterialCategory.Paints, new Quantity(10, PackageContentUnit.Mililiter), new Money(19, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialVallejoAcrylic, MaterialCategory.Paints, new Quantity(17, PackageContentUnit.Mililiter), new Money(19, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialAtomAcrylic, MaterialCategory.Paints, new Quantity(20, PackageContentUnit.Mililiter), new Money(3, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialAmmoAcrylic, MaterialCategory.Paints, new Quantity(17, PackageContentUnit.Mililiter), new Money(2.5, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialAmmoTransparent, MaterialCategory.Paints, new Quantity(60, PackageContentUnit.Mililiter), new Money(5, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialAmmoDrybrush, MaterialCategory.Paints, new Quantity(40, PackageContentUnit.Mililiter), new Money(4, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialAmmoCandy, MaterialCategory.Paints, new Quantity(30, PackageContentUnit.Mililiter), new Money(7.5, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialArmyPainterAcrylic, MaterialCategory.Paints, new Quantity(17, PackageContentUnit.Mililiter), new Money(19, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialVallejoGlaze, MaterialCategory.Paints, new Quantity(60, PackageContentUnit.Mililiter), new Money(8, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialAmmoTransparator, MaterialCategory.Paints, new Quantity(60, PackageContentUnit.Mililiter), new Money(5, "EUR")));

        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialSunluWaterWashableStandardResin, MaterialCategory.Resins, new Quantity(1000, PackageContentUnit.Gram), new Money(1082 / 6, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialSunluAbsResin, MaterialCategory.Resins, new Quantity(1000, PackageContentUnit.Gram), new Money(1082 / 6, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialJayoAbsResin, MaterialCategory.Resins, new Quantity(1000, PackageContentUnit.Gram), new Money(188 / 15, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialAnycubricClearResin, MaterialCategory.Resins, new Quantity(1000, PackageContentUnit.Gram), new Money(75/5, "EUR")));

        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialSuperGlue1g, MaterialCategory.Others, new Quantity(50, PackageContentUnit.Each), new Money(8, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMilliput, MaterialCategory.Others, new Quantity(45, PackageContentUnit.Gram), new Money(9.5, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialNailWoodSwab, MaterialCategory.Others, new Quantity(500, PackageContentUnit.Each), new Money(6, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialPaintMixingCupCorrugated, MaterialCategory.Others, new Quantity(150, PackageContentUnit.Each), new Money(3.75, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialPaintMixingCupNonCorrugated, MaterialCategory.Others, new Quantity(200, PackageContentUnit.Each), new Money(8.5, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialDspiaeSandingDisk, MaterialCategory.Others, new Quantity(480, PackageContentUnit.Each), new Money(6.3, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialDisposableBrush, MaterialCategory.Others, new Quantity(100, PackageContentUnit.Each), new Money(7, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialArmyPainterSnow, MaterialCategory.Others, new Quantity(150, PackageContentUnit.Gram), new Money(40, "DKK")));

        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMagnet2x2, MaterialCategory.Magnets, new Quantity(120, PackageContentUnit.Each), new Money(10, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMagnet3x2, MaterialCategory.Magnets, new Quantity(200, PackageContentUnit.Each), new Money(9.5, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMagnet3x3, MaterialCategory.Magnets, new Quantity(200, PackageContentUnit.Each), new Money(4.5, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMagnet4x3, MaterialCategory.Magnets, new Quantity(100, PackageContentUnit.Each), new Money(4.5, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMagnet5x1, MaterialCategory.Magnets, new Quantity(200, PackageContentUnit.Each), new Money(8.5, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMagnet5x2, MaterialCategory.Magnets, new Quantity(100, PackageContentUnit.Each), new Money(7.5, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMagnet5x3, MaterialCategory.Magnets, new Quantity(120, PackageContentUnit.Each), new Money(9.5, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMagnet6x3, MaterialCategory.Magnets, new Quantity(108, PackageContentUnit.Each), new Money(15, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMagnet6x4, MaterialCategory.Magnets, new Quantity(200, PackageContentUnit.Each), new Money(11, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMagnet8x3, MaterialCategory.Magnets, new Quantity(108, PackageContentUnit.Each), new Money(15, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMagnet8x4, MaterialCategory.Magnets, new Quantity(100, PackageContentUnit.Each), new Money(10, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMagnet10x5, MaterialCategory.Magnets, new Quantity(50, PackageContentUnit.Each), new Money(8.5, "USD")));

        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMaskingTapeGeneric2mm, MaterialCategory.Masking, new Quantity(50, PackageContentUnit.Meter), new Money(2, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMaskingTapeGeneric3mm, MaterialCategory.Masking, new Quantity(50, PackageContentUnit.Meter), new Money(2, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMaskingTapeGeneric5mm, MaterialCategory.Masking, new Quantity(50, PackageContentUnit.Meter), new Money(2, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMaskingTapeGeneric10mm, MaterialCategory.Masking, new Quantity(50, PackageContentUnit.Meter), new Money(2, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMaskingTapeGeneric25mm, MaterialCategory.Masking, new Quantity(15, PackageContentUnit.Meter), new Money(2, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMaskingTapeTamiyaFlexible2mm, MaterialCategory.Masking, new Quantity(20, PackageContentUnit.Meter), new Money(7.5, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMaskingTapeTamiya10mm, MaterialCategory.Masking, new Quantity(18, PackageContentUnit.Meter), new Money(24, "USD")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMaskingTapeTamiya20mm, MaterialCategory.Masking, new Quantity(18, PackageContentUnit.Meter), new Money(32, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMaskingTape3m25mm, MaterialCategory.Masking, new Quantity(41, PackageContentUnit.Meter), new Money(4.1, "EUR")));
        context.SaveChanges();
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialVallejoLiquidMask, MaterialCategory.Masking, new Quantity(85, PackageContentUnit.Mililiter), new Money(11, "EUR")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialMaskingPutty, MaterialCategory.Masking, new Quantity(80, PackageContentUnit.Gram), new Money(120, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialModellingPutty, MaterialCategory.Others, new Quantity(100, PackageContentUnit.Gram), new Money(120, "DKK")));
        context.Set<Material>().Add(new Material("rodrigo.basniak", _materialGlooves, MaterialCategory.Others, new Quantity(100, PackageContentUnit.Each), new Money(12, "EUR")));

        context.SaveChanges();
    }
}