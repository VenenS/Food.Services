using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Food.Services.Config.Sections
{
    public class AuthToken
    {
        private readonly string _key = "D4476F8A-63B3-4D7C-88E1-D17A09F6B5E9";

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public int TokenLifeTime { get; set; }

        public string Decryption { get; set; }

        public string DecryptionKey { get; set; }

        public string Validation { get; set; }

        public string ValidationKey { get; set; }

        public bool RequireHttpsMetadata { get; set; }


        public SecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_key));
        }
    }
}
