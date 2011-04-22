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

        /// <summary>
        /// Gets or sets a bool which indicates if the request for a response should also be evaluated. <br />
        /// This means, if this property is set to true, that if a request is a match, the according response will also be a match.<br />
        /// If this property is set to false, a request will be evaluated, regardless of the evaluation result for the response. 
        /// </summary>
        public bool EvaluateRequestForResponse { get; set; }

        private bool bLastRequestWasMatch;

        public HeaderCondition()
        {
            Header = "Host";
            Pattern = "*";
            EvaluateRequestForResponse = true;
        }

        public override bool IsMatch(HTTPMessage httpMessage)
        {
            bool bResult = false;

            if (httpMessage.MessageType == HTTPMessageType.Response && EvaluateRequestForResponse && bLastRequestWasMatch)
            {
                bResult = true;
            }
            else
            {
                foreach (HTTPHeader hHeader in httpMessage.Headers[Header])
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(hHeader.Value, Pattern))
                    {
                        bResult = true;
                        break;
                    }

                }

                if (httpMessage.MessageType == HTTPMessageType.Request)
                {
                    bLastRequestWasMatch = bResult;
                }
            }

            bool bBaseResult = base.IsMatch(httpMessage);
            return bResult && bBaseResult;
        }

        public override string Name
        {
            get { return "Regex Header Condition"; }
        }

        public override string GetLongDescription()
        {
            return "If " + (EvaluateRequestForResponse ? "Request or Response " : "") + "header \"" + Header + "\" matches \"" + Pattern + "\"";
        }

        public override string GetShortDescription()
        {
            return "If " + (EvaluateRequestForResponse ? "REQ/RSP " : "") + "header \"" + Header + "\" matches \"" + Pattern + "\"";
        }

        public override object Clone()
        {
            HeaderCondition hdCondition = new HeaderCondition();
            hdCondition.Header = this.Header;
            hdCondition.Pattern = this.Pattern;
            hdCondition.EvaluateRequestForResponse = this.EvaluateRequestForResponse;
            CloneChildsTo(hdCondition);
            return hdCondition;
        }
    }
}
