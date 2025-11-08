namespace Source.Models;

public class Member
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }

    public Member(string username, string email, string password)
    {
        Username = username;
        Password = password;
        Email = email;
    }
}
