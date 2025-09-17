namespace Edu_mobileapp.Models;

public class Customer
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Gender { get; set; } = "Diðer";
    public string AddressTR { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string AddressJP { get; set; } = "";
    public string LanguageSchoolName { get; set; } = "";
    public string LanguageSchoolAddress { get; set; } = "";
    public string PassportNo { get; set; } = "";
    public DateTime? PassportIssuedDate { get; set; }
    public DateTime? PassportExpiryDate { get; set; }
    public string CoaFilePath { get; set; } = "";
    public string CoeFilePath { get; set; } = "";
    public string EmergencyContactName { get; set; } = "";
    public string EmergencyContactPhone { get; set; } = "";
    public string EmergencyContactRelation { get; set; } = "";

    public string FullName => $"{FirstName} {LastName}".Trim();
}
