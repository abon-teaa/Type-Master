using System.Text.RegularExpressions;
public static class ValidationManager
{
    public static bool IsNotEmpty(string value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
    public static bool IsValidEmail(string email)
    {
        if (!IsNotEmpty(email)) return false;
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }
    public static bool IsValidUsername(string username)
    {
        if (!IsNotEmpty(username)) return false;
        string pattern = @"^[a-zA-Z0-9_]{3,20}$";
        return Regex.IsMatch(username, pattern);
    }
    public static bool IsStrongPassword(string password)
    {
        if (!IsNotEmpty(password)) return false;
        if (password.Length < 8) return false;
        bool hasUpper = Regex.IsMatch(password, @"[A-Z]");
        bool hasLower = Regex.IsMatch(password, @"[a-z]");
        bool hasDigit = Regex.IsMatch(password, @"[0-9]");
        return hasUpper && hasLower && hasDigit;
    }
    public static bool DoPasswordsMatch(string password, string confirmPassword)
    {
        return password == confirmPassword;
    }
    public static bool IsValidAge(string ageText, out int age)
    {
        if (!int.TryParse(ageText, out age)) return false;
        return age >= 13 && age <= 120; 
    }
    public static bool IsValidOtpFormat(string otp)
    {
        if (!IsNotEmpty(otp)) return false;
        return Regex.IsMatch(otp, @"^\d{6}$");
    }
}
