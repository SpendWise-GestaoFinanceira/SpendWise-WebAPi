namespace SpendWise.Application.DTOs.Auth;

public class ForgotPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
}

public class ForgotPasswordResponseDto
{
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }
}

public class ResetPasswordRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ResetPasswordResponseDto
{
    public string Message { get; set; } = string.Empty;
    public bool Success { get; set; }
}
