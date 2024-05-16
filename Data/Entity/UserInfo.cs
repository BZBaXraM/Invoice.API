namespace InvoiceManager.API.Data.Entity;

/// <summary>
///  This class is used to define the UserInfo.
/// </summary>
public class UserInfo
{
    /// <summary>
    ///    This property is used to define the Id property.
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    ///   This property is used to define the Username property.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// This constructor is used to define the UserInfo.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="username"></param>
    public UserInfo(string id, string username)
    {
        Id = id;
        Username = username;
    }
}