using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace CustomSuffix
{
    public static class Permissions
    {
        [Description("Allow user to set their own suffix and toggle.")]
        public static readonly string sufset = "customsuffix.set";

        [Description("Allow user to check and set another player's suffix.")]
        public static readonly string others = "customsuffix.others";
    }
}
