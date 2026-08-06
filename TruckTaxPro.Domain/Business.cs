namespace TruckTaxPro.Domain;

public class Business
{
    public int Id { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string Ein { get; set; } = string.Empty;         // format: XX-XXXXXXX
    public string BusinessType { get; set; } = string.Empty; // dropdown value
    public string AddressLine1 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}