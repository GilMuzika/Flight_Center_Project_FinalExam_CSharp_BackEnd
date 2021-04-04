using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web_Api_interface.Models
{
    public class BooleanFunctionAnswer<T>
    {
        public bool flagAnswer { get; set; }
        public string Explaination { get; set; }

        //The 'string' key of the Dictionary is the property explaination
        public Dictionary<string, T> OptionalProps { get; set; } = new Dictionary<string, T>();

        public BooleanFunctionAnswer(bool flagAnswer, string explaination)
        {
            this.flagAnswer = flagAnswer;
            Explaination = explaination;
        }
    }
}