﻿using System;
using CodingConnected.TLCFI.NET.Core.Models.Generic;
using CodingConnected.TLCFI.NET.Core.Models.TLC.Base;
using CodingConnected.TLCFI.NET.Core.Models.TLC.Base;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Core.Models.TLC
{
    public class ConsumerApplication : TLCSessionBase
    {
        #region Fields

        private string _id;

        #endregion // Fields

        #region Properties

        // NON FI

        [JsonIgnore]
        public override ApplicationType? SessionType => ApplicationType.Consumer;

        [JsonIgnore]
        public override TLCObjectType ObjectType => TLCObjectType.Session;

        [JsonIgnore]
        public override bool StateChanged => false;

        // META

        [ObjectId]
        [JsonProperty("sessionid")]
        public override string Id // TLC-FI type: ObjectID
        {
            get => _id;
            set
            {
                Tools.ValueChecker.CheckValidObjectId(value);
                _id = value;
            }
        }
        [JsonProperty("type")]
        public ApplicationType Type { get; set; }

        #endregion // Properties

        #region TLCObjectBase Methods

        public override object GetMeta()
        {
            return new
            {
                sessionid = Id,
                type = Type
            };
        }

        public override void CopyState(object o)
        {
            throw new UnauthorizedAccessException();
        }

        public override object GetState(bool tlc = false)
        {
            throw new UnauthorizedAccessException();
        }

        public override object GetFullState(bool tlc = false)
        {
            throw new UnauthorizedAccessException();
        }

        #endregion // TLCObjectBase Methods
    }
}
