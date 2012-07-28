using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace LoveSeat.Support
{
    public class HttpException : Exception
    {
        public HttpException(string error)
            : base(error)
        {}
    }
}