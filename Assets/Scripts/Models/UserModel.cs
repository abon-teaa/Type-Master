using System;
[Serializable]
public class UserModel
{
    public string id;          
    public string full_name;
    public string username;
    public string email;
    public int age;
    public string photo_url;
    public int high_score;
    public string created_at;
    public string updated_at;
}
[Serializable]
public class NewUserProfile
{
    public string id;          
    public string full_name;
    public string username;
    public string email;
    public int age;
    public string photo_url;
    public int high_score;
}
[Serializable]
public class UserListWrapper
{
    public UserModel[] users;
}
[Serializable]
public class SignUpRequest
{
    public string email;
    public string password;
}
[Serializable]
public class SignInRequest
{
    public string email;
    public string password;
}
[Serializable]
public class AuthResponse
{
    public string access_token;
    public string refresh_token;
    public string token_type;
    public int expires_in;
    public AuthUser user;
}
[Serializable]
public class AuthUser
{
    public string id;
    public string email;
}
[Serializable]
public class SupabaseErrorResponse
{
    public string message;
    public string error_description;
    public string msg; 
}
[Serializable]
public class UpdatePasswordRequest
{
    public string password;
}
