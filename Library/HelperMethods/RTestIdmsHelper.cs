namespace Library.HelperMethods
{
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Automation;
    using Skyline.DataMiner.Core.DataMinerSystem.Common;
    using System;
    using System.Collections.Generic;

    public class RTestIdmsHelper
    {
        private IEngine _engine;
        private IDms dms;

        public RTestIdmsHelper(IEngine engine)
        {
            _engine = engine;
            dms = engine.GetDms();
        }

        public IDmsElement ScheduAllElement
        {
            get
            {
                ICollection<IDmsElement> elements = dms.GetElements();

                foreach (IDmsElement element in elements)
                {
                    if (element.Protocol.Name.Equals("ScheduAll Generic Interop Manager") && element.Protocol.Version.Equals("Production", StringComparison.OrdinalIgnoreCase))
                    {
                        return element;
                    }
                }

                return null;
            }
        }
    }
}
