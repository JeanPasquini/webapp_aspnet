namespace webAPP_ASPNET.Models
{
    public class User
    {
        public int ID { get; set; }
        public string USERNAME { get; set; }
        public string PASSWORD { get; set; }
        public string FULLNAME { get; set; }
        public string EMAIL { get; set; }
    }
    public class UserWithDepartment
    {
        public User User { get; set; }
        public DepartmentRelation DepartmentRelation { get; set; }
        public Department Department { get; set; }
    }

    public static class LoggedUser
    {
        public static User User { get; set; } = new User();
        public static DepartmentRelation DepartmentRelation { get; set; } = new DepartmentRelation();
        public static Department Department { get; set; } = new Department();
    }
}
