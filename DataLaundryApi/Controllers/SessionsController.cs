using DataLaundryApi.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using DataLaundryApi.App_Filter;

namespace DataLaundryApi.Controllers
{
    [Route("rpde")]
    [AuthorizationHandler(false)]   
    [ApiController]    
    public class SessionsController : ControllerBase
    {
        private static AppSettings _mySettings;
        public SessionsController(IOptions<AppSettings> settings)
        {
            //This is always null
            _mySettings = settings.Value;
        }
        // GET api/sessions?afterTimestamp=xxxxx&afterId=xxxxx
        [Route("scheduled-sessions")]
        public ActionResult Get(string afterTimestamp = "", string afterId = "")
        {
            long? lafterTimestamp = null, lafterId = null;
            long lafterTimestampTemp, lafterIdTemp;

            if (long.TryParse(afterTimestamp, out lafterTimestampTemp))
                lafterTimestamp = lafterTimestampTemp;

            if (long.TryParse(afterId, out lafterIdTemp))
                lafterId = lafterIdTemp;

            //var sessionContainer = FeedHelper.GetSessions(lafterTimestamp, lafterId);
            var sessionContainer = FeedHelper.GetSessions_v1(lafterTimestamp, lafterId);

            if (!string.IsNullOrEmpty(sessionContainer?.Next) && sessionContainer?.Items?.Count <= 0)
            {
                if (!sessionContainer.Next.Contains("?"))
                {
                    if (!string.IsNullOrEmpty(afterTimestamp))
                    {
                        sessionContainer.Next += "?afterTimestamp=" + afterTimestamp;

                        if (!string.IsNullOrEmpty(afterId))
                            sessionContainer.Next += "&afterId=" + afterId;
                    }
                }
            }           
            return Ok(sessionContainer);
        }
    }
}
