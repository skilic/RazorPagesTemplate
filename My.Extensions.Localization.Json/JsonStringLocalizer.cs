﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using My.Extensions.Localization.Json.Internal;

namespace My.Extensions.Localization.Json
{
    public class JsonStringLocalizer : IStringLocalizer
    {
        private readonly ConcurrentDictionary<string, IEnumerable<KeyValuePair<string, string>>> _resourcesCache = new ConcurrentDictionary<string, IEnumerable<KeyValuePair<string, string>>>();
        private readonly string _resourcesPath;
        private readonly string _resourceName;
        private readonly ILogger _logger;

        private string _searchedLocation;

        public JsonStringLocalizer(
            string resourcesPath,
            string resourceName, // TODO: Use optional parameter in upcoming major release
            ILogger logger)
        {
            _resourcesPath = resourcesPath ?? throw new ArgumentNullException(nameof(resourcesPath));

            //i dont know why it is coming as "rce"
            if (resourceName.Equals("rce"))
                _resourceName = "SharedResource";
            else
                _resourceName = resourceName;

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public LocalizedString this[string name]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var value = GetStringSafely(name);

                return new LocalizedString(name, value ?? name, resourceNotFound: value == null, searchedLocation: _searchedLocation);
            }
        }

         

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                if (name == null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                var qty = -1;
                if ((arguments?.Length > 0) == true)
                {
                    var qtyTmp = 0;
                    if (int.TryParse(arguments[0].ToString(), out qtyTmp))
                        qty = int.Parse(arguments[0].ToString());
                }

                var format = GetStringSafely(name, qty);
                var value = string.Format(format ?? name, arguments);
                if (qty != -1)
                    value = format ?? name;

                return new LocalizedString(name, value, resourceNotFound: format == null, searchedLocation: _searchedLocation);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            GetAllStrings(includeParentCultures, CultureInfo.CurrentUICulture);

        public IStringLocalizer WithCulture(CultureInfo culture) => this;

        protected IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures, CultureInfo culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            var resourceNames = includeParentCultures
                ? GetAllStringsFromCultureHierarchy(culture)
                : GetAllResourceStrings(culture);

            foreach (var name in resourceNames)
            {
                var value = GetStringSafely(name);
                yield return new LocalizedString(name, value ?? name, resourceNotFound: value == null, searchedLocation: _searchedLocation);
            }
        }

        protected virtual string GetStringSafely(string name, int qty=-1)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var culture = CultureInfo.CurrentUICulture;
            

             
            if (qty >-1 )
            {
                if (IsItPlural(qty, culture))
                    name += "_plural";
                else
                    name += "_none";
            }
            

            string value = null;

            while (culture != culture.Parent)
            {
                BuildResourcesCache(culture.Name);

                if (_resourcesCache.TryGetValue(culture.Name, out IEnumerable<KeyValuePair<string, string>> resources))
                {
                    var resource = resources?.SingleOrDefault(s => s.Key == name);

                    value = resource?.Value ?? null;
                    _logger.SearchedLocation(name, _searchedLocation, culture);

                    if (value != null)
                    {
                        break;
                    }

                    culture = culture.Parent;
                }
            }

            return value;
        }

        private bool IsItPlural(int qty, CultureInfo culture)
        {
            //http://docs.translatehouse.org/projects/localization-guide/en/latest/l10n/pluralforms.html

            switch (culture.TwoLetterISOLanguageName)
            {
                case "de":
                case "en":
                case "es":
                    return qty != 1;
                    break;
                case "fr":
                case "tr":
                    return qty > 1;
                default:
                    return qty > 1;
                    break;
            }
            return false;
        }

        private IEnumerable<string> GetAllStringsFromCultureHierarchy(CultureInfo startingCulture)
        {
            var currentCulture = startingCulture;
            var resourceNames = new HashSet<string>();

            while (currentCulture != currentCulture.Parent)
            {
                var cultureResourceNames = GetAllResourceStrings(currentCulture);

                if (cultureResourceNames != null)
                {
                    foreach (var resourceName in cultureResourceNames)
                    {
                        resourceNames.Add(resourceName);
                    }
                }

                currentCulture = currentCulture.Parent;
            }

            return resourceNames;
        }

        private IEnumerable<string> GetAllResourceStrings(CultureInfo culture)
        {
            BuildResourcesCache(culture.Name);

            if (_resourcesCache.TryGetValue(culture.Name, out IEnumerable<KeyValuePair<string, string>> resources))
            {
                foreach (var resource in resources)
                {
                    yield return resource.Key;
                }
            }
            else
            {
                yield return null;
            }
        }

        private void BuildResourcesCache(string culture)
        {
            _resourcesCache.GetOrAdd(culture, _ =>
            {
                var resourceFile = string.IsNullOrEmpty(_resourceName)
                    ? $"{culture}.json"
                    : $"{_resourceName}.{culture}.json";

                _searchedLocation = Path.Combine( _resourcesPath, resourceFile);

                if (!File.Exists(_searchedLocation))
                {
                    if (resourceFile.Count(r => r == '.') > 1)
                    {
                        var resourceFileWithoutExtension = Path.GetFileNameWithoutExtension(resourceFile);
                        var resourceFileWithoutCulture = resourceFileWithoutExtension.Substring(0, resourceFileWithoutExtension.LastIndexOf('.'));
                        resourceFile = $"{resourceFileWithoutCulture.Replace('.', Path.DirectorySeparatorChar)}.{culture}.json";
                        _searchedLocation = Path.Combine(_resourcesPath, resourceFile);
                    }
                }
                
                IEnumerable<KeyValuePair<string, string>> value = null;

                if (File.Exists(_searchedLocation))
                {
                    var builder = new ConfigurationBuilder()
                    .SetBasePath(_resourcesPath)
                    .AddJsonFile(resourceFile, optional: false, reloadOnChange: true);

                    var config = builder.Build();
                    value = config.AsEnumerable();
                }

                return value;
            });
        }
    }
}
