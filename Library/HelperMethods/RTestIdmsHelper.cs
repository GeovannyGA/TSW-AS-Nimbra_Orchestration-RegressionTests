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

        public static string GetRowWithChainIdAndWorkOrderId(IDmsTable table, string bookingChainId, string bookingWordOrder)
        {
            string[] keys = table.GetPrimaryKeys();

            if (keys.Length == 0)
            {
                return String.Empty;
            }

            var chainIdColumn = table.GetColumn<string>(ConstantVariables.ChainPid);
            var workOrderIdColumn = table.GetColumn<string>(ConstantVariables.WorkOrderPid);

            for (int i = 0; i < keys.Length; i++)
            {
                string chainId = chainIdColumn.GetValue(keys[i], KeyType.PrimaryKey);
                string workOrderId = workOrderIdColumn.GetValue(keys[i], KeyType.PrimaryKey);
                if (chainId.Equals(bookingChainId) && workOrderId.Equals(bookingWordOrder))
                {
                    return keys[i];
                }
            }

            return String.Empty;
        }
    }
}
