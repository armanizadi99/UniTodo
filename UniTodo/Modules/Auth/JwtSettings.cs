namespace UniTodo.Modules.Auth
{
    public class JwtSettings
    {
        public string Issuer { get; set;  } = string.Empty;
public string Audience { get; set;  } = string.Empty;
public int ExpirationMinutes { get; set;  }
public string SecretSigningKey { get; set;  } = string.Empty;
    }
}
