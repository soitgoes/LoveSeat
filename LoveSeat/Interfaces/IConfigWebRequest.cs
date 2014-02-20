namespace LoveSeat.Interfaces
{
    using System.Net;

    public interface IConfigWebRequest
    {
        /// <summary>
        /// Used to configure the web request created by LoveSeat. For example, to set things like
        /// SSL settings.
        /// </summary>
        /// <param name="webRequest"></param>
        void ConfigWebRequest(HttpWebRequest webRequest);
    }
}
