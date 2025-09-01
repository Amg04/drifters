using DAL.Models;

namespace PL.Services.RtspUrlBuilder
{
    public interface IRtspUrlBuilder
    {
        string Build(Camera cam, string decryptedPassword);
    }
}
