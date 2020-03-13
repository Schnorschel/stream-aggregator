using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace stream_aggregator
{
  public class CourseEnrolled
  {
    public int CourseId { get; set; }
    [JsonIgnore]
    public int PersonId { get; set; }
    // public string CourseName { get; set; }
    // public Person Persons { get; set; }
    public virtual List<Person> Enrollees { get; set; } = new List<Person>();

  }
}