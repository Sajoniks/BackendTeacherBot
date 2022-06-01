using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace LearnBotVrk
{
    public class VkrRequest
    {
        private string _base;
        private dynamic _params;
        private List<string> _dirs;
        private List<int> _formats;

        public VkrRequest(string baseUrl)
        {
            _base = baseUrl;
            _dirs = new List<string>();
            _formats = new List<int>();
        }

        public VkrRequest Append(string dir)
        {
            _dirs.Add(dir);

            if (dir.First() == '{' && dir.Last() == '}')
            {
                _formats.Add(_dirs.Count - 1);
            }

            return this;
        }

        public VkrRequest WithParameters(dynamic parameters)
        {
            _params = parameters;
            return this;
        }

        private static IDictionary<string, object> ConvertToMap(object dyn)
        {
            if (dyn == null)
            {
                return new Dictionary<string, object>();
            }

            if (dyn is Dictionary<string, object> dictionary)
            {
                return dictionary;
            }

            var outDict = new Dictionary<string, object>();
            foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(dyn))
            {
                outDict.Add(propertyDescriptor.Name, propertyDescriptor.GetValue(dyn));
            }

            return outDict;
        }

        public VkrRequest AppendParameters(dynamic parameters)
        {
            if (_params == null)
            {
                return WithParameters(parameters);
            }

            if (parameters != null)
            {
                var a = ConvertToMap(_params) as IDictionary<string, object>;
                var b = ConvertToMap(parameters) as IDictionary<string, object>;

                var result = new ExpandoObject() as IDictionary<string, object>;
                foreach (var pair in a.Concat(b))
                {
                    result[pair.Key] = pair.Value;
                }

                _params = result;
            }

            return this;
        }

        public async Task<T> GetJsonAsync<T>(string method = WebRequestMethods.Http.Get)
        {
            try
            {
                var url = MakeUrl();
                var req = WebRequest.CreateHttp(url);

                req.Method = method;
                // todo
                req.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;

                using var response = await req.GetResponseAsync();
                using var stream = response.GetResponseStream();

                if (stream != null)
                {
                    var content = await new StreamReader(stream).ReadToEndAsync();
                    return JsonConvert.DeserializeObject<T>(content);
                }
            }
            catch (Exception e)
            {
            }

            return default;
        }

        private string MakeUrl()
        {
            var resultPath = _base;

            var type = ((object)_params)?.GetType();

            // collect values from dynamic
            var nameValue =
                type?
                    .GetProperties()
                    .Select(prop =>
                    {
                        var propName = prop.Name;
                        var propValue = (object)prop.GetValue(_params);
                        var propType = propValue?.GetType();

                        if (propValue == null)
                        {
                            return new KeyValuePair<string, string>(propName, null);
                        }

                        var propEncoded = "";
                        if (propType.IsPrimitive || propType == typeof(string) || propType.IsEnum)
                        {
                            propEncoded = propValue.ToString();
                            if (propType.IsEnum)
                            {
                                propEncoded = propEncoded.ToLower();
                            }
                        }
                        else
                        {
                            propEncoded = JsonConvert.SerializeObject(propValue);
                        }

                        return new KeyValuePair<string, string>(propName, propEncoded);
                    })
                    .Where(v => v.Value != null)
                    .ToDictionary(k => k.Key, v => v.Value);

            if (nameValue != null)
            {
                foreach (var i in _formats)
                {
                    var fmt = _dirs[i].Substring(1, _dirs[i].Length - 2);
                    _dirs[i] = nameValue[fmt];
                    nameValue.Remove(fmt);
                }

                var appendPath = String.Join("/", _dirs);
                var paramsPath = String.Join("&",
                    nameValue
                        .Select(kv => $"{kv.Key}={HttpUtility.UrlEncode(kv.Value)}")
                );

                return Path.Combine(resultPath, appendPath) + (paramsPath.Length > 0 ? $"?{paramsPath}" : "");
            }
            else
            {
                return Path.Combine(resultPath, String.Join("/", _dirs));
            }
        }
    }
}