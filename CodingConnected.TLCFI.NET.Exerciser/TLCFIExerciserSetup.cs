using System.Collections.Generic;
using JetBrains.Annotations;
using System.Xml.Serialization;

namespace TLCFI.NET.Exerciser
{
    [UsedImplicitly]
    public class TLCFIExerciserSetupElement
    {
        #region Properties

        public string Method { get; [UsedImplicitly] set; }
        public string Type { get; [UsedImplicitly] set; }
        public string Response { get; [UsedImplicitly] set; }

        #endregion // Properties
    }

    public class TLCFIExerciserSetup
    {
        #region Fields
        #endregion // Fields

        #region Properties
		
        [XmlArrayItem("Element")]
        public List<TLCFIExerciserSetupElement> Elements { get; set; }

        #endregion // Properties

        #region Constructor

        public TLCFIExerciserSetup()
        {
            Elements = new List<TLCFIExerciserSetupElement>();
        }

        #endregion // Constructor
    }
}