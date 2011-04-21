using System;
using System.Collections.Generic;
using System.Text;
using eExNetworkLibrary.HTTP;

namespace eExNetworkLibrary.TrafficModifiers.StreamModification.HTTP.Conditions
{
    public class HeaderCondition : HTTPStreamModifierCondition
    {
        /// <summary>
        /// Gets or sets the name of the header to search for
        /// </summary>
        public string Header { get; set; }
        /// <summary>
        /// Gets or sets the regular expression to match
        /// </summary>
        public string Pattern { get; set; }

        public override bool IsMatch(HTTPMessage httpMessage)
        {
            bool bResult = false;

            foreach (HTTPHeader hHeader in httpMessage.Headers[Header])
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(hHeader.Value, Pattern))
                {
                    bResult = true;
                    break;
                }

            }

            return bResult && base.IsMatch(httpMessage);
        }

        public override string Name
        {
            get { return "Regex Header Condition"; }
        }

        public override string GetLongDescription()
        {
            return "If header " + Header + " matches " + Pattern;
        }

        public override string GetShortDescription()
        {
            return Header + " matches " + Pattern;
        }

        public override object Clone()
        {
            HeaderCondition hdCondition = new HeaderCondition();
            hdCondition.Header = this.Header;
            hdCondition.Pattern = this.Pattern;
            CloneChildsTo(hdCondition);
            return hdCondition;
        }
    }
}
