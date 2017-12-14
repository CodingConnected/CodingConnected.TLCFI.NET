using CodingConnected.JsonRPC;
using CodingConnected.TLCFI.NET.Core.Models.Generic;
using NLog;

namespace CodingConnected.TLCFI.NET.Core.Extensions
{
	/// <summary>
	/// Extension for NLog.Logger, allowing specific logging for the JsonRpcException class
	/// </summary>
    public static class LoggerExtensions
    {
        public static void LogRpcException(this Logger logger, JsonRpcException e)
        {
            switch (e.Code)
            {
                case (int)ProtocolErrorCode.Error:
                    logger.Error("An unknown error occured with the TLC-FI. See trace for exception details.");
                    logger.Trace(e, "An unknown error occured with the TLC-FI:");
                    break;
                case (int)ProtocolErrorCode.NotAuthorized:
                    logger.Error("Client is not authorized to use the server. See trace for exception details.");
                    logger.Trace(e, "Client is not authorized to use the server:");
                    break;
                case (int)ProtocolErrorCode.NoRights:
                    logger.Error("Client has no (appropriate) rights on the server. See trace for exception details.");
                    logger.Trace(e, "Client has no (appropriate) rights on the server:");
                    break;
                case (int)ProtocolErrorCode.InvalidProtocol:
                    logger.Error("The protocol version used by the client is not compatible with that at the server. See trace for exception details.");
                    logger.Trace(e, "The protocol version used by the client is not compatible with that at the server:");
                    break;
                case (int)ProtocolErrorCode.AlreadyRegistered:
                    logger.Error("Client is already registered. See trace for exception details.");
                    logger.Trace(e, "Client is already registered:");
                    break;
                case (int)ProtocolErrorCode.UnknownObjectType:
                    logger.Error("Unknown object type. See trace for exception details.");
                    logger.Trace(e, "Unknown object type:");
                    break;
                case (int)ProtocolErrorCode.MissingAttribute:
                    logger.Error("Missing attribute. See trace for exception details.");
                    logger.Trace(e, "Missing attribute:");
                    break;
                case (int)ProtocolErrorCode.InvalidAttributeType:
                    logger.Error("Invalid attribute type. See trace for exception details.");
                    logger.Trace(e, "Invalid attribute type:");
                    break;
                case (int)ProtocolErrorCode.InvalidAttributeValue:
                    logger.Error("Invalid attribute value. See trace for exception details.");
                    logger.Trace(e, "Invalid attribute value:");
                    break;
                case (int)ProtocolErrorCode.InvalidObjectReference:
                    logger.Error("Invalid object reference. See trace for exception details.");
                    logger.Trace(e, "Invalid object reference:");
                    break;
                default:
                    logger.Error("Unknown type of error ({0} is not a valid value for enum ProtocolErrorCode). See trace for exception details.", e.Code);
                    logger.Trace(e, "Unknown type of error ({0} is not a valid value for enum ProtocolErrorCode):", e.Code);
                    break;
            }
        }
    }
}