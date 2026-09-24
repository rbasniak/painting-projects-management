using FluentValidation.Results;

namespace PaintingProjectsManagement.Features.Models;

public sealed class Rating
{
    private Rating()
    {
        // for EF
    }

    public Rating(int value)
    {
        if (value < 0 || value > 5)
        {
            throw new ValidationException([new ValidationFailure(nameof(value), "Rating must be between 0 and 5.")]);
        }

        Value = value;
    }

    public int Value { get; private set; }

    public override string ToString() => Value.ToString();
}
