using System;
using System.Text.RegularExpressions;
using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TaskManager.Domain.Validators
{
    public static class ValidatorExtensions
    {
        public static IRuleBuilderOptions<T, string> ValidJson<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must(data => IsValidJson(data)).WithMessage("Data not in JSON format");
        }

        public static IRuleBuilderOptions<T, string> HttpOrHttpsUrl<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must(text => Url(text)).WithMessage("Data not in HTTP/HTTPS URI format");
        }

        private static bool IsValidJson(string strInput)
        {
            if (
                !(string.IsNullOrWhiteSpace(strInput)) &&
                ((strInput.StartsWith("{") && strInput.EndsWith("}")) ||
                (strInput.StartsWith("[") && strInput.EndsWith("]"))))
            {
                try
                {
                    var jsonSerializerSettings = new JsonSerializerSettings()
                    {
                        DateParseHandling = DateParseHandling.None
                    };
                    var jsonString = JsonConvert.DeserializeObject<JToken>(strInput, jsonSerializerSettings).ToString();

                    return string.Equals(
                            strInput.RemoveEscapeCharacters(),
                            jsonString.RemoveEscapeCharacters(), 
                            StringComparison.OrdinalIgnoreCase
                            );
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private static string RemoveEscapeCharacters(this string str)
        {
            return Regex.Replace(str, @"[\r\n'/\\\s+]", "", RegexOptions.None, TimeSpan.FromMilliseconds(100));
        }

        private static bool Url(string text)
        {
            return Uri.TryCreate(text, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
