﻿using Newtonsoft.Json;
using System.Net;

namespace TehGM.Wolfringo.Messages.Responses
{
    public class WolfResponse : IWolfResponse
    {
        [JsonProperty("code")]
        private readonly int _code;

        public HttpStatusCode ResponseCode => (HttpStatusCode)_code;
    }
}
