public class UserRepository : IUserRepository
{
    private List<UserDto> _users => new() 
    {
        new UserDto("John", "123"),
        new UserDto("Danil", "9001"),
        new UserDto("Rebeca", "1999")    
    };
    public UserDto GetUser(UserModel userModel) =>
        _users.FirstOrDefault(um =>
            string.Equals(um.UsernName, userModel.UserName) &&
            string.Equals(um.Password, userModel.Password)) ??
            throw new Exception();
}