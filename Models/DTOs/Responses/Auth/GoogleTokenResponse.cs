namespace CaptchaGenerator.Models.DTOs.Responses.Auth;

public sealed record GoogleTokenResponse(string access_token,string id_token,string token_type,string refresh_token,string scope,int expires_in);




