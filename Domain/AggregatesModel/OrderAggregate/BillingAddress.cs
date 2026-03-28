using Domain.SeedWork;

namespace Domain.AggregatesModel.OrderAggregate;

public class BillingAddress : ValueObject
{
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Address1 { get; private set; } = default!;
    public string? Address2 { get; private set; }
    public string City { get; private set; } = default!;
    public string? Province { get; private set; }
    public string Country { get; private set; } = default!;
    public string? PostalCode { get; private set; }
    public string? Phone { get; private set; }

    private BillingAddress() { }

    public static BillingAddress Create(
        string firstName,
        string lastName,
        string address1,
        string city,
        string country,
        string? address2 = null,
        string? province = null,
        string? postalCode = null,
        string? phone = null)
    {
        return new BillingAddress
        {
            FirstName = firstName,
            LastName = lastName,
            Address1 = address1,
            Address2 = address2,
            City = city,
            Province = province,
            Country = country,
            PostalCode = postalCode,
            Phone = phone
        };
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
        yield return Address1;
        yield return City;
        yield return Country;
    }
}
