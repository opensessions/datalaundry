public class AppSettings
{
    public decimal defaultLatitude{ get; set; }
    public decimal defaultLongitude{ get; set; }
    public decimal defaultRadius{ get; set; }
    public string sessionDetailAPI{ get; set; }
    public string organisationDetailAPI{ get; set; }
    public string opportunityAPI{ get; set; }
    public string sessionAPI{ get; set; }
    public string organisationAPI{ get; set; }
    public string locationAPI{ get; set; }
    public string scheduledSessionAPI{ get; set; }
    public int CommandTimeout{ get; set; }
    public string IsCacheEnabled{ get; set; }
    public string IsServiceLogEnabled { get; set; }
}