using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace stream_aggregator
{
  public class Person
  {
    public int PersonId { get; set; }
    public string Name { get; set; }
    [JsonIgnore]
    [ForeignKey("PersonId")]
    public CourseEnrolled CourseEnrolled { get; set; }
  }
}