using System;
using System.Collections.Generic;
using System.Text;
using SenseNet.ContentRepository.Storage.Data;
using System.Globalization;

namespace SenseNet.ContentRepository.Storage.ApplicationMessaging
{
    public class UploadToken
    {
        private Guid _uploadGuid;
        public Guid UploadGuid
        {
            get { return _uploadGuid; }
        }

        private int _userId;
        public int UserId
        {
            get { return _userId; }
        }

        internal UploadToken(Guid uploadGuid, int userId)
        {
            if (uploadGuid == null)
                throw new ArgumentNullException("guid");
            if (userId == 0)
                throw new ArgumentOutOfRangeException("userId", "The 'userId' must not be 0 and have to be a valid user id.");

            _uploadGuid = uploadGuid;
            _userId = userId;
        }

        public static UploadToken Generate(int userId)
        {
            UploadToken generatedToken = new UploadToken(Guid.NewGuid(), userId);
            return generatedToken;
        }

        public void Persist()
        {
            DataProvider.Current.PersistUploadToken(this);
        }

        public static UploadToken GetUploadToken(Guid uploadGuid)
        {
            int userId = DataProvider.Current.GetUserIdByUploadGuid(uploadGuid);
            if (userId != 0)
            {
                return new UploadToken(uploadGuid, userId);
            }
            else
            {
                return null;
            }
        }
    }
}