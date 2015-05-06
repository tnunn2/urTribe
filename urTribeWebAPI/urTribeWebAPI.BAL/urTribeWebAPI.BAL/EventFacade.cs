﻿using System;
using System.Collections.Generic;
using urTribeWebAPI.Common.Interfaces;
using urTribeWebAPI.Common.Logging;
using urTribeWebAPI.DAL.Interfaces;
using urTribeWebAPI.DAL.Factory;
using urTribeWebAPI.Common.Concrete;

namespace urTribeWebAPI.BAL
{
    class EventFacade
    {
        #region Member Variables
        private readonly IEventRepository _repository; 
        #endregion

        #region Properties
        private IEventRepository EvtRepository
        {
            get
            {
                return _repository;
            }
        }
        #endregion

        #region Constructors
        public EventFacade ()
        {
                var factory = RepositoryFactory.Instance;
                _repository = factory.Create<IEventRepository>();
        }
        #endregion

        #region Public Methods
        public void UpdateEvent(IEvent user)
        {
            try
            {
                EvtRepository.Update(user);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log = new ExceptionDTO() { FaultClass = "EventFacade", FaultMethod = "UpdateEvent", Exception = ex };
            }
        }
        public IEvent FindEvent(Guid eventId)
        {
            try
            {
                var eventList = EvtRepository.Find(evt => evt.ID == eventId);

                foreach (IEvent evt in eventList)
                    return evt;
                return null;
            }
            catch (Exception ex)
            {
                Logger.Instance.Log = new ExceptionDTO() { FaultClass = "EventFacade", FaultMethod = "FindEvent", Exception = ex };
                return null;
            }
        }
        public void AddUserToEvent(IUser user, IEvent evt)
        {
            try
            {
                EvtRepository.LinkToEvent(user, evt);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log = new ExceptionDTO() { FaultClass = "EventFacade", FaultMethod = "AddUserToEvent", Exception = ex };
            }
        }



        #endregion
    }
}
