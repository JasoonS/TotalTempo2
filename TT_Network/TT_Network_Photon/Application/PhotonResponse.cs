// Class to represent a network response.

using System.Collections.Generic;

using TT_Network_Framework;

namespace TT_Network_Photon.Application
{
    public class PhotonResponse: IMessage
    {
        private readonly byte _code;
        private readonly int? _subCode;
        private readonly Dictionary<byte, object> _parameters;
        private readonly string _debugMessage;
        private readonly short _returnCode;

        public PhotonResponse(byte code, int? subCode, Dictionary<byte, object> parameters, string debugMessage, short returnCode)
        {
            _code = code;
            _subCode = subCode;
            _parameters = parameters;
            _debugMessage = debugMessage;
            _returnCode = returnCode;
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

        public string DebugMessage
        {
            get { return _debugMessage; }
        }

        public short ReturnCode
        {
            get { return _returnCode; }
        }
    }
}