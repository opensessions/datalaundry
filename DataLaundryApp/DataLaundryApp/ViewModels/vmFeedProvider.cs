
using System.ComponentModel.DataAnnotations;

namespace DataLaundryApp.ViewModels
{
    public class vmFeedProvider
    {
        public int Id { get; set; }
        [Required]
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Name only alphabets and numbers allowed.")]
        //[System.Web.Mvc.Remote("IsNameAvailable", "FeedProvider", "Home", ErrorMessage = "Name Already Exists")]
        [Display(Name ="name")]
        public string Name { get; set; }
        [Required(ErrorMessage="The URL field is required.")]
        [RegularExpression(@"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$", ErrorMessage = "Please enter a valid url")]
        [Display(Name = "url")]
        public string Source { get; set; }
        public bool IsIminConnector { get; set; }
        [Required]
        [Display(Name = "data type")]
        public int? FeedDataTypeId { get; set; }
        public bool EndpointUp { get; set; }
        public bool UsesPagingSpec { get; set; }
        public bool IsOpenActiveCompatible { get; set; }
        public bool IncludesCoordinates { get; set; }
    }
   
}