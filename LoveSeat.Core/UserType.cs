using System.Collections.Generic;

namespace LoveSeat.Core
{
    public class UserType
    {
        public UserType()
        {
            names = new List<string>();
            roles = new List<string>();
        }

        public List<string> names { get; set; }
        public List<string> roles { get; set; }
    }

}
