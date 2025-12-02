namespace Source.Models;

public class Member
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }

    public Member(string username, string email, string password)
    {
        Id = Guid.NewGuid();
        Username = username;
        Password = password;
        Email = email;
    }
}
