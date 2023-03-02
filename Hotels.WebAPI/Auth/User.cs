public record UserDto(string UsernName, string Password);

public record UserModel 
{
    [Required]
    public string UserName { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}