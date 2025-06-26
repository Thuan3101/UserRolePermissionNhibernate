namespace SalesManagement.Common.Model
{
    public class AuthResult
    {
        public bool Succeeded { get; private set; }
        public IEnumerable<AuthError> Errors { get; private set; }

        private AuthResult(bool succeeded, IEnumerable<AuthError> errors)
        {
            Succeeded = succeeded;
            Errors = errors;
        }

        public static AuthResult Success => new AuthResult(true, Array.Empty<AuthError>());
        
        public static AuthResult Failed(params AuthError[] errors)
            => new AuthResult(false, errors);
    }
}