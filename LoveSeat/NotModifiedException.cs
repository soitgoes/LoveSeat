using System;

namespace LoveSeat
{
    public class NotModifiedException : Exception
    {
        public NotModifiedException(string s):base(s)
        {            
        }
    }
}