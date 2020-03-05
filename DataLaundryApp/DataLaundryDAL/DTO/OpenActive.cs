using Newtonsoft.Json;
using System;
using System.Collections.Generic;
namespace DataLaundryDAL.DTO
{
    public class Root
    {
        [JsonProperty("items")]
        public List<Item> Items { get; set; }

        [JsonProperty("license")]
        public string License { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }
    }
           
    public class Item
    {
        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("modified")]
        public DateTimeOffset Modified { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }
           
    public class Data
    {
        [JsonProperty("@context")]
        public List<string> Context { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("organizer")]
        public Organizer Organizer { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("identifier")]
        public long Identifier { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("beta:formattedDescription")]
        public string BetaFormattedDescription { get; set; }

        [JsonProperty("disambiguatingDescription")]
        public string DisambiguatingDescription { get; set; }

        [JsonProperty("activity")]
        public List<string> Activity { get; set; }

        [JsonProperty("programme")]
        public string Programme { get; set; }

        [JsonProperty("startDate")]
        public DateTimeOffset StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTimeOffset EndDate { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("location")]
        public Location Location { get; set; }

        [JsonProperty("toLocation")]
        public List<object> ToLocation { get; set; }
        [JsonProperty("subEvent")]
        public List<SubEvent> SubEvent { get; set; }

        [JsonProperty("remainingAttendeeCapacity", NullValueHandling = NullValueHandling.Ignore)]
        public long? RemainingAttendeeCapacity { get; set; }

        [JsonProperty("maximumAttendeeCapacity", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaximumAttendeeCapacity { get; set; }

        [JsonProperty("leader")]
        public Leader Leader { get; set; }

        [JsonProperty("beta:distance")]
        public BetaDistance BetaDistance { get; set; }

        [JsonProperty("beta:level")]
        public string BetaLevel { get; set; }

        [JsonProperty("beta:hasCoaching")]
        public bool BetaHasCoaching { get; set; }

        [JsonProperty("beta:registrationCount")]
        public long BetaRegistrationCount { get; set; }

        [JsonProperty("image")]
        public List<SubEvent> Image { get; set; }

        [JsonProperty("ageRange")]
        public string AgeRange { get; set; }

        [JsonProperty("genderRestriction")]
        public string GenderRestriction { get; set; }

        [JsonProperty("isAccessibleForFree")]
        public bool IsAccessibleForFree { get; set; }

        [JsonProperty("publicAccess")]
        public bool PublicAccess { get; set; }

        [JsonProperty("beta:orderPostUrl")]
        public string BetaOrderPostUrl { get; set; }

        [JsonProperty("offers")]
        public Offers Offers { get; set; }
    }

    public class Image
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
    }
           
    public class BetaDistance
    {
        [JsonProperty("value")]
        public long? Value { get; set; }

        [JsonProperty("unit")]
        public string Unit { get; set; }
    }
           
    public class Leader
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
           
    public class Location
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("address")]
        public Address Address { get; set; }

        [JsonProperty("geo")]
        public Geo Geo { get; set; }

        [JsonProperty("areaServed")]
        public string AreaServed { get; set; }

        [JsonProperty("beta:meetingPoint")]
        public string BetaMeetingPoint { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
           
    public class Address
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("streetAddress")]
        public string StreetAddress { get; set; }

        [JsonProperty("addressLocality")]
        public string AddressLocality { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }
    }
           
    public class Geo
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }
    }
    
    public class SubEvent
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("startDate")]
        public DateTimeOffset StartDate { get; set; }
        [JsonProperty("endDate")]
        public DateTimeOffset EndDate { get; set; }
        [JsonProperty("duration")]
        public string Duration { get; set; }
        [JsonProperty("beta:availability")]
        public string BetaAvailability { get; set; }
    }
       
    public class Offers
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }
    }
           
    public class Organizer
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("telephone")]
        public string Telephone { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("logo")]
        public string Logo { get; set; }
    }
}
