using DataLaundryApi.Constants;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace DataLaundryApi.Models
{
    #region Session
    public class SessionContainer
    {
        [JsonProperty(PropertyName = "next")]
        public string Next { get; set; } = "/api/sessions";

        [JsonProperty(PropertyName = "items")]
        public List<SessionDetails> Items { get; set; }

        [JsonProperty(PropertyName = "license")]
        public string License { get; set; } = "https://creativecommons.org/licenses/by/4.0/";
        public SessionContainer()
        {
            Items = new List<SessionDetails>();
        }
    }

    public class SessionContainer_v1
    {
        [JsonProperty(PropertyName = "next")]
        public string Next { get; set; } = (Settings.GetAppSetting("scheduledSessionAPI") ?? "/rpde/scheduled-sessions");

        [JsonProperty(PropertyName = "items")]
        public List<SessionDetails_v1> Items { get; set; }

        [JsonProperty(PropertyName = "license")]
        public string License { get; set; } = "https://creativecommons.org/licenses/by/4.0/";
        public SessionContainer_v1()
        {
            Items = new List<SessionDetails_v1>();
        }
    }

public class SessionDetails
    {
        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; } = "Session";

        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }
    
   


        [JsonProperty(PropertyName = "modified")]
        public long Modified { get; set; }

        [JsonProperty(PropertyName = "data", NullValueHandling = NullValueHandling.Ignore)]
        public Session Data { get; set; }
    }

    public class SessionDetails_v1
    {
        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; } = "Session";

        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        [JsonProperty(PropertyName = "modified")]
        public long Modified { get; set; }

        [JsonProperty(PropertyName = "data", NullValueHandling = NullValueHandling.Ignore)]
        public ExpandoObject Sessions { get; set; }
    }

    public class Session
    {

        [JsonProperty("@context")]
        public List<string> Context { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = "Event";

        [JsonProperty("url")]
        public string Url { get; set; }
        
        [JsonProperty(PropertyName = "SessionId")]
       
        public string SessionId { get; set; }
         [JsonProperty(PropertyName = "Distance")]
       
        public decimal Distance { get; set; }
        [JsonProperty("identifier")]
        public long? Identifier { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("image")]
        public Image Image { get; set; }

        [JsonProperty("startDate")]
        public DateTimeOffset? StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTimeOffset? EndDate { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }
         
     
        [JsonProperty("location")]
        public List<Location> Location { get; set; }

        [JsonProperty("organizer")]
        public Person Organizer { get; set; }

        [JsonProperty("contributor")]
        public Person Contributor { get; set; }

        [JsonProperty("maximumAttendeeCapacity", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaximumAttendeeCapacity { get; set; }

        [JsonProperty("remainingAttendeeCapacity", NullValueHandling = NullValueHandling.Ignore)]
        public long? RemainingAttendeeCapacity { get; set; }
        [JsonProperty("Price")]

        public string Price { get; set; }

        [JsonProperty("occurrences")]
        public List<Occurrences> Occurrences { get; set; }

        [JsonProperty("eventStatus")]
        public string EventStatus { get; set; }

        [JsonProperty("subEvent")]
        public List<Session> SubEvent { get; set; }

        [JsonProperty("superEvent")]
        public Session SuperEvent { get; set; }

        [JsonProperty("eventSchedule")]
        public EventSchedule EventSchedule { get; set; }

        [JsonProperty("activity")]
        public List<Activity> Activity { get; set; }

        [JsonProperty("category")]
        public string[] Category { get; set; }

        [JsonProperty("ageRange")]
        public string AgeRange { get; set; }

        [JsonProperty("genderRestriction")]
        public string GenderRestriction { get; set; }

        [JsonProperty("programme")]
        public Programme Programme { get; set; }

        [JsonProperty("attendeeInstructions")]
        public string AttendeeInstructions { get; set; }

        [JsonProperty("leader")]
        public Person Leader { get; set; }

        [JsonProperty("accessibilitySupport")]
        public string[] AccessibilitySupport { get; set; }

        [JsonProperty("accessibilityInformation")]
        public string AccessibilityInformation { get; set; }

        [JsonProperty("level")]
        public string[] Level { get; set; }

        [JsonProperty("isCoached")]
        public bool? IsCoached { get; set; }

        [JsonProperty("meetingPoint")]
        public string MeetingPoint { get; set; }

        public Session()
        {
            Context = new List<string>();
            Activity = new List<Activity>();
            Location = new List<Location>();
            Occurrences = new List<Occurrences>();

            Context.Add("https://www.openactive.io/ns/oa.jsonld");
            Context.Add("https://www.openactive.io/ns-beta/oa.jsonld");
        }
    }

    public class Image
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }
    }

    public class Person
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("logo")]
        public string Logo { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("telephone")]
        public string Telephone { get; set; }
    }

    public class Location
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }
      
        [JsonProperty("distance")]
        public string Distance { get; set; }




        [JsonProperty("geo")]
        public Geo Geo { get; set; }

        [JsonProperty("telephone")]
        public string Telephone { get; set; }

        [JsonProperty("faxNumber")]
        public string FaxNumber { get; set; }

        [JsonProperty("containsPlace")]
        public Location ContainsPlace { get; set; }

        [JsonProperty("containedInPlace")]
        public Location ContainedInPlace { get; set; }

        public List<AmenityFeature> AmenityFeature { get; set; }

        [JsonProperty("openingHoursSpecification")]
        public List<OpeningHoursSpecification> OpeningHoursSpecification { get; set; }

    }

    public class OpeningHoursSpecification
    {
        [JsonProperty("@type")]
        public string Type { get; set; }
        [JsonProperty("closes")]
        public string Closes { get; set; }
        [JsonProperty("dayOfWeek")]
        public string DayOfWeek { get; set; }
        [JsonProperty("opens")]
        public string Opens { get; set; }
        [JsonProperty("validFrom")]
        public string ValidFrom { get; set; }
        [JsonProperty("validThrough")]
        public string ValidThrough { get; set; }
    }

    public class Geo
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }
    }

    public class AmenityFeature
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public bool? Value { get; set; }

    }

    public class Activity
    {
        public string PrefLabel { get; set; }
        public string AltLabel { get; set; }
        public string InScheme { get; set; }
        public string Notation { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }

        public Activity Broader { get; set; }
        public Activity Narrower { get; set; }
    }

    public class Programme
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class EventSchedule
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Frequency { get; set; }
        public string[] ByDay { get; set; }
        public string[] ByMonth { get; set; }
        public string[] ByMonthDay { get; set; }
        public int? RepeatCount { get; set; }
        public string RepeatFrequency { get; set; }
    }
    #endregion

    #region Opportunities
    public class OpportunityContainer
    {
        public OpportunityContainer()
        {
            Items = new List<OpportunityDetail>();
        }
        [JsonProperty(PropertyName = "next")]
        public string Next { get; set; }

        [JsonProperty(PropertyName = "prev")]
        public string Prev { get; set; }

        [JsonProperty(PropertyName = "limit")]
        public long? Limit { get; set; }

        [JsonProperty(PropertyName = "page")]
        public long? Page { get; set; }

        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }

        [JsonProperty(PropertyName = "items", NullValueHandling = NullValueHandling.Ignore)]
        public List<OpportunityDetail> Items { get; set; }
    }
    public class OpportunityDetail
    {
        public OpportunityDetail()
        {
            Restrictions = new Restrictions();
            Occurrences = new List<Occurrences>();
            Categories = new Categories();
            Location = new Location_Event();
            Contact = new Contact();
            Tags = Array.Empty<string>();
            Weekdays = Array.Empty<int>();
        }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("restrictions")]
        public Restrictions Restrictions { get; set; }

        [JsonProperty("weekdays")]
        public int[] Weekdays { get; set; }

        [JsonProperty("checkoutUrl")]
        public string CheckoutUrl { get; set; }

        [JsonProperty("occurrences")]
        public List<Occurrences> Occurrences { get; set; }

        [JsonProperty("durationMins")]
        public int DurationMins { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("categories")]
        public Categories Categories { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("cost")]
        public string Cost { get; set; } = "Contact organiser";

        [JsonProperty("location")]
        public Location_Event Location { get; set; }

        [JsonProperty("contact")]
        public Contact Contact { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }
    }
    public class Restrictions
    {
        [JsonProperty("minAge")]
        public int? MinAge { get; set; }

        [JsonProperty("maxAge")]
        public int? MaxAge { get; set; }

        [JsonProperty("gender")]
        public string Gender { get; set; }
    }
    public class Occurrences
    {
        [JsonProperty("start")]
        public string Start { get; set; }

        [JsonProperty("end")]
        public string End { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("availability")]
        public string Availability { get; set; }

        [JsonProperty("checkoutUrl")]
        public string CheckoutUrl { get; set; }
    }
    public class Categories
    {
        public Categories()
        {
            Activities = Array.Empty<string>();
            DisabilitySupport = Array.Empty<string>();
        }

        [JsonProperty("activities")]
        public string[] Activities { get; set; }

        [JsonProperty("disabilitySupport")]
        public string[] DisabilitySupport { get; set; }
    }
    public class Location_Event
    {
        public Location_Event()
        {
            Coordinates = new LocationCoordinates();
        }

        [JsonProperty("coordinates")]
        public LocationCoordinates Coordinates { get; set; }

        [JsonProperty("distance")]
        public decimal Distance { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }
    }
    public class LocationCoordinates
    {
        [JsonProperty("lat")]
        public decimal Lat { get; set; }

        [JsonProperty("lng")]
        public decimal Lng { get; set; }
    }
    public class Contact
    {
        public Contact()
        {
            PointOfContact = new ContactPointOfContact();
            Organisation = new ContactOrganisation();
        }

        [JsonProperty("pointOfContact")]
        public ContactPointOfContact PointOfContact { get; set; }

        [JsonProperty("organisation")]
        public ContactOrganisation Organisation { get; set; }
    }
    public class ContactPointOfContact
    {
        [JsonProperty("telephone")]
        public string Telephone { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }
    }
    public class ContactOrganisation
    {
        [JsonProperty("twitter")]
        public string Twitter { get; set; }

        [JsonProperty("Facebook")]
        public string Facebook { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("telephone")]
        public string Telephone { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }
    }
    #endregion

    #region Organisation
    public class OrganisationContainer
    {
        public OrganisationContainer()
        {
            Items = new List<OrganisationDetail>();
        }
        [JsonProperty(PropertyName = "next")]
        public string Next { get; set; } = "";

        [JsonProperty(PropertyName = "prev")]
        public string Prev { get; set; } = "";

        [JsonProperty(PropertyName = "limit")]
        public long? Limit { get; set; }

        [JsonProperty(PropertyName = "page")]
        public long? Page { get; set; }

        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }

        [JsonProperty(PropertyName = "items", NullValueHandling = NullValueHandling.Ignore)]
        public List<OrganisationDetail> Items { get; set; }
    }
    public class OrganisationDetail
    {
        public OrganisationDetail()
        {
            Restrictions = new Restrictions();
            Categories = new Categories();
            Location = new Location_Event();
            Contact = new Contact();
            Tags = Array.Empty<string>();
        }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("tags")]
        public string[] Tags { get; set; }

        [JsonProperty("restrictions")]
        public Restrictions Restrictions { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("categories")]
        public Categories Categories { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("location")]
        public Location_Event Location { get; set; }

        [JsonProperty("contact")]
        public Contact Contact { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }
    }
    #endregion

    #region Location
    public class LocationContainer
    {
        public LocationContainer()
        {
            Items = new List<LocationDetail>();
        }

        [JsonProperty(PropertyName = "next")]
        public string Next { get; set; }

        [JsonProperty(PropertyName = "prev")]
        public string Prev { get; set; }

        [JsonProperty(PropertyName = "limit")]
        public long? Limit { get; set; }

        [JsonProperty(PropertyName = "page")]
        public long? Page { get; set; }

        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }

        [JsonProperty(PropertyName = "items", NullValueHandling = NullValueHandling.Ignore)]
        public List<LocationDetail> Items { get; set; }
    }
    public class LocationDetail
    {
        public LocationDetail()
        {
            Coordinates = new LocationCoordinates();
            Sessions = new TotalSession();
            Organisations = new TotalOrganisation();
        }

        [JsonProperty("coordinates")]
        public LocationCoordinates Coordinates { get; set; }

        [JsonProperty("sessions")]
        public TotalSession Sessions { get; set; }

        [JsonProperty("organisations")]
        public TotalOrganisation Organisations { get; set; }

        [JsonProperty("distance")]
        public int Distance { get; set; }
    }
    public class TotalSession
    {
        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }
    public class TotalOrganisation
    {
        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }
    #endregion

    #region Offer
    public class Offer
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "Offer";

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("priceCurrency")]
        public string PriceCurrency { get; set; }

        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("beta:description")]
        public string Description { get; set; }
    }
    #endregion

    #region  Facility
    public class FacilityUse
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("url")]
        public string URL { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("provider", NullValueHandling = NullValueHandling.Ignore)]
        public Provider Provider { get; set; }
        [JsonProperty("individualFacilityUse", NullValueHandling = NullValueHandling.Ignore)]
        public List<FacilityUse> IndividualFacilityUse { get; set; }
    }

    public class Provider
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
    #endregion

    #region  Slot
    public class Slot
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("startDate")]
        public string StartDate { get; set; }

        [JsonProperty("endDate")]
        public string EndDate { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("offerID", NullValueHandling = NullValueHandling.Ignore)]
        public string OfferID { get; set; }

        [JsonProperty("remainingUses")]
        public string RemainingUses { get; set; }
        [JsonProperty("maximumUses")]
        public string MaximumUses { get; set; }

        [JsonProperty("offers", NullValueHandling = NullValueHandling.Include)]
        public List<Offer> Offers { get; set; }

        public Slot()
        {
            Offers = new List<Offer>();
            Type = "Slot";
        }

    }
    #endregion
}

