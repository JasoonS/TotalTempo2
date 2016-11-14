using System.Collections.Generic;

using ExitGames.Logging;

using TT_Network_Framework;

namespace TT_Network_Photon.Server
{
    public class PhotonServerHandlerList
    {
        private readonly DefaultRequestHandler _defaultRequestHandler;
        private readonly DefaultResponseHandler _defaultResponseHandler;
        private readonly DefaultEventHandler _defaultEventHandler;

        protected readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<int, PhotonServerHandler> _requestHandlerList;
        private readonly Dictionary<int, PhotonServerHandler> _responseHandlerList;
        private readonly Dictionary<int, PhotonServerHandler> _eventHandlerList;


        public PhotonServerHandlerList(IEnumerable<IHandler<PhotonServerHandler>> handlers, DefaultRequestHandler defaultRequestHandler, DefaultResponseHandler defaultResponseHandler, DefaultEventHandler defaultEventHandler)
        {
            _defaultRequestHandler = defaultRequestHandler;
            _defaultResponseHandler = defaultResponseHandler;
            _defaultEventHandler = defaultEventHandler;

            _requestHandlerList = new Dictionary<int, PhotonServerHandler>();
            _responseHandlerList = new Dictionary<int, PhotonServerHandler>();
            _eventHandlerList = new Dictionary<int, PhotonServerHandler>();

            foreach (PhotonServerHandler handler in handlers)
            {
                if (!RegisterHandler(handler))
                    Log.WarnFormat("Attempted to register handler {0} for type {1}:{2}", handler.GetType().Name, handler.Type, handler.Code);
            }
        }

        public bool RegisterHandler(PhotonServerHandler handler)
        {
            var registered = false;

            if ((handler.Type & MessageType.Request) == MessageType.Request)
            {
                if (handler.SubCode.HasValue && !_requestHandlerList.ContainsKey(handler.SubCode.Value))
                {
                    _requestHandlerList.Add(handler.SubCode.Value, handler);

                    registered = true;
                }

                else if (!_requestHandlerList.ContainsKey(handler.Code))
                {
                    _requestHandlerList.Add(handler.Code, handler);

                    registered = true;
                }

                else
                {
                    Log.ErrorFormat("RequestHandler list already contains handler for {0} - cannot add {1}", handler.Code, handler.GetType().Name);
                }
            }

            if ((handler.Type & MessageType.Response) == MessageType.Response)
            {
                if (handler.SubCode.HasValue && !_responseHandlerList.ContainsKey(handler.SubCode.Value))
                {
                    _responseHandlerList.Add(handler.SubCode.Value, handler);

                    registered = true;
                }

                else if (!_responseHandlerList.ContainsKey(handler.Code))
                {
                    _responseHandlerList.Add(handler.Code, handler);

                    registered = true;
                }

                else
                {
                    Log.ErrorFormat("ResponseHandler list already contains handler for {0} - cannot add {1}", handler.Code, handler.GetType().Name);
                }
            }

            if ((handler.Type & MessageType.Async) == MessageType.Async)
            {
                if (handler.SubCode.HasValue && !_eventHandlerList.ContainsKey(handler.SubCode.Value))
                {
                    _eventHandlerList.Add(handler.SubCode.Value, handler);

                    registered = true;
                }

                else if (!_eventHandlerList.ContainsKey(handler.Code))
                {
                    _eventHandlerList.Add(handler.Code, handler);

                    registered = true;
                }

                else
                {
                    Log.ErrorFormat("EventHandler list already contains handler for {0} - cannot add {1}", handler.Code, handler.GetType().Name);
                }
            }

            return registered;
        }

        public bool HandleMessage(IMessage message, PhotonServerPeer peer)
        {
            bool handled = false;

            switch(message.Type)
            {
                case MessageType.Request:
                    if (message.SubCode.HasValue && _requestHandlerList.ContainsKey(message.SubCode.Value))
                    {
                        _requestHandlerList[message.SubCode.Value].HandleMessage(message, peer);

                        handled = true;
                    }

                    else if (!message.SubCode.HasValue && _requestHandlerList.ContainsKey(message.Code))
                    {
                        _requestHandlerList[message.Code].HandleMessage(message, peer);

                        handled = true;
                    }

                    else
                    {
                        _defaultRequestHandler.HandleMessage(message, peer);
                    }

                    break;

                case MessageType.Response:
                    if (message.SubCode.HasValue && _responseHandlerList.ContainsKey(message.SubCode.Value))
                    {
                        _responseHandlerList[message.SubCode.Value].HandleMessage(message, peer);

                        handled = true;
                    }

                    else if (!message.SubCode.HasValue && _responseHandlerList.ContainsKey(message.Code))
                    {
                        _responseHandlerList[message.Code].HandleMessage(message, peer);

                        handled = true;
                    }

                    else
                    {
                        _defaultResponseHandler.HandleMessage(message, peer);
                    }

                    break;

                case MessageType.Async:
                    if (message.SubCode.HasValue && _eventHandlerList.ContainsKey(message.SubCode.Value))
                    {
                        _eventHandlerList[message.SubCode.Value].HandleMessage(message, peer);

                        handled = true;
                    }

                    else if (!message.SubCode.HasValue && _eventHandlerList.ContainsKey(message.Code))
                    {
                        _eventHandlerList[message.Code].HandleMessage(message, peer);

                        handled = true;
                    }

                    else
                    {
                        _defaultEventHandler.HandleMessage(message, peer);
                    }

                    break;
            }

            return handled;
        }
    }
}