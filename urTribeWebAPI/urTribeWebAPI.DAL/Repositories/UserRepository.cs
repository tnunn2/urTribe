﻿using System;
using System.Collections.Generic;
using Neo4jClient;
using urTribeWebAPI.Common;
using urTribeWebAPI.DAL.Interfaces;
using System.Configuration;

namespace urTribeWebAPI.DAL.Repositories
{
    public class UserRepository<userImpl> : IUserRepository where userImpl : IUser
    {
        #region Member Variable
        private readonly GraphClient _dbms;
        #endregion

        #region Constructor
        public UserRepository ()
        {
            string neo4jLocation = Properties.Settings.Default.Neo4jLocation;
             _dbms = new GraphClient(new Uri(neo4jLocation));
             _dbms.Connect();
        }
        #endregion

        #region Public Methods
        public void Add(IUser poco)
        {
            _dbms.Cypher.Merge("(user:User { ID: {ID} })").OnCreate().Set("user = {poco}").WithParams(new { ID = poco.ID, poco }).ExecuteWithoutResults();
        }
        public void Update(IUser usr)
        {
            _dbms.Cypher.Match ("(user:User)")
                        .Where ((userImpl user) => user.ID.ToString() == usr.ID.ToString())
                        .Set("user = {user}")
                        .WithParam ("user", usr)
                        .ExecuteWithoutResults();
        }
        public void Remove(IUser poco)
        {
            _dbms.Cypher.Match("(user:User)").Where((userImpl user) => user.ID == poco.ID).
                  OptionalMatch("(user:User)-[r]-()").Where((userImpl user) => user.ID == poco.ID).Delete("user").ExecuteWithoutResults();
        }
        public IEnumerable<IUser> Find(System.Linq.Expressions.Expression<Func<IUser, bool>> predicate)
        {
            var query = _dbms.Cypher.Match("(user:User)").Where(predicate).Return(user => user.As<userImpl>());
            var ienum = query.Results;
            
            List<IUser> list = new List<IUser>();
            foreach (IUser usr in ienum)
                list.Add(usr);

            return list;
        }
        public void AddToContactList(Guid usrId, Guid friendId)
        {
            _dbms.Cypher.Match("(user1:User)", "(user2:User)").Where((User user1) => user1.ID.ToString() == usrId.ToString())
                 .AndWhere((User user2) => user2.ID.ToString() == friendId.ToString()).CreateUnique("user1-[:FRIENDS_WITH]->user2")
                 .ExecuteWithoutResults();
        }
        public IEnumerable<IUser> RetrieveContacts(Guid userId)
        {
            //need to confirm
            var query = _dbms.Cypher.Match("(user:User)-[:FRIENDS_WITH]->(friend:User)").Where((User user) => user.ID == userId).Return(friend => friend.As<userImpl>());
            var ienum = query.Results;

            List<IUser> list = new List<IUser>();
            foreach (IUser usr in ienum)
                list.Add(usr);

            return list;
        }
        public void RemoveContact(Guid usrId, Guid friendId)
        {
            _dbms.Cypher.Match("(user:User)-[rel:FRIENDS_WITH]->(friend:User)").
                         Where((User user) => user.ID == usrId).
                         AndWhere((User friend) => friend.ID == friendId).
                         Delete("rel").ExecuteWithoutResults();
        }

        public EventAttendantsStatus RetrieveEventStatus (Guid usrId, Guid eventId)
        {
            var relationships = _dbms.Cypher.Match("(user:User)-[rel]->(evt:Event)")
                                   .Where((User user) => user.ID.ToString() == usrId.ToString())
                                   .AndWhere((ScheduledEvent evt) => evt.ID.ToString() == eventId.ToString())
                                   .Return(rel => rel.As<EventRelationship>())
                                   .Results;

            foreach (var relationship in relationships)
                return relationship.AttendStatus;

            throw new Exception ("Relationship Error");
        }

        public IEnumerable<IEvent> RetrieveAllEventsByStatus(Guid usrId, EventAttendantsStatus status)
        {
            var query =  _dbms.Cypher.Match("(user:User)-[rel:EVENTOWNER]->(evt:Event)")
                                     .Where((User user) => user.ID.ToString() == usrId.ToString())
                                     .AndWhere((ScheduledEvent evt) => evt.Active == true)
                                     .AndWhere((EventRelationship rel) => rel.AttendStatus.ToString() == status.ToString() || status.ToString() == EventAttendantsStatus.All.ToString())
                                     .Return(evt => evt.As<ScheduledEvent>())
                                     .Union()
                                     .Match("(user:User)-[rel:GUEST]->(evt:Event)")
                                     .Where((User user) => user.ID.ToString() == usrId.ToString())
                                     .AndWhere((ScheduledEvent evt) => evt.Active == true)
                                     .AndWhere((EventRelationship rel) => rel.AttendStatus.ToString() == status.ToString() || status.ToString() == EventAttendantsStatus.All.ToString())
                                     .Return(evt => evt.As<ScheduledEvent>())
                                     .Results;
            return query;
        }
        #endregion
    }
}
