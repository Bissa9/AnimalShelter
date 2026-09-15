using AnimalShelter.Application.DTOs;

namespace AnimalShelter.Application.Validators;

public class AnimalQueryValidator
{
    public Dictionary<string, string[]> Validate(
        AnimalQueryDto query)
    {
        var errors = new Dictionary<string, string[]>();

        var page = query.Page ?? 1;
        var pageSize = query.PageSize ?? 10;

        if (page < 1)
        {
            errors["page"] =
                ["Page must be greater than 0."];
        }

        if (pageSize < 1 || pageSize > 100)
        {
            errors["pageSize"] =
                ["PageSize must be between 1 and 100."];
        }

        return errors;
    }
}