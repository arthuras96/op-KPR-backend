using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Functions
{
    public class Utils
    {
        public string ExtractNumbers(string toNormalize)
        {
            string resultString = string.Empty;
            Regex regexObj = new Regex(@"[^\d]");
            resultString = regexObj.Replace(toNormalize, "");
            return resultString;
        }

        public string RemoveAccents(string text)
        {
            string withAccents = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇç";
            string withoutAccents = "AAAAAAaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUuuuuCc";

            if (text != "")
            {
                for (int i = 0; i < withAccents.Length; i++)
                {
                    text = text.Replace(withAccents[i].ToString(), withoutAccents[i].ToString());
                }
            }
            return text;
        }

        public string RemoveLeadingZero(string text)
        {
            if(text.Length > 0 && text != null)
                while(text[0] == '0')
                {
                    text = text.Substring(1);
                }
            return text;
        }
    }
}
