namespace rbkApiModules.Commons.Core; 

public interface ILocalizationService
{
    string LocalizeString(Enum value);

    string GetLanguageTemplate(string localization = null);
}