using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;

namespace rbkApiModules.Commons.Core; 

public interface ILocalizationService
{
    string LocalizeString(Enum value);

    string GetLanguageTemplate(string localization = null);
}

public class LocalizationCache
{
    private readonly ILogger<LocalizationCache> _logger;

    public SortedDictionary<string, SortedDictionary<string, string>> LanguagesCache = new();
    public SortedDictionary<string, string> DefaultValues = new();

    public LocalizationCache(ILogger<LocalizationCache> logger)
    {
        _logger = logger;

        LoadDefaultValues();

        LoadLocalizedValues();
    }

    private void LoadLocalizedValues()
    {
        _logger.LogInformation("Looking for localization files in resources");

        LanguagesCache.Add("en-us", DefaultValues);

        var resources = Assembly.GetAssembly(typeof(ILocalizationService)).GetManifestResourceNames().Where(x => x.EndsWith(".localization")).ToList();

        foreach (var resource in resources)
        {
            _logger.LogInformation("Processing localization resource file {resource}", resource);

            var data = new StreamReader(Assembly.GetAssembly(typeof(ILocalizationService)).GetManifestResourceStream(resource)).ReadToEnd();

            var newDictionary = JsonSerializer.Deserialize<SortedDictionary<string, string>>(data);

            var localization = resource.ToLower().Replace(".localization", "").Split('.').Last().Split('_').Last();

            if (!LanguagesCache.ContainsKey(localization))
            {
                _logger.LogInformation("Dictionary for language nof found, initializing with default en-us values");

                var tempDictionary = new SortedDictionary<string, string>();

                foreach (var localizedString in DefaultValues)
                {
                    tempDictionary.Add(localizedString.Key, localizedString.Value);
                }

                LanguagesCache.Add(localization.ToLower(), tempDictionary);

                _logger.LogInformation("{language} initialized successfully", localization);
            }

            var existingDictionary = LanguagesCache[localization];


            foreach (var newEntry in newDictionary)
            {
                if (existingDictionary.ContainsKey(newEntry.Key))
                {
                    _logger.LogInformation("Replacing value for key {key}", newEntry.Key);

                    existingDictionary[newEntry.Key] = newEntry.Value;
                }
                else
                {
                    _logger.LogInformation("Adding key {key}", newEntry.Key);

                    existingDictionary.Add(newEntry.Key, newEntry.Value);
                }
            }
        }

        _logger.LogInformation("Localization files loaded successfully");
    }

    private void LoadDefaultValues()
    {
        _logger.LogInformation("Initializing default localized values for en-us");

        DefaultValues = new SortedDictionary<string, string>();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.FullName.StartsWith("Microsoft") && !x.FullName.StartsWith("System"));

        var localizableEnums = assemblies
            .Where(x => !x.FullName.StartsWith("Microsoft"))
            .Where(x => !x.FullName.StartsWith("System"))
            .SelectMany(assembly => assembly.GetTypes().Where(type => typeof(ILocalizedResource).IsAssignableFrom(type) && !type.IsInterface)
                .SelectMany(typeImplementingInterface => typeImplementingInterface.GetNestedTypes().Where(x => x.IsEnum)))
                    .ToList();

        foreach (var localizableEnum in localizableEnums)
        {
            _logger.LogInformation("Processing enum {enum}", localizableEnum.FullName.Split('.').Last());

            var enumValues = Enum.GetValues(localizableEnum);

            foreach (var enumValue in enumValues)
            {
                var key = ((Enum)enumValue).GetLocalizedEnumIdentifier();
                var value = ((Enum)enumValue).GetEnumDescription();

                _logger.LogInformation("Adding key {key}", key);

                DefaultValues.Add(key, value);
            }
        }
    }
}

public class LocalizationService : ILocalizationService
{
    private readonly string _systemLanguage = "en-us";
    private readonly string _currentLanguage = "en-us";

    private readonly LocalizationCache _localizationCache;
    private readonly ILogger<LocalizationService> _logger;

    public LocalizationService(IHttpContextAccessor httpContextAccessor, RbkApiCoreOptions coreOptions, LocalizationCache localizationCache, ILogger<LocalizationService> logger)
    {
        _logger = logger;

        _logger.LogInformation("Instantiating service: {service}", nameof(LocalizationService));

        _localizationCache = localizationCache;

        if (coreOptions != null)
        {
            _systemLanguage = coreOptions._defaultLocalization.ToLower();
        }

        if (httpContextAccessor != null && httpContextAccessor.HttpContext != null)
        {
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("localization", out var languageHeaderValues);

            if (languageHeaderValues.Count() > 0)
            {
                _currentLanguage = languageHeaderValues[0].ToLower();
            }
            else
            {
                _currentLanguage = _systemLanguage;
            }
        }
        else
        {
            _currentLanguage = _systemLanguage;
        }

        _logger.LogInformation("Using {language} for _systemLanguage", _systemLanguage);
        _logger.LogInformation("Using {language} for _currentLanguage", _currentLanguage);
    }

    public string LocalizeString(Enum value)
    {
        var dictionary = _localizationCache.DefaultValues;

        if (_localizationCache.LanguagesCache.ContainsKey(_currentLanguage))
        {
            dictionary = _localizationCache.LanguagesCache[_currentLanguage];
        }

        var key = value.GetLocalizedEnumIdentifier();

        if (dictionary.TryGetValue(key, out var response))
        {
            return response;
        }
        else
        {
            return value.GetEnumDescription();
        }
    }

    public string GetLanguageTemplate(string localization = null)
    {
        var dictionary = _localizationCache.DefaultValues;

        if (localization != null && _localizationCache.LanguagesCache.ContainsKey(localization))
        {
            dictionary = _localizationCache.LanguagesCache[localization];
        }

        return JsonSerializer.Serialize(dictionary, new JsonSerializerOptions { WriteIndented = true });
    }
}

file static class Extensions
{
    public static string GetLocalizedEnumIdentifier(this Enum enumValue)
    {
        var localizableEnum = enumValue.GetType();

        var prefix = localizableEnum.FullName.Split('.').Last().Replace("+", "::");

        if (localizableEnum.FullName.StartsWith("rbk"))
        {
            prefix = "rbkApiModules::" + prefix;
        }

        return $"{prefix}::{enumValue.ToString()}";
    }

    public static string GetEnumDescription(this Enum value)
    {
        var type = value.GetType();
        var name = Enum.GetName(type, value);
        if (name != null)
        {
            var field = type.GetField(name);
            if (field != null)
            {
                var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    return attribute.Description;
                }
            }
        }

        return value.ToString();
    }
}
