using System;
using System.Text.Json;
using SharpQuest.Contracts;

namespace Capstone.Core;
//the mapper to create the correct course type from JSON
public class CourseMapper : IItemMapper
{
    public string ToJson(IItem item)
    {
        var course = (Course)item;
        
        var dto = new CourseDto(
            course.Id, 
            course.DisplayName, 
            course.Credits, 
            course.AssessmentType.ToString()
        );
        
        return JsonSerializer.Serialize(dto);
    }

    public IItem? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dto = JsonSerializer.Deserialize<CourseDto>(json, options);

            if (dto == null || 
                string.IsNullOrWhiteSpace(dto.Id) || 
                string.IsNullOrWhiteSpace(dto.DisplayName) || 
                dto.Credits == null || 
                dto.AssessmentType == null)
            {
                return null;
            }

            if (Enum.TryParse<Assessment>(dto.AssessmentType, true, out var assessment))
            {
                if (assessment == Assessment.Exam)
                {
                    return new MandatoryCourse
                    {
                        Id = dto.Id,
                        DisplayName = dto.DisplayName,
                        Credits = dto.Credits.Value,
                        AssessmentType = assessment
                    };
                }
                else
                {
                    return new ElectiveCourse
                    {
                        Id = dto.Id,
                        DisplayName = dto.DisplayName,
                        Credits = dto.Credits.Value,
                        AssessmentType = assessment
                    };
                }
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}