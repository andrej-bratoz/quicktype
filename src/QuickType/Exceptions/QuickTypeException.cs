using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickType.Exceptions
{
    public class QuickTypeException : Exception
    {
        public QuickTypeException(string message) : base(message)
        {

        }

        public QuickTypeException(string message, Exception innerEx) : base(message, innerEx)
        {

        }
    }
}
