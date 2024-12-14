namespace FortitudeAsia.Models
{
    public class Partner
    {
        public string PartnerNo { get; set; }
        public string AllowedPartner { get; set; }
        public string Password { get; set; }
    }

    public static class PartnerDb
    {
        // Temporary
        private static readonly List<Partner> allowedPartners =
        [
            new Partner { PartnerNo = "FG-00001", AllowedPartner = "FAKEGOOGLE", Password = "FAKEPASSWORD1234" },
            new Partner { PartnerNo = "FG-00002", AllowedPartner = "FAKEPEOPLE", Password = "FAKEPASSWORD4578" }
        ];

        public static List<Partner> GetPartners()
        {
            return allowedPartners;
        }
    }
}
