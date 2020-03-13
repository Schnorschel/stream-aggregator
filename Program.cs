using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
// using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.Json;
// using System.Text.Json.Serialization
using Newtonsoft.Json;

namespace stream_aggregator
{
  class Program
  {
    static List<Person> Persons = new List<Person>();
    static List<Course> Courses = new List<Course>();
    static List<CourseEnrolled> CoursesEnrolled = new List<CourseEnrolled>();

    // Process all events in proper order
    static void ConsumeEvents(IEnumerable<EventStream> Jmodel)
    {
      foreach (var evIt in Jmodel.OrderBy(ev => ev.EventId))
      {
        ConsumeEvent(evIt);
      }
    }

    static void ConsumeEvent(EventStream evIt)
    {
      switch (evIt.Event.ToLower())
      {
        case "person_new":
          // Add new person to (local) data structure
          // Check if a person with same Id already exists
          var existingPerson = Persons.FirstOrDefault(p => p.PersonId == evIt.StreamId);
          if (existingPerson == null)
          {
            // Person does not exist, so make sure name has been provided, and if so, 
            // add new person to (local) data structure
            if (evIt.data.Name != "")
            {
              // Instantiate a new Person object, populate its properties..
              var newPerson = new Person();
              newPerson.PersonId = evIt.StreamId;
              newPerson.Name = evIt.data.Name;
              // ..and add it to List Collection Persons
              Persons.Add(newPerson);
              // Log that "person_new" event was successfully processed
              AddToLog(evIt.EventId, evIt.StreamId, $"Successfully processed event {evIt.Event} to add person '{evIt.data.Name}'.");
            }
            else
            {
              // Issue error that new person name has not been provided
              AddToLog(evIt.EventId, evIt.StreamId, $"Could not process event {evIt.Event} due to missing person name.");
            }
          }
          else
          {
            // Issue error that person already exists and cannot be replaced/overwritten
            AddToLog(evIt.EventId, evIt.StreamId, $"Could not process event {evIt.Event} due to person {evIt.data.Name} already existing.");
          }
          break;
        case "course_new":
          // Add new course to (local) data structure
          // Check first, if course already exists
          var existingCourse = Courses.FirstOrDefault(c => c.CourseId == evIt.StreamId);
          if (existingCourse == null)
          {
            // Course does not exist, so make sure course name has been provided, and if so, 
            // add it to (local) data structure
            if (evIt.data.Name != "")
            {
              var newCourse = new Course();
              newCourse.CourseId = evIt.StreamId;
              newCourse.Name = evIt.data.Name;
              Courses.Add(newCourse);
              // Log that "course_new" event was successfully processed
            }
            else
            {
              // Issue error that course name has not been provided
            }
          }
          else
          {
            // Issue error that course already exists and cannot be replaced/overwritten
          }
          break;
        case "course_enrolled":
          // Enroll existing person into existing course
          // Check that course and person exist
          existingCourse = Courses.FirstOrDefault(c => c.CourseId == evIt.StreamId);
          if (existingCourse != null)
          {
            existingPerson = Persons.FirstOrDefault(p => p.PersonId == evIt.data.PersonId);
            if (existingPerson != null)
            {
              // Make sure that enrollment does not already exist
              var existingCourseEnrolled = CoursesEnrolled.FirstOrDefault(ce => ce.CourseId == evIt.StreamId && ce.PersonId == evIt.data.PersonId);
              if (existingCourseEnrolled == null)
              {
                // Create a new CourseEnrollment object, populate all properties and save to data structure
                var newCourseEnrolled = new CourseEnrolled();
                newCourseEnrolled.CourseId = evIt.StreamId;
                // newCourseEnrolled.CourseName = existingCourse.Name;
                newCourseEnrolled.PersonId = (int)evIt.data.PersonId;
                // newCourseEnrolled.Persons = existingPerson;
                CoursesEnrolled.Add(newCourseEnrolled);
                // Log that "course_enrolled" event was successfully processed
              }
              else
              {
                // Issue error that course enrollment does already exist
              }
            }
            else
            {
              // Issue error that person does not exist
            }
          }
          else
          {
            // Issue error that course does not exist
          }
          break;
        case "course_renamed":
          // Rename existing course
          // Ensure course to rename exists
          existingCourse = Courses.FirstOrDefault(c => c.CourseId == evIt.StreamId);
          if (existingCourse != null)
          {
            if (evIt.data.Name != null)
            {
              existingCourse.Name = evIt.data.Name;
            }
            else
            {
              // Issue error that new course name was not provided
            }
          }
          else
          {
            // Issue error that course to rename does not exist
          }
          break;
        case "course_disenrolled":
          // Unenroll person from course
          // First, ensure enrollment record exists
          var existingCourseEnrolled2 = CoursesEnrolled.FirstOrDefault(ce => ce.CourseId == evIt.StreamId && ce.PersonId == evIt.data.PersonId);
          if (existingCourseEnrolled2 != null)
          {
            CoursesEnrolled.Remove(existingCourseEnrolled2);
            // Log that "course_disenrolled" event was successfully processed
          }
          else
          {
            // Issue error that course enrollment does not exist
          }
          break;
        case "person_renamed":
          // Rename existing person
          // Ensure person exists, and new name has been provided; if so, overwrite name
          existingPerson = Persons.FirstOrDefault(p => p.PersonId == evIt.StreamId);
          if (existingPerson != null && evIt.data.Name != null && evIt.data.Name != "")
          {
            existingPerson.Name = evIt.data.Name;
            // Log that "person_renamed" event was successfully processed

          }
          else
          {
            // Person does not exist or name not provided: cannot rename, issue appropriate error
          }
          break;
        default:
          // Log that event could not be processed, because it has no code handler
          break;
      }
    }

    static void DisplayEventStream(IEnumerable<EventStream> Jmodel)
    // Basic Output routine to show select data of JSON object (used for testing)
    {
      foreach (var evs in Jmodel.OrderBy(ev => ev.EventId))
      {
        Console.Write($"eventId: {evs.EventId}.");
        if (evs.data.Name != "" && evs.data.Name != null)
        {
          Console.Write($" data.name: {evs.data.Name}.");
        }
        if (evs.data.PersonId != null)
        {
          Console.Write($" data.personId: {evs.data.PersonId}");
        }
        Console.WriteLine();
      }
    }

    static void AddToLog(int eventId, int streamId, string description)
    {
      Console.WriteLine($"EventId: {eventId}. StreamId: {streamId}. [{description}]");
    }

    static void Main(string[] args)
    // - Import the event stream from external JSON file into local List object
    // - Process the event stream in proper order and - after successful validation 
    //   of each event - write data to local objects 
    {
      // Specify options for built-in JSON serializer
      var options = new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        IgnoreNullValues = true,
        WriteIndented = true
      };

      // Specify options for json.net (Newtonsoft) serializer
      var settings = new JsonSerializerSettings
      {
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore
      };

      // Import the event stream from external JSON file into local string
      var jsonString = File.ReadAllText("events.json");
      // var jsonModel = JsonSerializer.Deserialize<EventStream>(jsonString, options);
      // Port imported JSON string data into List object
      var jsonModel = JsonConvert.DeserializeObject<List<EventStream>>(jsonString, settings);

      // Display the events to be processed
      DisplayEventStream(jsonModel);

      // Process events
      ConsumeEvents(jsonModel);

      // var modelJson = System.Text.Json.JsonSerializer.Serialize(jsonModel, options);
      Console.WriteLine("Courses:");
      Console.WriteLine("--------");
      Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(Courses, options));
      Console.WriteLine();
      Console.WriteLine("Persons:");
      Console.WriteLine("--------");
      Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(Persons, options));
      Console.WriteLine();
      Console.WriteLine("Course Enrollments:");
      Console.WriteLine("-------------------");
      Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(CoursesEnrolled.Include(ce => ce.Enrollees), options));



      // Console.WriteLine(modelJson);
    }
  }
}
