using Platform;

namespace Pulxer
{
    public class BotResult : IBotResult
    {
        private string _message;
        private bool _isSuccess;

        public BotResult(bool isSuccess, string msg)
        {
            _isSuccess = isSuccess;
            _message = msg;
        }

        public bool IsSuccess
        {
            get
            {
                return _isSuccess;
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }
        }
    }
}
