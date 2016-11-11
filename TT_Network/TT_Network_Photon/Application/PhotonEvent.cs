// Class to represent a network event.

using System.Collections.Generic;

using TT_Network_Framework;

namespace TT_Network_Photon.Application
{
    public class PhotonEvent: IMessage
    {
        private readonly byte _code;
        private readonly int? _subCode;
        private readonly Dictionary<byte, object> _parameters;

        public PhotonEvent(byte code, int? subCode, Dictionary<byte, object> parameters)
        {
            _code = code;
            _subCode = subCode;
            _parameters = parameters;
        }

        public MessageType Type
        {
            get { return MessageType.Async; }
        }

        public byte Code
        {
            get { return _code; }
        }

        public int? SubCode
        {
            get { return _subCode; }
        }

        public Dictionary<byte, object> Parameters
        {
            get { return _parameters; }
        }
    }
}