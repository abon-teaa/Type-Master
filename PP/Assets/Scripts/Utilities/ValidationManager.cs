using System.Text.RegularExpressions;
public static class ValidationManager
{
    public static bool IsNotEmpty(string value) => !string.IsNullOrWhiteSpace(value);
    public static bool IsValidEmail(string email)
    {
        if (!IsNotEmpty(email)) return false;
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
    public static bool IsValidUsername(string username)
    {
        if (!IsNotEmpty(username)) return false;
        return Regex.IsMatch(username, @"^[a-zA-Z0-9_]{3,20}$");
    }
    public static bool IsStrongPassword(string password)
    {
        if (!IsNotEmpty(password) || password.Length < 8) return false;
        return Regex.IsMatch(password, @"[A-Z]") && Regex.IsMatch(password, @"[a-z]") && Regex.IsMatch(password, @"[0-9]");
    }
    public static bool DoPasswordsMatch(string password, string confirmPassword) => password == confirmPassword;
    public static bool IsValidAge(string ageText, out int age)
    {
        return int.TryParse(ageText, out age) && age >= 13 && age <= 120;
    }
    public static bool IsValidOtpFormat(string otp)
    {
        if (!IsNotEmpty(otp)) return false;
        return Regex.IsMatch(otp, @"^\d{6}$");
    }
}