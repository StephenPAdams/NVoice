using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using NVoice.Helpers.Extensions;
using PCLWebUtility;

namespace NVoice
{
    public class SharpVoice
    {
        private string _rnrse;
        private readonly CookieWebClient _webClient;
        private readonly string _username;
        private readonly string _password;

        private const string ServiceLoginUrl = "https://accounts.google.com/ServiceLogin?service=grandcentral";
        private const string GoogleVoiceUrl = "https://www.google.com/voice/m";
        private const string GoogleVoiceSendUrl = "https://www.google.com/voice/sms/send";
        private const Int32 MaxCharactersPerSms = 160;
        private const string UserAgent = "Mozilla/5.0 (iPhone; U; CPU iPhone OS 2_2_1 like Mac OS X; en-us) AppleWebKit/525.18.1 (KHTML, like Gecko) Version/3.1.1 Mobile/5H11 Safari/525.20";

        public SharpVoice(string username, string password)
        {
            _username = username;
            _password = password;
            _webClient = new CookieWebClient();
        }

        private String GetResponse(HttpRequestMessage httpRequestMessage)
        {
            var taskMessage = String.Empty;
            var task = _webClient.SendAsync(httpRequestMessage)
                                           .ContinueWith((taskwithmsg) =>
                                           {
                                               var response = taskwithmsg.Result;
                                               var stringTask = response.Content.ReadAsStringAsync();
                                               stringTask.Wait();
                                               taskMessage = stringTask.Result;
                                           });
            task.Wait();

            return taskMessage;
        }

        private void Login()
        {
            var requestLoginStartMessage = GetHttpGetRequestMessage(ServiceLoginUrl);
            var loginStartResponse = GetResponse(requestLoginStartMessage);

            // Get "GALX" field value from google login page                
            var galxRegex = new Regex(@"name=""GALX"".*?value=""(.*?)""", RegexOptions.Singleline);
            var galx = galxRegex.Match(loginStartResponse).Groups[1].Value;

            // Sending login info
            var requestLoginMessage = GetHttpPostRequestMessage(ServiceLoginUrl);
            var postData = PostParameters(new Dictionary<String, String>
                                              {
                                                  {"Email", _username},
                                                  {"Passwd", _password},
                                                  {"GALX", galx},
                                                  {"PersistentCookie", "yes"}
                                              });

            requestLoginMessage.Content = new StringContent(postData.ToStringContent(), Encoding.UTF8,
                                                            "application/x-www-form-urlencoded");

            GetResponse(requestLoginMessage);
        }

        /// <summary>
        /// creates a byte-array ready to be sent with a POST-request
        /// </summary>
        private static byte[] PostParameters(IDictionary<string, string> parameters)
        {
            var paramStr = parameters.Keys.Aggregate("",
                                                     (current, key) =>
                                                     current +
                                                     (key + "=" + WebUtility.UrlEncode(parameters[key]) + "&"));

            return Encoding.UTF8.GetBytes(paramStr);
        }

        private void TryGetRNRSE()
        {
            if (!GetRNRSE())
            {
                // We can't find the required field on the page. Probably we're logged out. Let's try to login
                Login();
                if (!GetRNRSE())
                    throw new Exception("Unable to get the Session-id field from Google.");
            }
        }

        private HttpRequestMessage GetRequestMessage(String url, HttpMethod httpMethod)
        {
            var requestMessage = new HttpRequestMessage(httpMethod, url);
            requestMessage.Headers.Add("User-agent", UserAgent);
            return requestMessage;
        }

        private HttpRequestMessage GetHttpGetRequestMessage(String url)
        {
            return GetRequestMessage(url, HttpMethod.Get);
        }

        private HttpRequestMessage GetHttpPostRequestMessage(String url)
        {
            return GetRequestMessage(url, HttpMethod.Post);
        }

        /// <summary>
        /// Gets google's "session id" hidden-field value
        /// </summary>
        private bool GetRNRSE()
        {
            var requestMessage = GetHttpGetRequestMessage(GoogleVoiceUrl);
            var taskResponse = GetResponse(requestMessage);

            // Find the hidden field
            var rnrRegex = new Regex(@"<input.*?name=""_rnr_se"".*?value=""(.*?)""");
            if (rnrRegex.IsMatch(taskResponse))
            {
                _rnrse = rnrRegex.Match(taskResponse).Groups[1].Value;
                return true;
            }

            return false;
        }


        /// <summary>
        /// Sends text-message to teh specified number
        /// </summary>
        /// <param name="number">Phone number in '+1234567890'-format </param>
        /// <param name="text">Message text</param>
        /// <param name="totalMessages"></param>
        public Boolean SendSMS(string number, string text, out Int32 totalMessages)
        {
            if (!ValidateNumber(number))
                throw new FormatException("Wrong number format. Should be '+1234567890'. Please try again.");

            TryGetRNRSE();

            // Iterate through and send multiple broken up
            var messages = text.ChunksUpTo(MaxCharactersPerSms, MaxCharactersPerSms).ToList();
            foreach (var message in messages)
            {
                var postData = PostParameters(new Dictionary<string, string>
                                                  {
                                                      {"phoneNumber", number},
                                                      {"text", message},
                                                      {"_rnr_se", _rnrse}
                                                  });

                var requestMessage = GetHttpPostRequestMessage(GoogleVoiceSendUrl);
                requestMessage.Content = new StringContent(postData.ToStringContent(), Encoding.UTF8,
                                                           "application/x-www-form-urlencoded");

                var taskResponse = GetResponse(requestMessage);
                if (taskResponse.IndexOf("\"ok\":true", StringComparison.Ordinal) == -1)
                    throw new Exception("Google did not answer with an OK response\n\n" + taskResponse);
            }

            totalMessages = messages.Count();

            return true;
        }

        public static bool ValidateNumber(string number)
        {
            if (number == null) return false;
            return Regex.IsMatch(number, @"^\+\d{11}$");
        }
    }

    internal class CookieWebClient : HttpClient
    {
        private CookieContainer _cookieContainer = new CookieContainer();

        public CookieContainer CookieContainer
        {
            get { return _cookieContainer; }
            set { _cookieContainer = value; }
        }
    }
}
