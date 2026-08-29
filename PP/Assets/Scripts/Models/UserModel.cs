using System;
[Serializable]
public class UserModel
{
    public string id;
    public string full_name, username, email, photo_url, updated_at;
    public int age;
}
[Serializable]
public class UserListWrapper
{
    public UserModel[] users;
}
[Serializable]
public class SignUpRequest
{
    public string email, password;
}
[Serializable]
public class SignInRequest
{
    public string email, password;
}
[Serializable]
public class UpdatePasswordRequest
{
    public string password;
}
[Serializable]
public class NewUserProfile
{
    public string id, full_name, username, email, photo_url;
    public int age;
}
[Serializable]
public class AuthResponse
{
    public string access_token, refresh_token;
    public UserAuthData user;
}
[Serializable]
public class UserAuthData
{
    public string id, email;
}
[Serializable]
public class SupabaseErrorResponse
{
    public string msg, message, error_description;
}