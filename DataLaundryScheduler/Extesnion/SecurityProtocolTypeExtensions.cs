namespace DataLaundryScheduler.Extesnion
{
    using System.Net;
    using System.Security.Authentication;

    public static class SecurityProtocolTypeExtensions
    {
        public const SecurityProtocolType Tls12 = (SecurityProtocolType)SslProtocolsExtensions.Tls12;

        public const SecurityProtocolType Tls11 = (SecurityProtocolType)SslProtocolsExtensions.Tls11;

        public const SecurityProtocolType SystemDefault = (SecurityProtocolType)0;
    }
    public static class SslProtocolsExtensions
    {

        public const SslProtocols Tls12 = (SslProtocols)0x00000C00;

        public const SslProtocols Tls11 = (SslProtocols)0x00000300;

    }
}