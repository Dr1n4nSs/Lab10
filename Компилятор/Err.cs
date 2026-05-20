namespace Компилятор
{
    public struct Err
    {
        private TextPosition _errorPosition;
        private byte _errorCode;
        private string _errorDescription;
        
        public TextPosition ErrorPosition
        {
            get
            {
                return _errorPosition;
            }
        }
        
        public byte ErrorCode
        {
            get
            {
                return _errorCode;
            }
        }
        
        public string ErrorDescription
        {
            get
            {
                return _errorDescription;
            }
        }

        public Err(TextPosition errorPosition, byte errorCode, string errorDescription)
        {
            this._errorPosition = errorPosition;
            this._errorCode = errorCode;
            this._errorDescription = errorDescription;
        }
    }
}