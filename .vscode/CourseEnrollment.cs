using System.Collections.Generic;
namespace stream_aggregator
{
  public class CourseEnrollment
  {
    public int CourseId { get; set; }
    // public string CourseName {get; set;}
    public List<Person> Enrollees { get; set; } = new List<Person>();
  }
}