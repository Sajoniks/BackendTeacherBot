using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using LearnBotVrk.botApi;
using LearnBotVrk.Telegram.BotAPI;
using Newtonsoft.Json;

namespace LearnBotVrk
{
    public static class Utility
    {
        public class HttpConfigurableRequest
        {
            private readonly String _methodName;
            private readonly String _baseUrl;
            private readonly List<String> _parms;
            private readonly List<String> _vals;
            private int _lastParamIdx;
            
            public String BaseURL => _baseUrl;
            protected List<String> Parameters => _parms;
            protected List<String> Values => _vals;
            protected int LastParameterIndex
            {
                get => _lastParamIdx;
                set => _lastParamIdx = value;
            }

            protected String MethodName => _methodName;
            
            public HttpConfigurableRequest(string baseUrl, string methodName)
            {
                _baseUrl = baseUrl;
                _lastParamIdx = -1;
                _methodName = methodName;
                _parms = new List<string>();
                _vals = new List<string>();
            }
            
            protected void WriteParam(string value)
            {
                _vals[_lastParamIdx + 1] = value;
                ++_lastParamIdx;
            }

            public HttpConfigurableRequest AddJsonParam<T>(T value)
            {
                WriteParam(value != null ? JsonConvert.SerializeObject(value) : null);
                return this;
            }

            public HttpConfigurableRequest AddParam(int? value)
            {
                WriteParam(value?.ToString());
                return this;
            }

            public HttpConfigurableRequest AddParam(long? value)
            {
                WriteParam(value?.ToString());
                return this;
            }
            
            public HttpConfigurableRequest AddParam(Enum enumerator)
            {
                WriteParam(enumerator?.ToString().ToLowerInvariant());
                return this;
            }

            public HttpConfigurableRequest AddParam(string value, string fallback = null)
            {
                WriteParam(value ?? fallback);
                return this;
            }

            public HttpConfigurableRequest AddParam(bool? value)
            {
                WriteParam(value?.ToString());
                return this;
            }

            protected virtual String BuildApiRequestUrl(StringBuilder sb)
            {
                sb.Append(_methodName);
                AppendParameters(sb);
                return sb.ToString();
            }

            private void RemoveNullParameters()
            {
                for (int i = 0; i < _parms.Count;)
                {
                    if (_vals[i] == null)
                    {
                        _vals.RemoveAt(i);
                        _parms.RemoveAt(i);
                    }
                    else
                    {
                        ++i;
                    }
                }
            }

            protected virtual void AppendParameters(StringBuilder sb)
            {
                RemoveNullParameters();
                if (_parms.Count > 0)
                {
                    sb.Append("?");
                    
                    for (int i = 0; i < _parms.Count; ++i)
                    {
                        sb.AppendFormat("{0}={1}", _parms[i], _vals[i]);
                        if (i + 1 < _parms.Count)
                        {
                            sb.Append('&');
                        }
                    }
                }
            }

            /// <summary>
            /// Build url and get response
            /// </summary>
            /// <returns>TelegramResponse object</returns>
            public T GetResponse<T>(string methodType = "GET")
            {
                RemoveNullParameters();
                StringBuilder sb = new StringBuilder(_baseUrl);
                
                string apiString = BuildApiRequestUrl(sb);
                var request = WebRequest.CreateHttp(apiString);
                request.Method = methodType;
                request.ServerCertificateValidationCallback += ServerCertificateValidationCallback;

                using var response = request.GetResponse();
                using var responseStream = response.GetResponseStream();
                
                if (responseStream == null) return default;
                
                string responseJson = new StreamReader(responseStream).ReadToEnd();
                return JsonConvert.DeserializeObject<T>(responseJson);
            }

            // todo
            protected virtual bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
            {
                return true;
            }

            public virtual Task<T> GetResponseAndResult<T>(string methodType = "GET")
            {
                var response = GetResponse<T>(methodType);
                return Task.FromResult(response);
            }
        }
        public class ApiMethod : Attribute
        {
            private String Name { get; }

            public ApiMethod(String name)
            {
                this.Name = name;
            }

            public static string Resolve(MethodBase methodBase)
            {
                return methodBase?.GetCustomAttribute<ApiMethod>()?.Name;
            }
        }
        public class ApiParam : Attribute
        {
            private String Name { get; }

            public ApiParam(String name)
            {
                this.Name = name;
            }

            public static string Resolve(ParameterInfo param)
            {
                return param?.GetCustomAttribute<ApiParam>()?.Name;
            }
        }
    }
}