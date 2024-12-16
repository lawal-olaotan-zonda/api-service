public class EmailDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? UserEmail { get; set; }
    public bool IsSent { get; set; }

    public EmailDTO(){}
    public EmailDTO(Email email) => 
    (Id, Name, IsSent) = (email.Id, email.Name, email.IsSent);
    
}