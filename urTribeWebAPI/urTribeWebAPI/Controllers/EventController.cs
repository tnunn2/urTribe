using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using urTribeWebAPI.Common;
using urTribeWebAPI.DAL.Interfaces;
using urTribeWebAPI.DAL.Repositories;
using urTribeWebAPI.Messaging;
using urTribeWebAPI.Models;
using WebGrease.Css.Extensions;

namespace urTribeWebAPI.Controllers
{
    [RoutePrefix("api/events")]
    public class EventController : ApiController
    {
        private readonly IMessageBroker _broker = new RealTimeBroker();
        private readonly IRepository<IUser> _userRepo = new UserRepository<User> ();
        private readonly IRepository<IEvent> _eventRepo = new EventRepository<ScheduledEvent>();


        
        /*todo: 
         * A)Get IUsers from repository
         * 
         * B)implement following responses:
         *      1)If creator doesn't exist 
         *          -- don't create, send error
         *      2)If creator exists, but event creation is unsuccessful 
         *          --send error
         *      2.5)If 2 because event already exists 
         *          -- redo with new GUID
         *      3)If creator exists, event creation is successful, but authentication is not 
         *          --don't create event, send appropriate messaging, delete event table from RTF
         *      4)If above successful, but not all invitees valid
         *          --create event, indicate what invitees invalid
         *      5)If above successful, but not all invitees authenticated
         *          --create event, but don't invite un-authenticated invitees
         *      6)If everything successful
         *          --send success
         *      
         * C)Once users authenticated, how are they subscribed?      
         * 
          */
        [HttpPut]
        public string PutEvent(Guid creatorGuid, List<Guid> inviteeIDs )
        {
            IUser creator = _userRepo.Find(u => u.ID == creatorGuid).FirstOrDefault();
            List<string> invalidIDs = new List<string>();
            IEnumerable<IUser> invitees = _userRepo.Find(u => inviteeIDs.Contains(u.ID));
            

            //Guid eventID = _repo.newEvent(creator, invitees);
            //TODO fix this
            Guid eventID = Guid.NewGuid();
            _broker.CreateAuthAndInvite(eventID, creator, invitees);

            //for now assuming #6
            return eventID.ToString();
        }


        [HttpPut]
        [ActionName("InviteTo")]
        public void AddInvitee(List<Guid> inviteeGuids, Guid eventID)
        {
            IEnumerable<IUser> invitees = _userRepo.Find(u => inviteeGuids.Contains(u.ID));
            invitees.ForEach(u => _broker.AddToChannel(u, eventID));
        }

        //should do nothing if refuse. 
        [HttpPut]
        public bool PutInviteResponse(Guid userID, Guid eventID, bool accept)
        {
            IUser user = _userRepo.Find(u => u.ID == userID).FirstOrDefault();
            BrokerResult result =_broker.RespondToInvite(user, eventID, accept);
            if (result.type == ResultType.fullsuccess && accept)
            {
                IEvent ev = _eventRepo.Find(e => e.ID == eventID).FirstOrDefault();

                // Use EventFacade  --> EventAttendeesByStatus(Guid userId, Guid eventId, EventAttendantsStatus attendStatus)
   //             ev.attendingUsers.Add(user);  
   //             ev.invitedUsers.Remove(user);
            }

            return result.ok();
        }

        public EventDescription GetEvent(Guid userID, Guid eventID)
        {
            return sampleEventFactory.getEvent1(userID, eventID);
        }

        public EventListResponse GetEvents(Guid userID)
        {
            return sampleEventFactory.user1Events(userID);
        }
    }
}
