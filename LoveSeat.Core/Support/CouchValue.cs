namespace LoveSeat.Core
{
    public static class CouchValue
    {
        static object _value = new object();

        public static object MaxValue
        {
            get { return _value; }
        }

        public static object MinValue
        {
            get { return null; }
        }
    }
}
