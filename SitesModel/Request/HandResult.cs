using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitesModel.Request
{
    public enum HandResult
    {
        /// <summary>
        /// Request to resend http request
        /// </summary>
        RequestResend,
        /// <summary>
        /// Data hand successful
        /// </summary>
        HandComplete,
        /// <summary>
        /// Handed data but found data error
        /// </summary>
        HandedButWrong,
        /// <summary>
        /// Force to stop current request event auto repeat
        /// </summary>
        ForceStopCurrent
    }
}
