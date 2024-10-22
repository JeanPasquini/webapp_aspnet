namespace webAPP_ASPNET.Models
{
    public class Department
    {
        public int ID { get; set; }
        public string DEPARTMENTNAME { get; set; }
        public string DESCRIPTION { get; set; }
    }

    public class DepartmentRelation
    {
        public int ID { get; set; }
        public int IDUSER { get; set; }
        public int IDDEPARTMENT { get; set; }

    }
}
