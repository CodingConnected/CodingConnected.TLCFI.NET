using System.Linq;
using CodingConnected.TLCFI.NET.Client.Data;
using CodingConnected.TLCFI.NET.Models.TLC;
using NLog;

namespace CodingConnected.TLCFI.NET.Client.Helpers
{
    public static class TLCFIClientCompatabilityChecker
    {
        #region Fields

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        #endregion // Fields

        #region Public Static Methods

        public static bool IsCLACompatibleWithTLC(TLCFIClientConfig tlcfiClientConfig, TLCFacilities facilities)
        {
            // SignalGroups: all ids must match
            foreach (var id1 in facilities.Signalgroups)
            {
                if (tlcfiClientConfig.SignalGroupIds.All(id2 => id1 != id2))
                {
                    _logger.Error("SignalGroup with id {0} from TLC facilities not found in CLA config", id1);
                    return false;
                }
            }
            foreach (var id1 in tlcfiClientConfig.SignalGroupIds)
            {
                if (facilities.Signalgroups.All(id2 => id1 != id2))
                {
                    _logger.Error("SignalGroup with id {0} from CLA config not found in TLC facilities", id1);
                    return false;
                }
            }

            // Detectors, out- and inputs: all ids from CLA must be present in TLC
            foreach (var id1 in tlcfiClientConfig.DetectorIds)
            {
                if (facilities.Detectors.All(id2 => id1 != id2))
                {
                    _logger.Error("Detector with id {0} from CLA config not found in TLC facilities", id1);
                    return false;
                }
            }
            foreach (var id1 in facilities.Detectors)
            {
                if (tlcfiClientConfig.DetectorIds.All(id2 => id1 != id2))
                {
                    _logger.Warn("Detector with id {0} from TLC facilities not found in CLA config ", id1);
                }
            }
            foreach (var id1 in tlcfiClientConfig.InputIds)
            {
                if (facilities.Inputs.All(id2 => id1 != id2))
                {
                    _logger.Error("Input with id {0} from CLA config not found in TLC facilities", id1);
                    return false;
                }
            }
            foreach (var id1 in facilities.Inputs)
            {
                if (tlcfiClientConfig.InputIds.All(id2 => id1 != id2))
                {
                    _logger.Warn("Input with id {0} from TLC facilities not found in CLA config ", id1);
                }
            }
            foreach (var id1 in tlcfiClientConfig.OutputIds)
            {
                if (facilities.Outputs.All(id2 => id1 != id2))
                {
                    _logger.Error("Output with id {0} from CLA config not found in TLC facilities", id1);
                    return false;
                }
            }
            foreach (var id1 in facilities.Outputs)
            {
                if (tlcfiClientConfig.OutputIds.All(id2 => id1 != id2))
                {
                    _logger.Warn("Output with id {0} from TLC facilities not found in CLA config ", id1);
                }
            }

            return true;
        }

        public static bool IsCLACompatibleWithIntersection(TLCFIClientConfig tlcfiClientConfig, Intersection intersection, TLCFacilities facilities)
        {
            // SignalGroups: all ids must match
            foreach (var id1 in intersection.Signalgroups)
            {
                if (tlcfiClientConfig.SignalGroupIds.All(id2 => id1 != id2))
                {
                    _logger.Error("SignalGroup with id {0} from TLC intersection not found in CLA config", id1);
                    return false;
                }
            }
            foreach (var id1 in tlcfiClientConfig.SignalGroupIds)
            {
                if (intersection.Signalgroups.All(id2 => id1 != id2))
                {
                    _logger.Error("SignalGroup with id {0} from CLA config not found in TLC intersection", id1);
                    return false;
                }
            }

            // Detectors, out- and inputs: all ids from CLA must be present in TLC
            foreach (var id1 in tlcfiClientConfig.DetectorIds)
            {
                if (intersection.Detectors.All(id2 => id1 != id2))
                {
                    _logger.Error("Detector with id {0} from CLA config not found in TLC intersection", id1);
                    return false;
                }
            }
            foreach (var id1 in intersection.Detectors)
            {
                if (tlcfiClientConfig.DetectorIds.All(id2 => id1 != id2))
                {
                    _logger.Warn("Detector with id {0} from TLC intersection not found in CLA config ", id1);
                }
            }
            foreach (var id1 in tlcfiClientConfig.InputIds)
            {
                if (intersection.Inputs.All(id2 => id1 != id2))
                {
                    _logger.Error("Input with id {0} from CLA config not found in TLC intersection", id1);
                    return false;
                }
            }
            foreach (var id1 in intersection.Inputs)
            {
                if (tlcfiClientConfig.InputIds.All(id2 => id1 != id2))
                {
                    _logger.Warn("Input with id {0} from TLC intersection not found in CLA config ", id1);
                }
            }

            // for outputs: use outputs from TLC if facilities argument is not null
            // this facilitates configuring non-exclusive outputs
            foreach (var id1 in tlcfiClientConfig.OutputIds)
            {
                if (facilities != null && facilities.Outputs.All(id2 => id1 != id2) ||
                    facilities == null && intersection.Outputs.All(id2 => id1 != id2))
                {
                    _logger.Error("Output with id {0} from CLA config not found in TLC intersection", id1);
                    return false;
                }
            }
            if (facilities == null)
            {
                foreach (var id1 in intersection.Outputs)
                {
                    if (tlcfiClientConfig.OutputIds.All(id2 => id1 != id2))
                    {
                        _logger.Warn("Output with id {0} from TLC intersection not found in CLA config ", id1);
                    }
                }
            }
            else
            {
                foreach (var id1 in facilities.Outputs)
                {
                    if (tlcfiClientConfig.OutputIds.All(id2 => id1 != id2))
                    {
                        _logger.Warn("Output with id {0} from TLC facilities not found in CLA config ", id1);
                    }
                }
            }

            return true;
        }

        #endregion // Public Static Methods
    }
}
