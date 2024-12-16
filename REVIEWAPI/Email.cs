public class Email
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? UserEmail { get; set; }
    public bool IsSent { get; set; }
    
    public string? Secret { get; set; }
}