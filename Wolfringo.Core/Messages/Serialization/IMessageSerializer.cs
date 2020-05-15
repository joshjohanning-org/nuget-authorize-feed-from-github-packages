﻿using System.Collections.Generic;

namespace TehGM.Wolfringo.Messages.Serialization
{
    public interface IMessageSerializer
    {
        string Serialize(IWolfMessage message);
        IWolfMessage Deserialize(string command, string payload, IEnumerable<byte[]> buffers);
    }
}
