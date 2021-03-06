﻿using System;

namespace urTribeWebAPI.BAL
{
    class UserContactAssociationException : Exception
    {
        #region Constant
        private const string MESSAGE = "Unable to associate these two individuals as user/contact.";
        #endregion
        public UserContactAssociationException() : base(MESSAGE)
        {
        }

        public UserContactAssociationException(string message)
            : base(message)
        {
        }


        public UserContactAssociationException(Exception inner)
            : base(MESSAGE, inner)
        {
        }

    }
}
